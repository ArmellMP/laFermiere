#!/bin/bash
# Sauvegarde de la base : une copie complète (données, triggers, procédures) chaque nuit,
# compressée dans deploiement/sauvegardes/. Les copies de plus de CONSERVER_JOURS jours
# sont supprimées.
#
#   sauvegarder.sh            boucle : une copie au démarrage (après 1 min), puis chaque nuit à 2 h
#   sauvegarder.sh --une-fois une copie tout de suite, puis s'arrête
#
# Restaurer une copie : voir README.md (« Restaurer une sauvegarde »).
set -uo pipefail

DOSSIER=/sauvegardes
JOURS="${CONSERVER_JOURS:-14}"

sauvegarder() {
  local nom fichier
  nom="lafermiere-$(date +%Y-%m-%d_%H%M).sql.gz"
  fichier="$DOSSIER/$nom"
  # Fichier « .partiel » tant que la copie n'est pas complète : une copie ratée n'est jamais
  # confondue avec une bonne, et les anciennes copies ne sont pas effacées.
  if ! MYSQL_PWD="$MOT_DE_PASSE_ROOT_BASE" mariadb-dump --host=base --user=root \
      --single-transaction --routines --triggers --events --databases lafermiere \
      | gzip > "$fichier.partiel"; then
    rm -f "$fichier.partiel"
    return 1
  fi
  mv "$fichier.partiel" "$fichier"
  find "$DOSSIER" -name 'lafermiere-*.sql.gz' -mtime "+$JOURS" -delete
  echo "[Sauvegarde] $nom ($(du -h "$fichier" | cut -f1))"
}

if [ "${1:-}" = "--une-fois" ]; then
  sauvegarder
  exit $?
fi

sleep 60
while true; do
  if sauvegarder; then
    # Prochaine copie : demain à 2 h du matin
    sleep $(( $(date -d 'tomorrow 02:00' +%s) - $(date +%s) ))
  else
    echo "[Sauvegarde] ÉCHEC : nouvel essai dans 1 heure" >&2
    sleep 3600
  fi
done
