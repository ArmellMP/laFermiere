-- ============================================================================
-- SCHÉMA COMPLET DE LA BASE lafermiere (MySQL / MariaDB)
-- Source : script d'origine (Wuba\BDD.docx), aligné le 2026-10-31 sur la base
--          réellement utilisée (structure comparée colonne par colonne),
--          + corrections des migrations.
-- Dernière migration intégrée : 003_stock_achats_modification_suppression.sql
--
-- RÈGLE : les valeurs des colonnes ENUM doivent être EXACTEMENT les noms des
-- enums C# correspondants (Domain/Enums). Le code écrit enum.ToString() en base :
-- un nom absent de l'ENUM SQL serait enregistré comme une chaîne vide.
--
-- ⚠️  ATTENTION : ce script commence par DROP DATABASE.
--     Il SUPPRIME la base existante et TOUTES ses données.
--     À utiliser uniquement pour créer une base neuve (nouveau poste, tests).
--     Pour une base existante, exécuter les fichiers de database/migrations/.
-- ============================================================================

-- ============================================
-- SUPPRESSION ET CRÉATION DE LA BASE
-- ============================================
DROP DATABASE IF EXISTS lafermiere;
CREATE DATABASE lafermiere CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE lafermiere;

-- ============================================
-- TABLE: BANDES
-- ============================================
CREATE TABLE Bandes (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Code VARCHAR(50) UNIQUE NOT NULL COMMENT 'Format: ANNEE-MOIS-JOUR-NUM',
    DateArrivee DATE NOT NULL,
    NbPoussinsInit INT NOT NULL,
    NbPoussinsActuel INT NOT NULL DEFAULT 0,
    Statut ENUM('EnPreparation', 'EnCours', 'Cloturee', 'Archivee') NOT NULL DEFAULT 'EnPreparation',
    DateCloture DATE,
    Observations TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    SuperficieCalculee DECIMAL(8,2) GENERATED ALWAYS AS (NbPoussinsInit / 10.0) STORED
);

-- CHECK constraints pour Bandes
ALTER TABLE Bandes 
ADD CONSTRAINT chk_nb_poussins_init CHECK (NbPoussinsInit > 0);

ALTER TABLE Bandes 
ADD CONSTRAINT chk_nb_poussins_actuel CHECK (NbPoussinsActuel >= 0);

ALTER TABLE Bandes 
ADD CONSTRAINT chk_poussins_coherence CHECK (NbPoussinsActuel <= NbPoussinsInit);

ALTER TABLE Bandes 
ADD CONSTRAINT chk_date_cloture CHECK (DateCloture IS NULL OR DateCloture >= DateArrivee);

-- ============================================
 -- TABLE: UTILISATEURS
