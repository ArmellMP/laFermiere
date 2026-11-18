import { useCallback, useEffect, useState } from 'react'
import { ErreurApi } from '../api/client'
import { IconeAlerte } from './Icones'

/** Charge des données au montage ; recharger() relance l'appel (bouton « Réessayer »). */
export function useDonnees<T>(charger: () => Promise<T>, dependances: unknown[]) {
  const [donnees, setDonnees] = useState<T | null>(null)
  const [erreur, setErreur] = useState<string | null>(null)
  const [enCours, setEnCours] = useState(true)

  const chargerMemo = useCallback(charger, dependances)

  const recharger = useCallback(async () => {
    setEnCours(true)
    setErreur(null)
    try {
      setDonnees(await chargerMemo())
    } catch (err) {
      setErreur(err instanceof ErreurApi ? err.message : 'Chargement impossible.')
    } finally {
      setEnCours(false)
    }
  }, [chargerMemo])

  useEffect(() => {
    void recharger()
  }, [recharger])

  return { donnees, erreur, enCours, recharger }
}

/** Pendant le chargement : cartes « fantômes » qui scintillent. En cas d'erreur : message + Réessayer. */
export function EtatChargement({
  enCours,
  erreur,
  recharger,
  squelettes = 2,
}: {
  enCours: boolean
  erreur: string | null
  recharger: () => void
  squelettes?: number
}) {
  if (enCours)
    return (
      <div className="squelettes" role="status" aria-label="Chargement">
        {Array.from({ length: squelettes }, (_, i) => (
          <div key={i} className="squelette" />
        ))}
      </div>
    )
  if (erreur)
    return (
      <div className="message message-erreur apparition" role="alert">
        <IconeAlerte />
        <div>
          <p>{erreur}</p>
          <button className="bouton-secondaire" onClick={recharger}>
            Réessayer
          </button>
        </div>
      </div>
    )
  return null
}
