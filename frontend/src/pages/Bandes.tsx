import { useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { FormulaireBande } from '../formulaires/Formulaires'
import type { Bande, StatutBande } from '../api/types'
import { Anneau } from '../components/Anime'
import { EtatChargement, useDonnees } from '../components/Chargement'
import { IconeChevron, IconeCoeur, IconeHorloge, IconePlus, IconePoussin, IconeRecherche } from '../components/Icones'
import {
  Badge,
  BarreOutils,
  BasculeVue,
  EnTetePage,
  Indicateurs,
  Jauge,
  Puces,
  Recherche,
  SelecteurTri,
  Vide,
  useEtatUrl,
  type Vue,
} from '../components/Page'
import { Tableau, type Colonne } from '../components/Tableau'
import { LIBELLE_STATUT, TON_STATUT, ageBande, estActive, finDeCycleProche, joursRestants, tonMortalite } from '../metier'
import { DUREE_CYCLE, dateLisible, moisLisible, nombre, normaliser } from '../outils'

type FiltreStatut = 'actives' | 'toutes' | StatutBande
type Tri = 'recentes' | 'anciennes' | 'age' | 'mortalite' | 'effectif' | 'code'

const TRIS: { valeur: Tri; libelle: string }[] = [
  { valeur: 'recentes', libelle: 'Arrivée : plus récentes' },
  { valeur: 'anciennes', libelle: 'Arrivée : plus anciennes' },
  { valeur: 'age', libelle: 'Âge : plus avancées' },
  { valeur: 'mortalite', libelle: 'Mortalité : plus élevée' },
  { valeur: 'effectif', libelle: 'Effectif : plus grand' },
  { valeur: 'code', libelle: 'Code (A → Z)' },
]

const COMPARER: Record<Tri, (a: Bande, b: Bande) => number> = {
  recentes: (a, b) => b.dateArrivee.localeCompare(a.dateArrivee) || b.code.localeCompare(a.code),
  anciennes: (a, b) => a.dateArrivee.localeCompare(b.dateArrivee) || a.code.localeCompare(b.code),
  age: (a, b) => ageBande(b) - ageBande(a),
  mortalite: (a, b) => b.tauxMortalite - a.tauxMortalite,
  effectif: (a, b) => b.nbPoussinsActuel - a.nbPoussinsActuel,
  code: (a, b) => a.code.localeCompare(b.code),
}

export default function Bandes() {
  const [statut, setStatut] = useEtatUrl<FiltreStatut>('statut', 'actives')
  const [tri, setTri] = useEtatUrl<Tri>('tri', 'recentes')
  const [vue, setVue] = useEtatUrl<Vue>('vue', 'cartes')
  const [recherche, setRecherche] = useState('')
  const [formulaire, setFormulaire] = useEtatUrl<'' | 'bande'>('formulaire', '')
  const { aLeRole } = useAuth()
  const naviguer = useNavigate()

  const { donnees, erreur, enCours, recharger } = useDonnees(() => api.liste<Bande>('/Bandes'), [])
  const toutes = useMemo(() => donnees ?? [], [donnees])

  const compte = (f: (b: Bande) => boolean) => toutes.filter(f).length
  const actives = toutes.filter(estActive)
  const sujets = actives.reduce((s, b) => s + b.nbPoussinsActuel, 0)
  const initiaux = actives.reduce((s, b) => s + b.nbPoussinsInit, 0)
  const mortalite = initiaux ? ((initiaux - sujets) / initiaux) * 100 : 0
  const finProche = toutes.filter(finDeCycleProche).length

  const filtrees = useMemo(() => {
    const q = normaliser(recherche.trim())
    return toutes
      .filter((b) => (statut === 'toutes' ? true : statut === 'actives' ? estActive(b) : b.statut === statut))
      .filter((b) => !q || normaliser(`${b.code} ${b.observations ?? ''}`).includes(q))
      .sort(COMPARER[tri])
  }, [toutes, statut, tri, recherche])

  // Rangement par mois d'arrivée quand la liste est triée par date
  const groupes = useMemo(() => {
    if (tri !== 'recentes' && tri !== 'anciennes') return [{ titre: null as string | null, bandes: filtrees }]
    const parMois = new Map<string, Bande[]>()
    filtrees.forEach((b) => {
      const mois = moisLisible(b.dateArrivee)
      parMois.set(mois, [...(parMois.get(mois) ?? []), b])
    })
    return [...parMois].map(([titre, bandes]) => ({ titre, bandes }))
  }, [filtrees, tri])

  const colonnes: Colonne<Bande>[] = [
    { cle: 'code', titre: 'Bande', rendu: (b) => <strong className="code">{b.code}</strong>, tri: (b) => b.code, mobile: 'titre' },
    {
      cle: 'statut',
      titre: 'Statut',
      rendu: (b) => (
        <Badge ton={TON_STATUT[b.statut]} point={estActive(b)}>
          {LIBELLE_STATUT[b.statut]}
        </Badge>
      ),
      tri: (b) => b.statut,
      mobile: 'droite',
    },
    { cle: 'arrivee', titre: 'Arrivée', rendu: (b) => dateLisible(b.dateArrivee), tri: (b) => b.dateArrivee, mobile: 'sous-titre' },
    {
      cle: 'age',
      titre: 'Cycle',
      rendu: (b) => (
        <span className="cellule-cycle">
          <span>
            J{ageBande(b)}
            <span className="discret">/{DUREE_CYCLE}</span>
          </span>
          <Jauge valeur={ageBande(b)} max={DUREE_CYCLE} ton={finDeCycleProche(b) ? 'soleil' : 'vert'} />
        </span>
      ),
      tri: (b) => ageBande(b),
      mobile: 'detail',
    },
    {
      cle: 'effectif',
      titre: 'Effectif',
      rendu: (b) => (
        <>
          {nombre(b.nbPoussinsActuel)} <span className="discret">/ {nombre(b.nbPoussinsInit)}</span>
        </>
      ),
      tri: (b) => b.nbPoussinsActuel,
      alignement: 'droite',
      mobile: 'detail',
    },
    {
      cle: 'mortalite',
      titre: 'Mortalité',
      rendu: (b) => <Badge ton={tonMortalite(b.tauxMortalite)}>{nombre(b.tauxMortalite, 1)} %</Badge>,
      tri: (b) => b.tauxMortalite,
      alignement: 'droite',
      mobile: 'detail',
    },
  ]

  return (
    <main className="page">
      <EnTetePage
        miettes={[{ libelle: 'Tableau de bord', vers: '/' }, { libelle: 'Élevage' }, { libelle: 'Bandes' }]}
        titre="Bandes"
        sousTitre="Tous vos lots de poulets, du jour d’arrivée à la clôture."
        actions={
          aLeRole('Admin', 'Fermier') && (
            <button className="bouton-principal" onClick={() => setFormulaire('bande')}>
              <IconePlus taille={20} /> Nouvelle bande
            </button>
          )
        }
      />

      <FormulaireBande
        ouvert={formulaire === 'bande'}
        onFermer={() => setFormulaire('')}
        onCree={(b) => naviguer(`/bandes/${b.id}`, { state: { message: `Bande ${b.code} créée !` } })}
      />

      <Indicateurs
        elements={[
          {
            libelle: 'Bandes actives',
            valeur: actives.length,
            icone: <IconePoussin taille={20} />,
            ton: 'vert',
            detail: `${compte((b) => b.statut === 'EnCours')} en cours · ${compte((b) => b.statut === 'EnPreparation')} en préparation`,
          },
          { libelle: 'Poulets sur pied', valeur: sujets, icone: <IconePoussin taille={20} />, ton: 'ciel', detail: `sur ${nombre(initiaux)} au départ` },
          {
            libelle: 'Mortalité moyenne',
            valeur: mortalite,
            decimales: 1,
            suffixe: ' %',
            icone: <IconeCoeur taille={20} />,
            ton: tonMortalite(mortalite),
            detail: 'bandes actives',
          },
          {
            libelle: 'Fin de cycle proche',
            valeur: finProche,
            icone: <IconeHorloge taille={20} />,
            ton: finProche ? 'soleil' : 'neutre',
            detail: '7 jours ou moins',
          },
        ]}
      />

      <BarreOutils>
        <Puces
          libelle="Filtrer par statut"
          valeur={statut}
          onChange={setStatut}
          options={[
            { valeur: 'actives', libelle: 'Actives', compte: actives.length, ton: 'vert' },
            { valeur: 'EnCours', libelle: 'En cours', compte: compte((b) => b.statut === 'EnCours') },
            { valeur: 'EnPreparation', libelle: 'En préparation', compte: compte((b) => b.statut === 'EnPreparation') },
            { valeur: 'Cloturee', libelle: 'Clôturées', compte: compte((b) => b.statut === 'Cloturee') },
            { valeur: 'Archivee', libelle: 'Archivées', compte: compte((b) => b.statut === 'Archivee') },
            { valeur: 'toutes', libelle: 'Toutes', compte: toutes.length },
          ]}
        />
        <div className="barre-outils-ligne">
          <Recherche valeur={recherche} onChange={setRecherche} placeholder="Rechercher un code…" />
          <SelecteurTri options={TRIS} valeur={tri} onChange={setTri} />
          <BasculeVue valeur={vue} onChange={setVue} />
        </div>
      </BarreOutils>

      <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} squelettes={4} />

      {!enCours && !erreur && (
        <>
          <p className="resultat-compte">
            {filtrees.length} bande{filtrees.length > 1 ? 's' : ''}
            {recherche && ` pour « ${recherche} »`}
          </p>

          {filtrees.length === 0 ? (
            <Vide
              icone={<IconeRecherche taille={28} />}
              titre="Aucune bande ne correspond"
              texte="Essayez un autre statut ou une autre recherche."
              action={
                <button className="bouton-contour" onClick={() => (setStatut('toutes'), setRecherche(''))}>
                  Voir toutes les bandes
                </button>
              }
            />
          ) : vue === 'tableau' ? (
            <Tableau lignes={filtrees} colonnes={colonnes} cle={(b) => b.id} lien={(b) => `/bandes/${b.id}`} />
          ) : (
            groupes.map((g) => (
              <section key={g.titre ?? 'tout'} className="groupe">
                {g.titre && (
                  <h2 className="groupe-titre">
                    <span>{g.titre}</span>
                    <span className="groupe-compte">{g.bandes.length}</span>
                  </h2>
                )}
                <ul className="grille-cartes">
                  {g.bandes.map((b, i) => (
                    <li key={b.id} className="apparition" style={{ animationDelay: `${Math.min(i, 8) * 50}ms` }}>
                      <CarteBande bande={b} />
                    </li>
                  ))}
                </ul>
              </section>
            ))
          )}
        </>
      )}
    </main>
  )
}

