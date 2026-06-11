import axios from 'axios'

/**
 * Cliente HTTP central. A baseURL aponta para a API .NET; em dev e em Docker o
 * navegador acessa a API publicada em http://localhost:5000.
 */
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5000/api',
  headers: { 'Content-Type': 'application/json' },
})

/* Ponte para o sistema de notificações do Ant Design. O interceptor não pode usar
   hooks de React, então um componente registra o notificador via setApiErrorNotifier. */
type ApiErrorNotifier = (message: string, description?: string) => void
let notifier: ApiErrorNotifier | null = null

export function setApiErrorNotifier(fn: ApiErrorNotifier | null) {
  notifier = fn
}

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status as number | undefined
    // Notifica globalmente apenas falhas inesperadas (rede ou 5xx).
    // Erros de negócio 4xx são tratados pelas telas/mutations.
    if (notifier && (status === undefined || status >= 500)) {
      notifier(
        'Erro de comunicação com o servidor',
        status ? `O servidor respondeu com erro ${status}.` : 'Verifique sua conexão e tente novamente.',
      )
    }
    return Promise.reject(error)
  },
)
