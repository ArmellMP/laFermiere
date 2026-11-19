// Arborescence de l'application : une seule définition, utilisée par la barre latérale
// (ordinateur), la barre du bas et le menu (téléphone), et le fil d'Ariane.
// Les rôles reprennent exactement les droits de l'API (AppRoles côté .NET) :
// un écran n'est proposé que si l'API autorise l'utilisateur à en lire les données.
import type { ReactNode } from 'react'
import type { Role } from './api/types'
import {
  IconeCamion,
  IconeCarton,
  IconeCloche,
  IconePanier,
  IconePersonnes,
  IconePoussin,
  IconeTableauBord,
  IconeVente,
} from './components/Icones'

export interface EntreeNavigation {
  chemin: string
  libelle: string
  icone: ReactNode
  roles?: Role[] // absent : tous les rôles
  /** Affichée dans la barre du bas sur téléphone */
  principale?: boolean
}

export interface GroupeNavigation {
  titre?: string
  entrees: EntreeNavigation[]
}

export const NAVIGATION: GroupeNavigation[] = [
  {
    entrees: [{ chemin: '/', libelle: 'Tableau de bord', icone: <IconeTableauBord />, principale: true }],
  },
  {
    titre: 'Élevage',
    entrees: [
      { chemin: '/bandes', libelle: 'Bandes', icone: <IconePoussin />, principale: true },
      { chemin: '/stocks', libelle: 'Stocks', icone: <IconeCarton />, principale: true },
    ],
  },
  {
    titre: 'Commerce',
    entrees: [
      { chemin: '/ventes', libelle: 'Ventes', icone: <IconeVente />, roles: ['Admin', 'Comptable'] },
      { chemin: '/achats', libelle: 'Achats', icone: <IconePanier />, roles: ['Admin', 'Comptable', 'Fermier'] },
      { chemin: '/clients', libelle: 'Clients', icone: <IconePersonnes />, roles: ['Admin', 'Comptable'] },
      { chemin: '/fournisseurs', libelle: 'Fournisseurs', icone: <IconeCamion />, roles: ['Admin', 'Comptable'] },
    ],
  },
  {
    titre: 'Suivi',
    entrees: [{ chemin: '/alertes', libelle: 'Alertes', icone: <IconeCloche />, principale: true }],
  },
]

export function navigationPour(role: Role | undefined): GroupeNavigation[] {
  return NAVIGATION.map((g) => ({
    ...g,
    entrees: g.entrees.filter((e) => !e.roles || (role !== undefined && e.roles.includes(role))),
  })).filter((g) => g.entrees.length > 0)
}

/** Entrée de navigation correspondant à une adresse (la plus longue qui correspond). */
export function entreeActive(chemin: string): EntreeNavigation | undefined {
  const toutes = NAVIGATION.flatMap((g) => g.entrees)
  return toutes
    .filter((e) => (e.chemin === '/' ? chemin === '/' : chemin === e.chemin || chemin.startsWith(`${e.chemin}/`)))
    .sort((a, b) => b.chemin.length - a.chemin.length)[0]
}
