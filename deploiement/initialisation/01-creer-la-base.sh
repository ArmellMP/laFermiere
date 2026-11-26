#!/bin/bash
# Exécuté UNE SEULE FOIS par MariaDB, à la création de la base (volume « donnees-base » vide).
#
#   - Si une sauvegarde est posée dans deploiement/import/ (lafermiere.sql ou lafermiere.sql.gz) :
#     elle est importée (vos données existantes, par ex. l'export phpMyAdmin de XAMPP).
#   - Sinon : base neuve depuis database/schema.sql.
#
# Puis le compte « lafermiere » utilisé par l'API reçoit uniquement les droits dont il a besoin
# (lire et écrire des données ; les triggers gardent les droits de leur créateur).
set -euo pipefail

client=(mariadb --protocol=socket --user=root "--password=${MARIADB_ROOT_PASSWORD}")

if [ -f /import/lafermiere.sql ] || [ -f /import/lafermiere.sql.gz ]; then
  echo "[La Fermière] Import de la sauvegarde présente dans import/"
  "${client[@]}" -e "CREATE DATABASE IF NOT EXISTS lafermiere CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci"
  if [ -f /import/lafermiere.sql.gz ]; then
    gunzip -c /import/lafermiere.sql.gz | "${client[@]}" lafermiere
  else
    "${client[@]}" lafermiere < /import/lafermiere.sql
  fi
else
  echo "[La Fermière] Base neuve depuis schema.sql"
  "${client[@]}" < /schema/schema.sql
fi

"${client[@]}" -e "GRANT SELECT, INSERT, UPDATE, DELETE ON lafermiere.* TO 'lafermiere'@'%'; FLUSH PRIVILEGES;"
echo "[La Fermière] Base prête"
