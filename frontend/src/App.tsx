import { Navigate, Route, Routes } from 'react-router-dom'
import type { Role } from './api/types'
import { useAuth } from './auth/AuthContext'
import Coquille from './components/Coquille'
import Alertes from './pages/Alertes'
import BandeDetail from './pages/BandeDetail'
import Bandes from './pages/Bandes'
import { Achats, Clients, Fournisseurs, Ventes } from './pages/Commerce'
import Connexion from './pages/Connexion'
import SaisieSuivi from './pages/SaisieSuivi'
import Stocks from './pages/Stocks'
import TableauDeBord from './pages/TableauDeBord'

/** Écran réservé à certains rôles (mêmes droits que l'API) ; sinon retour au tableau de bord. */
function Reserve({ roles, children }: { roles: Role[]; children: React.ReactElement }) {
  const { aLeRole } = useAuth()
  return aLeRole(...roles) ? children : <Navigate to="/" replace />
}

export default function App() {
  const { utilisateur } = useAuth()

  // Pas connecté (ou session expirée) : seul l'écran de connexion est accessible
  if (!utilisateur) return <Connexion />

  return (
    <Coquille>
      <Routes>
        <Route path="/" element={<TableauDeBord />} />
        <Route path="/bandes" element={<Bandes />} />
        <Route path="/bandes/:id" element={<BandeDetail />} />
        <Route path="/bandes/:id/suivi" element={<SaisieSuivi />} />
        <Route path="/stocks" element={<Stocks />} />
        <Route path="/ventes" element={<Reserve roles={['Admin', 'Comptable']}><Ventes /></Reserve>} />
        <Route path="/achats" element={<Reserve roles={['Admin', 'Comptable', 'Fermier']}><Achats /></Reserve>} />
        <Route path="/clients" element={<Reserve roles={['Admin', 'Comptable']}><Clients /></Reserve>} />
        <Route path="/fournisseurs" element={<Reserve roles={['Admin', 'Comptable']}><Fournisseurs /></Reserve>} />
        <Route path="/alertes" element={<Alertes />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Coquille>
  )
}
