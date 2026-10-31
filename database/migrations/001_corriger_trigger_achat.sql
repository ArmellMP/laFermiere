-- ============================================================================
-- MIGRATION 001 - Correction du trigger trg_after_achat_insert
-- Date    : 2026-10-31
-- Base    : lafermiere (MySQL / MariaDB)
-- Effet   : remplace uniquement le trigger. AUCUNE donnée n'est modifiée.
-- Exécution (exemple XAMPP) :
--   C:\xampp\mysql\bin\mysql.exe -u root -p lafermiere < database\migrations\001_corriger_trigger_achat.sql
-- ============================================================================
--
-- PROBLÈMES CORRIGÉS
-- 1. Les poussins entraient dans le stock.
--    L'ancien CASE transformait 'Poussins' en 'Autre', puis le test
--    IF v_stock_type IN ('Aliment','Medicament','Materiel','Autre') était
--    toujours vrai : TOUT achat (poussins, services, transport) créait ou
--    augmentait un stock "Autre produit".
--    -> Les poussins sont des animaux (comptés dans Bandes.NbPoussinsInit),
--       les services et le transport sont des dépenses : rien à stocker.
--
-- 2. Les unités pouvaient corrompre le stock en silence.
--    Achats accepte 'Heure' et 'Km', que la colonne Stocks.UniteMesure ne
--    connaît pas. Le serveur n'étant pas en mode strict, la valeur devenait
--    une chaîne vide sans aucune erreur.
--    -> Une unité inconnue des stocks est maintenant refusée avec un message.
--
-- 3. Additionner des unités différentes faussait les quantités.
--    Ex : stock en Kg + achat en Sac => 500 Kg + 10 Sac = "510 Kg".
--    -> Si un stock existe déjà avec une autre unité, l'achat est refusé.
--
-- Comment fonctionne un refus : SIGNAL SQLSTATE '45000' lève une erreur.
-- Comme le trigger s'exécute dans la même instruction que l'INSERT de l'achat,
-- MySQL annule tout : l'achat n'est pas enregistré, le stock n'est pas touché.
-- ============================================================================

DROP TRIGGER IF EXISTS trg_after_achat_insert;

DELIMITER $$

CREATE TRIGGER trg_after_achat_insert
AFTER INSERT ON Achats
FOR EACH ROW
BEGIN
    -- Variables locales (DEFAULT NULL = "pas encore de valeur")
    DECLARE v_stock_type  VARCHAR(20) DEFAULT NULL;  -- type de stock visé
    DECLARE v_stock_id    INT         DEFAULT NULL;  -- stock existant trouvé
    DECLARE v_stock_unite VARCHAR(20) DEFAULT NULL;  -- unité de ce stock

    -- 1) Quel type de stock cet achat alimente-t-il ?
    --    NULL signifie : cet achat ne concerne pas le stock.
    SET v_stock_type =
        CASE NEW.TypeAchat
            WHEN 'Aliment'    THEN 'Aliment'
            WHEN 'Medicament' THEN 'Medicament'
            WHEN 'Materiel'   THEN 'Materiel'
            WHEN 'Autre'      THEN 'Autre'
            ELSE NULL   -- 'Poussins', 'Services', 'Transport'
        END;

    -- On ne fait rien si l'achat ne va pas dans le stock
    IF v_stock_type IS NOT NULL THEN

        -- 2) L'unité de l'achat doit exister dans Stocks.UniteMesure.
        --    Unités communes aux deux tables : Kg, L, Unite, Sac, Dose, Paquet.
        IF NEW.UniteMesure NOT IN ('Kg', 'L', 'Unite', 'Sac', 'Dose', 'Paquet') THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Unité incompatible avec le stock : utilisez Kg, L, Unite, Sac, Dose ou Paquet pour ce type d''achat';
        END IF;

        -- 3) Chercher le stock de ce type pour cette bande.
        --    (Stocks a une clé unique (BandeId, Type) : au plus un résultat.)
        --    SET ... = (SELECT ...) renvoie NULL proprement s'il n'y a aucune ligne.
        SET v_stock_id = (
            SELECT Id FROM Stocks
            WHERE BandeId = NEW.BandeId AND Type = v_stock_type
            LIMIT 1
        );

        IF v_stock_id IS NOT NULL THEN
            -- 4a) Le stock existe : vérifier que l'unité est la même
            SET v_stock_unite = (SELECT UniteMesure FROM Stocks WHERE Id = v_stock_id);

            IF v_stock_unite <> NEW.UniteMesure THEN
                SIGNAL SQLSTATE '45000'
                    SET MESSAGE_TEXT = 'L''unité de l''achat est différente de celle du stock existant pour ce type et cette bande';
            END IF;

            -- Ajouter la quantité achetée et retenir le dernier prix / fournisseur
            UPDATE Stocks
            SET QuantiteInitiale = QuantiteInitiale + NEW.Quantite,
                QuantiteActuelle = QuantiteActuelle + NEW.Quantite,
                PrixUnitaire     = NEW.PrixUnitaire,
                FournisseurId    = NEW.FournisseurId,
                UpdatedAt        = NOW()
            WHERE Id = v_stock_id;
        ELSE
            -- 4b) Pas encore de stock : le créer à partir de l'achat
            INSERT INTO Stocks (BandeId, Type, Nom, QuantiteInitiale, QuantiteActuelle,
                                UniteMesure, PrixUnitaire, FournisseurId, CreatedAt, UpdatedAt)
            VALUES (NEW.BandeId,
                    v_stock_type,
                    CASE v_stock_type
                        WHEN 'Aliment'    THEN 'Aliment acheté'
                        WHEN 'Medicament' THEN 'Médicament acheté'
                        WHEN 'Materiel'   THEN 'Matériel acheté'
                        ELSE 'Autre produit acheté'
                    END,
                    NEW.Quantite, NEW.Quantite,
                    NEW.UniteMesure, NEW.PrixUnitaire, NEW.FournisseurId, NOW(), NOW());
        END IF;
    END IF;
END$$

DELIMITER ;
