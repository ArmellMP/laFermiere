import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { FournisseurAuth } from '../auth/AuthContext'
import Connexion from './Connexion'

describe('écran de connexion', () => {
  beforeEach(() => localStorage.clear())
  afterEach(() => vi.unstubAllGlobals())

  it("affiche le message de l'API quand le mot de passe est faux", async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify({ success: false, message: 'Email ou mot de passe incorrect.' }), { status: 401 }),
      ),
    )
    render(
      <FournisseurAuth>
        <Connexion />
      </FournisseurAuth>,
    )

    await userEvent.type(screen.getByLabelText('Adresse e-mail'), 'awa@ferme.local')
    await userEvent.type(screen.getByLabelText('Mot de passe'), 'faux')
    await userEvent.click(screen.getByRole('button', { name: 'Se connecter' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Email ou mot de passe incorrect.')
  })

  it('bouton désactivé tant que les champs sont vides', () => {
    render(
      <FournisseurAuth>
        <Connexion />
      </FournisseurAuth>,
    )

    expect(screen.getByRole('button', { name: 'Se connecter' })).toBeDisabled()
  })
})
