import { Navigate, Route, Routes, useLocation } from 'react-router-dom'
import { useAuth } from './auth/AuthContext'
import { IconeSortie } from './components/Icones'
import BandeDetail from './pages/BandeDetail'
import Bandes from './pages/Bandes'
import Connexion from './pages/Connexion'
import SaisieSuivi from './pages/SaisieSuivi'

export default function App() {
  const { utilisateur, deconnexion } = useAuth()
  const location = useLocation()

  // Pas connecté (ou session expirée) : seul l'écran de connexion est accessible
  if (!utilisateur) return <Connexion />

  const initiales = utilisateur.nom
    .split(/\s+/)
    .map((m) => m[0])
    .join('')
    .slice(0, 2)
    .toUpperCase()

  return (
    <>
      <header className="barre">
        <div className="barre-marque">
          <img src="/icone.svg" alt="" width={30} height={30} />
          <span>La Fermière</span>
        </div>
        <div className="barre-utilisateur">
          <span className="avatar" title={`${utilisateur.nom} — ${utilisateur.roleLibelle}`}>
            {initiales}
          </span>
          <button className="bouton-icone" onClick={() => void deconnexion()} aria-label="Se déconnecter" title="Se déconnecter">
            <IconeSortie />
          </button>
        </div>
      </header>
      {/* key : chaque changement de page rejoue l'animation d'entrée */}
      <div key={location.pathname} className="transition-page">
        <Routes location={location}>
          <Route path="/" element={<Bandes />} />
          <Route path="/bandes/:id" element={<BandeDetail />} />
          <Route path="/bandes/:id/suivi" element={<SaisieSuivi />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </div>
    </>
  )
}
