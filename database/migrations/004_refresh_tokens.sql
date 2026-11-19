-- ============================================================================
-- MIGRATION 004 - Table RefreshTokens (sessions de connexion renouvelables)
-- Date    : 2026-11-18
-- Base    : lafermiere (MySQL / MariaDB)
-- Effet   : crée une table. AUCUNE donnée existante n'est modifiée ni supprimée.
-- ============================================================================
--
-- POURQUOI
-- Le jeton d'accès (JWT) dure désormais 15 minutes. Pour ne pas obliger
-- l'utilisateur à se reconnecter toutes les 15 minutes, /authenticate renvoie
-- aussi un "refresh token" (7 jours) que le client échange contre un nouveau
-- jeton via POST /api/v1/Utilisateurs/refresh.
--
-- CONTENU
--   TokenHash   : empreinte SHA-256 du refresh token. Le token lui-même n'est
--                 JAMAIS stocké : une copie de la base ne permet pas de se connecter.
--   FamilleId   : identifiant de la session. Chaque renouvellement crée un
--                 nouveau token de la même famille (rotation).
--   UtiliseLe   : date à laquelle le token a été échangé. Un token déjà utilisé
--                 qui revient = token volé : toute la famille est révoquée.
--   RevoqueLe   : déconnexion, changement de mot de passe / rôle, désactivation.
--   Dates en UTC.
-- ============================================================================

CREATE TABLE RefreshTokens (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UtilisateurId INT NOT NULL,
    TokenHash CHAR(64) NOT NULL COMMENT 'SHA-256 (hexadécimal) du refresh token',
    FamilleId VARCHAR(36) NOT NULL COMMENT 'Session : tokens issus d''une même connexion (VARCHAR : CHAR(36) serait lu comme un Guid par MySqlConnector)',
    ExpireLe DATETIME NOT NULL COMMENT 'UTC',
    UtiliseLe DATETIME NULL COMMENT 'UTC, renseigné lors de la rotation',
    RevoqueLe DATETIME NULL COMMENT 'UTC',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY idx_token_hash (TokenHash),
    KEY idx_famille (FamilleId),
    KEY idx_utilisateur (UtilisateurId),
    FOREIGN KEY (UtilisateurId) REFERENCES Utilisateurs(Id) ON DELETE CASCADE
);
