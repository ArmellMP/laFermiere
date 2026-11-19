import { useState, type FormEvent, type ReactNode } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { EnTetePage } from '../components/Page'
import { ErreurApi, api } from '../api/client'
import type { Bande, NouveauSuivi, Suivi, TypeSoin } from '../api/types'
import { EtatChargement, useDonnees } from '../components/Chargement'
import {
  IconeAlerte,
  IconeBalance,
  IconeCalendrier,
  IconeCoeur,
  IconeMoins,
  IconeNote,
  IconePlus,
  IconeSeringue,
} from '../components/Icones'
import { aujourdhui, dateLisible, jourDElevage, partieDate } from '../outils'

const TYPES_SOIN: { valeur: TypeSoin; libelle: string; emoji: string }[] = [
  { valeur: 'Vaccination', libelle: 'Vaccin', emoji: '💉' },
  { valeur: 'Traitement', libelle: 'Traitement', emoji: '💊' },
  { valeur: 'Prevention', libelle: 'Prévention', emoji: '🛡️' },
  { valeur: 'Supplement', libelle: 'Vitamines', emoji: '🌿' },
  { valeur: 'Autre', libelle: 'Autre', emoji: '📝' },
]

const CAUSES = ['Chaleur', 'Maladie', 'Écrasement', 'Prédateur', 'Inconnue']

/** Texte saisi -> nombre (accepte la virgule : "1,25"), ou undefined si vide. */
function versNombre(texte: string): number | undefined {
  const t = texte.trim().replace(',', '.')
  return t === '' ? undefined : Number(t)
}

/** Nombre -> texte d'un champ (virgule décimale), vide si absent. */
const versTexte = (n?: number | null) => (n == null ? '' : String(n).replace('.', ','))

/**
 * /bandes/:id/suivi            : saisie du suivi du jour
 * /bandes/:id/suivi/:suiviId   : modification d'un suivi existant (la date reste fixe)
 */
export default function SaisieSuivi() {
  const { id, suiviId } = useParams()
  const { donnees, erreur, enCours, recharger } = useDonnees(async () => {
    const [bande, suivi] = await Promise.all([
      api.get<Bande>(`/Bandes/${id}`),
      suiviId ? api.get<Suivi>(`/SuiviJournalier/${suiviId}`) : Promise.resolve(undefined),
    ])
    return { bande, suivi }
  }, [id, suiviId])

  if (!donnees) {
    return (
      <main className="page page-saisie">
        <EnTetePage miettes={[{ libelle: 'Bandes', vers: '/bandes' }, { libelle: 'Suivi du jour' }]} titre={suiviId ? 'Modifier le suivi' : 'Suivi du jour'} />
        <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} />
      </main>
    )
  }
  return <FormulaireSuivi key={suiviId ?? 'nouveau'} bande={donnees.bande} existant={donnees.suivi} />
}

