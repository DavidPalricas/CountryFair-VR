import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { fileURLToPath } from 'node:url'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],

  // O .env esta na raiz da WebApp (CountryFairWebApp/.env), partilhado com o servidor.
  envDir: fileURLToPath(new URL('..', import.meta.url)),

  // Por defeito o Vite so expoe as variaveis VITE_*; SERVER_PORT precisa do prefixo SERVER_.
  envPrefix: ['VITE_', 'SERVER_'],

  // Escuta em 0.0.0.0 para que o tablet do terapeuta, ligado ao hotspot do PC, consiga abrir
  // a pagina em http://<IP do hotspot>:5173 (por defeito o Vite so aceita localhost).
  server: { host: true },
  preview: { host: true },
})
