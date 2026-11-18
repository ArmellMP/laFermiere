// Briques communes à tous les écrans : même structure partout
//   En-tête (fil d'Ariane, titre, actions) → Indicateurs → Barre d'outils → Contenu
import { useCallback, type ReactNode } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { Compteur } from './Anime'
import { IconeChevron, IconeFermer, IconeGrille, IconeListe, IconeRecherche, IconeTri } from './Icones'

// ---------------------------------------------------------------------------
// État d'un écran dans l'adresse (?statut=EnCours&tri=age) : un filtre survit au
// rechargement et au bouton « retour », et une vue filtrée peut être partagée.
// ---------------------------------------------------------------------------
export function useEtatUrl<T extends string>(cle: string, defaut: T): [T, (v: T) => void] {
  const [params, setParams] = useSearchParams()
  const valeur = (params.get(cle) as T | null) ?? defaut
  const changer = useCallback(
    (v: T) =>
      setParams(
        (p) => {
          const suivant = new URLSearchParams(p)
          if (v === defaut || v === '') suivant.delete(cle)
          else suivant.set(cle, v)
          return suivant
        },
        { replace: true },
      ),
    [cle, defaut, setParams],
  )
  return [valeur, changer]
}

// ---------------------------------------------------------------------------
// En-tête de page
// ---------------------------------------------------------------------------
export interface Miette {
  libelle: string
  vers?: string
}

export function EnTetePage({
  miettes,
  titre,
  sousTitre,
  badge,
  actions,
}: {
  miettes?: Miette[]
  titre: ReactNode
  sousTitre?: ReactNode
  badge?: ReactNode
  actions?: ReactNode
}) {
  return (
    <header className="entete-page">
      {miettes && miettes.length > 0 && (
        <nav className="fil-ariane" aria-label="Fil d’Ariane">
          {miettes.map((m, i) => (
            <span key={i} className="miette">
              {m.vers ? <Link to={m.vers}>{m.libelle}</Link> : <span>{m.libelle}</span>}
              {i < miettes.length - 1 && <IconeChevron taille={14} />}
            </span>
          ))}
        </nav>
      )}
      <div className="entete-page-ligne">
        <div className="entete-page-texte">
          <div className="entete-page-titre">
            <h1>{titre}</h1>
            {badge}
          </div>
          {sousTitre && <p className="entete-page-sous-titre">{sousTitre}</p>}
        </div>
        {actions && <div className="entete-page-actions">{actions}</div>}
      </div>
    </header>
  )
}

// ---------------------------------------------------------------------------
// Bande d'indicateurs (KPI)
// ---------------------------------------------------------------------------
export type Ton = 'neutre' | 'vert' | 'soleil' | 'danger' | 'soin' | 'ciel' | 'terre'

export interface Indicateur {
  libelle: string
  valeur: number | null
  format?: (v: number) => string
  decimales?: number
  suffixe?: string
  detail?: ReactNode
  icone?: ReactNode
  ton?: Ton
  vers?: string
}

export function Indicateurs({ elements }: { elements: Indicateur[] }) {
  return (
    <section className="indicateurs" aria-label="Indicateurs">
      {elements.map((ind, i) => {
        const contenu = (
          <>
            <div className="indicateur-haut">
              <span className="indicateur-libelle">{ind.libelle}</span>
              {ind.icone && <span className={`indicateur-icone ton-${ind.ton ?? 'neutre'}`}>{ind.icone}</span>}
            </div>
            <div className="indicateur-valeur">
              {ind.valeur === null ? (
                '—'
              ) : ind.format ? (
                ind.format(ind.valeur)
              ) : (
                <Compteur valeur={ind.valeur} decimales={ind.decimales} suffixe={ind.suffixe} />
              )}
            </div>
            {ind.detail && <div className="indicateur-detail">{ind.detail}</div>}
          </>
        )
        return ind.vers ? (
          <Link key={i} to={ind.vers} className={`indicateur cliquable bord-${ind.ton ?? 'neutre'}`} style={{ animationDelay: `${i * 50}ms` }}>
            {contenu}
          </Link>
        ) : (
          <div key={i} className={`indicateur bord-${ind.ton ?? 'neutre'}`} style={{ animationDelay: `${i * 50}ms` }}>
            {contenu}
          </div>
        )
      })}
    </section>
  )
}

// ---------------------------------------------------------------------------
// Barre d'outils : recherche, filtres à puces, tri, choix de la vue
// ---------------------------------------------------------------------------
export function BarreOutils({ children }: { children: ReactNode }) {
  return <div className="barre-outils">{children}</div>
}

export function Recherche({ valeur, onChange, placeholder }: { valeur: string; onChange: (v: string) => void; placeholder: string }) {
  return (
    <label className="recherche">
      <IconeRecherche taille={18} />
      <input type="search" value={valeur} onChange={(e) => onChange(e.target.value)} placeholder={placeholder} aria-label={placeholder} />
      {valeur && (
        <button type="button" className="recherche-effacer" onClick={() => onChange('')} aria-label="Effacer la recherche">
          <IconeFermer taille={16} />
        </button>
      )}
    </label>
  )
}

