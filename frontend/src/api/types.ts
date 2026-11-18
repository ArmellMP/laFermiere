// Formes des données échangées avec l'API (src/PresentationApi/Models).
// Les dates arrivent en texte ISO : "2026-11-01T00:00:00".

export type Role = 'Admin' | 'Fermier' | 'Veterinaire' | 'Comptable'

export interface Utilisateur {
  id: number
  nom: string
  email: string
  role: Role
  roleLibelle: string
}

/** Réponse de /authenticate et /refresh. */
export interface ReponseAuth {
  success: boolean
  message?: string
  utilisateur?: Utilisateur
  token?: string
  expiration?: string
  /** Absent tant que l'API n'a pas les refresh tokens : la session dure alors le temps du jeton. */
  refreshToken?: string
  refreshExpiration?: string
}

export type StatutBande = 'EnPreparation' | 'EnCours' | 'Cloturee' | 'Archivee'

export interface Bande {
  id: number
  code: string
  dateArrivee: string
  nbPoussinsInit: number
  nbPoussinsActuel: number
  statut: StatutBande
  statutLibelle: string
  dateCloture?: string | null
  observations?: string | null
  tauxMortalite: number
}

export type TypeSoin = 'Vaccination' | 'Traitement' | 'Prevention' | 'Supplement' | 'Autre'

export interface Suivi {
  id: number
  bandeId: number
  jour: number
  dateSuivi: string
  nbMortalites: number
  causeMortalite?: string | null
  poidsMoyen?: number | null
  nbSujetsPeses?: number | null
  typeSoin?: TypeSoin | null
  typeSoinLibelle?: string | null
  produitUtilise?: string | null
  dosage?: string | null
  temperature?: number | null
  observations?: string | null
  createurNom?: string | null
}

/** Corps de POST /SuiviJournalier (CreateSuiviRequest). */
export interface NouveauSuivi {
  bandeId: number
  jour: number
  dateSuivi: string
  nbMortalites: number
  causeMortalite?: string
  poidsMoyen?: number
  nbSujetsPeses?: number
  typeSoin?: TypeSoin
  produitUtilise?: string
  dosage?: string
  temperature?: number
  observations?: string
}

/** Enveloppe des listes paginées (GET /Bandes...). */
export interface Page<T> {
  items: T[]
  page: number
  taille: number
  total: number
  totalPages: number
}

/** Format commun des erreurs de l'API (ErrorResponse). */
export interface ReponseErreur {
  success: false
  message: string
  errors?: Record<string, string[]> | null
  traceId?: string
}
