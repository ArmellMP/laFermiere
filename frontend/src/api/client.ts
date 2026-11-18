import { enregistrerSession, lireSession, sessionDepuisReponse } from './session'
import type { Page, ReponseAuth, ReponseErreur } from './types'

/** Erreur renvoyée par l'API, avec un message lisible par le fermier. */
export class ErreurApi extends Error {
  readonly statut: number
  /** Erreurs par champ du formulaire (validation), ex. { nbMortalites: ["..."] }. */
  readonly champs: Record<string, string[]>

  constructor(statut: number, message: string, champs: Record<string, string[]> = {}) {
    super(message)
    this.name = 'ErreurApi'
    this.statut = statut
    this.champs = champs
  }
}

const BASE = '/api/v1'

// Un seul renouvellement à la fois : si plusieurs appels reçoivent 401 en même temps,
// ils attendent tous le même /refresh (sinon le 2e réutiliserait un token déjà échangé
// et l'API fermerait la session).
let renouvellementEnCours: Promise<boolean> | null = null

async function renouvelerSession(): Promise<boolean> {
  const session = lireSession()
  if (!session?.refreshToken) return false

  try {
    const reponse = await fetch(`${BASE}/Utilisateurs/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken: session.refreshToken }),
    })
    if (!reponse.ok) return false
    enregistrerSession(sessionDepuisReponse((await reponse.json()) as ReponseAuth))
    return true
  } catch {
    return false
  }
}

function renouvelerUneFois(): Promise<boolean> {
  renouvellementEnCours ??= renouvelerSession().finally(() => {
    renouvellementEnCours = null
  })
  return renouvellementEnCours
}

async function lireErreur(reponse: Response): Promise<ErreurApi> {
  let corps: Partial<ReponseErreur> | null = null
  try {
    corps = (await reponse.json()) as Partial<ReponseErreur>
  } catch {
    // Corps vide ou non JSON
  }

  const messageParDefaut: Record<number, string> = {
    401: 'Votre session a expiré. Veuillez vous reconnecter.',
    403: "Votre compte n'a pas le droit de faire cette action.",
    404: 'Élément introuvable.',
    429: 'Trop de tentatives. Patientez une minute puis réessayez.',
  }

  return new ErreurApi(
    reponse.status,
    corps?.message || messageParDefaut[reponse.status] || 'Une erreur est survenue. Réessayez dans un instant.',
    corps?.errors ?? {},
  )
}

/**
 * Appel à l'API : ajoute le jeton, renouvelle la session une fois si le jeton a expiré,
 * et transforme toute erreur en ErreurApi avec un message en français.
 */
export async function requete<T>(chemin: string, options: RequestInit = {}, dejaRenouvele = false): Promise<T> {
  const session = lireSession()
  const entetes = new Headers(options.headers)
  if (options.body) entetes.set('Content-Type', 'application/json')
  if (session) entetes.set('Authorization', `Bearer ${session.token}`)

  let reponse: Response
  try {
    reponse = await fetch(`${BASE}${chemin}`, { ...options, headers: entetes })
  } catch {
    throw new ErreurApi(0, 'Impossible de joindre le serveur. Vérifiez votre connexion internet.')
  }

  if (reponse.status === 401 && session && !dejaRenouvele) {
    if (await renouvelerUneFois()) {
      return requete<T>(chemin, options, true)
    }
    enregistrerSession(null) // l'interface revient à l'écran de connexion
  }

  if (!reponse.ok) throw await lireErreur(reponse)
  if (reponse.status === 204) return undefined as T
  return (await reponse.json()) as T
}

/**
 * Lit une liste, qu'elle soit paginée ({ items, total... }) ou un simple tableau.
 * La pagination des listes principales arrive avec la PR #12 : ce code fonctionne avant et après.
 */
export function lireListe<T>(donnees: T[] | Page<T>): T[] {
  return Array.isArray(donnees) ? donnees : donnees.items
}

/**
 * Charge TOUTE une liste : avec la pagination de l'API (100 éléments au plus par page),
 * les pages suivantes sont demandées jusqu'à la dernière. Sans pagination, un seul appel.
 */
export async function listeComplete<T>(chemin: string): Promise<T[]> {
  const separateur = chemin.includes('?') ? '&' : '?'
  const premiere = await requete<T[] | Page<T>>(`${chemin}${separateur}page=1&taille=100`)
  if (Array.isArray(premiere)) return premiere

  const autres = await Promise.all(
    Array.from({ length: Math.max(0, premiere.totalPages - 1) }, (_, i) =>
      requete<Page<T>>(`${chemin}${separateur}page=${i + 2}&taille=100`),
    ),
  )
  return [premiere, ...autres].flatMap((p) => p.items)
}

export const api = {
  get: <T>(chemin: string) => requete<T>(chemin),
  post: <T>(chemin: string, corps: unknown) => requete<T>(chemin, { method: 'POST', body: JSON.stringify(corps) }),
  liste: listeComplete,
}
