// Pop-up de confirmation au centre de l'écran, pour les actions sérieuses :
// suppression, clôture, démarrage, consommation, abandon d'une saisie…
// « Annuler » a le focus par défaut : un appui sur Entrée par réflexe ne valide rien.
import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from 'react'
import { createPortal } from 'react-dom'
import { IconeAlerte } from './Icones'

export type TonDialogue = 'danger' | 'attention'

/** Fenêtre de confirmation (présentation seule) : utilisée par useConfirmer et par la suppression. */
export function Dialogue({
  ouvert,
  titre,
  ton = 'danger',
  libelleConfirmer,
  libelleAnnuler = 'Annuler',
  enCours = false,
  libelleEnCours,
  erreur,
  onConfirmer,
  onAnnuler,
  sansAnnuler = false,
  children,
}: {
  ouvert: boolean
  titre: string
  ton?: TonDialogue
  libelleConfirmer: string
  libelleAnnuler?: string
  enCours?: boolean
  libelleEnCours?: string
  erreur?: string | null
  onConfirmer: () => void
  onAnnuler: () => void
  /** Simple information : un seul bouton (Échap ou clic à côté appellent onAnnuler) */
  sansAnnuler?: boolean
  children?: ReactNode
}) {
  const annuler = useRef<HTMLButtonElement>(null)
  const confirmerRef = useRef<HTMLButtonElement>(null)

  useEffect(() => {
    if (!ouvert) return
    const precedent = document.activeElement as HTMLElement | null
    const echap = (e: KeyboardEvent) => {
      if (e.key !== 'Escape') return
      e.stopPropagation()
      if (!enCours) onAnnuler()
    }
    // En phase de capture : le panneau éventuellement ouvert dessous ne se ferme pas en même temps
    document.addEventListener('keydown', echap, true)
    ;(annuler.current ?? confirmerRef.current)?.focus()
    return () => {
      document.removeEventListener('keydown', echap, true)
      precedent?.focus?.()
    }
  }, [ouvert, enCours, onAnnuler])

  if (!ouvert) return null
  return createPortal(
    <div className="dialogue">
      <div className="dialogue-voile" onClick={enCours ? undefined : onAnnuler} />
      <div className={`dialogue-boite dialogue-${ton}`} role="alertdialog" aria-modal="true" aria-labelledby="dialogue-titre">
        <span className="dialogue-icone" aria-hidden="true">
          <IconeAlerte taille={26} />
        </span>
        <h2 id="dialogue-titre">{titre}</h2>
        <div className="dialogue-texte">{children}</div>
        {erreur && (
          <div className="message message-erreur" role="alert">
            <IconeAlerte />
            <p>{erreur}</p>
          </div>
        )}
        <div className={`dialogue-boutons ${sansAnnuler ? 'un-seul' : ''}`}>
          {!sansAnnuler && (
            <button ref={annuler} type="button" className="bouton-contour" onClick={onAnnuler} disabled={enCours}>
              {libelleAnnuler}
            </button>
          )}
          <button
            ref={confirmerRef}
            type="button"
            className={`bouton-principal ${ton === 'danger' ? 'bouton-danger' : ''}`}
            onClick={onConfirmer}
            disabled={enCours}
          >
            {enCours ? <span className="points">{libelleEnCours ?? 'Patientez'}</span> : libelleConfirmer}
          </button>
        </div>
      </div>
    </div>,
    document.body,
  )
}

// ---------------------------------------------------------------------------
// useConfirmer() : `if (await confirmer({...})) faire()` depuis n'importe quel écran
// ---------------------------------------------------------------------------
export interface DemandeConfirmation {
  titre: string
  message: ReactNode
  libelleConfirmer: string
  libelleAnnuler?: string
  ton?: TonDialogue
}

type Confirmer = (demande: DemandeConfirmation) => Promise<boolean>

const ContexteConfirmation = createContext<Confirmer | null>(null)

export function FournisseurConfirmation({ children }: { children: ReactNode }) {
  const [demande, setDemande] = useState<(DemandeConfirmation & { repondre: (ok: boolean) => void }) | null>(null)

  const confirmer = useCallback<Confirmer>(
    (d) =>
      new Promise<boolean>((repondre) => {
        setDemande({ ...d, repondre })
      }),
    [],
  )

  const repondre = useCallback(
    (ok: boolean) => {
      demande?.repondre(ok)
      setDemande(null)
    },
    [demande],
  )
  const annuler = useCallback(() => repondre(false), [repondre])

  return (
    <ContexteConfirmation.Provider value={confirmer}>
      {children}
      <Dialogue
        ouvert={demande !== null}
        titre={demande?.titre ?? ''}
        ton={demande?.ton}
        libelleConfirmer={demande?.libelleConfirmer ?? 'Confirmer'}
        libelleAnnuler={demande?.libelleAnnuler}
        onConfirmer={() => repondre(true)}
        onAnnuler={annuler}
      >
        {typeof demande?.message === 'string' ? <p>{demande.message}</p> : demande?.message}
      </Dialogue>
    </ContexteConfirmation.Provider>
  )
}

export function useConfirmer(): Confirmer {
  const confirmer = useContext(ContexteConfirmation)
  if (!confirmer) throw new Error('useConfirmer doit être utilisé dans <FournisseurConfirmation>.')
  return confirmer
}

/** Message commun quand on quitte une saisie commencée. */
export const ABANDON_SAISIE: DemandeConfirmation = {
  titre: 'Abandonner la saisie ?',
  message: 'Ce que vous avez saisi ou modifié ne sera pas enregistré.',
  libelleConfirmer: 'Abandonner',
  libelleAnnuler: 'Continuer la saisie',
  ton: 'attention',
}
