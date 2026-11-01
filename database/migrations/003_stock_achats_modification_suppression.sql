-- ============================================================================
-- MIGRATION 003 - Le stock suit aussi la modification et la suppression d'un achat
-- Date    : 2026-11-01
-- Base    : lafermiere (MySQL / MariaDB)
-- Effet   : crée 1 fonction, 1 procédure et 2 triggers ; remplace le trigger
--           d'insertion (même comportement). AUCUNE donnée n'est modifiée.
-- Exécution (exemple XAMPP) :
--   C:\xampp\mysql\bin\mysql.exe -u root -p lafermiere < database\migrations\003_stock_achats_modification_suppression.sql
-- ============================================================================
--
-- PROBLÈME CORRIGÉ
-- Le stock n'était mis à jour qu'à la CRÉATION d'un achat (trigger d'insertion).
--   Achat de 50 Kg d'aliment  -> stock +50          (correct)
--   Achat corrigé à 60 Kg     -> stock inchangé     (faux : il manque 10 Kg)
--   Achat supprimé            -> stock inchangé     (faux : 50 Kg "fantômes")
--
-- SOLUTION
-- Une seule procédure, sp_ajuster_stock_achat, applique un ÉCART de quantité
-- (positif = entrée en stock, négatif = sortie) au stock concerné.
-- Les trois triggers ne font que calculer cet écart et l'appeler :
--   INSERT : + quantité achetée
--   DELETE : - quantité de l'achat supprimé
--   UPDATE : même stock          -> écart = nouvelle quantité - ancienne
--            stock différent     -> on retire l'ancien achat de son stock,
--            (type changé)          puis on ajoute le nouveau dans le sien
--
-- RÈGLE MÉTIER (validée le 2026-11-01) : le stock ne descend jamais sous zéro.
-- Si la quantité à retirer a déjà été consommée (ex. achat de 50 Kg, 45 Kg
-- consommés, suppression de l'achat), l'opération est REFUSÉE.
--
-- Comment fonctionne un refus : SIGNAL SQLSTATE '45000' lève une erreur.
-- Le trigger s'exécute dans la même instruction que l'INSERT/UPDATE/DELETE
-- de l'achat : MySQL annule TOUT (l'achat reste tel quel, le stock aussi).
-- L'API renvoie alors HTTP 400 avec le message (ExceptionMiddleware).
-- ============================================================================

DROP TRIGGER IF EXISTS trg_after_achat_insert;
DROP TRIGGER IF EXISTS trg_after_achat_update;
DROP TRIGGER IF EXISTS trg_after_achat_delete;
DROP PROCEDURE IF EXISTS sp_ajuster_stock_achat;
DROP FUNCTION IF EXISTS fn_type_stock_achat;

DELIMITER $$

