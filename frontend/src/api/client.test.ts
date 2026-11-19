import { ErreurApi, listeComplete, lireListe, requete } from './client'
import { enregistrerSession, lireSession } from './session'

const utilisateur = { id: 1, nom: 'Awa', email: 'awa@ferme.local', role: 'Fermier' as const, roleLibelle: 'Fermier' }

function reponseJson(statut: number, corps: unknown): Response {
  return new Response(JSON.stringify(corps), { status: statut, headers: { 'Content-Type': 'application/json' } })
}

describe('client API', () => {
  const fetchSimule = vi.fn<typeof fetch>()

  beforeEach(() => {
    fetchSimule.mockReset()
    vi.stubGlobal('fetch', fetchSimule)
    localStorage.clear()
  })

  afterEach(() => vi.unstubAllGlobals())

  it('ajoute le jeton de la session', async () => {
    enregistrerSession({ utilisateur, token: 'jeton-1' })
    fetchSimule.mockResolvedValueOnce(reponseJson(200, []))

    await requete('/Bandes')

    const entetes = new Headers(fetchSimule.mock.calls[0][1]?.headers)
    expect(entetes.get('Authorization')).toBe('Bearer jeton-1')
  })

  it('jeton expiré : renouvelle la session une fois puis rejoue la requête', async () => {
    enregistrerSession({ utilisateur, token: 'ancien', refreshToken: 'r1' })
    fetchSimule
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(reponseJson(200, { success: true, utilisateur, token: 'nouveau', refreshToken: 'r2' }))
      .mockResolvedValueOnce(reponseJson(200, [{ id: 3 }]))

    const resultat = await requete<{ id: number }[]>('/Bandes')

    expect(resultat).toEqual([{ id: 3 }])
    expect(fetchSimule.mock.calls[1][0]).toBe('/api/v1/Utilisateurs/refresh')
    expect(JSON.parse(String(fetchSimule.mock.calls[1][1]?.body))).toEqual({ refreshToken: 'r1' })
    expect(new Headers(fetchSimule.mock.calls[2][1]?.headers).get('Authorization')).toBe('Bearer nouveau')
    expect(lireSession()?.refreshToken).toBe('r2')
  })

  it('deux requêtes expirées en même temps : un seul renouvellement', async () => {
    enregistrerSession({ utilisateur, token: 'ancien', refreshToken: 'r1' })
    fetchSimule.mockImplementation(async (url, options) => {
      if (String(url).endsWith('/refresh')) {
        return reponseJson(200, { success: true, utilisateur, token: 'nouveau', refreshToken: 'r2' })
      }
      const jeton = new Headers(options?.headers).get('Authorization')
      return jeton === 'Bearer nouveau' ? reponseJson(200, []) : new Response(null, { status: 401 })
    })

    await Promise.all([requete('/Bandes'), requete('/Stocks')])

    const appelsRefresh = fetchSimule.mock.calls.filter(([url]) => String(url).endsWith('/refresh'))
    expect(appelsRefresh).toHaveLength(1)
  })

  it('renouvellement refusé : la session est effacée (retour à la connexion)', async () => {
    enregistrerSession({ utilisateur, token: 'ancien', refreshToken: 'r1' })
    fetchSimule
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(reponseJson(401, { success: false, message: 'Session expirée' }))

    await expect(requete('/Bandes')).rejects.toBeInstanceOf(ErreurApi)
    expect(lireSession()).toBeNull()
  })

  it("erreur de validation : message et erreurs par champ de l'API", async () => {
    fetchSimule.mockResolvedValueOnce(
      reponseJson(400, { success: false, message: 'Les données envoyées ne sont pas valides.', errors: { poidsMoyen: ['Trop lourd'] } }),
    )

    const erreur = await requete('/SuiviJournalier', { method: 'POST', body: '{}' }).catch((e: unknown) => e)

    expect(erreur).toBeInstanceOf(ErreurApi)
    expect((erreur as ErreurApi).statut).toBe(400)
    expect((erreur as ErreurApi).champs).toEqual({ poidsMoyen: ['Trop lourd'] })
  })

  it('serveur injoignable : message clair', async () => {
    fetchSimule.mockRejectedValueOnce(new TypeError('Failed to fetch'))

    await expect(requete('/Bandes')).rejects.toThrow('Impossible de joindre le serveur')
  })

  it('listeComplete : demande toutes les pages de la pagination', async () => {
    fetchSimule.mockImplementation(async (url) => {
      const page = Number(new URL(String(url), 'http://x').searchParams.get('page'))
      return reponseJson(200, { items: [page * 10], page, taille: 100, total: 3, totalPages: 3 })
    })

    expect(await listeComplete('/Bandes')).toEqual([10, 20, 30])
    expect(fetchSimule).toHaveBeenCalledTimes(3)
  })

  it('listeComplete : un simple tableau (API sans pagination) suffit', async () => {
    fetchSimule.mockResolvedValueOnce(reponseJson(200, [1, 2]))

    expect(await listeComplete('/Bandes')).toEqual([1, 2])
  })

  it('lireListe accepte un tableau ou une page', () => {
    expect(lireListe([1, 2])).toEqual([1, 2])
    expect(lireListe({ items: [3], page: 1, taille: 20, total: 1, totalPages: 1 })).toEqual([3])
  })
})
