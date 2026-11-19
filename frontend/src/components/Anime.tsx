import { useEffect, useState } from 'react'
import { nombre } from '../outils'

const mouvementReduit = () =>
  typeof window !== 'undefined' && window.matchMedia?.('(prefers-reduced-motion: reduce)').matches

/** Fait défiler un nombre de 0 jusqu'à sa valeur (≈ 0,9 s, ralenti à la fin). */
export function useCompteur(cible: number, duree = 900): number {
  const [valeur, setValeur] = useState(() => (mouvementReduit() ? cible : 0))

  useEffect(() => {
    if (mouvementReduit()) {
      setValeur(cible)
      return
    }
    let image = 0
    const debut = performance.now()
    const avancer = (maintenant: number) => {
      const t = Math.min(1, (maintenant - debut) / duree)
      const ralenti = 1 - Math.pow(1 - t, 3)
      setValeur(cible * ralenti)
      if (t < 1) image = requestAnimationFrame(avancer)
    }
    image = requestAnimationFrame(avancer)
    return () => cancelAnimationFrame(image)
  }, [cible, duree])

  return valeur
}

/** Nombre qui défile à l'affichage. */
export function Compteur({ valeur, decimales = 0, suffixe = '' }: { valeur: number; decimales?: number; suffixe?: string }) {
  const v = useCompteur(valeur)
  return (
    <span className="compteur-anime">
      {nombre(decimales ? v : Math.round(v), decimales)}
      {suffixe}
    </span>
  )
}

/**
 * Anneau de progression du cycle (jour / 45) : se remplit à l'affichage.
 * Le dégradé va du vert pousse au jaune soleil quand la fin du cycle approche.
 */
export function Anneau({
  jour,
  total,
  taille = 76,
  epaisseur = 8,
  clair = false,
}: {
  jour: number
  total: number
  taille?: number
  epaisseur?: number
  clair?: boolean
}) {
  const rayon = (taille - epaisseur) / 2
  const circonference = 2 * Math.PI * rayon
  const part = Math.max(0, Math.min(1, jour / total))
  const [rempli, setRempli] = useState(() => (mouvementReduit() ? part : 0))
  const id = `degrade-${taille}-${clair ? 'c' : 'f'}`

  useEffect(() => {
    const image = requestAnimationFrame(() => setRempli(part))
    return () => cancelAnimationFrame(image)
  }, [part])

  return (
    <div className={`anneau ${clair ? 'anneau-clair' : ''}`} style={{ width: taille, height: taille }}>
      <svg width={taille} height={taille} viewBox={`0 0 ${taille} ${taille}`} aria-hidden="true">
        <defs>
          <linearGradient id={id} x1="0" y1="0" x2="1" y2="1">
            <stop offset="0%" stopColor="var(--pousse)" />
            <stop offset="100%" stopColor="var(--soleil)" />
          </linearGradient>
        </defs>
        <circle className="anneau-fond" cx={taille / 2} cy={taille / 2} r={rayon} strokeWidth={epaisseur} fill="none" />
        <circle
          className="anneau-valeur"
          cx={taille / 2}
          cy={taille / 2}
          r={rayon}
          strokeWidth={epaisseur}
          fill="none"
          stroke={`url(#${id})`}
          strokeLinecap="round"
          strokeDasharray={circonference}
          strokeDashoffset={circonference * (1 - rempli)}
          transform={`rotate(-90 ${taille / 2} ${taille / 2})`}
        />
      </svg>
      <div className="anneau-texte">
        <span className="anneau-jour">J{jour}</span>
        <span className="anneau-total">/{total}</span>
      </div>
    </div>
  )
}

/** Paysage animé : soleil qui se lève, collines qui glissent, nuages, poule qui picore. */
export function Paysage({ poule = true }: { poule?: boolean }) {
  return (
    <div className="paysage" aria-hidden="true">
      <div className="soleil">
        <div className="soleil-halo" />
      </div>
      <div className="nuage nuage-1" />
      <div className="nuage nuage-2" />
      <svg className="colline colline-3" viewBox="0 0 1440 320" preserveAspectRatio="none">
        <path d="M0 200C240 140 420 230 720 190s480-90 720-30v160H0z" />
      </svg>
      <svg className="colline colline-2" viewBox="0 0 1440 320" preserveAspectRatio="none">
        <path d="M0 240c200-60 420 10 660-30s520-110 780-40v150H0z" />
      </svg>
      {poule && (
        <div className="poule">
          <svg viewBox="0 0 64 64" width="64" height="64">
            <g className="poule-corps">
              <path d="M14 38c0-12 9-20 20-20 4 0 7-4 7-8 0-3 3-6 6-6s6 3 6 6c0 6-3 11-7 14 4 4 6 9 6 14 0 9-8 14-19 14S14 47 14 38z" fill="#fffaf0" />
              <path d="M30 34c4 4 10 5 14 2" stroke="#e8d9bd" strokeWidth="2.5" fill="none" strokeLinecap="round" />
              <path d="M44 4c2-3 6-3 7 0-2 0-3 1-3 3" fill="#e2703a" />
              <path d="M59 11l5 2-5 2z" fill="#ffb627" />
              <circle cx="51" cy="10" r="1.6" fill="#1b2a20" />
              <path d="M53 17c1 2 0 4-2 4" stroke="#e2703a" strokeWidth="2.5" fill="none" strokeLinecap="round" />
            </g>
            <path d="M28 54v7M38 54v7" stroke="#ffb627" strokeWidth="3" strokeLinecap="round" />
          </svg>
        </div>
      )}
      <svg className="colline colline-1" viewBox="0 0 1440 320" preserveAspectRatio="none">
        <path d="M0 270c260-40 500 20 760-10s460-60 680-20v80H0z" />
      </svg>
    </div>
  )
}
