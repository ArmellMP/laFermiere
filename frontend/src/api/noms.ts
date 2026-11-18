// Noms liés (code de bande, nom du client, du fournisseur).
//
// L'API renvoie aujourd'hui ces champs vides dans les ventes, achats et stocks
// (les jointures SQL sont faites mais perdues avant la réponse). En attendant la
// correction côté API, l'appli les complète à partir des listes de référence.
// Dès que l'API les renvoie, sa valeur est gardée telle quelle.
import { api } from './client'
import { surChangementSession } from './session'
import type { Achat, Bande, Client, Fournisseur, Stock, Vente } from './types'

type Annuaire = Map<number, string>

// Une seule requête par liste et par session de navigation
const cache = new Map<string, Promise<Annuaire>>()

function annuaire<T extends { id: number }>(chemin: string, nom: (e: T) => string): Promise<Annuaire> {
  if (!cache.has(chemin)) {
    cache.set(
      chemin,
      api
        .liste<T>(chemin)
        .then((liste) => new Map(liste.map((e) => [e.id, nom(e)])))
        // Rôle sans accès à cette liste (ex. Fermier pour les fournisseurs) : annuaire vide
        .catch(() => new Map()),
    )
  }
  return cache.get(chemin)!
}

const bandes = () => annuaire<Bande>('/Bandes', (b) => b.code)
const clients = () => annuaire<Client>('/Clients', (c) => c.nom)
const fournisseurs = () => annuaire<Fournisseur>('/Fournisseurs', (f) => f.nom)

/** À appeler après une création ou une modification qui change un nom. */
export function oublierNoms(): void {
  cache.clear()
}

// Connexion, déconnexion ou changement de compte : les droits changent, on repart de zéro
surChangementSession(() => oublierNoms())

export async function avecNomsVentes(ventes: Vente[]): Promise<Vente[]> {
  const [b, c] = await Promise.all([bandes(), clients()])
  return ventes.map((v) => ({
    ...v,
    bandeCode: v.bandeCode ?? b.get(v.bandeId) ?? `Bande n°${v.bandeId}`,
    clientNom: v.clientNom ?? c.get(v.clientId) ?? `Client n°${v.clientId}`,
  }))
}

export async function avecNomsAchats(achats: Achat[]): Promise<Achat[]> {
  const [b, f] = await Promise.all([bandes(), fournisseurs()])
  return achats.map((a) => ({
    ...a,
    bandeCode: a.bandeCode ?? b.get(a.bandeId) ?? `Bande n°${a.bandeId}`,
    fournisseurNom: a.fournisseurNom ?? f.get(a.fournisseurId) ?? `Fournisseur n°${a.fournisseurId}`,
  }))
}

export async function avecNomsStocks(stocks: Stock[]): Promise<Stock[]> {
  const b = await bandes()
  return stocks.map((s) => ({ ...s, bandeCode: s.bandeCode ?? b.get(s.bandeId) ?? `Bande n°${s.bandeId}` }))
}
