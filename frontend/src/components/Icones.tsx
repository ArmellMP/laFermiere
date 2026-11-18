// Icônes dessinées pour l'appli (trait 2 px, coins arrondis), sans bibliothèque externe.
import type { SVGProps } from 'react'

type Props = SVGProps<SVGSVGElement> & { taille?: number }

function Icone({ taille = 22, children, ...props }: Props & { children: React.ReactNode }) {
  return (
    <svg
      width={taille}
      height={taille}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={2}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      {...props}
    >
      {children}
    </svg>
  )
}

export const IconePoussin = (p: Props) => (
  <Icone {...p}>
    <path d="M7 14c0-3.3 2.2-6 5-6s5 2.7 5 6-2.2 5-5 5-5-1.7-5-5z" />
    <path d="M12 8a3 3 0 1 1 3-3" />
    <path d="M15.5 5.5 18 6l-2.2 1" />
    <path d="M10 19l-1 2M14 19l1 2" />
  </Icone>
)

export const IconeCoeur = (p: Props) => (
  <Icone {...p}>
    <path d="M12 20s-7-4.4-7-10a4 4 0 0 1 7-2.6A4 4 0 0 1 19 10c0 5.6-7 10-7 10z" />
  </Icone>
)

export const IconeBalance = (p: Props) => (
  <Icone {...p}>
    <path d="M5 8h14l-1.5 11h-11z" />
    <path d="M9 8a3 3 0 0 1 6 0" />
    <path d="M12 12v3" />
  </Icone>
)

export const IconeSeringue = (p: Props) => (
  <Icone {...p}>
    <path d="m18 2 4 4M16 4l4 4M7 13l4 4M14 6l4 4-9 9H5v-4z" />
    <path d="m2 22 3-3" />
  </Icone>
)

export const IconeThermometre = (p: Props) => (
  <Icone {...p}>
    <path d="M14 14.8V5a2 2 0 1 0-4 0v9.8a4 4 0 1 0 4 0z" />
  </Icone>
)

export const IconeCalendrier = (p: Props) => (
  <Icone {...p}>
    <rect x="3" y="5" width="18" height="16" rx="3" />
    <path d="M3 10h18M8 3v4M16 3v4" />
  </Icone>
)

export const IconeNote = (p: Props) => (
  <Icone {...p}>
    <path d="M5 4h10l4 4v12H5z" />
    <path d="M9 12h6M9 16h4" />
  </Icone>
)

export const IconeChevron = (p: Props) => (
  <Icone {...p}>
    <path d="m9 6 6 6-6 6" />
  </Icone>
)

export const IconeRetour = (p: Props) => (
  <Icone {...p}>
    <path d="M15 6l-6 6 6 6" />
  </Icone>
)

export const IconePlus = (p: Props) => (
  <Icone {...p}>
    <path d="M12 5v14M5 12h14" />
  </Icone>
)

export const IconeMoins = (p: Props) => (
  <Icone {...p}>
    <path d="M5 12h14" />
  </Icone>
)

export const IconeCoche = (p: Props) => (
  <Icone {...p}>
    <path d="m5 12 5 5 9-10" />
  </Icone>
)

export const IconeSortie = (p: Props) => (
  <Icone {...p}>
    <path d="M15 4h3a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-3" />
    <path d="M10 8l-4 4 4 4M6 12h10" />
  </Icone>
)

export const IconeAlerte = (p: Props) => (
  <Icone {...p}>
    <path d="M12 3 2 20h20z" />
    <path d="M12 10v4M12 17h.01" />
  </Icone>
)

export const IconeSoleil = (p: Props) => (
  <Icone {...p}>
    <circle cx="12" cy="12" r="4" />
    <path d="M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4" />
  </Icone>
)

export const IconeLune = (p: Props) => (
  <Icone {...p}>
    <path d="M20 14.5A8 8 0 1 1 9.5 4a6.5 6.5 0 0 0 10.5 10.5z" />
  </Icone>
)
