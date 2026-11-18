import { Link, useLocation, useParams } from 'react-router-dom'
import { api } from '../api/client'
import type { Bande, Suivi } from '../api/types'
import { useAuth } from '../auth/AuthContext'
import { EtatChargement, useDonnees } from '../components/Chargement'
import { DUREE_CYCLE, aujourdhui, dateLisible, jourDElevage, nombre, partieDate } from '../outils'
import { estActive } from './Bandes'

export default function BandeDetail() {
  const { id } = useParams()
  const { aLeRole } = useAuth()
  const message = (useLocation().state as { message?: string } | null)?.message

  const { donnees, erreur, enCours, recharger } = useDonnees(async () => {
    const [bande, suivis] = await Promise.all([
      api.get<Bande>(`/Bandes/${id}`),
      api.get<Suivi[]>(`/SuiviJournalier/bande/${id}`),
    ])
    return { bande, suivis: [...suivis].sort((a, b) => b.dateSuivi.localeCompare(a.dateSuivi)) }
  }, [id])

  if (!donnees) {
    return (
      <main className="page">
        <Link to="/" className="retour">← Mes bandes</Link>
        <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} />
      </main>
    )
  }

  const { bande, suivis } = donnees
  const active = estActive(bande)
  const jour = jourDElevage(bande.dateArrivee, aujourdhui())
  const morts = bande.nbPoussinsInit - bande.nbPoussinsActuel
  const suiviDuJour = suivis.find((s) => partieDate(s.dateSuivi) === aujourdhui())
  const peutSaisir = active && aLeRole('Admin', 'Fermier', 'Veterinaire')
  const dernierPoids = suivis.find((s) => s.poidsMoyen)?.poidsMoyen

  return (
    <main className="page">
      <Link to="/" className="retour">← Mes bandes</Link>

      <div className="entete-page">
        <h1>{bande.code}</h1>
        <span className={`pastille ${active ? 'pastille-active' : ''}`}>{bande.statutLibelle}</span>
      </div>

      {message && (
        <p className="alerte succes" role="status">
          {message}
        </p>
      )}

      <section className="carte">
        <p className="discret">Arrivée le {dateLisible(bande.dateArrivee)}</p>
        {active && (
          <p className="grand">
            Jour {jour}
            <span className="discret"> / {DUREE_CYCLE}</span>
          </p>
        )}
        <div className="chiffres">
          <div>
            <span className="valeur">{nombre(bande.nbPoussinsActuel)}</span>
            <span className="etiquette">sujets sur {nombre(bande.nbPoussinsInit)}</span>
          </div>
          <div>
            <span className={`valeur ${bande.tauxMortalite >= 5 ? 'attention' : ''}`}>{nombre(morts)}</span>
            <span className="etiquette">morts ({nombre(bande.tauxMortalite, 1)} %)</span>
          </div>
          <div>
            <span className="valeur">{dernierPoids ? `${nombre(dernierPoids, 2)} kg` : '—'}</span>
            <span className="etiquette">dernier poids moyen</span>
          </div>
        </div>
      </section>

      {peutSaisir &&
        (suiviDuJour ? (
          <p className="alerte succes">Suivi d’aujourd’hui déjà saisi (jour {suiviDuJour.jour}).</p>
        ) : (
          <Link to={`/bandes/${bande.id}/suivi`} className="bouton principal bloc">
            + Saisir le suivi du jour
          </Link>
        ))}

      <h2>Suivis journaliers</h2>
      {suivis.length === 0 ? (
        <p className="carte discret">Aucun suivi saisi pour cette bande.</p>
      ) : (
        <ul className="liste">
          {suivis.map((s) => (
            <li key={s.id} className="carte suivi">
              <div className="ligne">
                <strong>Jour {s.jour}</strong>
                <span className="discret">{dateLisible(s.dateSuivi)}</span>
              </div>
              <ul className="details">
                <li className={s.nbMortalites > 0 ? 'attention' : ''}>
                  {s.nbMortalites === 0 ? 'Aucun mort' : `${s.nbMortalites} mort${s.nbMortalites > 1 ? 's' : ''}`}
                  {s.causeMortalite ? ` — ${s.causeMortalite}` : ''}
                </li>
                {s.poidsMoyen != null && (
                  <li>
                    Pesée : {nombre(s.poidsMoyen, 2)} kg{s.nbSujetsPeses ? ` (${s.nbSujetsPeses} sujets)` : ''}
                  </li>
                )}
                {s.typeSoin && (
                  <li>
                    Soin : {s.typeSoinLibelle ?? s.typeSoin}
                    {s.produitUtilise ? ` — ${s.produitUtilise}` : ''}
                    {s.dosage ? ` (${s.dosage})` : ''}
                  </li>
                )}
                {s.temperature != null && <li>Température : {nombre(s.temperature, 1)} °C</li>}
                {s.observations && <li className="discret">{s.observations}</li>}
              </ul>
            </li>
          ))}
        </ul>
      )}
    </main>
  )
}
