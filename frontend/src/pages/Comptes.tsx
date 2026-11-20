// Gestion des comptes (administrateur) : qui peut se connecter et avec quels droits.
// Créer un compte pour un fermier, réinitialiser un mot de passe oublié,
// désactiver un compte (ses saisies sont conservées), supprimer un compte vierge.
import { useMemo, useState, type ReactNode } from 'react'
import { api } from '../api/client'
import type { CompteUtilisateur, Role } from '../api/types'
import { useAuth } from '../auth/AuthContext'
import { EtatChargement, useDonnees } from '../components/Chargement'
import { Dialogue } from '../components/Confirmation'
import { Champ, FormulairePanneau, Panneau, useEnvoi, useNotifier } from '../components/Formulaire'
import { IconeCompte, IconeCopier, IconePlus, IconeRecherche } from '../components/Icones'
import { Badge, BarreOutils, EnTetePage, Indicateurs, Puces, Recherche, Vide, useEtatUrl, type Ton } from '../components/Page'
import { Tableau, type Colonne } from '../components/Tableau'
import { ConfirmationSuppression, type DemandeSuppression } from '../formulaires/Actions'
import { BoutonLigne, BoutonsLigne } from '../formulaires/Gestion'
import { dateLisible, normaliser } from '../outils'

// ---------------------------------------------------------------------------
// Rôles : ce que chacun peut faire (repris des droits de l'API)
// ---------------------------------------------------------------------------
export const ROLES: { valeur: Role; libelle: string; description: string; ton: Ton }[] = [
  { valeur: 'Fermier', libelle: 'Fermier', description: 'Bandes, suivi du jour, stocks et achats.', ton: 'vert' },
  { valeur: 'Veterinaire', libelle: 'Vétérinaire', description: 'Consulte les bandes, saisit les suivis et les soins.', ton: 'soin' },
  { valeur: 'Comptable', libelle: 'Comptable', description: 'Ventes, achats, clients et fournisseurs.', ton: 'ciel' },
  { valeur: 'Admin', libelle: 'Administrateur', description: 'Tout, y compris la gestion des comptes.', ton: 'soleil' },
]
const roleInfo = (r: Role) => ROLES.find((x) => x.valeur === r) ?? ROLES[0]

// ---------------------------------------------------------------------------
// Mot de passe : mêmes règles que l'API (6 caractères, majuscule, minuscule, chiffre)
// ---------------------------------------------------------------------------
export const REGLES_MOT_DE_PASSE = [
  { texte: 'Au moins 6 caractères', ok: (m: string) => m.length >= 6 },
  { texte: 'Une majuscule', ok: (m: string) => /[A-Z]/.test(m) },
  { texte: 'Une minuscule', ok: (m: string) => /[a-z]/.test(m) },
  { texte: 'Un chiffre', ok: (m: string) => /[0-9]/.test(m) },
]
export const motDePasseValide = (m: string) => m.length <= 100 && REGLES_MOT_DE_PASSE.every((r) => r.ok(m))

const MOTS = ['Plume', 'Grain', 'Soleil', 'Prairie', 'Poussin', 'Recolte', 'Ferme', 'Panier', 'Colline', 'Riviere', 'Mais', 'Manioc']

/** Mot de passe facile à dicter au téléphone : « Plume-Soleil-48 ». */
export function genererMotDePasse(): string {
  const aleatoire = new Uint32Array(3)
  crypto.getRandomValues(aleatoire)
  const mot = (n: number) => MOTS[n % MOTS.length]
  return `${mot(aleatoire[0])}-${mot(aleatoire[1])}-${10 + (aleatoire[2] % 90)}`
}

/** « aujourd'hui », « hier », « il y a 5 jours », sinon la date. */
function depuis(iso?: string | null): string {
  if (!iso) return 'Jamais connecté'
  const jours = Math.floor((Date.now() - new Date(iso).getTime()) / 86_400_000)
  if (jours <= 0) return 'Aujourd’hui'
  if (jours === 1) return 'Hier'
  if (jours < 30) return `Il y a ${jours} jours`
  return dateLisible(iso)
}

