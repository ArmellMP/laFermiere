# La Fermière

API REST de gestion d'élevage de **poulets de chair**, pensée pour les petits et moyens éleveurs (contexte camerounais). Elle remplace les cahiers et fichiers Excel par un outil centralisé : bandes, stocks, suivi journalier (mortalités, pesées, soins), clients, fournisseurs, ventes, achats, alertes et paramètres.

Projet d'apprentissage : C#, .NET, Clean Architecture et développement d'API, une entité à la fois, avec du code commenté pour être compris ligne par ligne.

---

## Sommaire

1. [Stack technique](#stack-technique)
2. [Architecture](#architecture)
3. [Installation](#installation)
4. [Lancer l'API](#lancer-lapi)
5. [Utilisateurs et rôles](#utilisateurs-et-rôles)
6. [Base de données](#base-de-données)
7. [Tests](#tests)
8. [Conventions](#conventions)

---

## Stack technique

| Domaine | Technologie |
|---|---|
| Langage / plateforme | C# sur .NET 10 (ASP.NET Core Web API) |
| CQRS | MediatR 12 (Commands / Queries / Handlers) |
| Accès aux données | Dapper + MySqlConnector |
| Base de données | MariaDB 10.4 (via XAMPP), compatible MySQL |
| Validation | FluentValidation (règles métier) + DataAnnotations (forme des requêtes) |
| Sécurité | JWT Bearer, mots de passe hachés en BCrypt, autorisation par rôles |
| Documentation API | OpenAPI + Swagger UI (en développement) |
| Tests | xUnit v3, NSubstitute, WebApplicationFactory |

## Architecture

Clean Architecture en 4 projets. Les dépendances vont toujours vers le centre : `Domain` ne dépend de personne.

```
src/
├── Domain/           Entités et enums métier (aucune dépendance technique)
├── Application/      Cas d'utilisation : Commands, Queries, Handlers, DTOs,
│                     validateurs, interfaces des repositories
├── Infrastructure/   Accès à MySQL : repositories Dapper, connexion
└── PresentationApi/  API HTTP : contrôleurs, middleware d'erreurs, JWT, Swagger
tests/
├── LaFermiere.UnitTests/         Tests rapides, sans base de données
└── LaFermiere.IntegrationTests/  Tests de l'API réelle sur une base de test
database/
├── schema.sql        Création complète d'une base neuve
└── migrations/       Modifications d'une base existante
```

Trajet d'une requête :

```
HTTP -> Controller -> MediatR -> ValidationBehavior (FluentValidation)
     -> Handler -> Repository (Dapper) -> MariaDB (contraintes + triggers)
```

## Installation

### Prérequis

- [.NET SDK 10](https://dotnet.microsoft.com/download) (`dotnet --version` doit afficher 10.x)
- [XAMPP](https://www.apachefriends.org/) avec MariaDB, démarré depuis le panneau XAMPP
- Git

> **Arrêter MySQL proprement** : avant d'éteindre le PC, cliquer sur **Stop** pour MySQL dans le panneau XAMPP. Un arrêt brutal finit par abîmer les tables internes de MariaDB.

### 1. Récupérer le code

```powershell
git clone git@github.com:ArmellMP/laFermiere.git C:\dev\LaFermiere
cd C:\dev\LaFermiere
```

> Placer le dépôt **hors de OneDrive** (par exemple `C:\dev`) : OneDrive empêche Git de changer de branche ("cannot stat 'src': Invalid argument").

### 2. Créer la base de données

⚠️ `schema.sql` commence par `DROP DATABASE lafermiere` : **il efface la base si elle existe déjà**. Pour une base existante, utiliser les migrations (voir [Base de données](#base-de-données)).

Depuis le dossier du dépôt, ouvrir la console MariaDB :

```powershell
& "C:\xampp\mysql\bin\mysql.exe" -u root -p --default-character-set=utf8mb4
```

Saisir le mot de passe root (rien ne s'affiche pendant la saisie, c'est normal), puis :

```sql
source database/schema.sql;
exit
```

La base `lafermiere` est créée avec ses 10 tables, ses triggers et un compte administrateur.

> En PowerShell, la redirection `mysql ... < schema.sql` ne fonctionne pas : utiliser `source` comme ci-dessus.

### 3. Configurer les secrets

Les secrets ne sont **jamais** dans le dépôt : ils sont stockés dans les *user-secrets* .NET, dans le profil Windows (`%APPDATA%\Microsoft\UserSecrets\`).

**Chaîne de connexion** (remplacer `VOTRE_MOT_DE_PASSE` par le mot de passe root de MariaDB) :

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=lafermiere;User=root;Password=VOTRE_MOT_DE_PASSE;CharSet=utf8mb4;AllowUserVariables=true" --project src/PresentationApi
```

**Clé de signature JWT** (au moins 32 caractères). Pour en générer une aléatoire :

```powershell
$octets = New-Object byte[] 48
[Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($octets)
$cle = [Convert]::ToBase64String($octets)
dotnet user-secrets set "JwtSettings:SecretKey" $cle --project src/PresentationApi
```

Vérifier (les valeurs s'affichent : ne pas partager cette sortie) :

```powershell
dotnet user-secrets list --project src/PresentationApi
```

> `dotnet user-secrets set` garde la commande dans l'historique PowerShell. Pour l'éviter, modifier directement le fichier `secrets.json` dans le dossier indiqué plus haut.

## Lancer l'API

```powershell
dotnet run --project src/PresentationApi
```

| Adresse | Rôle |
|---|---|
| http://localhost:5196/swagger | Documentation interactive (Development uniquement) |
| http://localhost:5196/health | État de l'API |
| http://localhost:5196/api/v1/... | Routes de l'API |

### Se connecter

Toutes les routes exigent un jeton JWT, sauf la connexion.

1. Dans Swagger, appeler `POST /api/v1/Utilisateurs/authenticate` avec le compte créé par `schema.sql` :
   ```json
   { "email": "admin@ferme.local", "password": "Admin123" }
   ```
2. Copier la valeur `token` de la réponse.
3. Cliquer sur **Authorize** (en haut de Swagger), coller le jeton, valider.

⚠️ `Admin123` est un mot de passe de **développement**, publié dans ce dépôt. Le changer (`PUT /api/v1/Utilisateurs/{id}`) sur toute base contenant de vraies données.

## Utilisateurs et rôles

Quatre rôles : **Admin**, **Fermier**, **Veterinaire**, **Comptable**.

| Données | Lecture | Écriture |
|---|---|---|
| Bandes, Stocks | Tous | Admin, Fermier |
| Suivis journaliers | Tous | Admin, Fermier, Vétérinaire |
| Clients, Ventes, Fournisseurs | Admin, Comptable | Admin, Comptable |
| Achats | Admin, Comptable, Fermier | Admin, Comptable |
| Alertes | Tous | Marquer comme lue : tous ; le reste : Admin |
| Paramètres, Utilisateurs | Admin | Admin |

Réponses : `401` sans jeton valide, `403` si le rôle n'a pas le droit.

## Base de données

- **Base neuve** : `database/schema.sql` (voir [Installation](#2-créer-la-base-de-données)).
- **Base existante** : appliquer dans l'ordre les fichiers de `database/migrations/` qui ne l'ont pas encore été, avec `source` dans la console MariaDB.
- Toute modification de structure se fait **à deux endroits** : une nouvelle migration numérotée **et** `schema.sql`. Détails dans [database/README.md](database/README.md).

Une partie des règles métier vit dans la base : contraintes `CHECK` et triggers. Par exemple, le stock suit automatiquement la création, la modification et la suppression des achats. Les refus des triggers (`SIGNAL SQLSTATE '45000'`) sont renvoyés par l'API en erreur `400` avec leur message.

## Tests

```powershell
dotnet test
```

Résultat attendu : tous les tests réussis.

| Projet | Contenu | Base de données |
|---|---|---|
| `tests/LaFermiere.UnitTests` | Validateurs, handlers (repositories simulés avec NSubstitute), ValidationBehavior | Aucune |
| `tests/LaFermiere.IntegrationTests` | API réelle en mémoire : sécurité, format des erreurs, triggers de stock, alignement des enums C# / SQL | `lafermiere_tests` |

Les tests d'intégration ont besoin de MariaDB démarré. À chaque lancement, ils **suppriment et recréent** la base `lafermiere_tests` à partir de `schema.sql`, puis y créent un compte par rôle. La base `lafermiere` n'est jamais touchée.

Identifiants MySQL utilisés par les tests, dans cet ordre :

1. la variable d'environnement `LAFERMIERE_TEST_DB_CONNECTION` (chaîne de connexion complète) ;
2. sinon, le user-secret `ConnectionStrings:DefaultConnection` de PresentationApi.

Pour ne lancer que les tests unitaires (sans MariaDB) :

```powershell
dotnet test --project tests/LaFermiere.UnitTests
```

> Le fichier `global.json` active le moteur de tests *Microsoft.Testing.Platform*, exigé par le SDK .NET 10 pour xUnit v3.

## Conventions

- **Format des erreurs** : toutes les erreurs ont la même forme. `errors` liste les messages par champ, avec le nom du champ tel qu'il apparaît dans le JSON envoyé.
  ```json
  {
    "success": false,
    "message": "Les données envoyées ne sont pas valides.",
    "errors": { "nbPoussinsInit": ["Le nombre de poussins doit être entre 1 et 50 000"] },
    "traceId": "..."
  }
  ```
- **Validation** : une règle métier s'écrit dans le validateur FluentValidation (`Application`) **et** dans les attributs de la Request (`PresentationApi`), avec les mêmes valeurs.
- **Code** : commentaires en français, pédagogiques (rôle, raison, conséquences) ; identifiants de code selon les conventions .NET.
- **Git** : une branche par sujet (`feature/`, `fix/`, `test/`, `docs/`...), des commits petits et explicites, fusion dans `master` par Pull Request en *merge commit* (l'historique détaillé est conservé).
