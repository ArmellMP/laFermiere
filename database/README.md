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

```
C:\xampp\mysql\bin\mysql.exe -u root -p lafermiere < database\migrations\001_corriger_trigger_achat.sql
```

Le mot de passe est demandé à l'exécution : ne jamais l'écrire dans un fichier du dépôt.

## Historique des migrations

| N° | Date | Description |
|---|---|---|
| 001 | 2026-10-31 | Trigger `trg_after_achat_insert` : poussins, services et transport n'entrent plus en stock ; unités vérifiées. |
| 002 | 2026-10-31 | Colonne `UpdatedAt` ajoutée à `SuivisJournaliers` (la modification d'un suivi échouait). |
| 003 | 2026-11-01 | Le stock suit la modification et la suppression d'un achat (procédure `sp_ajuster_stock_achat` + triggers INSERT/UPDATE/DELETE). Refus si la quantité à retirer est déjà consommée. |