const initiales = (nom: string) =>
  nom
    .split(/\s+/)
    .map((m) => m[0])
    .join('')
    .slice(0, 2)
    .toUpperCase()

// ===========================================================================
// Écran
// ===========================================================================
type Filtre = 'actifs' | 'desactives' | 'tous'

export default function Comptes() {
  const { utilisateur } = useAuth()
  const notifier = useNotifier()
  const [filtre, setFiltre] = useEtatUrl<Filtre>('etat', 'actifs')
  const [recherche, setRecherche] = useState('')
  const [nouveau, setNouveau] = useState(false)
  const [edition, setEdition] = useState<CompteUtilisateur | null>(null)
  const [motDePasse, setMotDePasse] = useState<CompteUtilisateur | null>(null)
  const [identifiants, setIdentifiants] = useState<Identifiants | null>(null)
  const [suppression, setSuppression] = useState<DemandeSuppression | null>(null)
  const [bascule, setBascule] = useState<CompteUtilisateur | null>(null)

  const { donnees, erreur, enCours, recharger } = useDonnees(() => api.liste<CompteUtilisateur>('/Utilisateurs'), [])
  const comptes = useMemo(() => donnees ?? [], [donnees])
  const actifs = comptes.filter((c) => c.estActif)

  const filtres = comptes
    .filter((c) => (filtre === 'actifs' ? c.estActif : filtre === 'desactives' ? !c.estActif : true))
    .filter((c) => !recherche.trim() || normaliser(`${c.nom} ${c.email} ${c.roleLibelle}`).includes(normaliser(recherche.trim())))

  const estMoi = (c: CompteUtilisateur) => c.id === utilisateur?.id

  const colonnes: Colonne<CompteUtilisateur>[] = [
    {
      cle: 'nom',
      titre: 'Utilisateur',
      rendu: (c) => (
        <span className="compte-identite">
          <span className={`avatar petit ${c.estActif ? '' : 'eteint'}`} aria-hidden="true">
            {initiales(c.nom)}
          </span>
          <span className="compte-textes">
            <strong>
              {c.nom} {estMoi(c) && <Badge ton="ciel">Vous</Badge>}
            </strong>
            <span className="discret">{c.email}</span>
          </span>
        </span>
      ),
      tri: (c) => c.nom,
      mobile: 'titre',
    },
    {
      cle: 'role',
      titre: 'Rôle',
      rendu: (c) => <Badge ton={roleInfo(c.role).ton}>{roleInfo(c.role).libelle}</Badge>,
      tri: (c) => c.roleLibelle,
      mobile: 'droite',
    },
    {
      cle: 'etat',
      titre: 'État',
      rendu: (c) => (c.estActif ? <Badge ton="vert" point>Actif</Badge> : <Badge>Désactivé</Badge>),
      tri: (c) => Number(c.estActif),
      mobile: 'detail',
    },
    {
      cle: 'connexion',
      titre: 'Dernière connexion',
      rendu: (c) => <span className={c.derniereConnexion ? '' : 'discret'}>{depuis(c.derniereConnexion)}</span>,
      tri: (c) => c.derniereConnexion ?? '',
      mobile: 'detail',
    },
  ]

  return (
    <main className="page">
      <EnTetePage
        miettes={[{ libelle: 'Tableau de bord', vers: '/' }, { libelle: 'Administration' }, { libelle: 'Comptes' }]}
        titre="Comptes"
        sousTitre="Qui peut se connecter à La Fermière, et ce que chacun peut faire."
        actions={
          <button className="bouton-principal" onClick={() => setNouveau(true)}>
            <IconePlus taille={20} /> Nouveau compte
          </button>
        }
      />

      <Indicateurs
        elements={[
          { libelle: 'Comptes actifs', valeur: actifs.length, icone: <IconeCompte taille={20} />, ton: 'vert', detail: `${comptes.length - actifs.length} désactivé${comptes.length - actifs.length > 1 ? 's' : ''}` },
          { libelle: 'Fermiers', valeur: actifs.filter((c) => c.role === 'Fermier').length, icone: <IconeCompte taille={20} />, ton: 'soleil', detail: 'comptes actifs' },
          { libelle: 'Administrateurs', valeur: actifs.filter((c) => c.role === 'Admin').length, icone: <IconeCompte taille={20} />, ton: 'ciel', detail: 'gèrent les comptes' },
        ]}
      />

      <BarreOutils>
        <Puces
          libelle="Filtrer par état"
          valeur={filtre}
          onChange={setFiltre}
          options={[
            { valeur: 'actifs', libelle: 'Actifs', compte: actifs.length },
            { valeur: 'desactives', libelle: 'Désactivés', compte: comptes.length - actifs.length },
            { valeur: 'tous', libelle: 'Tous', compte: comptes.length },
          ]}
        />
        <div className="barre-outils-ligne">
          <Recherche valeur={recherche} onChange={setRecherche} placeholder="Nom, e-mail, rôle…" />
        </div>
      </BarreOutils>

      <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} squelettes={3} />
      {!enCours && !erreur && (
        <Tableau
          lignes={filtres}
          colonnes={colonnes}
          cle={(c) => c.id}
          triInitial={{ cle: 'nom', sens: 'asc' }}
          vide={<Vide icone={<IconeRecherche taille={28} />} titre="Aucun compte" texte="Aucun compte ne correspond à ces filtres." />}
          actions={(c) => (
            <BoutonsLigne>
              <BoutonLigne libelle="Modifier" icone="modifier" onClick={() => setEdition(c)} />
              <BoutonLigne libelle="Mot de passe" icone="motDePasse" onClick={() => setMotDePasse(c)} />
              {/* Pas sur son propre compte : on ne se ferme pas la porte soi-même */}
              {!estMoi(c) &&
                (c.estActif ? (
                  <BoutonLigne libelle="Désactiver" icone="desactiver" danger onClick={() => setBascule(c)} />
                ) : (
                  <BoutonLigne libelle="Réactiver" icone="reactiver" onClick={() => setBascule(c)} />
                ))}
              {!estMoi(c) && (
                <BoutonLigne
                  libelle="Supprimer"
                  icone="supprimer"
                  danger
                  onClick={() =>
                    setSuppression({
                      titre: `Supprimer le compte de ${c.nom}`,
                      route: `/Utilisateurs/${c.id}`,
                      consequence:
                        'Possible seulement si ce compte n’a encore rien enregistré. Sinon, désactivez-le : il ne pourra plus se connecter et ses saisies seront conservées.',
                      description: (
                        <>
                          <strong>{c.nom}</strong> · {c.email} · {roleInfo(c.role).libelle}
                        </>
                      ),
                    })
                  }
                />
              )}
            </BoutonsLigne>
          )}
        />
      )}

      <FormulaireCompte
        ouvert={nouveau}
        onFermer={() => setNouveau(false)}
        onFait={(compte, mdp) => {
          setNouveau(false)
          void recharger()
          setIdentifiants({ nom: compte.nom, email: compte.email, motDePasse: mdp, nouveau: true })
        }}
      />
      <FormulaireCompte
        ouvert={edition !== null}
        existant={edition ?? undefined}
        estMoi={edition !== null && estMoi(edition)}
        onFermer={() => setEdition(null)}
        onFait={(compte) => {
          setEdition(null)
          notifier(`Compte de ${compte.nom} modifié`)
          void recharger()
        }}
      />
      <FormulaireMotDePasse
        compte={motDePasse}
        estMoi={motDePasse !== null && estMoi(motDePasse)}
        onFermer={() => setMotDePasse(null)}
        onFait={(compte, mdp) => {
          setMotDePasse(null)
          setIdentifiants({ nom: compte.nom, email: compte.email, motDePasse: mdp, nouveau: false })
        }}
      />
      <BasculeActivation
        compte={bascule}
        onFermer={() => setBascule(null)}
        onFait={(compte) => {
          setBascule(null)
          notifier(compte.estActif ? `${compte.nom} peut de nouveau se connecter` : `Compte de ${compte.nom} désactivé`)
          void recharger()
        }}
      />
      <ConfirmationSuppression
        demande={suppression}
        onFermer={() => setSuppression(null)}
        onFait={() => {
          setSuppression(null)
          notifier('Compte supprimé')
          void recharger()
        }}
      />
      <DialogueIdentifiants identifiants={identifiants} onFermer={() => setIdentifiants(null)} />
    </main>
  )
}

