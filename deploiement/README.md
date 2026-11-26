# Mettre La Fermière en ligne

Ce dossier installe toute l'application sur **un serveur Linux** (Oracle Cloud, Hetzner, OVH…) avec Docker :

| Conteneur | Rôle |
|---|---|
| `web` | **Caddy** : adresse **https** (certificat Let's Encrypt obtenu et renouvelé tout seul), sert l'application, transmet `/api` à l'API |
| `api` | L'API .NET (jamais exposée directement sur Internet) |
| `base` | **MariaDB 10.11** (version à support long, compatible avec la base XAMPP) |
| `sauvegarde` | Copie de la base **chaque nuit à 2 h** dans `sauvegardes/`, 14 jours gardés |

Le kit est testé à chaque modification par la CI GitHub (job « Kit de mise en ligne ») : construction, démarrage, vérifications, sauvegarde puis restauration sur un serveur neuf.

---

## 1. Ce qu'il faut

- Un serveur Linux (2 Go de RAM au moins, 4 Go conseillés), processeur Intel/AMD ou Arm.
- Les ports **80** et **443** ouverts vers Internet (sur Oracle Cloud : dans la « Security List » **et** dans le pare-feu du serveur).
- Une **adresse** pour le site :
  - un nom de domaine qui pointe vers l'adresse IP du serveur (ex. `app.lafermiere.cm`) ;
  - ou, gratuitement et sans rien acheter : l'adresse IP écrite avec des tirets suivie de `.sslip.io`
    (serveur `203.0.113.7` → `203-0-113-7.sslip.io`).

## 2. Installer (une seule fois)

```bash
# Docker (Ubuntu / Debian)
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER   # puis se déconnecter / reconnecter

# Le code
git clone https://github.com/ArmellMP/laFermiere.git
cd laFermiere/deploiement

# Les réglages : mots de passe techniques générés automatiquement
bash preparer.sh
nano .env        # compléter DOMAINE, ADMIN_EMAIL, ADMIN_NOM, ADMIN_MOT_DE_PASSE
```

> **Vos données de XAMPP ?** Avant de démarrer, posez votre export phpMyAdmin dans `import/lafermiere.sql`
> (voir la section 5). Sinon, une base **neuve** est créée.

```bash
docker compose up -d --build    # 5 à 10 minutes la première fois
bash verifier.sh                # doit finir par « Tout fonctionne ✅ »
```

Ouvrez `https://<DOMAINE>` sur un téléphone : connexion avec `ADMIN_EMAIL` / `ADMIN_MOT_DE_PASSE`,
puis **Administration › Comptes** pour créer les comptes des fermiers.

**Le compte public `admin@ferme.local` / `Admin123`** de la base neuve est remplacé automatiquement
par votre compte au premier démarrage : il ne fonctionne jamais sur le serveur.

## 3. Mettre à jour (nouvelle version fusionnée sur GitHub)

```bash
cd laFermiere/deploiement
git pull
docker compose up -d --build
bash verifier.sh
```

Les données ne sont pas touchées. Si la version contient une **migration de base**
(nouveau fichier dans `database/migrations/`), l'appliquer après le `git pull` :

```bash
set -a; source .env; set +a
docker compose exec -T base mariadb -uroot -p"$MOT_DE_PASSE_ROOT_BASE" lafermiere < ../database/migrations/005_exemple.sql
```

## 4. Sauvegardes

- Une copie complète chaque nuit dans `deploiement/sauvegardes/lafermiere-AAAA-MM-JJ_HHMM.sql.gz`.
- Copie immédiate : `docker compose exec sauvegarde bash /scripts/sauvegarder.sh --une-fois`
- ⚠️ Ces copies sont **sur le même serveur** : récupérez-en une régulièrement sur votre ordinateur
  (ex. `scp utilisateur@serveur:laFermiere/deploiement/sauvegardes/lafermiere-*.sql.gz .`).

### Restaurer une sauvegarde

Remplace **toutes** les données actuelles par celles de la copie :

```bash
set -a; source .env; set +a
docker compose stop api
gunzip -c sauvegardes/lafermiere-2026-12-01_0200.sql.gz | docker compose exec -T base mariadb -uroot -p"$MOT_DE_PASSE_ROOT_BASE"
docker compose start api
bash verifier.sh
```

## 5. Reprendre vos données de XAMPP

1. Sur votre PC, phpMyAdmin → base **lafermiere** → **Exporter** → **Exporter** : fichier `lafermiere.sql`.
2. Copiez-le sur le serveur dans `laFermiere/deploiement/import/lafermiere.sql`.
3. **Avant le premier** `docker compose up` : il est importé automatiquement à la création de la base.

Les comptes viennent alors de votre base : connectez-vous avec vos identifiants habituels.
Si votre base contient encore `admin@ferme.local` / `Admin123`, il est remplacé par `ADMIN_EMAIL` au démarrage.

(Base déjà créée ? L'import automatique n'a lieu qu'une fois. Utilisez « Restaurer une sauvegarde »
avec votre fichier `lafermiere.sql`, sans `gunzip -c` : `docker compose exec -T base mariadb … lafermiere < import/lafermiere.sql`.)

## 6. En cas de problème

```bash
docker compose ps                  # état des conteneurs
docker compose logs -f api         # journaux de l'API (Ctrl+C pour sortir)
docker compose logs web            # Caddy : certificat https, accès
docker compose restart api
```

- **Pas de https / certificat refusé** : vérifier que `DOMAINE` pointe bien vers l'IP du serveur et que les ports 80 et 443 sont ouverts.
- **L'API redémarre en boucle** : `docker compose logs api` affiche la cause (ex. `CompteInitial:MotDePasse` trop faible).
- **Ne jamais** lancer `docker compose down --volumes` sur le serveur : cela **efface la base**.