export interface OptionFiltre<T extends string> {
  valeur: T
  libelle: string
  compte?: number
  ton?: Ton
}

/** Filtres exclusifs sous forme de puces, avec le nombre d'éléments de chaque filtre. */
export function Puces<T extends string>({
  options,
  valeur,
  onChange,
  libelle,
}: {
  options: OptionFiltre<T>[]
  valeur: T
  onChange: (v: T) => void
  libelle: string
}) {
  return (
    <div className="puces-filtre" role="radiogroup" aria-label={libelle}>
      {options.map((o) => (
        <button
          key={o.valeur}
          type="button"
          role="radio"
          aria-checked={valeur === o.valeur}
          className={`puce-filtre ${valeur === o.valeur ? 'active' : ''} ton-${o.ton ?? 'neutre'}`}
          onClick={() => onChange(o.valeur)}
        >
          {o.libelle}
          {o.compte !== undefined && <span className="puce-filtre-compte">{o.compte}</span>}
        </button>
      ))}
    </div>
  )
}

export function SelecteurTri<T extends string>({
  options,
  valeur,
  onChange,
}: {
  options: { valeur: T; libelle: string }[]
  valeur: T
  onChange: (v: T) => void
}) {
  return (
    <label className="selecteur-tri">
      <IconeTri taille={18} />
      <select value={valeur} onChange={(e) => onChange(e.target.value as T)} aria-label="Trier par">
        {options.map((o) => (
          <option key={o.valeur} value={o.valeur}>
            {o.libelle}
          </option>
        ))}
      </select>
    </label>
  )
}

export type Vue = 'cartes' | 'tableau'

export function BasculeVue({ valeur, onChange }: { valeur: Vue; onChange: (v: Vue) => void }) {
  return (
    <div className="bascule-vue" role="radiogroup" aria-label="Affichage">
      <button type="button" role="radio" aria-checked={valeur === 'cartes'} className={valeur === 'cartes' ? 'active' : ''} onClick={() => onChange('cartes')} title="Cartes">
        <IconeGrille taille={18} />
      </button>
      <button type="button" role="radio" aria-checked={valeur === 'tableau'} className={valeur === 'tableau' ? 'active' : ''} onClick={() => onChange('tableau')} title="Tableau">
        <IconeListe taille={18} />
      </button>
    </div>
  )
}

// ---------------------------------------------------------------------------
// Onglets
// ---------------------------------------------------------------------------
export function Onglets<T extends string>({
  onglets,
  valeur,
  onChange,
}: {
  onglets: { valeur: T; libelle: string; compte?: number }[]
  valeur: T
  onChange: (v: T) => void
}) {
  return (
    <div className="onglets" role="tablist">
      {onglets.map((o) => (
        <button
          key={o.valeur}
          type="button"
          role="tab"
          aria-selected={valeur === o.valeur}
          className={`onglet ${valeur === o.valeur ? 'actif' : ''}`}
          onClick={() => onChange(o.valeur)}
        >
          {o.libelle}
          {o.compte !== undefined && <span className="onglet-compte">{o.compte}</span>}
        </button>
      ))}
    </div>
  )
}

// ---------------------------------------------------------------------------
// Divers
// ---------------------------------------------------------------------------
export function Badge({ ton = 'neutre', point = false, children }: { ton?: Ton; point?: boolean; children: ReactNode }) {
  return <span className={`badge ton-${ton} ${point ? 'avec-point' : ''}`}>{children}</span>
}

export function Carte({ titre, action, children, className = '' }: { titre?: ReactNode; action?: ReactNode; children: ReactNode; className?: string }) {
  return (
    <section className={`carte ${className}`}>
      {(titre || action) && (
        <div className="carte-entete">
          {titre && <h2>{titre}</h2>}
          {action}
        </div>
      )}
      {children}
    </section>
  )
}

export function Vide({ icone, titre, texte, action }: { icone: ReactNode; titre: string; texte?: string; action?: ReactNode }) {
  return (
    <div className="vide">
      <span className="vide-icone">{icone}</span>
      <strong>{titre}</strong>
      {texte && <p>{texte}</p>}
      {action}
    </div>
  )
}

/** Barre de progression : valeur / maximum. */
export function Jauge({ valeur, max, ton = 'vert' }: { valeur: number; max: number; ton?: Ton }) {
  const part = max > 0 ? Math.max(0, Math.min(1, valeur / max)) : 0
  return (
    <span className={`jauge ton-${ton}`} role="progressbar" aria-valuenow={Math.round(part * 100)} aria-valuemin={0} aria-valuemax={100}>
      <span style={{ width: `${part * 100}%` }} />
    </span>
  )
}