// ===========================================================================
// Créer / modifier un compte
// ===========================================================================
function FormulaireCompte({
  ouvert,
  existant,
  estMoi = false,
  onFermer,
  onFait,
}: {
  ouvert: boolean
  existant?: CompteUtilisateur
  estMoi?: boolean
  onFermer: () => void
  onFait: (compte: CompteUtilisateur, motDePasse: string) => void
}) {
  return (
    <Panneau
      ouvert={ouvert}
      onFermer={onFermer}
      titre={existant ? `Modifier ${existant.nom}` : 'Nouveau compte'}
      sousTitre={existant ? existant.email : 'Pour un fermier, un vétérinaire ou un comptable de la ferme.'}
    >
      <CorpsCompte key={existant?.id ?? 'nouveau'} existant={existant} estMoi={estMoi} onFermer={onFermer} onFait={onFait} />
    </Panneau>
  )
}

function CorpsCompte({
  existant,
  estMoi,
  onFermer,
  onFait,
}: {
  existant?: CompteUtilisateur
  estMoi: boolean
  onFermer: () => void
  onFait: (compte: CompteUtilisateur, motDePasse: string) => void
}) {
  const [nom, setNom] = useState(existant?.nom ?? '')
  const [email, setEmail] = useState(existant?.email ?? '')
  const [role, setRole] = useState<Role>(existant?.role ?? 'Fermier')
  const [motDePasse, setMotDePasse] = useState(() => (existant ? '' : genererMotDePasse()))
  const { envoi, erreur, champs, envoyer, signaler } = useEnvoi()

  const valider = () => {
    if (!nom.trim()) return signaler('nom', 'Indiquez le nom de la personne.')
    if (!/^\S+@\S+\.\S+$/.test(email.trim())) return signaler('email', 'Indiquez une adresse e-mail valide (elle sert d’identifiant de connexion).')
    if (!existant && !motDePasseValide(motDePasse)) return signaler('password', 'Le mot de passe ne respecte pas les règles ci-dessous.')
    void envoyer(async () => {
      const compte = existant
        ? await api.put<CompteUtilisateur>(`/Utilisateurs/${existant.id}`, {
            id: existant.id,
            nom: nom.trim(),
            email: email.trim(),
            ...(estMoi ? {} : { role }),
          })
        : await api.post<CompteUtilisateur>('/Utilisateurs', { nom: nom.trim(), email: email.trim(), password: motDePasse, role })
      onFait(compte, motDePasse)
    })
  }

  return (
    <FormulairePanneau
      onSubmit={valider}
      erreur={erreur}
      envoi={envoi}
      libelleValider={existant ? 'Enregistrer les modifications' : 'Créer le compte'}
      onAnnuler={onFermer}
    >
      <Champ libelle="Nom complet" obligatoire erreurs={champs.nom}>
        <input type="text" maxLength={100} value={nom} onChange={(e) => setNom(e.target.value)} placeholder="Ex. : Jean Mbarga" autoComplete="off" />
      </Champ>
      <Champ libelle="Adresse e-mail" obligatoire erreurs={champs.email} aide="Elle sert d’identifiant pour se connecter.">
        <input type="email" inputMode="email" maxLength={100} value={email} onChange={(e) => setEmail(e.target.value)} placeholder="jean@exemple.cm" autoComplete="off" />
      </Champ>

      <fieldset className="champ choix-role" disabled={estMoi}>
        <legend>
          Rôle <span className="obligatoire">*</span>
        </legend>
        {estMoi && <small className="aide">Vous ne pouvez pas changer votre propre rôle.</small>}
        <div className="choix-role-liste" role="radiogroup" aria-label="Rôle">
          {ROLES.map((r) => (
            <button
              key={r.valeur}
              type="button"
              role="radio"
              aria-checked={role === r.valeur}
              className={`choix-role-tuile ${role === r.valeur ? 'choisie' : ''}`}
              onClick={() => setRole(r.valeur)}
            >
              <strong>{r.libelle}</strong>
              <span>{r.description}</span>
            </button>
          ))}
        </div>
        {champs.role && <span className="erreur-champ">{champs.role.join(' ')}</span>}
      </fieldset>

      {!existant && <ChampMotDePasse valeur={motDePasse} onChange={setMotDePasse} erreurs={champs.password} />}
    </FormulairePanneau>
  )
}