function FormulaireSuivi({ bande, existant }: { bande: Bande; existant?: Suivi }) {
  const naviguer = useNavigate()
  const [date, setDate] = useState(existant ? partieDate(existant.dateSuivi) : aujourdhui())
  const [morts, setMorts] = useState(existant?.nbMortalites ?? 0)
  const [cause, setCause] = useState(existant?.causeMortalite ?? '')
  const [pesee, setPesee] = useState(existant?.poidsMoyen != null)
  const [poids, setPoids] = useState(versTexte(existant?.poidsMoyen))
  const [sujetsPeses, setSujetsPeses] = useState(versTexte(existant?.nbSujetsPeses))
  const [soin, setSoin] = useState(!!existant?.typeSoin)
  const [typeSoin, setTypeSoin] = useState<TypeSoin>(existant?.typeSoin ?? 'Vaccination')
  const [produit, setProduit] = useState(existant?.produitUtilise ?? '')
  const [dosage, setDosage] = useState(existant?.dosage ?? '')
  const [temperature, setTemperature] = useState(versTexte(existant?.temperature))
  const [observations, setObservations] = useState(existant?.observations ?? '')

  const [envoi, setEnvoi] = useState(false)
  const [erreurEnvoi, setErreurEnvoi] = useState<string | null>(null)
  const [erreursChamps, setErreursChamps] = useState<Record<string, string[]>>({})

  // L'API ne sait pas effacer une valeur en modification (un champ vide = inchangé) :
  // décocher une pesée ou un soin déjà enregistrés ne les supprimerait pas.
  const effacementImpossible = !!existant && ((existant.poidsMoyen != null && !pesee) || (!!existant.typeSoin && !soin))

  const jour = jourDElevage(bande.dateArrivee, date)
  const dateArrivee = partieDate(bande.dateArrivee)
  const changerMorts = (n: number) => setMorts(Math.max(0, Math.min(1000, n)))

  /** Affiche les erreurs : messages précis dans le bandeau, puis défilement jusqu'au premier champ en cause. */
  const montrerErreurs = (message: string, champs: Record<string, string[]>) => {
    const details = [...new Set(Object.values(champs).flat())]
    setErreurEnvoi(details.length ? details.join(' ') : message)
    setErreursChamps(champs)
    requestAnimationFrame(() => {
      const premier = document.querySelector('.en-erreur, .erreur-champ')
      if (premier) premier.scrollIntoView({ behavior: 'smooth', block: 'center' })
      else window.scrollTo({ top: 0, behavior: 'smooth' })
    })
  }

  const valider = async (e: FormEvent) => {
    e.preventDefault()
    setErreurEnvoi(null)
    setErreursChamps({})

    // Même règle que l'API : un soin doit préciser le produit utilisé
    if (soin && !produit.trim()) {
      montrerErreurs('', { produitUtilise: ['Indiquez le produit utilisé pour ce soin.'] })
      return
    }

    if (effacementImpossible) {
      montrerErreurs('', {
        body: ['Une pesée ou un soin déjà enregistré ne peut pas être effacé par une modification. Supprimez ce suivi puis saisissez-le à nouveau.'],
      })
      return
    }

    const suivi: NouveauSuivi = {
      bandeId: bande.id,
      jour,
      dateSuivi: date,
      nbMortalites: morts,
      causeMortalite: morts > 0 && cause.trim() ? cause.trim() : undefined,
      poidsMoyen: pesee ? versNombre(poids) : undefined,
      nbSujetsPeses: pesee ? versNombre(sujetsPeses) : undefined,
      typeSoin: soin ? typeSoin : undefined,
      produitUtilise: soin && produit.trim() ? produit.trim() : undefined,
      dosage: soin && dosage.trim() ? dosage.trim() : undefined,
      temperature: versNombre(temperature),
      observations: observations.trim() || undefined,
    }

    setEnvoi(true)
    try {
      if (existant) {
        const { bandeId: _b, jour: _j, dateSuivi: _d, ...modifiable } = suivi
        const modifie = await api.put<Suivi>(`/SuiviJournalier/${existant.id}`, { id: existant.id, ...modifiable })
        naviguer(`/bandes/${bande.id}?onglet=journal`, { replace: true, state: { message: `Suivi du jour ${modifie.jour} modifié` } })
        return
      }
      const cree = await api.post<Suivi>('/SuiviJournalier', suivi)
      naviguer(`/bandes/${bande.id}`, { replace: true, state: { message: `Suivi du jour ${cree.jour} enregistré !` } })
    } catch (err) {
      if (err instanceof ErreurApi) {
        // "body" : règle sur l'ensemble du formulaire, déjà couverte par le message du champ
        const { body, ...champs } = err.champs
        montrerErreurs(err.message, Object.keys(champs).length ? champs : { body: body ?? [] })
      } else {
        montrerErreurs("L'enregistrement a échoué. Réessayez.", {})
      }
    } finally {
      setEnvoi(false)
    }
  }

  return (
    <main className="page page-saisie">
      <EnTetePage
        miettes={[
          { libelle: 'Bandes', vers: '/bandes' },
          { libelle: bande.code, vers: `/bandes/${bande.id}` },
          { libelle: existant ? `Suivi du jour ${existant.jour}` : 'Suivi du jour' },
        ]}
        titre={existant ? 'Modifier le suivi' : 'Suivi du jour'}
        badge={<span className="saisie-jour">Jour {jour}</span>}
        sousTitre={`${bande.code} · ${dateLisible(date)}`}
      />

      {erreurEnvoi && (
        <div className="message message-erreur apparition" role="alert" key={erreurEnvoi}>
          <IconeAlerte />
          <p>{erreurEnvoi}</p>
        </div>
      )}

      <form onSubmit={valider} noValidate>
        <Section icone={<IconeCalendrier />} couleur="ciel" titre="Date" retard={1}>
          <Champ erreurs={erreursChamps.dateSuivi}>
            <input type="date" value={date} min={dateArrivee} max={aujourdhui()} required disabled={!!existant} onChange={(e) => setDate(e.target.value)} />
          </Champ>
        </Section>

        <Section icone={<IconeCoeur />} couleur={morts > 0 ? 'danger' : 'vert'} titre="Mortalités" retard={2}>
          <div className="compteur">
            <button type="button" className="bouton-rond" aria-label="Un mort de moins" disabled={morts === 0} onClick={() => changerMorts(morts - 1)}>
              <IconeMoins taille={26} />
            </button>
            <div className="compteur-valeur">
              <input
                key={morts}
                className="pop"
                type="number"
                inputMode="numeric"
                min={0}
                max={1000}
                aria-label="Nombre de morts"
                value={morts}
                onChange={(e) => changerMorts(Number(e.target.value) || 0)}
              />
              <span>{morts === 0 ? 'aucun mort 🎉' : morts === 1 ? 'mort' : 'morts'}</span>
            </div>
            <button type="button" className="bouton-rond plus" aria-label="Un mort de plus" onClick={() => changerMorts(morts + 1)}>
              <IconePlus taille={26} />
            </button>
          </div>
          {erreursChamps.nbMortalites && <ErreursChamp erreurs={erreursChamps.nbMortalites} />}

          <Depliant ouvert={morts > 0}>
            <span className="etiquette-champ">Cause (si connue)</span>
            <div className="choix-rapides">
              {CAUSES.map((c) => (
                <button type="button" key={c} className={`pastille-choix ${cause === c ? 'choisie' : ''}`} onClick={() => setCause(cause === c ? '' : c)}>
                  {c}
                </button>
              ))}
            </div>
            <Champ erreurs={erreursChamps.causeMortalite}>
              <input type="text" maxLength={200} value={cause} onChange={(e) => setCause(e.target.value)} placeholder="Ou décrivez la cause…" />
            </Champ>
          </Depliant>
        </Section>

        <Section
          icone={<IconeBalance />}
          couleur="soleil"
          titre="Pesée"
          retard={3}
          action={<Interrupteur actif={pesee} onChange={setPesee} libelle="J’ai fait une pesée" />}
        >
          <Depliant ouvert={pesee}>
            <div className="deux-colonnes">
              <Champ libelle="Poids moyen (kg)" erreurs={erreursChamps.poidsMoyen}>
                <input type="text" inputMode="decimal" value={poids} onChange={(e) => setPoids(e.target.value)} placeholder="1,25" />
              </Champ>
              <Champ libelle="Sujets pesés" erreurs={erreursChamps.nbSujetsPeses}>
                <input type="number" inputMode="numeric" min={1} value={sujetsPeses} onChange={(e) => setSujetsPeses(e.target.value)} placeholder="20" />
              </Champ>
            </div>
          </Depliant>
        </Section>

        <Section
          icone={<IconeSeringue />}
          couleur="soin"
          titre="Soin"
          retard={4}
          action={<Interrupteur actif={soin} onChange={setSoin} libelle="J’ai donné un soin" />}
        >
          <Depliant ouvert={soin}>
            <div className="choix-soin">
              {TYPES_SOIN.map((t) => (
                <button type="button" key={t.valeur} className={`tuile-choix ${typeSoin === t.valeur ? 'choisie' : ''}`} onClick={() => setTypeSoin(t.valeur)}>
                  <span className="tuile-choix-emoji">{t.emoji}</span>
                  {t.libelle}
                </button>
              ))}
            </div>
            {erreursChamps.typeSoin && <ErreursChamp erreurs={erreursChamps.typeSoin} />}
            <Champ libelle="Produit (obligatoire)" erreurs={erreursChamps.produitUtilise}>
              <input type="text" maxLength={100} value={produit} onChange={(e) => setProduit(e.target.value)} placeholder="Nom du produit" />
            </Champ>
            <Champ libelle="Dosage" erreurs={erreursChamps.dosage}>
              <input type="text" maxLength={100} value={dosage} onChange={(e) => setDosage(e.target.value)} placeholder="1 g/L d’eau" />
            </Champ>
          </Depliant>
        </Section>

        <Section icone={<IconeNote />} couleur="terre" titre="Notes" retard={5}>
          <Champ libelle="Température du bâtiment (°C)" erreurs={erreursChamps.temperature}>
            <input type="text" inputMode="decimal" value={temperature} onChange={(e) => setTemperature(e.target.value)} placeholder="Facultatif" />
          </Champ>
          <Champ libelle="Observations" erreurs={erreursChamps.observations}>
            <textarea rows={3} maxLength={2000} value={observations} onChange={(e) => setObservations(e.target.value)} placeholder="Comportement, eau, aliment…" />
          </Champ>
        </Section>

        {effacementImpossible && (
          <div className="message message-erreur" role="alert">
            <IconeAlerte />
            <p>
              Une pesée ou un soin déjà enregistré ne peut pas être effacé ici : il resterait enregistré. Pour l’enlever, supprimez ce suivi
              depuis le journal puis saisissez-le à nouveau.
            </p>
          </div>
        )}
        {erreursChamps.body && <ErreursChamp erreurs={erreursChamps.body} />}

        <div className="barre-validation">
          <button className="bouton-principal" type="submit" disabled={envoi}>
            {envoi ? <span className="points">Enregistrement</span> : existant ? 'Enregistrer les modifications' : 'Enregistrer le suivi'}
          </button>
        </div>
      </form>
    </main>
  )
}

