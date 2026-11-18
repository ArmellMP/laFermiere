import { useState, type FormEvent } from 'react'
import { useAuth } from '../auth/AuthContext'
import { ErreurApi } from '../api/client'

export default function Connexion() {
  const { connexion } = useAuth()
  const [email, setEmail] = useState('')
  const [motDePasse, setMotDePasse] = useState('')
  const [erreur, setErreur] = useState<string | null>(null)
  const [envoi, setEnvoi] = useState(false)

  const valider = async (e: FormEvent) => {
    e.preventDefault()
    setErreur(null)
    setEnvoi(true)
    try {
      await connexion(email.trim(), motDePasse)
    } catch (err) {
      setErreur(err instanceof ErreurApi ? err.message : 'Connexion impossible. Réessayez.')
    } finally {
      setEnvoi(false)
    }
  }

  return (
    <main className="page-connexion">
      <div className="marque">
        <img src="/icone.svg" alt="" width={72} height={72} />
        <h1>La Fermière</h1>
        <p className="discret">Suivi de vos bandes de poulets</p>
      </div>

      <form className="carte formulaire" onSubmit={valider} noValidate>
        <label>
          Adresse e-mail
          <input
            type="email"
            inputMode="email"
            autoComplete="username"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </label>
        <label>
          Mot de passe
          <input
            type="password"
            autoComplete="current-password"
            required
            value={motDePasse}
            onChange={(e) => setMotDePasse(e.target.value)}
          />
        </label>

        {erreur && (
          <p className="alerte erreur" role="alert">
            {erreur}
          </p>
        )}

        <button className="bouton principal" type="submit" disabled={envoi || !email || !motDePasse}>
          {envoi ? 'Connexion…' : 'Se connecter'}
        </button>
      </form>
    </main>
  )
}
