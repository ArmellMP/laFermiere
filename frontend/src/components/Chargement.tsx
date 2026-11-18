import { useCallback, useEffect, useState } from 'react'
import { ErreurApi } from '../api/client'

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

export function EtatChargement({
  enCours,
  erreur,
  recharger,
}: {
  enCours: boolean
  erreur: string | null
  recharger: () => void
}) {
  if (enCours) return <p className="discret centre" role="status">Chargement…</p>
  if (erreur)
    return (
      <div className="alerte erreur" role="alert">
        <p>{erreur}</p>
        <button className="bouton" onClick={recharger}>
          Réessayer
        </button>
      </div>
    )
  return null
}
