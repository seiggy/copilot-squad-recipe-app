import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vite.dev/config/
export default defineConfig(() => {
  // Aspire's AddViteApp() auto-injects PORT; fall back to Vite's default for standalone dev.
  const port = Number(process.env.PORT) || 5173;

  return {
    plugins: [react()],
    server: {
      port,
      // host: true binds 0.0.0.0 so Aspire (and containers) can reach the dev server.
      host: true,
      // Standalone-dev convenience: proxy /api to the backend when VITE_API_BASE_URL isn't set.
      // Under Aspire, the app reads VITE_API_BASE_URL directly and this proxy is effectively unused.
      proxy: {
        '/api': {
          target: process.env.VITE_API_BASE_URL || 'http://localhost:5000',
          changeOrigin: true,
        },
      },
    },
  };
});