/** Mot de passe proposé (modifiable), règles cochées en direct, bouton « Proposer un autre ». */
function ChampMotDePasse({ valeur, onChange, erreurs }: { valeur: string; onChange: (v: string) => void; erreurs?: string[] }) {
  const [visible, setVisible] = useState(true)
  return (
    <div className="champ-mot-de-passe">
      <Champ libelle="Mot de passe" obligatoire erreurs={erreurs}>
        <div className="mot-de-passe-ligne">
          <input
            type={visible ? 'text' : 'password'}
            value={valeur}
            onChange={(e) => onChange(e.target.value)}
            maxLength={100}
            autoComplete="new-password"
            spellCheck={false}
          />
          <button type="button" className="bouton-contour petit" onClick={() => setVisible(!visible)}>
            {visible ? 'Masquer' : 'Afficher'}
          </button>
        </div>
      </Champ>
      <button type="button" className="lien-action" onClick={() => onChange(genererMotDePasse())}>
        Proposer un autre mot de passe
      </button>
      <ul className="regles-mot-de-passe" aria-label="Règles du mot de passe">
        {REGLES_MOT_DE_PASSE.map((r) => (
          <li key={r.texte} className={r.ok(valeur) ? 'ok' : ''}>
            {r.texte}
          </li>
        ))}
      </ul>
    </div>
  )
}

