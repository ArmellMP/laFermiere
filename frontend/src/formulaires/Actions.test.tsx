import type { ReactElement } from 'react'
import { render as rendre, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import type { Stock, Vente } from '../api/types'
import { ConfirmationSuppression, FormulaireConsommation } from './Actions'
import { FormulaireVente } from './Formulaires'
import { FournisseurConfirmation } from '../components/Confirmation'

/** Les pop-ups de confirmation ont besoin de leur fournisseur, comme dans l'application. */
const render = (ui: ReactElement) => rendre(ui, { wrapper: FournisseurConfirmation })

const stock: Stock = {
  id: 5, bandeId: 7, bandeCode: '2026-11-01-001', type: 'Aliment', typeLibelle: 'Aliment', nom: 'Démarrage',
  quantiteInitiale: 100, quantiteActuelle: 30, uniteMesure: 'Kg', uniteMesureLibelle: 'Kg', estEnAlerte: false,
  estPerime: false, joursAvantPeremption: 0, valeurStock: 0,
}

const vente: Vente = {
  id: 12, bandeId: 7, bandeCode: '2026-11-01-001', clientId: 3, clientNom: 'Restaurant Le Coq', dateVente: '2026-11-10T00:00:00',
  typeVente: 'PouletsVivants', typeVenteLibelle: 'Poulets vivants', quantite: 10, uniteMesure: 'Unite', uniteMesureLibelle: 'Unité',
  prixUnitaire: 3000, montantTotal: 30000, modePaiement: 'Especes', modePaiementLibelle: 'Espèces', estPaye: false,
}

function reponse(corps: unknown, statut = 200) {
  return new Response(corps === null ? null : JSON.stringify(corps), { status: statut, headers: { 'Content-Type': 'application/json' } })
}

/** API simulée : enregistre chaque appel (méthode, adresse, corps). */
function simulerApi() {
  const appels: { methode: string; url: string; corps?: unknown }[] = []
  vi.stubGlobal(
    'fetch',
    vi.fn(async (url: string, options?: RequestInit) => {
      const methode = options?.method ?? 'GET'
      appels.push({ methode, url, corps: options?.body ? JSON.parse(String(options.body)) : undefined })
      if (methode === 'DELETE') return reponse(null, 204)
      if (methode === 'POST' && url.includes('/consommer')) return reponse({ ...stock, quantiteActuelle: 18 })
      if (methode === 'PUT') return reponse({ ...vente, quantite: 15, montantTotal: 45000 })
      if (url.includes('/Bandes'))
        return reponse([{ id: 7, code: '2026-11-01-001', dateArrivee: '2026-11-01', nbPoussinsInit: 100, nbPoussinsActuel: 40, statut: 'EnCours', statutLibelle: 'En cours', tauxMortalite: 0 }])
      if (url.includes('/Clients')) return reponse([{ id: 3, nom: 'Restaurant Le Coq', estActif: true }])
      return reponse([])
    }),
  )
  return appels
}

describe('actions sur les saisies existantes', () => {
  beforeEach(() => localStorage.clear())
  afterEach(() => vi.unstubAllGlobals())

  it('consommation : affiche ce qui restera et refuse plus que le disponible', async () => {
    const appels = simulerApi()
    render(<FormulaireConsommation stock={stock} onFermer={() => {}} onFait={() => {}} />)

    await userEvent.type(screen.getByPlaceholderText('Ex. : 25'), '12')
    expect(screen.getByText('18 Kg')).toBeInTheDocument()

    await userEvent.clear(screen.getByPlaceholderText('Ex. : 25'))
    await userEvent.type(screen.getByPlaceholderText('Ex. : 25'), '40')
    await userEvent.click(screen.getByRole('button', { name: 'Enregistrer la consommation' }))

    expect((await screen.findAllByText('Il ne reste que 30 Kg.')).length).toBeGreaterThan(0)
    expect(appels.filter((a) => a.methode === 'POST')).toHaveLength(0)
  })

  it('consommation valide : appelle /Stocks/{id}/consommer', async () => {
    const appels = simulerApi()
    const fait = vi.fn()
    render(<FormulaireConsommation stock={stock} onFermer={() => {}} onFait={fait} />)

    await userEvent.click(screen.getByRole('button', { name: '10 Kg' }))
    await userEvent.click(screen.getByRole('button', { name: 'Enregistrer la consommation' }))

    // Pop-up : la consommation ne peut pas être annulée ensuite
    expect(await screen.findByRole('alertdialog', { name: 'Confirmer la consommation' })).toBeInTheDocument()
    expect(appels.filter((a) => a.methode === 'POST')).toHaveLength(0)
    await userEvent.click(screen.getByRole('button', { name: 'Oui, retirer du stock' }))

    await vi.waitFor(() => expect(fait).toHaveBeenCalled(), { timeout: 5000 })
    expect(appels.find((a) => a.methode === 'POST')).toMatchObject({ url: '/api/v1/Stocks/5/consommer', corps: { stockId: 5, quantiteConsommee: 10 } })
  })

  it('consommation : « Annuler » dans la pop-up n’envoie rien', async () => {
    const appels = simulerApi()
    const fait = vi.fn()
    render(<FormulaireConsommation stock={stock} onFermer={() => {}} onFait={fait} />)

    await userEvent.click(screen.getByRole('button', { name: '10 Kg' }))
    await userEvent.click(screen.getByRole('button', { name: 'Enregistrer la consommation' }))
    const dialogue = await screen.findByRole('alertdialog')
    // « Annuler » a le focus : un appui sur Entrée par réflexe ne valide rien
    expect(within(dialogue).getByRole('button', { name: 'Annuler' })).toHaveFocus()
    await userEvent.click(within(dialogue).getByRole('button', { name: 'Annuler' }))

    expect(screen.queryByRole('alertdialog')).not.toBeInTheDocument()
    expect(appels.filter((a) => a.methode === 'POST')).toHaveLength(0)
    expect(fait).not.toHaveBeenCalled()
  })

  it('panneau : fermer une saisie commencée demande confirmation', async () => {
    simulerApi()
    const fermer = vi.fn()
    render(<FormulaireConsommation stock={stock} onFermer={fermer} onFait={() => {}} />)

    // Rien saisi : fermeture directe
    await userEvent.click(screen.getByRole('button', { name: 'Annuler' }))
    expect(fermer).toHaveBeenCalledTimes(1)

    // Saisie commencée : « Continuer la saisie » garde le panneau, « Abandonner » le ferme
    await userEvent.type(screen.getByPlaceholderText('Ex. : 25'), '3')
    await userEvent.click(screen.getByRole('button', { name: 'Fermer' }))
    expect(await screen.findByRole('alertdialog', { name: 'Abandonner la saisie ?' })).toBeInTheDocument()
    await userEvent.click(screen.getByRole('button', { name: 'Continuer la saisie' }))
    expect(fermer).toHaveBeenCalledTimes(1)
    expect(screen.getByPlaceholderText('Ex. : 25')).toHaveValue('3')

    await userEvent.click(screen.getByRole('button', { name: 'Annuler' }))
    await userEvent.click(await screen.findByRole('button', { name: 'Abandonner' }))
    expect(fermer).toHaveBeenCalledTimes(2)
  })

  it('suppression refusée par l’API : le message reste dans la pop-up', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(async () => reponse({ statusCode: 409, message: 'Cet achat a déjà été consommé.', errors: {} }, 409)),
    )
    const fait = vi.fn()
    render(
      <ConfirmationSuppression demande={{ titre: 'Supprimer l’achat', route: '/Achats/3', description: 'Achat de test' }} onFermer={() => {}} onFait={fait} />,
    )
    await userEvent.click(screen.getByRole('button', { name: 'Supprimer définitivement' }))

    expect(await screen.findByText('Cet achat a déjà été consommé.')).toBeInTheDocument()
    expect(screen.getByRole('alertdialog')).toBeInTheDocument()
    expect(fait).not.toHaveBeenCalled()
  })

  it('suppression : demande confirmation puis envoie DELETE', async () => {
    const appels = simulerApi()
    const fait = vi.fn()
    render(
      <ConfirmationSuppression
        demande={{ titre: 'Supprimer la vente', route: '/Ventes/12', description: 'Vente de test' }}
        onFermer={() => {}}
        onFait={fait}
      />,
    )

    expect(screen.getByText('Cette action est définitive')).toBeInTheDocument()
    await userEvent.click(screen.getByRole('button', { name: 'Supprimer définitivement' }))

    await vi.waitFor(() => expect(fait).toHaveBeenCalled(), { timeout: 5000 })
    expect(appels.find((a) => a.methode === 'DELETE')?.url).toBe('/api/v1/Ventes/12')
  })

  it('modification d’une vente : pré-remplie, bande et client verrouillés, envoie PUT', async () => {
    const appels = simulerApi()
    const fait = vi.fn()
    render(<FormulaireVente existant={vente} ouvert onFermer={() => {}} onCree={fait} />)
    await screen.findByRole('option', { name: 'Restaurant Le Coq' })

    const quantite = screen.getByPlaceholderText('Ex. : 20')
    expect(quantite).toHaveValue('10')
    expect(screen.getByLabelText(/Bande/)).toBeDisabled()
    expect(screen.getByLabelText(/Client/)).toBeDisabled()
    // 40 restants dans la bande + les 10 de cette vente
    expect(screen.getByText('50 disponibles')).toBeInTheDocument()

    await userEvent.clear(quantite)
    await userEvent.type(quantite, '15')
    await userEvent.click(screen.getByRole('button', { name: 'Enregistrer les modifications' }))

    await vi.waitFor(() => expect(fait).toHaveBeenCalled(), { timeout: 5000 })
    const put = appels.find((a) => a.methode === 'PUT')
    expect(put?.url).toBe('/api/v1/Ventes/12')
    expect(put?.corps).toMatchObject({ id: 12, quantite: 15, typeVente: 'PouletsVivants', prixUnitaire: 3000, estPaye: false })
    expect(put?.corps).not.toHaveProperty('clientId')
  })
})
