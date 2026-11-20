import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import type { CompteUtilisateur } from '../api/types'
import { FournisseurAuth } from '../auth/AuthContext'
import { FournisseurConfirmation } from '../components/Confirmation'
import { FournisseurNotification } from '../components/Formulaire'
import Comptes, { genererMotDePasse, motDePasseValide } from './Comptes'

const moi: CompteUtilisateur = {
  id: 1, nom: 'Awa Admin', email: 'awa@ferme.local', role: 'Admin', roleLibelle: 'Administrateur', estActif: true, createdAt: '2026-10-01T00:00:00',
}
const jean: CompteUtilisateur = {
  id: 2, nom: 'Jean Mbarga', email: 'jean@ferme.local', role: 'Fermier', roleLibelle: 'Fermier', estActif: true, createdAt: '2026-10-02T00:00:00',
}

function reponse(corps: unknown, statut = 200) {
  return new Response(corps === null ? null : JSON.stringify(corps), { status: statut, headers: { 'Content-Type': 'application/json' } })
}

/** API simulée : enregistre les écritures ; `put` permet de simuler un refus. */
function simulerApi(put?: (corps: Record<string, unknown>) => Response) {
  const ecritures: { methode: string; url: string; corps?: Record<string, unknown> }[] = []
  vi.stubGlobal(
    'fetch',
    vi.fn(async (url: string, options?: RequestInit) => {
      const methode = options?.method ?? 'GET'
      const corps = options?.body ? JSON.parse(String(options.body)) : undefined
      if (methode !== 'GET') ecritures.push({ methode, url, corps })
      if (url.includes('/Alertes')) return reponse([])
      if (methode === 'POST') return reponse({ ...corps, id: 3, roleLibelle: 'Fermier', estActif: true, createdAt: '2026-11-19T00:00:00' }, 201)
      if (methode === 'PUT') return put ? put(corps) : reponse({ ...jean, ...corps })
      return reponse({ items: [moi, jean], page: 1, taille: 100, total: 2, totalPages: 1 })
    }),
  )
  return ecritures
}

function afficher() {
  localStorage.setItem('lafermiere.session', JSON.stringify({ utilisateur: { id: 1, nom: moi.nom, email: moi.email, role: 'Admin', roleLibelle: 'Administrateur' }, token: 'jeton' }))
  render(
    <MemoryRouter>
      <FournisseurAuth>
        <FournisseurNotification>
          <FournisseurConfirmation>
            <Comptes />
          </FournisseurConfirmation>
        </FournisseurNotification>
      </FournisseurAuth>
    </MemoryRouter>,
  )
}

/** Ligne du tableau (ordinateur) d'un compte */
async function ligne(nom: string) {
  const tableau = await screen.findByRole('table')
  return within(tableau).getAllByRole('row').find((r) => r.textContent?.includes(nom))!
}

describe('gestion des comptes', () => {
  beforeEach(() => localStorage.clear())
  afterEach(() => vi.unstubAllGlobals())

  it('mot de passe proposé : toujours conforme aux règles de l’API', () => {
    for (let i = 0; i < 200; i++) expect(motDePasseValide(genererMotDePasse())).toBe(true)
    expect(motDePasseValide('abc123')).toBe(false) // pas de majuscule
    expect(motDePasseValide('Abcdef')).toBe(false) // pas de chiffre
  })

  it('son propre compte ne peut être ni désactivé ni supprimé', async () => {
    simulerApi()
    afficher()

    const maLigne = await ligne('Awa Admin')
    expect(within(maLigne).getByText('Vous')).toBeInTheDocument()
    expect(within(maLigne).queryByRole('button', { name: 'Désactiver' })).not.toBeInTheDocument()
    expect(within(maLigne).queryByRole('button', { name: 'Supprimer' })).not.toBeInTheDocument()

    const ligneJean = await ligne('Jean Mbarga')
    expect(within(ligneJean).getByRole('button', { name: 'Désactiver' })).toBeInTheDocument()
    expect(within(ligneJean).getByRole('button', { name: 'Supprimer' })).toBeInTheDocument()
  })

  it('création : envoie le compte puis affiche les identifiants à transmettre', async () => {
    const ecritures = simulerApi()
    afficher()

    await userEvent.click(await screen.findByRole('button', { name: /Nouveau compte/ }))
    await userEvent.type(screen.getByPlaceholderText('Ex. : Jean Mbarga'), 'Paul Essomba')
    await userEvent.type(screen.getByPlaceholderText('jean@exemple.cm'), 'paul@ferme.local')
    await userEvent.click(screen.getByRole('radio', { name: /Vétérinaire/ }))
    const motDePasse = (screen.getByDisplayValue(/^[A-Za-z]+-[A-Za-z]+-\d{2}$/) as HTMLInputElement).value
    await userEvent.click(screen.getByRole('button', { name: 'Créer le compte' }))

    const dialogue = await screen.findByRole('alertdialog', { name: 'Compte de Paul Essomba créé' })
    expect(within(dialogue).getByText('paul@ferme.local')).toBeInTheDocument()
    expect(within(dialogue).getByText(motDePasse)).toBeInTheDocument()
    expect(ecritures).toEqual([
      { methode: 'POST', url: '/api/v1/Utilisateurs', corps: { nom: 'Paul Essomba', email: 'paul@ferme.local', password: motDePasse, role: 'Veterinaire' } },
    ])
  })

  it('désactivation refusée par l’API : le message reste dans la pop-up', async () => {
    const ecritures = simulerApi(() =>
      reponse({ success: false, message: 'Il doit rester au moins un administrateur actif.', errors: {} }, 400),
    )
    afficher()

    await userEvent.click(within(await ligne('Jean Mbarga')).getByRole('button', { name: 'Désactiver' }))
    const dialogue = await screen.findByRole('alertdialog', { name: /Désactiver le compte de Jean Mbarga/ })
    await userEvent.click(within(dialogue).getByRole('button', { name: 'Oui, désactiver' }))

    expect(await within(dialogue).findByText('Il doit rester au moins un administrateur actif.')).toBeInTheDocument()
    expect(ecritures[0]).toEqual({ methode: 'PUT', url: '/api/v1/Utilisateurs/2', corps: { id: 2, estActif: false } })
  })
})