-- ----------------------------------------------------------------------------
-- Fonction : quel type de stock un type d'achat alimente-t-il ?
-- Renvoie NULL si l'achat ne va pas en stock :
--   - Poussins : ce sont des animaux, comptés dans Bandes.NbPoussinsInit
--   - Services, Transport : des dépenses, rien à stocker
-- DETERMINISTIC : même entrée => même sortie (MySQL peut l'optimiser).
-- NO SQL : la fonction ne lit aucune table.
-- ----------------------------------------------------------------------------
CREATE FUNCTION fn_type_stock_achat(p_type_achat VARCHAR(20))
RETURNS VARCHAR(20)
DETERMINISTIC
NO SQL
BEGIN
    RETURN CASE p_type_achat
        WHEN 'Aliment'    THEN 'Aliment'
        WHEN 'Medicament' THEN 'Medicament'
        WHEN 'Materiel'   THEN 'Materiel'
        WHEN 'Autre'      THEN 'Autre'
        ELSE NULL
    END;
END$$

-- ----------------------------------------------------------------------------
-- Procédure : applique un écart de quantité au stock alimenté par un achat.
--   p_bande_id       : bande de l'achat (Stocks est unique par bande + type)
--   p_type_achat     : type de l'achat ('Aliment', 'Poussins'...)
--   p_ecart          : > 0 entrée en stock, < 0 sortie, 0 rien à déplacer
--   p_unite          : unité de l'achat (doit être celle du stock)
--   p_prix           : prix unitaire de l'achat (retenu seulement si entrée)
--   p_fournisseur_id : fournisseur de l'achat (retenu seulement si entrée)
-- ----------------------------------------------------------------------------
CREATE PROCEDURE sp_ajuster_stock_achat(
    IN p_bande_id       INT,
    IN p_type_achat     VARCHAR(20),
    IN p_ecart          DECIMAL(10,2),
    IN p_unite          VARCHAR(20),
    IN p_prix           DECIMAL(10,2),
    IN p_fournisseur_id INT
)
-- Le label "proc" permet de quitter la procédure plus tôt avec LEAVE proc
proc: BEGIN
    DECLARE v_stock_type  VARCHAR(20)   DEFAULT NULL;  -- type de stock visé
    DECLARE v_stock_id    INT           DEFAULT NULL;  -- stock existant trouvé
    DECLARE v_stock_unite VARCHAR(20)   DEFAULT NULL;  -- unité de ce stock
    DECLARE v_disponible  DECIMAL(10,2) DEFAULT NULL;  -- quantité actuelle du stock
    DECLARE v_message     VARCHAR(128)  DEFAULT NULL;  -- message d'erreur (128 car. max)

    -- 1) Cet achat concerne-t-il le stock ? Sinon, rien à faire.
    SET v_stock_type = fn_type_stock_achat(p_type_achat);
    IF v_stock_type IS NULL THEN
        LEAVE proc;
    END IF;

    -- 2) L'unité de l'achat doit exister dans Stocks.UniteMesure.
    --    Achats accepte aussi 'Heure' et 'Km', que Stocks ne connaît pas.
    IF p_unite NOT IN ('Kg', 'L', 'Unite', 'Sac', 'Dose', 'Paquet') THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Unité incompatible avec le stock : utilisez Kg, L, Unite, Sac, Dose ou Paquet pour ce type d''achat';
    END IF;

    -- 3) Chercher le stock de ce type pour cette bande (au plus un : clé unique).
    --    SET ... = (SELECT ...) donne NULL proprement s'il n'y a aucune ligne.
    SET v_stock_id = (
        SELECT Id FROM Stocks
        WHERE BandeId = p_bande_id AND Type = v_stock_type
        LIMIT 1
    );

    -- 4) Pas de stock : on le crée seulement s'il y a une entrée.
    --    Une sortie sans stock (stock supprimé à la main) n'a rien à retirer.
    IF v_stock_id IS NULL THEN
        IF p_ecart > 0 THEN
            INSERT INTO Stocks (BandeId, Type, Nom, QuantiteInitiale, QuantiteActuelle,
                                UniteMesure, PrixUnitaire, FournisseurId, CreatedAt, UpdatedAt)
            VALUES (p_bande_id,
                    v_stock_type,
                    CASE v_stock_type
                        WHEN 'Aliment'    THEN 'Aliment acheté'
                        WHEN 'Medicament' THEN 'Médicament acheté'
                        WHEN 'Materiel'   THEN 'Matériel acheté'
                        ELSE 'Autre produit acheté'
                    END,
                    p_ecart, p_ecart,
                    p_unite, p_prix, p_fournisseur_id, NOW(), NOW());
        END IF;
        LEAVE proc;
    END IF;

    -- 5) Le stock existe : additionner des unités différentes fausserait tout
    --    (500 Kg + 10 Sac n'a pas de sens).
    SELECT UniteMesure, QuantiteActuelle
      INTO v_stock_unite, v_disponible
      FROM Stocks
     WHERE Id = v_stock_id;

    IF v_stock_unite <> p_unite THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'L''unité de l''achat est différente de celle du stock existant pour ce type et cette bande';
    END IF;

    -- 6) Rien à déplacer (ex. seule la description de l'achat a changé)
    IF p_ecart = 0 THEN
        LEAVE proc;
    END IF;

    -- 7) RÈGLE MÉTIER : on ne retire pas ce qui a déjà été consommé.
    --    Ex. stock à 5 Kg, on veut retirer 50 Kg => 45 Kg déjà consommés : refus.
    IF v_disponible + p_ecart < 0 THEN
        SET v_message = CONCAT('Stock ', v_stock_type, ' insuffisant : ', v_disponible, ' ', v_stock_unite,
                               ' restants, ', -p_ecart, ' à retirer (déjà consommé)');
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_message;
    END IF;

    -- 8) Appliquer l'écart aux deux quantités.
    --    QuantiteInitiale = total entré en stock, QuantiteActuelle = ce qui reste.
    --    Les deux bougent ensemble : la consommation (Initiale - Actuelle) ne change pas,
    --    et la contrainte "Actuelle <= Initiale" reste respectée.
    --    Prix et fournisseur : seule une ENTRÉE les remplace (dernier achat connu) ;
    --    une sortie garde ceux du stock.
    UPDATE Stocks
       SET QuantiteInitiale = QuantiteInitiale + p_ecart,
           QuantiteActuelle = QuantiteActuelle + p_ecart,
           PrixUnitaire     = IF(p_ecart > 0, p_prix, PrixUnitaire),
           FournisseurId    = IF(p_ecart > 0, p_fournisseur_id, FournisseurId),
           UpdatedAt        = NOW()
     WHERE Id = v_stock_id;
