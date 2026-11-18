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

export type TypeStock = 'Aliment' | 'Medicament' | 'Vaccin' | 'Materiel' | 'Autre'

export interface Stock {
  id: number
  bandeId: number
  bandeCode?: string | null
  type: TypeStock
  typeLibelle: string
  nom: string
  quantiteInitiale: number
  quantiteActuelle: number
  uniteMesure: string
  uniteMesureLibelle: string
  seuilAlerte?: number | null
  estEnAlerte: boolean
  datePeremption?: string | null
  estPerime: boolean
  joursAvantPeremption: number
  prixUnitaire?: number | null
  valeurStock: number
  fournisseurNom?: string | null
}

export interface Vente {
  id: number
  bandeId: number
  bandeCode?: string | null
  clientId: number
  clientNom?: string | null
  dateVente: string
  typeVente: string
  typeVenteLibelle: string
  quantite: number
  uniteMesureLibelle: string
  prixUnitaire: number
  montantTotal: number
  modePaiementLibelle: string
  estPaye: boolean
  datePaiement?: string | null
}

export interface Achat {
  id: number
  fournisseurId: number
  fournisseurNom?: string | null
  bandeId: number
  bandeCode?: string | null
  dateAchat: string
  typeAchat: string
  typeAchatLibelle: string
  description?: string | null
  quantite: number
  uniteMesureLibelle: string
  prixUnitaire: number
  montantTotal: number
  factureNumero?: string | null
  estRecu: boolean
}

export interface Client {
  id: number
  nom: string
  typeClient: string
  typeClientLibelle: string
  telephone?: string | null
  email?: string | null
  estActif: boolean
  nombreVentes?: number | null
  montantTotalAchats?: number | null
}

export interface Fournisseur {
  id: number
  nom: string
  typeFournisseur: string
  typeFournisseurLibelle: string
  telephone?: string | null
  email?: string | null
  contactPersonne?: string | null
  estActif: boolean
  nombreAchats?: number | null
  montantTotalAchats?: number | null
}

export type NiveauAlerte = 'Info' | 'Avertissement' | 'Critique'

export interface Alerte {
  id: number
  typeAlerte: string
  typeAlerteLibelle: string
  entiteId: number
  niveau: NiveauAlerte
  niveauLibelle: string
  message: string
  estLue: boolean
  dateAlerte: string
}
