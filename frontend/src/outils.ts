// Petits calculs et formats partagés par les écrans.

/** Date du jour au format "AAAA-MM-JJ" (heure locale du téléphone). */
export function aujourdhui(): string {
  const d = new Date()
  const mois = String(d.getMonth() + 1).padStart(2, '0')
  const jour = String(d.getDate()).padStart(2, '0')
  return `${d.getFullYear()}-${mois}-${jour}`
}

/** Partie date d'une date ISO de l'API : "2026-11-01T00:00:00" -> "2026-11-01". */
export function partieDate(iso: string): string {
  return iso.slice(0, 10)
}

/** Nombre de jours entre deux dates "AAAA-MM-JJ" (sans effet des changements d'heure). */
function joursEntre(debut: string, fin: string): number {
  const utc = (d: string) => {
    const [a, m, j] = d.split('-').map(Number)
    return Date.UTC(a, m - 1, j)
  }
  return Math.round((utc(fin) - utc(debut)) / 86_400_000)
}

/** Âge de la bande (jour d'élevage) à une date : le jour d'arrivée est le jour 1. */
export function jourDElevage(dateArrivee: string, date: string): number {
  return joursEntre(partieDate(dateArrivee), date) + 1
}

/** "2026-11-01" ou ISO -> "1 nov. 2026" */
export function dateLisible(iso: string): string {
  const [a, m, j] = partieDate(iso).split('-').map(Number)
  return new Date(a, m - 1, j).toLocaleDateString('fr-FR', { day: 'numeric', month: 'short', year: 'numeric' })
}

export function nombre(n: number, decimales = 0): string {
  return n.toLocaleString('fr-FR', { maximumFractionDigits: decimales })
}

/** Durée normale d'un cycle (paramètre DureeCycle de l'API). */
export const DUREE_CYCLE = 45
