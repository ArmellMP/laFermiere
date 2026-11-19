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

export const IconeTableauBord = (p: Props) => (
  <Icone {...p}>
    <rect x="3" y="3" width="7" height="9" rx="2" />
    <rect x="14" y="3" width="7" height="5" rx="2" />
    <rect x="14" y="12" width="7" height="9" rx="2" />
    <rect x="3" y="16" width="7" height="5" rx="2" />
  </Icone>
)

export const IconeCarton = (p: Props) => (
  <Icone {...p}>
    <path d="M21 8 12 3 3 8v8l9 5 9-5z" />
    <path d="M3 8l9 5 9-5M12 13v8" />
  </Icone>
)

export const IconeVente = (p: Props) => (
  <Icone {...p}>
    <path d="M3 17l6-6 4 4 8-8" />
    <path d="M15 7h6v6" />
  </Icone>
)

export const IconePanier = (p: Props) => (
  <Icone {...p}>
    <path d="M3 4h2l2.4 11.2a2 2 0 0 0 2 1.6h7.7a2 2 0 0 0 2-1.5L21 8H6" />
    <circle cx="10" cy="20" r="1.3" />
    <circle cx="17" cy="20" r="1.3" />
  </Icone>
)

export const IconePersonnes = (p: Props) => (
  <Icone {...p}>
    <circle cx="9" cy="8" r="3.5" />
    <path d="M2.5 20a6.5 6.5 0 0 1 13 0" />
    <path d="M16 4.5a3.5 3.5 0 0 1 0 7M18 14a6 6 0 0 1 3.5 6" />
  </Icone>
)

export const IconeCamion = (p: Props) => (
  <Icone {...p}>
    <path d="M2 6h11v10H2zM13 10h4l4 4v2h-8z" />
    <circle cx="6.5" cy="18" r="2" />
    <circle cx="17" cy="18" r="2" />
  </Icone>
)

export const IconeCloche = (p: Props) => (
  <Icone {...p}>
    <path d="M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9" />
    <path d="M10.3 21a2 2 0 0 0 3.4 0" />
  </Icone>
)

export const IconeMenu = (p: Props) => (
  <Icone {...p}>
    <path d="M4 6h16M4 12h16M4 18h16" />
  </Icone>
)

export const IconeFermer = (p: Props) => (
  <Icone {...p}>
    <path d="M6 6l12 12M18 6 6 18" />
  </Icone>
)

export const IconeRecherche = (p: Props) => (
  <Icone {...p}>
    <circle cx="11" cy="11" r="7" />
    <path d="m20 20-3.5-3.5" />
  </Icone>
)

export const IconeGrille = (p: Props) => (
  <Icone {...p}>
    <rect x="3" y="3" width="7.5" height="7.5" rx="2" />
    <rect x="13.5" y="3" width="7.5" height="7.5" rx="2" />
    <rect x="3" y="13.5" width="7.5" height="7.5" rx="2" />
    <rect x="13.5" y="13.5" width="7.5" height="7.5" rx="2" />
  </Icone>
)

export const IconeListe = (p: Props) => (
  <Icone {...p}>
    <path d="M8 6h13M8 12h13M8 18h13M3.5 6h.01M3.5 12h.01M3.5 18h.01" />
  </Icone>
)

export const IconeTri = (p: Props) => (
  <Icone {...p}>
    <path d="M7 4v16M3.5 16.5 7 20l3.5-3.5M17 20V4M13.5 7.5 17 4l3.5 3.5" />
  </Icone>
)

export const IconeOeil = (p: Props) => (
  <Icone {...p}>
    <path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12z" />
    <circle cx="12" cy="12" r="3" />
  </Icone>
)

export const IconeHorloge = (p: Props) => (
  <Icone {...p}>
    <circle cx="12" cy="12" r="9" />
    <path d="M12 7v5l3 2" />
  </Icone>
)

export const IconeCrayon = (p: Props) => (
  <Icone {...p}>
    <path d="M4 20h4L19 9a2.8 2.8 0 0 0-4-4L4 16v4z" />
    <path d="m13.5 6.5 4 4" />
  </Icone>
)

export const IconePoubelle = (p: Props) => (
  <Icone {...p}>
    <path d="M4 7h16" />
    <path d="M10 11v6M14 11v6" />
    <path d="M6 7l1 12a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2l1-12" />
    <path d="M9 7V4h6v3" />
  </Icone>
)

/** Consommer : une écuelle que l'on remplit */
export const IconeVerser = (p: Props) => (
  <Icone {...p}>
    <path d="M3 13h18a9 9 0 0 1-18 0z" />
    <path d="M12 3v6M9 6l3 3 3-3" />
  </Icone>
)
