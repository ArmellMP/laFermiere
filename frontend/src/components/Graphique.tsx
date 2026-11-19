import { useId, useState } from 'react'
import { nombre } from '../outils'

export interface Point {
  x: number // jour d'élevage
  y: number
}

/**
 * Courbe simple (SVG, sans bibliothèque) : aire en dégradé, points, valeur au survol / toucher.
 * Utilisée pour l'évolution du poids moyen et de la mortalité cumulée d'une bande.
 */
export function Courbe({
  points,
  unite,
  decimales = 0,
  couleur = 'var(--feuille)',
  hauteur = 180,
  libelleX = 'Jour',
}: {
  points: Point[]
  unite: string
  decimales?: number
  couleur?: string
  hauteur?: number
  libelleX?: string
}) {
  const id = useId()
  const [survol, setSurvol] = useState<number | null>(null)
  const largeur = 600
  const marge = { haut: 16, droite: 16, bas: 28, gauche: 44 }

  if (points.length === 0) {
    return <p className="graphique-vide">Pas encore de données.</p>
  }

  const xs = points.map((p) => p.x)
  const ys = points.map((p) => p.y)
  const xMin = Math.min(...xs)
  const xMax = Math.max(...xs, xMin + 1)
  const yMax = Math.max(...ys) * 1.15 || 1
  const px = (x: number) => marge.gauche + ((x - xMin) / (xMax - xMin)) * (largeur - marge.gauche - marge.droite)
  const py = (y: number) => marge.haut + (1 - y / yMax) * (hauteur - marge.haut - marge.bas)

  const trace = points.map((p, i) => `${i === 0 ? 'M' : 'L'}${px(p.x).toFixed(1)},${py(p.y).toFixed(1)}`).join(' ')
  const aire = `${trace} L${px(points[points.length - 1].x)},${py(0)} L${px(points[0].x)},${py(0)} Z`
  const graduations = [0, 0.5, 1].map((t) => yMax * t)
  const actif = survol !== null ? points[survol] : null

  return (
    <div className="graphique">
      <svg viewBox={`0 0 ${largeur} ${hauteur}`} role="img" aria-label={`Courbe ${unite}`} onMouseLeave={() => setSurvol(null)}>
        <defs>
          <linearGradient id={`aire-${id}`} x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor={couleur} stopOpacity="0.28" />
            <stop offset="100%" stopColor={couleur} stopOpacity="0" />
          </linearGradient>
        </defs>
        {graduations.map((g) => (
          <g key={g}>
            <line className="graphique-grille" x1={marge.gauche} x2={largeur - marge.droite} y1={py(g)} y2={py(g)} />
            <text className="graphique-axe" x={marge.gauche - 8} y={py(g) + 4} textAnchor="end">
              {nombre(g, decimales)}
            </text>
          </g>
        ))}
        <text className="graphique-axe" x={marge.gauche} y={hauteur - 6}>
          {libelleX} {xMin}
        </text>
        <text className="graphique-axe" x={largeur - marge.droite} y={hauteur - 6} textAnchor="end">
          {libelleX} {xMax}
        </text>
        <path d={aire} fill={`url(#aire-${id})`} className="graphique-aire" />
        <path d={trace} fill="none" stroke={couleur} strokeWidth="3" strokeLinejoin="round" strokeLinecap="round" className="graphique-trait" />
        {points.map((p, i) => (
          <g key={i}>
            <circle cx={px(p.x)} cy={py(p.y)} r={survol === i ? 6 : 3.5} fill="var(--surface)" stroke={couleur} strokeWidth="2.5" />
            {/* Zone de toucher large autour de chaque point */}
            <rect
              x={px(p.x) - 14}
              y={marge.haut}
              width="28"
              height={hauteur - marge.haut - marge.bas}
              fill="transparent"
              onMouseEnter={() => setSurvol(i)}
              onClick={() => setSurvol(i)}
            />
          </g>
        ))}
        {actif && <line className="graphique-repere" x1={px(actif.x)} x2={px(actif.x)} y1={marge.haut} y2={hauteur - marge.bas} />}
      </svg>
      <p className="graphique-legende" aria-live="polite">
        {actif ? (
          <>
            <strong>
              {nombre(actif.y, decimales)} {unite}
            </strong>{' '}
            au {libelleX.toLowerCase()} {actif.x}
          </>
        ) : (
          <span className="discret">Touchez un point pour voir sa valeur</span>
        )}
      </p>
    </div>
  )
}