END$$

-- ----------------------------------------------------------------------------
-- Trigger INSERT : l'achat entre en stock (même comportement que la migration 001)
-- ----------------------------------------------------------------------------
CREATE TRIGGER trg_after_achat_insert
AFTER INSERT ON Achats
FOR EACH ROW
BEGIN
    CALL sp_ajuster_stock_achat(NEW.BandeId, NEW.TypeAchat, NEW.Quantite,
                                NEW.UniteMesure, NEW.PrixUnitaire, NEW.FournisseurId);
END$$

-- ----------------------------------------------------------------------------
-- Trigger DELETE : l'achat supprimé sort du stock
-- OLD = la ligne telle qu'elle était avant la suppression
-- ----------------------------------------------------------------------------
CREATE TRIGGER trg_after_achat_delete
AFTER DELETE ON Achats
FOR EACH ROW
BEGIN
    CALL sp_ajuster_stock_achat(OLD.BandeId, OLD.TypeAchat, -OLD.Quantite,
                                OLD.UniteMesure, NULL, NULL);
END$$

-- ----------------------------------------------------------------------------
-- Trigger UPDATE : OLD = avant modification, NEW = après
-- ----------------------------------------------------------------------------
CREATE TRIGGER trg_after_achat_update
AFTER UPDATE ON Achats
FOR EACH ROW
BEGIN
    DECLARE v_ancien_stock  VARCHAR(20) DEFAULT NULL;
    DECLARE v_nouveau_stock VARCHAR(20) DEFAULT NULL;

    SET v_ancien_stock  = fn_type_stock_achat(OLD.TypeAchat);
    SET v_nouveau_stock = fn_type_stock_achat(NEW.TypeAchat);

    IF v_ancien_stock IS NOT NULL
       AND v_ancien_stock = v_nouveau_stock
       AND OLD.BandeId = NEW.BandeId THEN
        -- Même stock : on applique seulement la différence.
        -- Retirer l'ancien puis ajouter le nouveau échouerait à tort :
        -- achat 50 Kg, 45 consommés, corrigé à 60 => retirer 50 d'un stock de 5 = refus,
        -- alors que le résultat final (+10) est valide.
        CALL sp_ajuster_stock_achat(NEW.BandeId, NEW.TypeAchat, NEW.Quantite - OLD.Quantite,
                                    NEW.UniteMesure, NEW.PrixUnitaire, NEW.FournisseurId);
    ELSE
        -- Stock différent (type changé, ex. Aliment -> Transport) :
        -- l'ancien achat sort de son stock, le nouveau entre dans le sien.
        -- Pour un type hors stock, la procédure ne fait rien.
        CALL sp_ajuster_stock_achat(OLD.BandeId, OLD.TypeAchat, -OLD.Quantite,
                                    OLD.UniteMesure, NULL, NULL);
        CALL sp_ajuster_stock_achat(NEW.BandeId, NEW.TypeAchat, NEW.Quantite,
                                    NEW.UniteMesure, NEW.PrixUnitaire, NEW.FournisseurId);
    END IF;
END$$

DELIMITER ;
