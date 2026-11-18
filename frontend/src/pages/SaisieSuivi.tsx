import { useState, type FormEvent, type ReactNode } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ErreurApi, api } from '../api/client'
import type { Bande, NouveauSuivi, Suivi, TypeSoin } from '../api/types'
import { EtatChargement, useDonnees } from '../components/Chargement'
import { aujourdhui, jourDElevage, partieDate } from '../outils'

const TYPES_SOIN: { valeur: TypeSoin; libelle: string }[] = [
  { valeur: 'Vaccination', libelle: 'Vaccination' },
  { valeur: 'Traitement', libelle: 'Traitement' },
  { valeur: 'Prevention', libelle: 'Prévention' },
  { valeur: 'Supplement', libelle: 'Complément (vitamines…)' },
  { valeur: 'Autre', libelle: 'Autre' },
]

/** Texte saisi -> nombre (accepte la virgule : "1,25"), ou undefined si vide. */
function versNombre(texte: string): number | undefined {
  const t = texte.trim().replace(',', '.')
  return t === '' ? undefined : Number(t)
}

export default function SaisieSuivi() {
  const { id } = useParams()
  const naviguer = useNavigate()
  const { donnees: bande, erreur, enCours, recharger } = useDonnees(() => api.get<Bande>(`/Bandes/${id}`), [id])

  const [date, setDate] = useState(aujourdhui())
  const [morts, setMorts] = useState(0)
  const [cause, setCause] = useState('')
  const [pesee, setPesee] = useState(false)
  const [poids, setPoids] = useState('')
  const [sujetsPeses, setSujetsPeses] = useState('')
  const [soin, setSoin] = useState(false)
  const [typeSoin, setTypeSoin] = useState<TypeSoin>('Vaccination')
  const [produit, setProduit] = useState('')
  const [dosage, setDosage] = useState('')
  const [temperature, setTemperature] = useState('')
  const [observations, setObservations] = useState('')

  const [envoi, setEnvoi] = useState(false)
  const [erreurEnvoi, setErreurEnvoi] = useState<string | null>(null)
  const [erreursChamps, setErreursChamps] = useState<Record<string, string[]>>({})

  if (!bande) {
    return (
      <main className="page">
        <Link to={`/bandes/${id}`} className="retour">← Retour</Link>
        <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} />
      </main>
    )
  }

  const jour = jourDElevage(bande.dateArrivee, date)
  const dateArrivee = partieDate(bande.dateArrivee)

  const valider = async (e: FormEvent) => {
    e.preventDefault()
    setErreurEnvoi(null)
    setErreursChamps({})

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
      const cree = await api.post<Suivi>('/SuiviJournalier', suivi)
      naviguer(`/bandes/${bande.id}`, { replace: true, state: { message: `Suivi du jour ${cree.jour} enregistré.` } })
    } catch (err) {
      if (err instanceof ErreurApi) {
        setErreurEnvoi(err.message)
        setErreursChamps(err.champs)
      } else {
        setErreurEnvoi("L'enregistrement a échoué. Réessayez.")
      }
      window.scrollTo({ top: 0, behavior: 'smooth' })
    } finally {
      setEnvoi(false)
    }
  }

  return (
    <main className="page">
      <Link to={`/bandes/${bande.id}`} className="retour">← {bande.code}</Link>
      <h1>Suivi du jour</h1>

      {erreurEnvoi && (
        <p className="alerte erreur" role="alert">
          {erreurEnvoi}
        </p>
      )}

      <form className="formulaire" onSubmit={valider} noValidate>
        <section className="carte">
          <Champ libelle="Date" erreurs={erreursChamps.dateSuivi}>
            <input type="date" value={date} min={dateArrivee} max={aujourdhui()} required onChange={(e) => setDate(e.target.value)} />
          </Champ>
          <p className="discret">Jour d’élevage : <strong>{jour}</strong></p>
        </section>

        <section className="carte">
          <h2>Mortalités</h2>
          <div className="compteur">
            <button type="button" className="bouton rond" aria-label="Un mort de moins" disabled={morts === 0} onClick={() => setMorts(morts - 1)}>
              −
            </button>
            <input
              type="number"
              inputMode="numeric"
              min={0}
              max={1000}
              aria-label="Nombre de morts"
              value={morts}
              onChange={(e) => setMorts(Math.max(0, Math.min(1000, Number(e.target.value) || 0)))}
            />
            <button type="button" className="bouton rond" aria-label="Un mort de plus" onClick={() => setMorts(Math.min(1000, morts + 1))}>
              +
            </button>
          </div>
          {erreursChamps.nbMortalites && <ErreursChamp erreurs={erreursChamps.nbMortalites} />}
          {morts > 0 && (
            <Champ libelle="Cause (si connue)" erreurs={erreursChamps.causeMortalite}>
              <input type="text" maxLength={200} value={cause} onChange={(e) => setCause(e.target.value)} placeholder="Ex. : chaleur, maladie, écrasement" />
            </Champ>
          )}
        </section>

        <section className="carte">
          <label className="interrupteur">
            <input type="checkbox" checked={pesee} onChange={(e) => setPesee(e.target.checked)} />
            J’ai fait une pesée
          </label>
          {pesee && (
            <div className="deux-colonnes">
              <Champ libelle="Poids moyen (kg)" erreurs={erreursChamps.poidsMoyen}>
                <input type="text" inputMode="decimal" value={poids} onChange={(e) => setPoids(e.target.value)} placeholder="Ex. : 1,25" />
              </Champ>
              <Champ libelle="Sujets pesés" erreurs={erreursChamps.nbSujetsPeses}>
                <input type="number" inputMode="numeric" min={1} value={sujetsPeses} onChange={(e) => setSujetsPeses(e.target.value)} />
              </Champ>
            </div>
          )}
        </section>

        <section className="carte">
          <label className="interrupteur">
            <input type="checkbox" checked={soin} onChange={(e) => setSoin(e.target.checked)} />
            J’ai donné un soin
          </label>
          {soin && (
            <>
              <Champ libelle="Type de soin" erreurs={erreursChamps.typeSoin}>
                <select value={typeSoin} onChange={(e) => setTypeSoin(e.target.value as TypeSoin)}>
                  {TYPES_SOIN.map((t) => (
                    <option key={t.valeur} value={t.valeur}>
                      {t.libelle}
                    </option>
                  ))}
                </select>
              </Champ>
              <Champ libelle="Produit" erreurs={erreursChamps.produitUtilise}>
                <input type="text" maxLength={100} value={produit} onChange={(e) => setProduit(e.target.value)} />
              </Champ>
              <Champ libelle="Dosage" erreurs={erreursChamps.dosage}>
                <input type="text" maxLength={100} value={dosage} onChange={(e) => setDosage(e.target.value)} placeholder="Ex. : 1 g/L d’eau" />
              </Champ>
            </>
          )}
        </section>

        <section className="carte">
          <Champ libelle="Température dans le bâtiment (°C, facultatif)" erreurs={erreursChamps.temperature}>
            <input type="text" inputMode="decimal" value={temperature} onChange={(e) => setTemperature(e.target.value)} />
          </Champ>
          <Champ libelle="Observations (facultatif)" erreurs={erreursChamps.observations}>
            <textarea rows={3} maxLength={2000} value={observations} onChange={(e) => setObservations(e.target.value)} />
          </Champ>
        </section>

        {erreursChamps.body && <ErreursChamp erreurs={erreursChamps.body} />}

        <button className="bouton principal bloc" type="submit" disabled={envoi}>
          {envoi ? 'Enregistrement…' : 'Enregistrer le suivi'}
        </button>
      </form>
    </main>
  )
}

function Champ({ libelle, erreurs, children }: { libelle: string; erreurs?: string[]; children: ReactNode }) {
  return (
    <label className={erreurs?.length ? 'en-erreur' : undefined}>
      {libelle}
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
