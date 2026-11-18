// Briques des formulaires : panneau coulissant, champs, choix segmentés,
// gestion de l'envoi et des erreurs de l'API (format ErrorResponse).
import { createContext, useCallback, useContext, useEffect, useRef, useState, type FormEvent, type ReactNode } from 'react'
import { createPortal } from 'react-dom'
import { ErreurApi } from '../api/client'
import { IconeAlerte, IconeFermer } from './Icones'

// ---------------------------------------------------------------------------
// Panneau : glisse du bas (téléphone) ou de la droite (ordinateur)
// ---------------------------------------------------------------------------
export function Panneau({
  ouvert,
  titre,
  sousTitre,
  onFermer,
  children,
}: {
  ouvert: boolean
  titre: string
  sousTitre?: string
  onFermer: () => void
  children: ReactNode
}) {
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!ouvert) return
    const echap = (e: KeyboardEvent) => e.key === 'Escape' && onFermer()
    document.addEventListener('keydown', echap)
    document.body.style.overflow = 'hidden'
    // Place le curseur dans le premier champ, une fois le panneau ouvert,
    // sauf si l'utilisateur a déjà commencé à remplir un champ (ne pas lui voler la saisie)
    const minuteur = setTimeout(() => {
      const panneau = ref.current
      if (!panneau || panneau.contains(document.activeElement)) return
      panneau.querySelector<HTMLElement>('input, select, textarea')?.focus()
    }, 350)
    return () => {
      document.removeEventListener('keydown', echap)
      document.body.style.overflow = ''
      clearTimeout(minuteur)
    }
  }, [ouvert, onFermer])

  // Rendu à la racine du document (portail) : sinon les animations d'entrée de page
  // l'enferment sous la barre d'onglets du téléphone, qui recouvrait le bouton Enregistrer.
  return createPortal(
    <div className={`panneau ${ouvert ? 'ouvert' : ''}`} aria-hidden={!ouvert}>
      <div className="panneau-voile" onClick={onFermer} />
      <div className="panneau-contenu" role="dialog" aria-modal="true" aria-label={titre} ref={ref}>
        <div className="panneau-poignee" aria-hidden="true" />
        <header className="panneau-entete">
          <div>
            <h2>{titre}</h2>
            {sousTitre && <p className="discret">{sousTitre}</p>}
          </div>
          <button type="button" className="bouton-icone" onClick={onFermer} aria-label="Fermer">
            <IconeFermer />
          </button>
        </header>
        {ouvert && children}
      </div>
    </div>,
    document.body,
  )
}

// ---------------------------------------------------------------------------
// Envoi d'un formulaire : état, erreurs générales et par champ
// ---------------------------------------------------------------------------
export function useEnvoi() {
  const [envoi, setEnvoi] = useState(false)
  const [erreur, setErreur] = useState<string | null>(null)
  const [champs, setChamps] = useState<Record<string, string[]>>({})

  const envoyer = useCallback(async (action: () => Promise<void>) => {
    setErreur(null)
    setChamps({})
    setEnvoi(true)
    try {
      await action()
    } catch (err) {
      if (err instanceof ErreurApi) {
        const { body, ...parChamp } = err.champs
        const details = [...new Set(Object.values(parChamp).flat())]
        setErreur(details.length ? details.join(' ') : body?.join(' ') || err.message)
        setChamps(parChamp)
      } else {
        setErreur("L'enregistrement a échoué. Réessayez.")
      }
    } finally {
      setEnvoi(false)
    }
  }, [])

  /** Erreur ajoutée par l'appli avant l'envoi (même présentation que celles de l'API). */
  const signaler = (champ: string, message: string) => {
    setErreur(message)
    setChamps({ [champ]: [message] })
  }

  return { envoi, erreur, champs, envoyer, signaler }
}

