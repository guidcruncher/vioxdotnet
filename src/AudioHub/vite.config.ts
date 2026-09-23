import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'
import Components from 'unplugin-vue-components/vite'

export default defineConfig({
  plugins: [
    vue(),
    Components({
      // Auto register components from these directories
      dirs: ['src/components', 'src/layouts', 'src/views'],
      extensions: ['vue'],
      deep: true,
      dts: 'src/components.d.ts', // Generates TypeScript declarations automatically
    }),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  build: {
    outDir: 'wwwroot',
    emptyOutDir: true,
    chunkSizeWarningLimit: 900,
  },
  server: {
    port: 5173,
    strictPort: true,
  },
})
