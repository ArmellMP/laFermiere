import type { ReactElement } from 'react'
import { render as rendre, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { FormulaireAchat, FormulaireBande, FormulaireVente } from './Formulaires'
import { FournisseurConfirmation } from '../components/Confirmation'

/** Les pop-ups de confirmation ont besoin de leur fournisseur, comme dans l'application. */
const render = (ui: ReactElement) => rendre(ui, { wrapper: FournisseurConfirmation })

const bande = {
  id: 7, code: '2026-11-01-001', dateArrivee: '2026-11-01T00:00:00', nbPoussinsInit: 100, nbPoussinsActuel: 40,
  statut: 'EnCours', statutLibelle: 'En cours', tauxMortalite: 0,
}

function reponse(corps: unknown, statut = 200) {
  return new Response(JSON.stringify(corps), { status: statut, headers: { 'Content-Type': 'application/json' } })
}

/** API simulée : listes de référence + enregistrement des POST reçus. */
function simulerApi() {
  const posts: { url: string; corps: unknown }[] = []
  vi.stubGlobal(
    'fetch',
    vi.fn(async (url: string, options?: RequestInit) => {
      if (options?.method === 'POST') {
        posts.push({ url, corps: JSON.parse(String(options.body)) })
        return reponse({ id: 99, montantTotal: 0, code: 'X' }, 201)
      }
      if (url.includes('/Bandes')) return reponse([bande])
      if (url.includes('/Clients')) return reponse([{ id: 3, nom: 'Restaurant Le Coq', estActif: true }])
      if (url.includes('/Fournisseurs')) return reponse([{ id: 4, nom: 'Provenderie', estActif: true }])
      return reponse([])
    }),
  )
  return posts
}

describe('formulaires de création', () => {
  beforeEach(() => localStorage.clear())
  afterEach(() => vi.unstubAllGlobals())

  it('nouvelle bande : nombre de poussins obligatoire, vérifié avant tout envoi', async () => {
    const posts = simulerApi()
    render(<FormulaireBande ouvert onFermer={() => {}} onCree={() => {}} />)

    await userEvent.click(screen.getByRole('button', { name: 'Créer la bande' }))

    expect(await screen.findAllByText('Indiquez un nombre de poussins entre 1 et 50 000.')).not.toHaveLength(0)
    expect(posts).toHaveLength(0)
  })

  it('vente : montant calculé pendant la saisie', async () => {
    simulerApi()
    render(<FormulaireVente bandeId={7} ouvert onFermer={() => {}} onCree={() => {}} />)

    await userEvent.type(screen.getByPlaceholderText('Ex. : 20'), '10')
    await userEvent.type(screen.getByPlaceholderText('Ex. : 3500'), '2500')

    expect(screen.getByText(/25\s?000 FCFA/)).toBeInTheDocument()
  })

  it('vente : impossible de vendre plus de poulets que la bande n’en compte', async () => {
    const posts = simulerApi()
    const cree = vi.fn()
    render(<FormulaireVente bandeId={7} ouvert onFermer={() => {}} onCree={cree} />)
    await screen.findByRole('option', { name: 'Restaurant Le Coq' })

    await userEvent.selectOptions(screen.getByLabelText(/Client/), '3')
    await userEvent.type(screen.getByPlaceholderText('Ex. : 20'), '50')
    await userEvent.type(screen.getByPlaceholderText('Ex. : 3500'), '2500')
    await userEvent.click(screen.getByRole('button', { name: 'Enregistrer la vente' }))

    expect((await screen.findAllByText('La bande ne compte que 40 poulets.')).length).toBeGreaterThan(0)
    expect(posts).toHaveLength(0)
    expect(cree).not.toHaveBeenCalled()
  })

  it('vente valide : envoie les bonnes valeurs à l’API', async () => {
    const posts = simulerApi()
    const cree = vi.fn()
    render(<FormulaireVente bandeId={7} ouvert onFermer={() => {}} onCree={cree} />)
    await screen.findByRole('option', { name: 'Restaurant Le Coq' })

    await userEvent.selectOptions(screen.getByLabelText(/Client/), '3')
    await userEvent.type(screen.getByPlaceholderText('Ex. : 20'), '12')
    await userEvent.type(screen.getByPlaceholderText('Ex. : 3500'), '3500')
    await userEvent.click(screen.getByRole('radio', { name: 'Mobile money' }))
    await userEvent.click(screen.getByRole('button', { name: 'Enregistrer la vente' }))

    // Délai large : plusieurs fichiers de tests tournent en parallèle
    await vi.waitFor(() => expect(cree).toHaveBeenCalled(), { timeout: 5000 })
    expect(posts[0].url).toBe('/api/v1/Ventes')
    expect(posts[0].corps).toMatchObject({
      bandeId: 7, clientId: 3, typeVente: 'PouletsVivants', quantite: 12, uniteMesure: 'Unite',
      prixUnitaire: 3500, modePaiement: 'PaiementMobile', estPaye: true,
    })
  })

  it('achat : seules les unités acceptées pour le type sont proposées', async () => {
    simulerApi()
    render(<FormulaireAchat ouvert onFermer={() => {}} onCree={() => {}} />)
    const unites = () => within(screen.getByLabelText('Unité')).getAllByRole('option').map((o) => o.textContent)

    expect(unites()).toEqual(['Kg', 'Sac']) // Aliment par défaut

    await userEvent.click(screen.getByRole('radio', { name: 'Poussins' }))
    expect(unites()).toEqual(['Unité'])

    await userEvent.click(screen.getByRole('radio', { name: 'Médicament' }))
    expect(unites()).toEqual(['Dose', 'Litre', 'Unité', 'Paquet'])
  })
})
