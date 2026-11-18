import { useState } from 'react'
import { Link } from 'react-router-dom'
import { api, lireListe } from '../api/client'
import type { Bande, Page } from '../api/types'
import { EtatChargement, useDonnees } from '../components/Chargement'
import { DUREE_CYCLE, aujourdhui, dateLisible, jourDElevage, nombre } from '../outils'

export const estActive = (b: Bande) => b.statut === 'EnPreparation' || b.statut === 'EnCours'

export default function Bandes() {
  const [voirTerminees, setVoirTerminees] = useState(false)
  const { donnees, erreur, enCours, recharger } = useDonnees(
    async () => lireListe(await api.get<Bande[] | Page<Bande>>('/Bandes?taille=100')),
    [],
  )

  const bandes = (donnees ?? [])
    .filter((b) => voirTerminees || estActive(b))
    .sort((a, b) => Number(estActive(b)) - Number(estActive(a)) || b.dateArrivee.localeCompare(a.dateArrivee))

  return (
    <main className="page">
      <div className="entete-page">
        <h1>Mes bandes</h1>
        <label className="interrupteur">
          <input type="checkbox" checked={voirTerminees} onChange={(e) => setVoirTerminees(e.target.checked)} />
          Voir les bandes terminées
        </label>
      </div>

      <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} />

      {!enCours && !erreur && bandes.length === 0 && (
        <p className="carte discret">
          {voirTerminees ? 'Aucune bande enregistrée.' : 'Aucune bande en cours. Cochez « Voir les bandes terminées » pour les anciennes.'}
        </p>
      )}

      <ul className="liste">
        {bandes.map((b) => (
          <li key={b.id}>
            <CarteBande bande={b} />
          </li>
        ))}
      </ul>
    </main>
  )
}

function CarteBande({ bande }: { bande: Bande }) {
  const active = estActive(bande)
  const jour = jourDElevage(bande.dateArrivee, aujourdhui())
  const mortalite = bande.tauxMortalite

  return (
    <Link to={`/bandes/${bande.id}`} className="carte carte-lien">
      <div className="ligne">
        <strong className="code">{bande.code}</strong>
        <span className={`pastille ${active ? 'pastille-active' : ''}`}>{bande.statutLibelle}</span>
      </div>
      {active ? (
        <>
          <p className="grand">
            Jour {jour}
            <span className="discret"> / {DUREE_CYCLE}</span>
          </p>
          <div className="progression" aria-hidden="true">
            <div style={{ width: `${Math.min(100, (jour / DUREE_CYCLE) * 100)}%` }} />
          </div>
        </>
      ) : (
        <p className="discret">Arrivée le {dateLisible(bande.dateArrivee)}</p>
      )}
      <div className="chiffres">
        <div>
          <span className="valeur">{nombre(bande.nbPoussinsActuel)}</span>
          <span className="etiquette">sujets</span>
        </div>
        <div>
          <span className={`valeur ${mortalite >= 5 ? 'attention' : ''}`}>{nombre(mortalite, 1)} %</span>
          <span className="etiquette">mortalité</span>
        </div>
      </div>
    </Link>
  )
}