function Section({
  icone,
  couleur,
  titre,
  action,
  retard,
  children,
}: {
  icone: ReactNode
  couleur: string
  titre: string
  action?: ReactNode
  retard: number
  children?: ReactNode
}) {
  return (
    <section className="section-saisie apparition" style={{ animationDelay: `${retard * 60}ms` }}>
      <div className="section-entete">
        <span className={`tuile-icone tuile-${couleur}`}>{icone}</span>
        <h2>{titre}</h2>
        {action}
      </div>
      {children}
    </section>
  )
}

/** Contenu qui s'ouvre et se referme en douceur. */
function Depliant({ ouvert, children }: { ouvert: boolean; children: ReactNode }) {
  return (
    <div className={`depliant ${ouvert ? 'ouvert' : ''}`} aria-hidden={!ouvert}>
      <div className="depliant-interieur">{ouvert && children}</div>
    </div>
  )
}

function Interrupteur({ actif, onChange, libelle }: { actif: boolean; onChange: (v: boolean) => void; libelle: string }) {
  return (
    <label className="interrupteur interrupteur-seul">
      <input type="checkbox" role="switch" aria-label={libelle} checked={actif} onChange={(e) => onChange(e.target.checked)} />
      <span className="interrupteur-piste" aria-hidden="true" />
    </label>
  )
}

function Champ({ libelle, erreurs, children }: { libelle?: string; erreurs?: string[]; children: ReactNode }) {
  return (
    <label className={`champ ${erreurs?.length ? 'en-erreur' : ''}`}>
      {libelle && <span>{libelle}</span>}
      {children}
      {erreurs?.length ? <ErreursChamp erreurs={erreurs} /> : null}
    </label>
  )
}

function ErreursChamp({ erreurs }: { erreurs: string[] }) {
  return (
    <span className="erreur-champ" role="alert">
      {erreurs.join(' ')}
    </span>
  )
}
