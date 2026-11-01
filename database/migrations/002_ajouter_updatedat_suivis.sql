-- ============================================================================
-- MIGRATION 002 - Colonne UpdatedAt sur SuivisJournaliers
-- Date    : 2026-10-31
-- Base    : lafermiere (MySQL / MariaDB)
-- Effet   : ajoute une colonne. AUCUNE donnée existante n'est modifiée ni supprimée.
-- Exécution (exemple XAMPP) :
--   C:\xampp\mysql\bin\mysql.exe -u root -p lafermiere < database\migrations\002_ajouter_updatedat_suivis.sql
-- ============================================================================
--
-- PROBLÈME CORRIGÉ
-- La modification d'un suivi échouait toujours (HTTP 500) :
--   "Unknown column 'UpdatedAt' in 'field list'"
-- Le repository exécute "UPDATE SuivisJournaliers SET ..., UpdatedAt = NOW()"
-- et l'entité C# SuiviJournalier possède une propriété UpdatedAt,
-- mais la table était la seule de la base à ne pas avoir cette colonne.
--
-- SOLUTION
-- Ajouter la colonne, définie exactement comme dans les autres tables :
--   - DEFAULT CURRENT_TIMESTAMP : les lignes existantes reçoivent la date d'exécution
--   - ON UPDATE CURRENT_TIMESTAMP : MySQL la met à jour seul à chaque modification
-- Placée après CreatedAt pour rester lisible.
-- ============================================================================

ALTER TABLE SuivisJournaliers
    ADD COLUMN UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
    AFTER CreatedAt;
