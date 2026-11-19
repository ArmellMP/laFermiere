import { jourDElevage, partieDate } from './outils'

describe("jourDElevage : le jour d'arrivée est le jour 1", () => {
  it.each([
    ['2026-11-01T00:00:00', '2026-11-01', 1],
    ['2026-11-01T00:00:00', '2026-11-02', 2],
    ['2026-11-01', '2026-12-15', 45],
    // Passage à l'heure d'hiver (fin octobre) : pas de jour perdu ni compté deux fois
    ['2026-10-20', '2026-11-05', 17],
  ])('arrivée %s, date %s -> jour %i', (arrivee, date, attendu) => {
    expect(jourDElevage(arrivee, date)).toBe(attendu)
  })

  it('partieDate garde AAAA-MM-JJ', () => {
    expect(partieDate('2026-11-01T08:30:00')).toBe('2026-11-01')
  })
})
