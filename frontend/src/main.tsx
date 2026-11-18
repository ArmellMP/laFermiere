import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import App from './App'
import { FournisseurAuth } from './auth/AuthContext'
import { FournisseurNotification } from './components/Formulaire'
import '@fontsource-variable/nunito'
import '@fontsource-variable/fraunces'
import './styles.css'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <FournisseurAuth>
        <FournisseurNotification>
          <App />
        </FournisseurNotification>
      </FournisseurAuth>
    </BrowserRouter>
  </StrictMode>,
)
