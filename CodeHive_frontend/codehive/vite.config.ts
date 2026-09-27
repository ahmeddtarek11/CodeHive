import react from '@vitejs/plugin-react';
import path from 'path';
import { defineConfig } from 'vite';

export default defineConfig(() => {
  return {
    plugins: [react()],
    resolve: {
      alias: {
        '@': path.resolve(__dirname, '.'),
      },
    },
    server: {
      port: 3000,
      host: '0.0.0.0',
      proxy: {
        // REST API
        '/api': {
          target: 'http://localhost:5023',
          changeOrigin: true,
        },
        // SignalR WebSocket hubs
        '/hubs': {
          target: 'http://localhost:5023',
          changeOrigin: true,
          ws: true,          // Enable WebSocket proxying for SignalR
        },
      },
      hmr: process.env.DISABLE_HMR !== 'true',
      watch: process.env.DISABLE_HMR === 'true' ? null : {},
    },
  };
});
