import type { ReponseAuth, Utilisateur } from './types'

/**
 * Session de l'utilisateur connecté, gardée dans le navigateur pour qu'il reste connecté
 * après avoir fermé l'appli. Le refresh token permet d'obtenir un nouveau jeton (15 min)
 * sans redemander le mot de passe, pendant 7 jours.
 */
export interface Session {
  utilisateur: Utilisateur
  token: string
  refreshToken?: string
}

const CLE = 'lafermiere.session'

type Ecouteur = (session: Session | null) => void
const ecouteurs = new Set<Ecouteur>()

export function lireSession(): Session | null {
  try {
    const brut = localStorage.getItem(CLE)
    return brut ? (JSON.parse(brut) as Session) : null
  } catch {
    return null
  }
}

export function enregistrerSession(session: Session | null): void {
  try {
    if (session) localStorage.setItem(CLE, JSON.stringify(session))
    else localStorage.removeItem(CLE)
  } catch {
    // Stockage indisponible (navigation privée...) : la session reste valable jusqu'au rechargement
  }
  ecouteurs.forEach((e) => e(session))
}

/** Prévient l'interface quand la session change (connexion, renouvellement, expiration). */
export function surChangementSession(ecouteur: Ecouteur): () => void {
  ecouteurs.add(ecouteur)
  return () => ecouteurs.delete(ecouteur)
}

export function sessionDepuisReponse(reponse: ReponseAuth): Session {
  if (!reponse.token || !reponse.utilisateur) {
    throw new Error('Réponse de connexion incomplète')
  }
  return { utilisateur: reponse.utilisateur, token: reponse.token, refreshToken: reponse.refreshToken }
}
