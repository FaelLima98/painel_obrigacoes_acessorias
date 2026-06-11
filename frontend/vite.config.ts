import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    // O chunk do Ant Design é grande por natureza; isolado em vendor próprio
    // (cacheável). As páginas são carregadas sob demanda (React.lazy).
    chunkSizeWarningLimit: 1300,
    rollupOptions: {
      output: {
        // Separa as libs grandes em chunks de vendor para melhor cache
        manualChunks(id) {
          if (!id.includes('node_modules')) return
          // Apenas as libs grandes e sempre-usadas ganham chunk fixo (bom cache).
          // O resto fica a cargo do rolldown, que mantém deps de import dinâmico
          // (ex.: jsPDF) em chunks assíncronos — carregados só quando exportar PDF.
          if (id.includes('antd') || id.includes('@ant-design') || id.includes('rc-'))
            return 'antd'
          if (id.includes('react') || id.includes('scheduler')) return 'react'
          return
        },
      },
    },
  },
})
