# La Fermière — application web (front-end)

Application **pour téléphone d'abord** : les fermiers saisissent le suivi journalier au poulailler.
React 19 + Vite + TypeScript. Elle appelle l'API .NET de `src/PresentationApi`.

## Écrans (version test)

| Écran | Rôle |
|---|---|
| Connexion | e-mail + mot de passe ; la session reste ouverte (refresh token, 7 jours) |
| Mes bandes | bandes en cours : jour d'élevage / 45, sujets restants, mortalité |
| Détail d'une bande | chiffres clés + historique des suivis |
| Suivi du jour | date, mortalités (boutons − / +), pesée, soin, température, observations |

Les comptes Comptable voient les bandes mais ne saisissent pas de suivi (même règle que l'API).

## Lancer en local

Prérequis : Node.js 20 ou plus, XAMPP (MySQL) démarré, et l'API lancée.

```powershell
# 1. L'API (dans un premier terminal, à la racine du dépôt)
dotnet run --project src/PresentationApi --launch-profile http

# 2. Le front-end (second terminal)
cd frontend
npm install      # la première fois seulement
npm run dev
```

Ouvrir http://localhost:5173. Vite transmet les appels `/api` à l'API (port 5196) : aucun réglage CORS en local.

**Essayer sur un téléphone** : PC et téléphone sur le même Wi-Fi, ouvrir `http://<adresse-IP-du-PC>:5173`
(adresse affichée par `npm run dev` sur la ligne « Network »). Le pare-feu Windows peut demander
d'autoriser Node.js sur le réseau privé.

## Commandes

| Commande | Effet |
|---|---|
| `npm run dev` | serveur de développement (rechargement automatique) |
| `npm test` | tests (Vitest) |
| `npm run build` | vérification TypeScript + version optimisée dans `dist/` |
