import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { api } from '../api/client'
import { enregistrerSession, lireSession, sessionDepuisReponse, surChangementSession, type Session } from '../api/session'
import type { ReponseAuth, Role, Utilisateur } from '../api/types'

interface ContexteAuth {
  utilisateur: Utilisateur | null
  connexion: (email: string, motDePasse: string) => Promise<void>
  deconnexion: () => Promise<void>
  /** Vrai si l'utilisateur a l'un de ces rôles. */
  aLeRole: (...roles: Role[]) => boolean
}

const Contexte = createContext<ContexteAuth | null>(null)

export function FournisseurAuth({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(() => lireSession())

  // Suit les changements faits ailleurs : renouvellement du jeton, session expirée
  useEffect(() => surChangementSession(setSession), [])

  const connexion = async (email: string, motDePasse: string) => {
    const reponse = await api.post<ReponseAuth>('/Utilisateurs/authenticate', { email, password: motDePasse })
    enregistrerSession(sessionDepuisReponse(reponse))
  }

  const deconnexion = async () => {
    const refreshToken = session?.refreshToken
    enregistrerSession(null)
    if (refreshToken) {
      // Ferme la session côté serveur ; sans réseau, elle expirera d'elle-même
      await api.post('/Utilisateurs/logout', { refreshToken }).catch(() => undefined)
    }
  }

  const utilisateur = session?.utilisateur ?? null
  const aLeRole = (...roles: Role[]) => !!utilisateur && roles.includes(utilisateur.role)

  return <Contexte.Provider value={{ utilisateur, connexion, deconnexion, aLeRole }}>{children}</Contexte.Provider>
}

export function useAuth(): ContexteAuth {
  const contexte = useContext(Contexte)
  if (!contexte) throw new Error('useAuth doit être utilisé dans <FournisseurAuth>')
  return contexte
}
