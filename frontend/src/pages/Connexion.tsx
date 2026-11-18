import { useState, type FormEvent } from 'react'
import { useAuth } from '../auth/AuthContext'
import { ErreurApi } from '../api/client'
import { Paysage } from '../components/Anime'

export default function Connexion() {
  const { connexion } = useAuth()
  const [email, setEmail] = useState('')
  const [motDePasse, setMotDePasse] = useState('')
  const [erreur, setErreur] = useState<string | null>(null)
  const [envoi, setEnvoi] = useState(false)
  // Change à chaque erreur : relance l'animation « secousse » de la carte
  const [essai, setEssai] = useState(0)

  const valider = async (e: FormEvent) => {
    e.preventDefault()
    setErreur(null)
    setEnvoi(true)
    try {
      await connexion(email.trim(), motDePasse)
    } catch (err) {
      setErreur(err instanceof ErreurApi ? err.message : 'Connexion impossible. Réessayez.')
      setEssai((n) => n + 1)
    } finally {
      setEnvoi(false)
    }
  }

  return (
    <main className="ecran-connexion">
      <Paysage />

      <div className="connexion-contenu">
        <div className="marque apparition">
          <div className="logo">
            <img src="/icone.svg" alt="" width={68} height={68} />
          </div>
          <h1>La Fermière</h1>
          <p>Votre élevage, au creux de la main</p>
        </div>

        <form key={essai} className={`carte-verre apparition retard-1 ${essai ? 'secousse' : ''}`} onSubmit={valider} noValidate>
          <h2>Bon retour parmi nous</h2>

          <label className="champ">
            <span>Adresse e-mail</span>
            <input
              type="email"
              inputMode="email"
              autoComplete="username"
              required
              placeholder="vous@ferme.local"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </label>
          <label className="champ">
            <span>Mot de passe</span>
            <input
              type="password"
              autoComplete="current-password"
              required
              placeholder="••••••••"
              value={motDePasse}
              onChange={(e) => setMotDePasse(e.target.value)}
            />
          </label>

          {erreur && (
            <p className="message message-erreur" role="alert">
              {erreur}
            </p>
          )}

          <button className="bouton-principal" type="submit" disabled={envoi || !email || !motDePasse}>
            {envoi ? <span className="points">Connexion</span> : 'Se connecter'}
          </button>
        </form>
      </div>
    </main>
  )
}
