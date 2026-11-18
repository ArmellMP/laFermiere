import { useState } from 'react'
import { Link } from 'react-router-dom'
import { api, lireListe } from '../api/client'
import type { Bande, Page } from '../api/types'
import { useAuth } from '../auth/AuthContext'
import { Anneau, Compteur } from '../components/Anime'
import { EtatChargement, useDonnees } from '../components/Chargement'
import { IconeChevron, IconeCoeur, IconeLune, IconePoussin, IconeSoleil } from '../components/Icones'
import { DUREE_CYCLE, aujourdhui, dateLisible, jourDElevage, nombre } from '../outils'

export const estActive = (b: Bande) => b.statut === 'EnPreparation' || b.statut === 'EnCours'

/** Couleur de la mortalité : vert < 2 %, jaune < 5 %, rouge au-delà (seuil d'alerte de l'API). */
export const niveauMortalite = (taux: number) => (taux >= 5 ? 'danger' : taux >= 2 ? 'vigilance' : 'ok')

function salutation(): { texte: string; nuit: boolean } {
  const heure = new Date().getHours()
  if (heure < 5 || heure >= 19) return { texte: 'Bonsoir', nuit: true }
  if (heure < 12) return { texte: 'Bonjour', nuit: false }
  return { texte: 'Bon après-midi', nuit: false }
}

export default function Bandes() {
  const { utilisateur } = useAuth()
  const [voirTerminees, setVoirTerminees] = useState(false)
  const { donnees, erreur, enCours, recharger } = useDonnees(
    async () => lireListe(await api.get<Bande[] | Page<Bande>>('/Bandes?taille=100')),
    [],
  )

  const toutes = donnees ?? []
  const actives = toutes.filter(estActive)
  const bandes = toutes
    .filter((b) => voirTerminees || estActive(b))
    .sort((a, b) => Number(estActive(b)) - Number(estActive(a)) || b.dateArrivee.localeCompare(a.dateArrivee))

  const sujets = actives.reduce((s, b) => s + b.nbPoussinsActuel, 0)
  const initiaux = actives.reduce((s, b) => s + b.nbPoussinsInit, 0)
  const mortalite = initiaux ? ((initiaux - sujets) / initiaux) * 100 : 0
  const { texte, nuit } = salutation()
  const prenom = utilisateur?.nom.split(/\s+/)[0] ?? ''
  const date = new Date().toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long' })

  return (
    <main>
      <section className="heros">
        <div className="heros-decor" aria-hidden="true">
          <span className="bulle bulle-1" />
          <span className="bulle bulle-2" />
          <span className="bulle bulle-3" />
        </div>
        <div className="heros-contenu">
          <p className="heros-date">
            {nuit ? <IconeLune taille={18} /> : <IconeSoleil taille={18} className="tourne" />}
            <span className="capitalise">{date}</span>
          </p>
          <h1>
            {texte} {prenom} <span className="main-salut">👋</span>
          </h1>
          <div className="resume">
            <div className="resume-carte apparition retard-1">
              <span className="resume-valeur">
                <Compteur valeur={actives.length} />
              </span>
              <span className="resume-etiquette">bande{actives.length > 1 ? 's' : ''} en cours</span>
            </div>
            <div className="resume-carte apparition retard-2">
              <span className="resume-valeur">
                <Compteur valeur={sujets} />
              </span>
              <span className="resume-etiquette">poulets sur pied</span>
            </div>
            <div className="resume-carte apparition retard-3">
              <span className={`resume-valeur niveau-${niveauMortalite(mortalite)}`}>
                <Compteur valeur={mortalite} decimales={1} suffixe=" %" />
              </span>
              <span className="resume-etiquette">mortalité</span>
            </div>
          </div>
        </div>
      </section>

      <div className="page">
        <div className="entete-section">
          <h2>Mes bandes</h2>
          <label className="interrupteur">
            <input type="checkbox" role="switch" checked={voirTerminees} onChange={(e) => setVoirTerminees(e.target.checked)} />
            <span className="interrupteur-piste" aria-hidden="true" />
            <span>Terminées</span>
          </label>
        </div>

        <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} squelettes={3} />

        {!enCours && !erreur && bandes.length === 0 && (
          <div className="vide apparition">
            <IconePoussin taille={48} />
            <p>{voirTerminees ? 'Aucune bande enregistrée.' : 'Aucune bande en cours pour le moment.'}</p>
          </div>
        )}

        <ul className="grille-bandes">
          {bandes.map((b, i) => (
            <li key={b.id} className="apparition" style={{ animationDelay: `${Math.min(i, 8) * 70}ms` }}>
              <CarteBande bande={b} />
            </li>
          ))}
        </ul>
      </div>
    </main>
  )
}

function CarteBande({ bande }: { bande: Bande }) {
  const active = estActive(bande)
  const jour = jourDElevage(bande.dateArrivee, aujourdhui())
  const niveau = niveauMortalite(bande.tauxMortalite)

  return (
    <Link to={`/bandes/${bande.id}`} className={`carte-bande ${active ? '' : 'carte-terminee'}`}>
      {active ? (
        <Anneau jour={Math.min(jour, 99)} total={DUREE_CYCLE} />
      ) : (
        <div className="anneau-termine">
          <IconePoussin taille={30} />
        </div>
      )}

      <div className="carte-bande-corps">
        <div className="carte-bande-haut">
          <strong className="code">{bande.code}</strong>
          <span className={`statut statut-${bande.statut}`}>{bande.statutLibelle}</span>
        </div>
        <p className="carte-bande-date">Arrivée le {dateLisible(bande.dateArrivee)}</p>
        <div className="carte-bande-chiffres">
          <span className="puce">
            <IconePoussin taille={16} />
            {nombre(bande.nbPoussinsActuel)}
          </span>
          <span className={`puce puce-${niveau}`}>
            <IconeCoeur taille={16} />
            {nombre(bande.tauxMortalite, 1)} %
          </span>
        </div>
      </div>

      <IconeChevron className="carte-fleche" />
    </Link>
  )
}
