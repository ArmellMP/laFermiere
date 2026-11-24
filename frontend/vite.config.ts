/// <reference types="vitest/config" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// En développement, Vite (port 5173) transmet les appels /api et /health à l'API .NET
// (profil "http" de launchSettings.json, port 5196). Le navigateur ne voit qu'une seule
// origine : pas de réglage CORS nécessaire.
// host: true rend le site accessible depuis un téléphone du même réseau Wi-Fi
// (http://<adresse-IP-du-PC>:5173).
export default defineConfig({
  plugins: [react()],
  server: {
    host: true,
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5196',
      '/health': 'http://localhost:5196',
    },
  },
  // `npm run preview` : version de production (avec le service worker), même renvoi vers l'API
  preview: {
    host: true,
    port: 4173,
    proxy: {
      '/api': 'http://localhost:5196',
      '/health': 'http://localhost:5196',
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: './src/test-setup.ts',
  },
})
