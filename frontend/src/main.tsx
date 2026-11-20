import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import App from './App'
import { FournisseurAuth } from './auth/AuthContext'
import { FournisseurConfirmation } from './components/Confirmation'
import { FournisseurNotification } from './components/Formulaire'
import { enregistrerServiceWorker } from './pwa'
import '@fontsource-variable/nunito'
import '@fontsource-variable/fraunces'
import './styles.css'

// Application installable sur le téléphone (version de production)
enregistrerServiceWorker()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <FournisseurAuth>
        <FournisseurNotification>
          <FournisseurConfirmation>
            <App />
          </FournisseurConfirmation>
        </FournisseurNotification>
      </FournisseurAuth>
    </BrowserRouter>
  </StrictMode>,
)
