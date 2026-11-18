import type { Bande } from './api/types'
import { ageBande, estActive, finDeCycleProche, tonMortalite } from './metier'
import { aujourdhui } from './outils'

const ilYa = (jours: number) => {
  const d = new Date()
  d.setDate(d.getDate() - jours)
  return d.toISOString().slice(0, 10)
}

const bande = (b: Partial<Bande>): Bande => ({
  id: 1, code: 'B', dateArrivee: aujourdhui(), nbPoussinsInit: 100, nbPoussinsActuel: 100,
  statut: 'EnCours', statutLibelle: 'En cours', tauxMortalite: 0, ...b,
})

describe('règles métier affichées', () => {
  it('mortalité : vert < 2 %, jaune < 5 %, rouge à partir de 5 % (seuil de l’API)', () => {
    expect(tonMortalite(1.9)).toBe('vert')
    expect(tonMortalite(2)).toBe('soleil')
    expect(tonMortalite(5)).toBe('danger')
  })

  it('seules les bandes en préparation ou en cours sont actives', () => {
    expect(estActive(bande({ statut: 'EnPreparation' }))).toBe(true)
    expect(estActive(bande({ statut: 'Cloturee' }))).toBe(false)
  })

  it("âge : jour 1 le jour d'arrivée ; une bande clôturée s'arrête à sa clôture", () => {
    expect(ageBande(bande({ dateArrivee: ilYa(20) }))).toBe(21)
    expect(ageBande(bande({ statut: 'Cloturee', dateArrivee: ilYa(60), dateCloture: ilYa(15) }))).toBe(46)
  })

  it('fin de cycle proche : bande active à 7 jours ou moins de la fin', () => {
    expect(finDeCycleProche(bande({ dateArrivee: ilYa(38) }))).toBe(true)
    expect(finDeCycleProche(bande({ dateArrivee: ilYa(20) }))).toBe(false)
    expect(finDeCycleProche(bande({ statut: 'Cloturee', dateArrivee: ilYa(40) }))).toBe(false)
  })
})