// ===========================================================================
// Réinitialiser un mot de passe
// ===========================================================================
function FormulaireMotDePasse({
  compte,
  estMoi,
  onFermer,
  onFait,
}: {
  compte: CompteUtilisateur | null
  estMoi: boolean
  onFermer: () => void
  onFait: (compte: CompteUtilisateur, motDePasse: string) => void
}) {
  return (
    <Panneau ouvert={compte !== null} onFermer={onFermer} titre="Nouveau mot de passe" sousTitre={compte ? `${compte.nom} · ${compte.email}` : undefined}>
      {compte && <CorpsMotDePasse key={compte.id} compte={compte} estMoi={estMoi} onFermer={onFermer} onFait={onFait} />}
    </Panneau>
  )
}

function CorpsMotDePasse({
  compte,
  estMoi,
  onFermer,
  onFait,
}: {
  compte: CompteUtilisateur
  estMoi: boolean
  onFermer: () => void
  onFait: (compte: CompteUtilisateur, motDePasse: string) => void
}) {
  const [motDePasse, setMotDePasse] = useState(genererMotDePasse)
  const { envoi, erreur, champs, envoyer, signaler } = useEnvoi()

  const valider = () => {
    if (!motDePasseValide(motDePasse)) return signaler('password', 'Le mot de passe ne respecte pas les règles ci-dessous.')
    void envoyer(async () => {
      const modifie = await api.put<CompteUtilisateur>(`/Utilisateurs/${compte.id}`, { id: compte.id, password: motDePasse })
      onFait(modifie, motDePasse)
    })
  }

  return (
    <FormulairePanneau onSubmit={valider} erreur={erreur} envoi={envoi} libelleValider="Changer le mot de passe" onAnnuler={onFermer}>
      <div className="encart-info">
        <strong>Ce qui va se passer</strong>
        <p>
          {estMoi
            ? 'Vos autres appareils seront déconnectés. Sur celui-ci, il faudra vous reconnecter d’ici 15 minutes avec le nouveau mot de passe.'
            : `${compte.nom} sera déconnecté de tous ses appareils et devra utiliser ce nouveau mot de passe. Transmettez-le-lui directement.`}
        </p>
      </div>
      <ChampMotDePasse valeur={motDePasse} onChange={setMotDePasse} erreurs={champs.password} />
    </FormulairePanneau>
  )
}

