// Service worker de La Fermière : rend l'application installable et accélère son ouverture.
//
// Ce qu'il garde en cache : les fichiers de l'application (pages, scripts, styles, icônes).
// Ce qu'il ne garde JAMAIS : les données (/api, /health). Elles viennent toujours du serveur,
// pour qu'un fermier ne voie jamais des chiffres périmés.
//
//   Pages (navigation)       : réseau d'abord (nouvelle version tout de suite), cache si hors ligne
//   /assets/* (nom avec hash) : cache d'abord (un fichier de ce nom ne change jamais)
//   Autres fichiers statiques : cache, mis à jour en arrière-plan
const CACHE = 'lafermiere-v1'
const MAX_FICHIERS = 80

self.addEventListener('install', (evenement) => {
  evenement.waitUntil(caches.open(CACHE).then((cache) => cache.addAll(['/', '/manifest.webmanifest', '/icone-192.png'])))
  self.skipWaiting()
})

self.addEventListener('activate', (evenement) => {
  evenement.waitUntil(
    caches
      .keys()
      .then((noms) => Promise.all(noms.filter((n) => n !== CACHE).map((n) => caches.delete(n))))
      .then(() => self.clients.claim()),
  )
})

self.addEventListener('fetch', (evenement) => {
  const requete = evenement.request
  const url = new URL(requete.url)
  if (requete.method !== 'GET' || url.origin !== self.location.origin) return
  if (url.pathname.startsWith('/api/') || url.pathname.startsWith('/health')) return

  if (requete.mode === 'navigate') {
    evenement.respondWith(reseauDAbord(requete))
  } else if (url.pathname.startsWith('/assets/')) {
    evenement.respondWith(cacheDAbord(requete))
  } else {
    evenement.respondWith(cachePuisMiseAJour(requete, evenement))
  }
})

async function reseauDAbord(requete) {
  const cache = await caches.open(CACHE)
  try {
    const reponse = await fetch(requete)
    if (reponse.ok) await cache.put('/', reponse.clone())
    return reponse
  } catch {
    // Hors ligne : l'application s'ouvre quand même (les écrans proposeront « Réessayer »)
    return (await cache.match('/')) ?? Response.error()
  }
}

async function cacheDAbord(requete) {
  const cache = await caches.open(CACHE)
  const enCache = await cache.match(requete)
  if (enCache) return enCache
  const reponse = await fetch(requete)
  if (reponse.ok) {
    await cache.put(requete, reponse.clone())
    await limiter(cache)
  }
  return reponse
}

async function cachePuisMiseAJour(requete, evenement) {
  const cache = await caches.open(CACHE)
  const enCache = await cache.match(requete)
  const miseAJour = fetch(requete)
    .then(async (reponse) => {
      if (reponse.ok) await cache.put(requete, reponse.clone())
      return reponse
    })
    .catch(() => undefined)
  if (enCache) {
    evenement.waitUntil(miseAJour)
    return enCache
  }
  return (await miseAJour) ?? Response.error()
}

/** Les anciennes versions des fichiers s'accumulent à chaque mise à jour : on garde les plus récents. */
async function limiter(cache) {
  const cles = await cache.keys()
  const assets = cles.filter((r) => new URL(r.url).pathname.startsWith('/assets/'))
  for (const ancienne of assets.slice(0, Math.max(0, assets.length - MAX_FICHIERS))) await cache.delete(ancienne)
}
