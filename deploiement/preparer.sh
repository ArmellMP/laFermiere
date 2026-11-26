#!/bin/bash
# Crée deploiement/.env à partir de .env.exemple, avec des mots de passe techniques aléatoires.
# À lancer une seule fois sur le serveur, depuis le dossier deploiement :  bash preparer.sh
set -euo pipefail
cd "$(dirname "$0")"

if [ -f .env ]; then
  echo ".env existe déjà : rien n'est modifié (supprimez-le d'abord pour repartir de zéro)."
  exit 1
fi

aleatoire() { openssl rand -hex "$1"; }

sed \
  -e "s/^MOT_DE_PASSE_BASE=.*/MOT_DE_PASSE_BASE=$(aleatoire 24)/" \
  -e "s/^MOT_DE_PASSE_ROOT_BASE=.*/MOT_DE_PASSE_ROOT_BASE=$(aleatoire 24)/" \
  -e "s/^CLE_JWT=.*/CLE_JWT=$(aleatoire 32)/" \
  .env.exemple > .env
chmod 600 .env
mkdir -p sauvegardes import

echo ".env créé. Ouvrez-le (nano .env) et complétez DOMAINE, ADMIN_EMAIL, ADMIN_NOM et ADMIN_MOT_DE_PASSE."