// ===========================================================================
// Désactiver / réactiver (pop-up de confirmation, erreur de l'API affichée dedans)
// ===========================================================================
function BasculeActivation({
  compte,
  onFermer,
  onFait,
}: {
  compte: CompteUtilisateur | null
  onFermer: () => void
  onFait: (compte: CompteUtilisateur) => void
}) {
  const { envoi, erreur, envoyer } = useEnvoi()
  const [pour, setPour] = useState<number | null>(null)
  const desactiver = compte?.estActif ?? true

  const confirmer = () => {
    if (!compte) return
    setPour(compte.id)
    void envoyer(async () => {
      const modifie = await api.put<CompteUtilisateur>(`/Utilisateurs/${compte.id}`, { id: compte.id, estActif: !compte.estActif })
      onFait(modifie)
    })
  }

  return (
    <Dialogue
      ouvert={compte !== null}
      titre={desactiver ? `Désactiver le compte de ${compte?.nom} ?` : `Réactiver le compte de ${compte?.nom} ?`}
      ton={desactiver ? 'danger' : 'attention'}
      libelleConfirmer={desactiver ? 'Oui, désactiver' : 'Oui, réactiver'}
      enCours={envoi}
      erreur={pour === compte?.id ? erreur : null}
      onConfirmer={confirmer}
      onAnnuler={onFermer}
    >
      <p>
        {desactiver
          ? 'Il ne pourra plus se connecter et sera déconnecté de ses appareils. Ses saisies sont conservées. Vous pourrez le réactiver à tout moment.'
          : 'Il pourra de nouveau se connecter avec son mot de passe actuel.'}
      </p>
    </Dialogue>
  )
}

// ===========================================================================
// Identifiants à transmettre (après création ou nouveau mot de passe)
// ===========================================================================
interface Identifiants {
  nom: string
  email: string
  motDePasse: string
  nouveau: boolean
}

function DialogueIdentifiants({ identifiants, onFermer }: { identifiants: Identifiants | null; onFermer: () => void }) {
  const [copie, setCopie] = useState(false)
  if (!identifiants) return null

  const texte = [
    'La Fermière — vos identifiants',
    `Adresse : ${window.location.origin}`,
    `E-mail : ${identifiants.email}`,
    `Mot de passe : ${identifiants.motDePasse}`,
  ].join('\n')

  const fermer = () => {
    setCopie(false)
    onFermer()
  }

  const copier = async () => {
    try {
      await navigator.clipboard.writeText(texte)
      setCopie(true)
    } catch {
      setCopie(false)
    }
  }

  return (
    <Dialogue
      ouvert
      titre={identifiants.nouveau ? `Compte de ${identifiants.nom} créé` : 'Mot de passe changé'}
      ton="attention"
      libelleConfirmer="J’ai noté"
      sansAnnuler
      onConfirmer={fermer}
      onAnnuler={fermer}
    >
      <p>Transmettez ces identifiants à {identifiants.nom}. Le mot de passe ne sera plus affiché ensuite.</p>
      <dl className="identifiants">
        <Ligne libelle="E-mail" valeur={identifiants.email} />
        <Ligne libelle="Mot de passe" valeur={identifiants.motDePasse} />
      </dl>
      <button type="button" className="bouton-contour bouton-copier" onClick={() => void copier()}>
        <IconeCopier taille={18} /> {copie ? 'Copié !' : 'Copier pour un SMS ou WhatsApp'}
      </button>
    </Dialogue>
  )
}

function Ligne({ libelle, valeur }: { libelle: string; valeur: ReactNode }) {
  return (
    <div>
      <dt>{libelle}</dt>
      <dd>{valeur}</dd>
    </div>
  )
}
