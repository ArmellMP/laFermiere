import { useMemo, useState, type ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import { IconeChevron } from './Icones'

export interface Colonne<T> {
  cle: string
  titre: string
  rendu: (ligne: T) => ReactNode
  /** Valeur utilisée pour trier la colonne (absente : colonne non triable). */
  tri?: (ligne: T) => number | string
  alignement?: 'gauche' | 'droite' | 'centre'
  /** Sur téléphone, chaque ligne devient une carte : 'titre', 'sous-titre', 'detail' ou masquée. */
  mobile?: 'titre' | 'sous-titre' | 'detail' | 'droite' | 'masquer'
}

/**
 * Tableau de données professionnel :
 *   - ordinateur : en-têtes cliquables pour trier (▲/▼), lignes survolables et cliquables
 *   - téléphone  : chaque ligne devient une carte lisible (titre, sous-titre, détails)
 *   - pagination par 20 lignes
 */
export function Tableau<T>({
  lignes,
  colonnes,
  cle,
  lien,
  triInitial,
  parPage = 20,
  vide,
  actions,
}: {
  lignes: T[]
  colonnes: Colonne<T>[]
  cle: (ligne: T) => string | number
  lien?: (ligne: T) => string
  triInitial?: { cle: string; sens: 'asc' | 'desc' }
  parPage?: number
  vide?: ReactNode
  /** Boutons d'action d'une ligne (Modifier, Supprimer, Consommer…), sans ouvrir la ligne. */
  actions?: (ligne: T) => ReactNode
}) {
  const naviguer = useNavigate()
  const [tri, setTri] = useState(triInitial ?? null)
  const [page, setPage] = useState(1)

  const triees = useMemo(() => {
    if (!tri) return lignes
    const colonne = colonnes.find((c) => c.cle === tri.cle)
    if (!colonne?.tri) return lignes
    const acces = colonne.tri
    return [...lignes].sort((a, b) => {
      const va = acces(a)
      const vb = acces(b)
      const r = typeof va === 'number' && typeof vb === 'number' ? va - vb : String(va).localeCompare(String(vb), 'fr')
      return tri.sens === 'asc' ? r : -r
    })
  }, [lignes, colonnes, tri])

  const pages = Math.max(1, Math.ceil(triees.length / parPage))
  const pageCourante = Math.min(page, pages)
  const visibles = triees.slice((pageCourante - 1) * parPage, pageCourante * parPage)

  const trier = (c: Colonne<T>) => {
    if (!c.tri) return
    setTri((t) => (t?.cle === c.cle ? { cle: c.cle, sens: t.sens === 'asc' ? 'desc' : 'asc' } : { cle: c.cle, sens: 'desc' }))
    setPage(1)
  }

  if (lignes.length === 0) return <>{vide}</>

  const parRole = (role: Colonne<T>['mobile']) => colonnes.filter((c) => c.mobile === role)

  return (
    <div className="tableau-conteneur">
      <table className="tableau">
        <thead>
          <tr>
            {colonnes.map((c) => (
              <th
                key={c.cle}
                scope="col"
                className={`aligne-${c.alignement ?? 'gauche'} ${c.tri ? 'triable' : ''}`}
                aria-sort={tri?.cle === c.cle ? (tri.sens === 'asc' ? 'ascending' : 'descending') : undefined}
                onClick={() => trier(c)}
              >
                <span className="th-contenu">
                  {c.titre}
                  {c.tri && <span className={`fleche-tri ${tri?.cle === c.cle ? tri.sens : ''}`} aria-hidden="true" />}
                </span>
              </th>
            ))}
            {actions && (
              <th scope="col" className="aligne-centre th-actions">
                <span className="th-contenu">Actions</span>
              </th>
            )}
            {lien && <th aria-hidden="true" />}
          </tr>
        </thead>
        <tbody>
          {visibles.map((l, i) => (
            <tr
              key={cle(l)}
              className={lien ? 'cliquable' : ''}
              style={{ animationDelay: `${Math.min(i, 12) * 25}ms` }}
              onClick={lien ? () => naviguer(lien(l)) : undefined}
            >
              {colonnes.map((c) => (
                <td key={c.cle} className={`aligne-${c.alignement ?? 'gauche'}`} data-titre={c.titre}>
                  {c.rendu(l)}
                </td>
              ))}
              {actions && (
                <td className="cellule-actions" onClick={(e) => e.stopPropagation()}>
                  {actions(l)}
                </td>
              )}
              {lien && (
                <td className="cellule-fleche">
                  <IconeChevron taille={18} />
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>

      {/* Téléphone : cartes */}
      <ul className="tableau-cartes">
        {visibles.map((l, i) => {
          const contenu = (
            <>
              <div className="tc-haut">
                <div className="tc-titres">
                  {parRole('titre').map((c) => (
                    <strong key={c.cle}>{c.rendu(l)}</strong>
                  ))}
                  {parRole('sous-titre').map((c) => (
                    <span key={c.cle} className="tc-sous-titre">
                      {c.rendu(l)}
                    </span>
                  ))}
                </div>
                <div className="tc-droite">
                  {parRole('droite').map((c) => (
                    <span key={c.cle}>{c.rendu(l)}</span>
                  ))}
                </div>
              </div>
              {parRole('detail').length > 0 && (
                <dl className="tc-details">
                  {parRole('detail').map((c) => (
                    <div key={c.cle}>
                      <dt>{c.titre}</dt>
                      <dd>{c.rendu(l)}</dd>
                    </div>
                  ))}
                </dl>
              )}
              {/* Actions en bas de la carte, après les informations */}
              {actions && (
                <div className="tc-actions" onClick={(e) => e.stopPropagation()} onKeyDown={(e) => e.stopPropagation()}>
                  {actions(l)}
                </div>
              )}
            </>
          )
          return (
            <li key={cle(l)} className="apparition" style={{ animationDelay: `${Math.min(i, 10) * 40}ms` }}>
              {lien ? (
                <div
                  role="link"
                  tabIndex={0}
                  className="tc-carte cliquable"
                  onClick={() => naviguer(lien(l))}
                  onKeyDown={(e) => (e.key === 'Enter' || e.key === ' ') && (e.preventDefault(), naviguer(lien(l)))}
                >
                  {contenu}
                </div>
              ) : (
                <div className="tc-carte">{contenu}</div>
              )}
            </li>
          )
        })}
      </ul>

      {pages > 1 && (
        <div className="pagination">
          <span className="discret">
            {(pageCourante - 1) * parPage + 1}–{Math.min(pageCourante * parPage, triees.length)} sur {triees.length}
          </span>
          <div className="pagination-boutons">
            <button type="button" className="bouton-contour petit" disabled={pageCourante === 1} onClick={() => setPage(pageCourante - 1)}>
              Précédent
            </button>
            <span className="pagination-page">
              {pageCourante} / {pages}
            </span>
            <button type="button" className="bouton-contour petit" disabled={pageCourante === pages} onClick={() => setPage(pageCourante + 1)}>
              Suivant
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
