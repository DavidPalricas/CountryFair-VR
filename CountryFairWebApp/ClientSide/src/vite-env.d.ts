/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Colyseus server port, read from the shared CountryFairWebApp/.env */
  readonly SERVER_PORT: string
  /** Optional full Colyseus URL; overrides the page-hostname default in network/client.ts */
  readonly VITE_SERVER_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
