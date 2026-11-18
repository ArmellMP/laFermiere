# La Fermière — application web (front-end)

Application **pour téléphone d'abord** : les fermiers saisissent le suivi journalier au poulailler.
React 19 + Vite + TypeScript. Elle appelle l'API .NET de `src/PresentationApi`.

## Arborescence

```
Tableau de bord
ÉLEVAGE    Bandes ──► Fiche bande (Aperçu · Journal · Stocks · Ventes · Achats) ──► Suivi du jour
           Stocks
COMMERCE   Ventes · Achats · Clients · Fournisseurs
SUIVI      Alertes
```

Chaque écran suit la même structure : fil d’Ariane, en-tête et actions, indicateurs,
barre d’outils (filtres avec compteurs, recherche, tri, vue), contenu.
Un module n’est proposé que si l’API autorise le rôle connecté à en lire les données
(une seule définition : `src/navigation.tsx`).

## Saisies possibles

| Action | Où | Rôles |
|---|---|---|
| Nouvelle bande | Bandes | Admin, Fermier |
| Démarrer l’élevage / Clôturer | Fiche bande | Admin, Fermier |
| Suivi du jour (mortalités, pesée, soin) | Fiche bande | Admin, Fermier, Vétérinaire |
| Nouvelle vente | Ventes, onglet Ventes d’une bande | Admin, Comptable |
| Nouvel achat | Achats, onglet Achats d’une bande | Admin, Comptable |
| Nouveau client / fournisseur | Clients / Fournisseurs | Admin, Comptable |
| Marquer une alerte comme lue | Alertes | tous |

Les formulaires s’ouvrent dans un panneau (du bas sur téléphone, de la droite sur ordinateur),
reprennent les règles de l’API (valeurs autorisées, unités compatibles, effectif disponible)
et affichent les erreurs champ par champ.

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
