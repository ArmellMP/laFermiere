// Application installable (PWA) : enregistrement du service worker et bouton « Installer ».
//
//   Android / ordinateur (Chrome, Edge) : le navigateur propose l'installation (événement
//   beforeinstallprompt) ; on le garde pour l'afficher au moment choisi par l'utilisateur.
//   iPhone / iPad (Safari) : pas d'événement ; on explique « Partager → Sur l'écran d'accueil ».
//
// L'installation n'est possible qu'en HTTPS (ou sur http://localhost) : sur l'adresse du PC
// en Wi-Fi (http://192.168…), le téléphone ouvre l'application mais ne propose pas de l'installer.
import { useEffect, useState } from 'react'

interface EvenementInstallation extends Event {
  prompt: () => Promise<void>
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed' }>
}

let invitation: EvenementInstallation | null = null
const ecouteurs = new Set<() => void>()
const prevenir = () => ecouteurs.forEach((e) => e())

// Écouté dès le chargement : l'événement peut arriver avant l'affichage de React
if (typeof window !== 'undefined') {
  window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault() // pas de bandeau automatique : le bouton du menu suffit
    invitation = e as EvenementInstallation
    prevenir()
  })
  window.addEventListener('appinstalled', () => {
    invitation = null
    prevenir()
  })
}

/** Enregistre le service worker (version de production seulement : en développement il gênerait Vite). */
export function enregistrerServiceWorker() {
  if (!import.meta.env.PROD || !('serviceWorker' in navigator)) return
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js').catch(() => undefined)
  })
}

/** Déjà ouverte comme une application (écran d'accueil), et non dans un onglet ? */
export function estInstallee(): boolean {
  return (
    window.matchMedia?.('(display-mode: standalone)').matches === true ||
    (navigator as Navigator & { standalone?: boolean }).standalone === true
  )
}

export function estIos(): boolean {
  const ua = navigator.userAgent
  return /iPhone|iPad|iPod/.test(ua) || (ua.includes('Macintosh') && navigator.maxTouchPoints > 1)
}

export function useInstallation() {
  const [, rafraichir] = useState(0)
  useEffect(() => {
    const ecouteur = () => rafraichir((n) => n + 1)
    ecouteurs.add(ecouteur)
    return () => {
      ecouteurs.delete(ecouteur)
    }
  }, [])

  const installee = estInstallee()
  return {
    /** Le navigateur sait installer l'application (Android, Chrome/Edge sur ordinateur) */
    installable: !installee && invitation !== null,
    /** iPhone / iPad pas encore installé : il faut expliquer la marche à suivre */
    instructionsIos: !installee && invitation === null && estIos(),
    installer: async () => {
      if (!invitation) return false
      await invitation.prompt()
      const { outcome } = await invitation.userChoice
      invitation = null
      prevenir()
      return outcome === 'accepted'
    },
  }
}
