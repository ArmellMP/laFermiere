import { useEffect, useState } from 'react'
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import type { Bande, Suivi } from '../api/types'
import { useAuth } from '../auth/AuthContext'
import { Anneau, Compteur } from '../components/Anime'
import { EtatChargement, useDonnees } from '../components/Chargement'
import {
  IconeBalance,
  IconeCoche,
  IconeCoeur,
  IconeNote,
  IconePlus,
  IconePoussin,
  IconeRetour,
  IconeSeringue,
  IconeThermometre,
} from '../components/Icones'
import { DUREE_CYCLE, aujourdhui, dateLisible, jourDElevage, nombre, partieDate } from '../outils'
import { estActive, niveauMortalite } from './Bandes'

export default function BandeDetail() {
  const { id } = useParams()
  const { aLeRole } = useAuth()
  const location = useLocation()
  const naviguer = useNavigate()
  const [celebration, setCelebration] = useState(
    () => (location.state as { message?: string } | null)?.message ?? null,
  )

  // Le message de réussite ne s'affiche qu'une fois (pas après un rechargement de la page)
  useEffect(() => {
    if (!celebration) return
    naviguer(location.pathname, { replace: true, state: null })
    const minuteur = setTimeout(() => setCelebration(null), 4500)
    return () => clearTimeout(minuteur)
  }, [])

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
        <Link to="/" className="lien-retour">
          <IconeRetour taille={20} /> Mes bandes
        </Link>
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
  const dernierPoids = suivis.find((s) => s.poidsMoyen)?.poidsMoyen ?? null
  const niveau = niveauMortalite(bande.tauxMortalite)

  return (
    <main>
      {celebration && <Celebration message={celebration} />}

      <section className="heros heros-bande">
        <div className="heros-decor" aria-hidden="true">
          <span className="bulle bulle-1" />
          <span className="bulle bulle-2" />
        </div>
        <div className="heros-contenu">
          <Link to="/" className="lien-retour clair">
            <IconeRetour taille={20} /> Mes bandes
          </Link>
          <div className="heros-bande-ligne">
            <div>
              <span className={`statut statut-${bande.statut} statut-clair`}>{bande.statutLibelle}</span>
              <h1>{bande.code}</h1>
              <p className="heros-date">Arrivée le {dateLisible(bande.dateArrivee)}</p>
            </div>
            {active && <Anneau jour={Math.min(jour, 99)} total={DUREE_CYCLE} taille={104} epaisseur={10} clair />}
          </div>
        </div>
      </section>

      <div className="page page-avec-action">
        <div className="tuiles">
          <div className="tuile apparition retard-1">
            <span className="tuile-icone tuile-vert">
              <IconePoussin />
            </span>
            <span className="tuile-valeur">
              <Compteur valeur={bande.nbPoussinsActuel} />
            </span>
            <span className="tuile-etiquette">sujets sur {nombre(bande.nbPoussinsInit)}</span>
          </div>
          <div className="tuile apparition retard-2">
            <span className={`tuile-icone tuile-${niveau}`}>
              <IconeCoeur />
            </span>
            <span className="tuile-valeur">
              <Compteur valeur={morts} />
            </span>
            <span className="tuile-etiquette">morts · {nombre(bande.tauxMortalite, 1)} %</span>
          </div>
          <div className="tuile apparition retard-3">
            <span className="tuile-icone tuile-soleil">
              <IconeBalance />
            </span>
            <span className="tuile-valeur">
              {dernierPoids ? <Compteur valeur={dernierPoids} decimales={2} suffixe=" kg" /> : '—'}
            </span>
            <span className="tuile-etiquette">dernier poids moyen</span>
          </div>
        </div>

        {peutSaisir && suiviDuJour && (
          <div className="message message-succes apparition">
            <IconeCoche />
            <p>Le suivi d’aujourd’hui est fait (jour {suiviDuJour.jour}). Beau travail !</p>
          </div>
        )}

        <div className="entete-section">
          <h2>Journal de la bande</h2>
          <span className="discret">{suivis.length} suivi{suivis.length > 1 ? 's' : ''}</span>
        </div>

        {suivis.length === 0 ? (
          <div className="vide apparition">
            <IconeNote taille={44} />
            <p>Aucun suivi pour l’instant. Le premier commence ici !</p>
          </div>
        ) : (
          <ol className="frise">
            {suivis.map((s, i) => (
              <li key={s.id} className="frise-element apparition" style={{ animationDelay: `${Math.min(i, 8) * 60}ms` }}>
                <span className={`frise-point ${s.nbMortalites > 0 ? 'point-danger' : s.typeSoin ? 'point-soin' : 'point-ok'}`} />
                <div className="frise-carte">
                  <div className="frise-haut">
                    <strong>Jour {s.jour}</strong>
                    <span className="discret">{dateLisible(s.dateSuivi)}</span>
                  </div>
                  <div className="frise-puces">
                    <span className={`puce ${s.nbMortalites > 0 ? 'puce-danger' : 'puce-ok'}`}>
                      <IconeCoeur taille={15} />
                      {s.nbMortalites === 0 ? 'Aucun mort' : `${s.nbMortalites} mort${s.nbMortalites > 1 ? 's' : ''}`}
                    </span>
                    {s.poidsMoyen != null && (
                      <span className="puce puce-soleil">
                        <IconeBalance taille={15} />
                        {nombre(s.poidsMoyen, 2)} kg
                      </span>
                    )}
                    {s.typeSoin && (
                      <span className="puce puce-soin">
                        <IconeSeringue taille={15} />
                        {s.typeSoinLibelle ?? s.typeSoin}
                      </span>
                    )}
                    {s.temperature != null && (
                      <span className="puce">
                        <IconeThermometre taille={15} />
                        {nombre(s.temperature, 1)} °C
                      </span>
                    )}
                  </div>
                  {(s.causeMortalite || s.produitUtilise || s.observations) && (
                    <p className="frise-texte">
                      {[s.causeMortalite && `Cause : ${s.causeMortalite}`, s.produitUtilise && `${s.produitUtilise}${s.dosage ? ` (${s.dosage})` : ''}`, s.observations]
                        .filter(Boolean)
                        .join(' · ')}
                    </p>
                  )}
                </div>
              </li>
            ))}
          </ol>
        )}
      </div>

      {peutSaisir && !suiviDuJour && (
        <Link to={`/bandes/${bande.id}/suivi`} className="bouton-flottant">
          <span className="bouton-flottant-icone">
            <IconePlus />
          </span>
          Suivi du jour {jour}
        </Link>
      )}
    </main>
  )
}

/** Petite fête après une saisie réussie : coche qui se dessine + confettis. */
function Celebration({ message }: { message: string }) {
  return (
    <div className="celebration" role="status">
      <div className="confettis" aria-hidden="true">
        {Array.from({ length: 14 }, (_, i) => (
          <span key={i} style={{ ['--i' as string]: i }} />
        ))}
      </div>
      <div className="celebration-carte">
        <svg className="coche-animee" viewBox="0 0 52 52" aria-hidden="true">
          <circle cx="26" cy="26" r="24" />
          <path d="M15 27l7 7 15-16" />
        </svg>
        <p>{message}</p>
      </div>
    </div>
  )
}
