import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    host: '0.0.0.0',
    port: process.env.PORT ? parseInt(process.env.PORT) : 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: process.env.BACKEND_URL || 'https://localhost:7254',
        changeOrigin: true,
        secure: false,
      }
    }
  },
  preview: {
    host: '0.0.0.0',
    port: process.env.PORT ? parseInt(process.env.PORT) : 5173,
    strictPort: true,
  }
})