export function CarteBande({ bande }: { bande: Bande }) {
  const active = estActive(bande)
  const age = ageBande(bande)
  const restants = joursRestants(bande)

  return (
    <Link to={`/bandes/${bande.id}`} className="carte-bande">
      <div className="carte-bande-haut">
        <div>
          <strong className="code">{bande.code}</strong>
          <span className="carte-bande-date">Arrivée le {dateLisible(bande.dateArrivee)}</span>
        </div>
        <Badge ton={TON_STATUT[bande.statut]} point={active}>
          {LIBELLE_STATUT[bande.statut]}
        </Badge>
      </div>

      <div className="carte-bande-milieu">
        {active ? (
          <Anneau jour={Math.min(age, 99)} total={DUREE_CYCLE} taille={68} epaisseur={7} />
        ) : (
          <span className="anneau-termine">
            <IconePoussin taille={26} />
          </span>
        )}
        <dl className="carte-bande-mesures">
          <div>
            <dt>Effectif</dt>
            <dd>
              {nombre(bande.nbPoussinsActuel)}
              <span className="discret"> / {nombre(bande.nbPoussinsInit)}</span>
            </dd>
          </div>
          <div>
            <dt>Mortalité</dt>
            <dd className={`texte-${tonMortalite(bande.tauxMortalite)}`}>{nombre(bande.tauxMortalite, 1)} %</dd>
          </div>
          <div>
            <dt>{active ? 'Reste' : 'Durée'}</dt>
            <dd>{active ? `${restants} j` : `${age} j`}</dd>
          </div>
        </dl>
      </div>

      <div className="carte-bande-bas">
        <span className={finDeCycleProche(bande) ? 'texte-soleil' : 'discret'}>
          {active ? (finDeCycleProche(bande) ? 'Fin de cycle proche' : `Jour ${age} sur ${DUREE_CYCLE}`) : `Terminée${bande.dateCloture ? ` le ${dateLisible(bande.dateCloture)}` : ''}`}
        </span>
        <span className="carte-bande-ouvrir">
          Ouvrir <IconeChevron taille={16} />
        </span>
      </div>
    </Link>
  )
}
