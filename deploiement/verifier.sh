#!/bin/bash
# Vérifie que La Fermière fonctionne une fois démarrée (sur le serveur ou dans la CI).
#   bash verifier.sh              contrôles de base (site, https, base, connexion admin)
#   bash verifier.sh --complet    + limite de connexion par appareil (bloque votre adresse 1 minute)
set -uo pipefail
cd "$(dirname "$0")"
set -a; source .env; set +a

URL="https://${DOMAINE}"
OPTIONS=(--silent --show-error --max-time 20)
# Certificat interne de Caddy pour « localhost » (CI) : non reconnu par curl, on l'accepte
[ "$DOMAINE" = "localhost" ] && OPTIONS+=(--insecure)

echecs=0
ok()     { echo "  ✅ $1"; }
echec()  { echo "  ❌ $1"; echecs=$((echecs + 1)); }
statut() { curl "${OPTIONS[@]}" -o /dev/null -w '%{http_code}' "$@"; }
connexion() {
  curl "${OPTIONS[@]}" -H 'Content-Type: application/json' \
    -d "{\"email\":\"$1\",\"password\":\"$2\"}" "$URL/api/v1/Utilisateurs/authenticate" "${@:3}"
}

echo "Vérification de $URL"

echo "- Attente de l'API (3 minutes au plus)…"
for _ in $(seq 1 36); do
  [ "$(statut "$URL/health")" = "200" ] && break
  sleep 5
done
[ "$(statut "$URL/health")" = "200" ] && ok "API et base de données joignables (/health)" || echec "/health ne répond pas 200"

page=$(curl "${OPTIONS[@]}" "$URL/")
echo "$page" | grep -q "<title>La Fermière</title>" && ok "Application servie en https" || echec "La page d'accueil n'est pas l'application"
[ "$(statut "$URL/bandes/1")" = "200" ] && ok "Adresses de l'application (/bandes/1) servies" || echec "/bandes/1 ne renvoie pas l'application"
curl "${OPTIONS[@]}" -I "$URL/sw.js" | grep -qi "cache-control: no-cache" && ok "sw.js jamais gardé en cache (mises à jour immédiates)" || echec "sw.js sans Cache-Control: no-cache"

[ "$(connexion admin@ferme.local Admin123 -o /dev/null -w '%{http_code}')" = "401" ] \
  && ok "Compte public admin@ferme.local / Admin123 refusé" || echec "Le compte public Admin123 fonctionne encore"

jeton=$(connexion "$ADMIN_EMAIL" "$ADMIN_MOT_DE_PASSE" | sed -n 's/.*"token":"\([^"]*\)".*/\1/p')
if [ -n "$jeton" ]; then
  ok "Connexion avec votre compte administrateur ($ADMIN_EMAIL)"
  [ "$(statut -H "Authorization: Bearer $jeton" "$URL/api/v1/Bandes")" = "200" ] \
    && ok "Lecture des bandes dans la base" || echec "GET /api/v1/Bandes en erreur"
else
  echec "Connexion impossible avec ADMIN_EMAIL / ADMIN_MOT_DE_PASSE"
fi

if [ "${1:-}" = "--complet" ]; then
  # 11 mauvais essais, chacun avec une fausse adresse X-Forwarded-For : Caddy doit les ignorer,
  # donc le 11e est bloqué (sinon n'importe qui contournerait la limite)
  dernier=""
  for i in $(seq 1 11); do
    dernier=$(connexion "$ADMIN_EMAIL" "Mauvais${i}x" -H "X-Forwarded-For: 10.9.8.$i" -o /dev/null -w '%{http_code}')
  done
  [ "$dernier" = "429" ] && ok "Limite de connexion appliquée, adresse falsifiée ignorée" || echec "11e essai : $dernier au lieu de 429"
fi

echo
if [ "$echecs" -eq 0 ]; then echo "Tout fonctionne ✅"; else echo "$echecs problème(s) ❌"; exit 1; fi
