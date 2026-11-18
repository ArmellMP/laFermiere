import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from './auth/AuthContext'
import BandeDetail from './pages/BandeDetail'
import Bandes from './pages/Bandes'
import Connexion from './pages/Connexion'
import SaisieSuivi from './pages/SaisieSuivi'

export default function App() {
  const { utilisateur, deconnexion } = useAuth()

  // Pas connecté (ou session expirée) : seul l'écran de connexion est accessible
  if (!utilisateur) return <Connexion />

  return (
    <>
      <header className="barre">
        <span className="titre">La Fermière</span>
        <span className="utilisateur">
          <span className="nom">{utilisateur.nom}</span>
          <button className="bouton discret" onClick={() => void deconnexion()}>
            Déconnexion
          </button>
        </span>
      </header>
      <Routes>
        <Route path="/" element={<Bandes />} />
        <Route path="/bandes/:id" element={<BandeDetail />} />
        <Route path="/bandes/:id/suivi" element={<SaisieSuivi />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </>
  )
}
