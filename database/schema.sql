-- ============================================================================
-- SCHÉMA COMPLET DE LA BASE lafermiere (MySQL / MariaDB)
-- Source : script d'origine (Wuba\BDD.docx) + corrections des migrations.
-- Dernière migration intégrée : 001_corriger_trigger_achat.sql
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
    TypeVente ENUM('PouletsVivants', 'PouletsAbattus', 'Fientes', 'Dechets', 'Plumes', 'Autre') NOT NULL,
    Quantite DECIMAL(10,2) NOT NULL,
    UniteMesure ENUM('Unite', 'Kg', 'Sac', 'Camion', 'Rouleau', 'Paquet') NOT NULL,
    PrixUnitaire DECIMAL(10,2) NOT NULL,
    MontantTotal DECIMAL(12,2) NOT NULL,
    ModePaiement ENUM('Especes', 'MobileMoney', 'Virement', 'Cheque', 'Credit') NOT NULL DEFAULT 'Especes',
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

-- Utilisateur admin par défaut
INSERT INTO Utilisateurs (Nom, Email, PasswordHash, Role) VALUES
    ('Administrateur', 'admin@ferme.local', '$2a$12$KQ5cZ2c8cXbGpVp8L9qB5.B7dQkYV2fW8cL7mN6pQrS9tUvW1xYzA', 'Admin');

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

-- Trigger : alimente le stock après un achat (version corrigée, migration 001).
-- Seuls Aliment, Medicament, Materiel et Autre entrent en stock ; unités vérifiées.
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