/** Corps du formulaire dans un panneau : contenu défilant + pied fixe avec les boutons. */
export function FormulairePanneau({
  onSubmit,
  erreur,
  envoi,
  libelleValider,
  onAnnuler,
  resume,
  children,
}: {
  onSubmit: () => void
  erreur: string | null
  envoi: boolean
  libelleValider: string
  onAnnuler: () => void
  resume?: ReactNode
  children: ReactNode
}) {
  const valider = (e: FormEvent) => {
    e.preventDefault()
    onSubmit()
  }
  return (
    <form className="formulaire-panneau" onSubmit={valider} noValidate>
      <div className="formulaire-corps">
        {erreur && (
          <div className="message message-erreur apparition" role="alert" key={erreur}>
            <IconeAlerte />
            <p>{erreur}</p>
          </div>
        )}
        {children}
      </div>
      <footer className="formulaire-pied">
        {resume && <div className="formulaire-resume">{resume}</div>}
        <div className="formulaire-boutons">
          <button type="button" className="bouton-contour" onClick={onAnnuler}>
            Annuler
          </button>
          <button type="submit" className="bouton-principal" disabled={envoi}>
            {envoi ? <span className="points">Enregistrement</span> : libelleValider}
          </button>
        </div>
      </footer>
    </form>
  )
}

// ---------------------------------------------------------------------------
// Champs
// ---------------------------------------------------------------------------
export function Champ({
  libelle,
  aide,
  erreurs,
  obligatoire,
  children,
  className = '',
}: {
  libelle?: string
  aide?: string
  erreurs?: string[]
  obligatoire?: boolean
  children: ReactNode
  className?: string
}) {
  return (
    <label className={`champ ${erreurs?.length ? 'en-erreur' : ''} ${className}`}>
      {libelle && (
        <span>
          {libelle}
          {obligatoire && <span className="obligatoire"> *</span>}
        </span>
      )}
      {children}
      {aide && !erreurs?.length && <small className="aide">{aide}</small>}
      {erreurs?.length ? (
        <span className="erreur-champ" role="alert">
          {erreurs.join(' ')}
        </span>
      ) : null}
    </label>
  )
}

export function Ligne({ children }: { children: ReactNode }) {
  return <div className="deux-colonnes">{children}</div>
}

/** Choix exclusif sous forme de boutons (remplace une liste déroulante courte). */
export function Segments<T extends string>({
  options,
  valeur,
  onChange,
  libelle,
}: {
  options: { valeur: T; libelle: string; icone?: ReactNode }[]
  valeur: T
  onChange: (v: T) => void
  libelle: string
}) {
  return (
    <div className="segments" role="radiogroup" aria-label={libelle}>
      {options.map((o) => (
        <button
          key={o.valeur}
          type="button"
          role="radio"
          aria-checked={valeur === o.valeur}
          className={`segment ${valeur === o.valeur ? 'choisi' : ''}`}
          onClick={() => onChange(o.valeur)}
        >
          {o.icone}
          {o.libelle}
        </button>
      ))}
    </div>
  )
}

export function Interrupteur({ actif, onChange, libelle }: { actif: boolean; onChange: (v: boolean) => void; libelle: string }) {
  return (
    <label className="interrupteur">
      <input type="checkbox" role="switch" checked={actif} onChange={(e) => onChange(e.target.checked)} />
      <span className="interrupteur-piste" aria-hidden="true" />
      <span>{libelle}</span>
    </label>
  )
}

/** Texte saisi -> nombre (accepte la virgule : "1,25"), ou undefined si vide ou invalide. */
export function versNombre(texte: string): number | undefined {
  const t = texte.trim().replace(/\s/g, '').replace(',', '.')
  if (t === '') return undefined
  const n = Number(t)
  return Number.isFinite(n) ? n : undefined
}

// ---------------------------------------------------------------------------
// Notifications (« Vente enregistrée ») affichées en haut de l'écran
// ---------------------------------------------------------------------------
type Notifier = (message: string) => void
const ContexteNotification = createContext<Notifier>(() => undefined)

export function FournisseurNotification({ children }: { children: ReactNode }) {
  const [notes, setNotes] = useState<{ id: number; message: string }[]>([])

  const notifier = useCallback<Notifier>((message) => {
    const id = Date.now() + Math.random()
    setNotes((n) => [...n, { id, message }])
    setTimeout(() => setNotes((n) => n.filter((x) => x.id !== id)), 4200)
  }, [])

  return (
    <ContexteNotification.Provider value={notifier}>
      {children}
      <div className="notifications" aria-live="polite">
        {notes.map((n) => (
          <div key={n.id} className="notification">
            <svg className="coche-animee" viewBox="0 0 52 52" aria-hidden="true">
              <circle cx="26" cy="26" r="24" />
              <path d="M15 27l7 7 15-16" />
            </svg>
            <p>{n.message}</p>
          </div>
        ))}
      </div>
    </ContexteNotification.Provider>
  )
}

export const useNotifier = () => useContext(ContexteNotification)
