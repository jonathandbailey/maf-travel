import { defineConfig } from 'vite'
import { env } from 'process';
import react from '@vitejs/plugin-react'


export default defineConfig({
  plugins: [react()],
  server: {
    host: true,
    port: parseInt(env.VITE_PORT || '5173'),

    strictPort: true
  }
})
