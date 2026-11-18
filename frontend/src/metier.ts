// Règles d'affichage tirées du métier (mêmes seuils que l'API).
import type { Bande, NiveauAlerte, StatutBande } from './api/types'
import type { Ton } from './components/Page'
import { DUREE_CYCLE, aujourdhui, jourDElevage } from './outils'

export const estActive = (b: Bande) => b.statut === 'EnPreparation' || b.statut === 'EnCours'

/** Mortalité : vert < 2 %, jaune < 5 %, rouge au-delà (paramètre SeuilAlerteMortalite = 5). */
export const tonMortalite = (taux: number): Ton => (taux >= 5 ? 'danger' : taux >= 2 ? 'soleil' : 'vert')

export const TON_STATUT: Record<StatutBande, Ton> = {
  EnPreparation: 'soleil',
  EnCours: 'vert',
  Cloturee: 'ciel',
  Archivee: 'neutre',
}

export const LIBELLE_STATUT: Record<StatutBande, string> = {
  EnPreparation: 'En préparation',
  EnCours: 'En cours',
  Cloturee: 'Clôturée',
  Archivee: 'Archivée',
}

export const TON_NIVEAU: Record<NiveauAlerte, Ton> = {
  Info: 'ciel',
  Avertissement: 'soleil',
  Critique: 'danger',
}

/** Jour d'élevage aujourd'hui (ou à la clôture pour une bande terminée). */
export function ageBande(b: Bande): number {
  const fin = b.dateCloture && !estActive(b) ? b.dateCloture.slice(0, 10) : aujourdhui()
  return Math.max(1, jourDElevage(b.dateArrivee, fin))
}

export const joursRestants = (b: Bande) => Math.max(0, DUREE_CYCLE - ageBande(b))

/** Une bande active proche de la fin de cycle (moins d'une semaine) */
export const finDeCycleProche = (b: Bande) => estActive(b) && joursRestants(b) <= 7