-- ============================================
CREATE TABLE Utilisateurs (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nom VARCHAR(100) NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role ENUM('Admin', 'Fermier', 'Veterinaire', 'Comptable') NOT NULL DEFAULT 'Fermier',
    EstActif BOOLEAN DEFAULT TRUE,
    DerniereConnexion DATETIME,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ============================================
-- TABLE: CLIENTS
-- ============================================
CREATE TABLE Clients (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nom VARCHAR(100) NOT NULL,
    TypeClient ENUM('Particulier', 'Restaurant', 'Marche', 'Grossiste', 'Transformateur', 'Autre') DEFAULT 'Particulier',
    Telephone VARCHAR(20),
    Email VARCHAR(100),
    Adresse TEXT,
    Notes TEXT,
    EstActif BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ============================================
-- TABLE: FOURNISSEURS
-- ============================================
CREATE TABLE Fournisseurs (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Nom VARCHAR(100) NOT NULL,
    TypeFournisseur ENUM('Poussiniere', 'Aliment', 'Medicament', 'Materiel', 'Services', 'Transport', 'Autre') NOT NULL,
    Telephone VARCHAR(20),
    Email VARCHAR(100),
    Adresse TEXT,
    ContactPersonne VARCHAR(100),
    Notes TEXT,
    EstActif BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ============================================
-- TABLE: STOCKS
-- ============================================
CREATE TABLE Stocks (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    BandeId INT NOT NULL,
    Type ENUM('Aliment', 'Medicament', 'Vaccin', 'Materiel', 'Autre') NOT NULL,
    Nom VARCHAR(100) NOT NULL,
    QuantiteInitiale DECIMAL(10,2) NOT NULL,
    QuantiteActuelle DECIMAL(10,2) NOT NULL,
    UniteMesure ENUM('Kg', 'g', 'L', 'ml', 'Unite', 'Sac', 'Dose', 'Rouleau', 'Paquet') NOT NULL,
    SeuilAlerte DECIMAL(10,2),
    DatePeremption DATE,
    PrixUnitaire DECIMAL(10,2),
    FournisseurId INT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY idx_bande_type (BandeId, Type),
    FOREIGN KEY (BandeId) REFERENCES Bandes(Id) ON DELETE CASCADE
);

-- CHECK constraints pour Stocks (une par une)
ALTER TABLE Stocks 
ADD CONSTRAINT chk_quantite_initiale CHECK (QuantiteInitiale >= 0);

ALTER TABLE Stocks 
ADD CONSTRAINT chk_quantite_actuelle CHECK (QuantiteActuelle >= 0);

ALTER TABLE Stocks 
ADD CONSTRAINT chk_prix_unitaire CHECK (PrixUnitaire >= 0);

ALTER TABLE Stocks 
ADD CONSTRAINT chk_quantite_coherence CHECK (QuantiteActuelle <= QuantiteInitiale);

-- ============================================
-- TABLE: SUIVIS_JOURNALIERS
-- ============================================
CREATE TABLE SuivisJournaliers (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    BandeId INT NOT NULL,
    Jour INT NOT NULL COMMENT 'Jour de l''élevage (1 à 45)',
    DateSuivi DATE NOT NULL,
    NbMortalites INT DEFAULT 0,
    CauseMortalite VARCHAR(200),
    PoidsMoyen DECIMAL(6,2) COMMENT 'NULL si pas de pesée ce jour',
    NbSujetsPeses INT,
    TypeSoin ENUM('Vaccination', 'Traitement', 'Prevention', 'Supplement', 'Autre'),
    ProduitUtilise VARCHAR(100),
    Dosage VARCHAR(100),
    DureeTraitement INT,
    CoutSoin DECIMAL(10,2),
    StockConsommeId INT,
    QuantiteConsommee DECIMAL(10,2),
    Temperature DECIMAL(4,1),
    Observations TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,  -- migration 002
    CreatedBy INT,
    FOREIGN KEY (BandeId) REFERENCES Bandes(Id) ON DELETE CASCADE,
    FOREIGN KEY (StockConsommeId) REFERENCES Stocks(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Utilisateurs(Id),
    UNIQUE KEY idx_bande_jour (BandeId, Jour),
    UNIQUE KEY idx_bande_date (BandeId, DateSuivi)
);

-- CHECK constraints pour SuivisJournaliers (une par une)
ALTER TABLE SuivisJournaliers 
ADD CONSTRAINT chk_jour_range CHECK (Jour >= 1 AND Jour <= 45);

ALTER TABLE SuivisJournaliers 
ADD CONSTRAINT chk_nb_mortalites CHECK (NbMortalites >= 0);

ALTER TABLE SuivisJournaliers 
ADD CONSTRAINT chk_poids CHECK (PoidsMoyen IS NULL OR PoidsMoyen > 0);

ALTER TABLE SuivisJournaliers 
ADD CONSTRAINT chk_temperature CHECK (Temperature IS NULL OR Temperature BETWEEN 10 AND 45);

-- ============================================
-- TABLE: VENTES
-- ============================================
CREATE TABLE Ventes (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    BandeId INT NOT NULL,
    ClientId INT NOT NULL,
    DateVente DATE NOT NULL,
    TypeVente ENUM('PouletsVivants', 'PouletsNettoyes', 'PouletsFumes', 'Fientes', 'Autres') NOT NULL,  -- = enum C# TypeVente
    Quantite DECIMAL(10,2) NOT NULL,
    UniteMesure ENUM('Unite', 'Kg', 'Sac', 'Camion', 'Rouleau', 'Paquet') NOT NULL,
    PrixUnitaire DECIMAL(10,2) NOT NULL,
    MontantTotal DECIMAL(12,2) NOT NULL,
    ModePaiement ENUM('Especes', 'PaiementMobile', 'VirementBancaire', 'Cheque', 'Credit') NOT NULL DEFAULT 'Especes',  -- = enum C# ModePaiement
    EstPaye BOOLEAN DEFAULT TRUE,
    DatePaiement DATE,
    Observations TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedBy INT,
    FOREIGN KEY (BandeId) REFERENCES Bandes(Id),
    FOREIGN KEY (ClientId) REFERENCES Clients(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Utilisateurs(Id)
);

-- CHECK constraints pour Ventes (une par une - SANS CURDATE)
ALTER TABLE Ventes 
ADD CONSTRAINT chk_quantite_vente CHECK (Quantite > 0);

ALTER TABLE Ventes 
ADD CONSTRAINT chk_prix_unitaire_vente CHECK (PrixUnitaire > 0);

ALTER TABLE Ventes 
ADD CONSTRAINT chk_montant_total_vente CHECK (MontantTotal > 0);

ALTER TABLE Ventes 
ADD CONSTRAINT chk_montant_coherent CHECK (MontantTotal = Quantite * PrixUnitaire);

-- Note: On ne peut pas utiliser CURDATE() dans CHECK, on gérera cette logique dans l'application

-- ============================================
-- TABLE: ACHATS
-- ============================================
CREATE TABLE Achats (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    FournisseurId INT NOT NULL,
    BandeId INT NOT NULL,
    DateAchat DATE NOT NULL,
    TypeAchat ENUM('Poussins', 'Aliment', 'Medicament', 'Materiel', 'Services', 'Transport', 'Autre') NOT NULL,
    Description TEXT,
    Quantite DECIMAL(10,2) NOT NULL,
    UniteMesure ENUM('Unite', 'Kg', 'L', 'Sac', 'Dose', 'Heure', 'Km', 'Paquet') NOT NULL,
    PrixUnitaire DECIMAL(10,2) NOT NULL,
    MontantTotal DECIMAL(12,2) NOT NULL,
    FactureNumero VARCHAR(50),
    DateReception DATE,
    Observations TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedBy INT,
    FOREIGN KEY (FournisseurId) REFERENCES Fournisseurs(Id),
    FOREIGN KEY (BandeId) REFERENCES Bandes(Id),
    FOREIGN KEY (CreatedBy) REFERENCES Utilisateurs(Id)
);

-- CHECK constraints pour Achats (une par une)
ALTER TABLE Achats 
ADD CONSTRAINT chk_quantite_achat CHECK (Quantite > 0);

ALTER TABLE Achats 
ADD CONSTRAINT chk_prix_unitaire_achat CHECK (PrixUnitaire > 0);

ALTER TABLE Achats 
ADD CONSTRAINT chk_montant_total_achat CHECK (MontantTotal > 0);

ALTER TABLE Achats 
ADD CONSTRAINT chk_montant_achat CHECK (MontantTotal = Quantite * PrixUnitaire);

ALTER TABLE Achats 
ADD CONSTRAINT chk_date_reception CHECK (DateReception IS NULL OR DateReception >= DateAchat);

-- ============================================
-- TABLE: PARAMETRES
-- ============================================
CREATE TABLE Parametres (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Cle VARCHAR(50) UNIQUE NOT NULL,
    Valeur TEXT,
    Description VARCHAR(200),
    Categorie ENUM('Elevage', 'Stock', 'Finance', 'Systeme', 'General') DEFAULT 'General',
    EstModifiable BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- ============================================
-- TABLE: ALERTES
-- ============================================
CREATE TABLE Alertes (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    TypeAlerte ENUM('StockFaible', 'Peremption', 'MortaliteElevee', 'Temperature', 'PeseeManquante', 'BandeACloturer') NOT NULL,
    EntiteId INT NOT NULL,
    Niveau ENUM('Info', 'Avertissement', 'Critique') DEFAULT 'Avertissement',
    Message TEXT NOT NULL,
    EstLue BOOLEAN DEFAULT FALSE,
    DateAlerte DATETIME DEFAULT CURRENT_TIMESTAMP,
    DateLecture DATETIME,
    UtilisateurId INT,
    FOREIGN KEY (UtilisateurId) REFERENCES Utilisateurs(Id)
);

-- ============================================
-- DONNÉES INITIALES
-- ============================================
-- Paramètres par défaut
INSERT INTO Parametres (Cle, Valeur, Description, Categorie) VALUES
    ('DensiteElevage', '10', 'Nombre de poulets par m²', 'Elevage'),
    ('DureeCycle', '45', 'Durée normale d''un cycle en jours', 'Elevage'),
    ('SeuilAlerteMortalite', '5', 'Seuil d''alerte mortalité (%)', 'Elevage'),
    ('JoursPesee', '7,14,21,28,35,42', 'Jours où effectuer les pesées', 'Elevage'),
    ('TemperatureIdealeMin', '20', 'Température minimale idéale (°C)', 'Elevage'),
    ('TemperatureIdealeMax', '25', 'Température maximale idéale (°C)', 'Elevage'),
    ('SeuilAlerteStock', '10', 'Seuil d''alerte stock (%)', 'Stock'),
    ('JoursAlertePeremption', '15', 'Jours avant péremption pour alerte', 'Stock'),
    ('TauxTVA', '19.25', 'Taux de TVA applicable (%)', 'Finance'),
    ('Devise', 'XAF', 'Devise par défaut', 'Finance'),
    ('EntrepriseNom', 'Ma Ferme Avicole', 'Nom de l''entreprise', 'Systeme');

-- Utilisateur admin par défaut, pour pouvoir se connecter sur une base neuve.
--   Email : admin@ferme.local   Mot de passe : Admin123
--   Hash BCrypt (facteur 12), même méthode que Application/Common/Security/PasswordHasher.
--   (L'ancien hash de ce script était factice : aucun mot de passe ne lui correspondait.)
--   ⚠️ Mot de passe de DÉVELOPPEMENT, public : à changer sur toute base réelle.
INSERT INTO Utilisateurs (Nom, Email, PasswordHash, Role) VALUES
    ('Administrateur', 'admin@ferme.local', '$2a$12$cno2DpEh3P/m54T4F0LDLOhnpxNd/JNoNvefsv3EwrXMk5GnzSv4e', 'Admin');

-- ============================================
-- INDEX POUR PERFORMANCES
-- ============================================
CREATE INDEX idx_bandes_statut ON Bandes(Statut);
CREATE INDEX idx_bandes_date_arrivee ON Bandes(DateArrivee);
CREATE INDEX idx_bandes_code ON Bandes(Code);
CREATE INDEX idx_stocks_bande_id ON Stocks(BandeId);
CREATE INDEX idx_stocks_type ON Stocks(Type);
CREATE INDEX idx_suivis_bande_id ON SuivisJournaliers(BandeId);
CREATE INDEX idx_suivis_date ON SuivisJournaliers(DateSuivi);
CREATE INDEX idx_ventes_bande_id ON Ventes(BandeId);
CREATE INDEX idx_ventes_date ON Ventes(DateVente);
CREATE INDEX idx_ventes_client_id ON Ventes(ClientId);
CREATE INDEX idx_achats_bande_id ON Achats(BandeId);
CREATE INDEX idx_achats_date ON Achats(DateAchat);
CREATE INDEX idx_achats_fournisseur ON Achats(FournisseurId);
CREATE INDEX idx_alertes_lues ON Alertes(EstLue);
CREATE INDEX idx_alertes_type ON Alertes(TypeAlerte);

-- ============================================
-- TRIGGERS
-- ============================================
DELIMITER $$

-- Stock alimenté par les achats (migration 003) : une procédure commune
-- applique l'écart de quantité ; les triggers INSERT, UPDATE et DELETE l'appellent.
-- Règle : le stock ne descend jamais sous zéro (sinon refus SIGNAL 45000).

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

-- Trigger pour vérifier les mortalités avant insertion
CREATE TRIGGER trg_before_suivi_insert
BEFORE INSERT ON SuivisJournaliers
FOR EACH ROW
BEGIN
    DECLARE v_poussins_restants INT;
    
    -- Récupérer le nombre de poussins actuels
    SELECT NbPoussinsActuel INTO v_poussins_restants
    FROM Bandes 
    WHERE Id = NEW.BandeId;
    
    -- Vérifier que les mortalités ne dépassent pas le nombre restant
    IF NEW.NbMortalites > v_poussins_restants THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Le nombre de mortalités dépasse le nombre de poussins restants dans la bande';
    END IF;
END$$

-- Trigger pour mettre à jour les poussins après mortalités
CREATE TRIGGER trg_after_suivi_insert
AFTER INSERT ON SuivisJournaliers
FOR EACH ROW
BEGIN
    -- Mettre à jour le nombre de poussins actuels dans la bande
    UPDATE Bandes 
    SET NbPoussinsActuel = NbPoussinsActuel - NEW.NbMortalites,
        UpdatedAt = NOW()
    WHERE Id = NEW.BandeId;
    
    -- Créer une alerte si mortalité élevée (plus de 5 poussins en un jour)
    IF NEW.NbMortalites >= 5 THEN
        INSERT INTO Alertes (TypeAlerte, EntiteId, Niveau, Message, DateAlerte)
        VALUES ('MortaliteElevee', NEW.BandeId, 'Avertissement',
                CONCAT('Mortalité élevée: ', NEW.NbMortalites, ' poussins jour ', NEW.Jour),
                NOW());
    END IF;
    
    -- Mettre à jour le stock si consommation enregistrée
    IF NEW.StockConsommeId IS NOT NULL AND NEW.QuantiteConsommee > 0 THEN
        UPDATE Stocks 
        SET QuantiteActuelle = QuantiteActuelle - NEW.QuantiteConsommee,
            UpdatedAt = NOW()
        WHERE Id = NEW.StockConsommeId;
    END IF;
END$$

-- Trigger pour vérifier le stock avant consommation
CREATE TRIGGER trg_before_suivi_update_stock
BEFORE UPDATE ON SuivisJournaliers
FOR EACH ROW
BEGIN
    DECLARE v_stock_disponible DECIMAL(10,2);
    
    -- Si on modifie la consommation et qu'un stock est concerné
    IF NEW.StockConsommeId IS NOT NULL AND NEW.QuantiteConsommee > 0 THEN
        -- Vérifier qu'il y a assez de stock
        SELECT QuantiteActuelle INTO v_stock_disponible
        FROM Stocks 
        WHERE Id = NEW.StockConsommeId;
        
        IF v_stock_disponible < NEW.QuantiteConsommee THEN
            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Stock insuffisant pour cette consommation';
        END IF;
    END IF;
END$$

DELIMITER ;

-- ============================================
-- VÉRIFICATION FINALE
-- ============================================
SELECT 'Base de données lafermiere créée avec succès!' as Message;

-- Afficher toutes les tables
SHOW TABLES;

-- Afficher le nombre de tables créées
SELECT COUNT(*) as NombreTables FROM information_schema.tables 
WHERE table_schema = 'lafermiere';

-- Afficher quelques données de test
SELECT 'Tables créées:' as Info;
SELECT TABLE_NAME as TableName FROM information_schema.tables 
WHERE table_schema = 'lafermiere' ORDER BY TABLE_NAME;
