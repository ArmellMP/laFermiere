# Base de données - La Fermière

Base MySQL / MariaDB nommée `lafermiere` (en local : MariaDB 10.4 via XAMPP).

## Contenu du dossier

| Fichier | Rôle |
|---|---|
| `schema.sql` | Création complète d'une base **neuve** (tables, contraintes, triggers, données initiales). ⚠️ Commence par `DROP DATABASE` : efface tout. |
| `migrations/NNN_description.sql` | Corrections à appliquer, dans l'ordre, sur une base **existante**. Ne suppriment pas les données. |

## Règle de travail

Toute modification de structure (table, colonne, trigger, index) se fait en deux endroits :

1. une **nouvelle migration** numérotée dans `migrations/` (pour les bases existantes) ;
2. la même modification reportée dans `schema.sql` (pour qu'une base neuve soit à jour).

Une migration déjà appliquée ne se modifie plus : on en crée une nouvelle.

## Exécuter un script (XAMPP)

Depuis la racine du dépôt, ouvrir la console MariaDB (le mot de passe est demandé, rien ne s'affiche pendant la saisie) :

```powershell
& "C:\xampp\mysql\bin\mysql.exe" -u root -p --default-character-set=utf8mb4
```

Puis, pour une migration (sur la base existante) :

```sql
USE lafermiere;
source database/migrations/003_stock_achats_modification_suppression.sql;
exit
```

Ou, pour une base neuve (⚠️ efface la base `lafermiere`) :

```sql
source database/schema.sql;
exit
```

Pourquoi `source` : en PowerShell, la redirection `mysql ... < fichier.sql` n'existe pas (erreur « opérateur réservé »). `source` lit le fichier depuis la console MariaDB et comprend les lignes `DELIMITER` des triggers.

Ne jamais écrire le mot de passe dans un fichier du dépôt.

## Historique des migrations

| N° | Date | Description |
|---|---|---|
| 001 | 2026-10-31 | Trigger `trg_after_achat_insert` : poussins, services et transport n'entrent plus en stock ; unités vérifiées. |
| 002 | 2026-10-31 | Colonne `UpdatedAt` ajoutée à `SuivisJournaliers` (la modification d'un suivi échouait). |
| 003 | 2026-11-01 | Le stock suit la modification et la suppression d'un achat (procédure `sp_ajuster_stock_achat` + triggers INSERT/UPDATE/DELETE). Refus si la quantité à retirer est déjà consommée. |
