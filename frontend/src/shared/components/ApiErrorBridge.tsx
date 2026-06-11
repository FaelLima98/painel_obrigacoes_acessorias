import { useEffect } from 'react'
import { App } from 'antd'
import { setApiErrorNotifier } from '../services/apiClient'

/**
 * Conecta o interceptor global do axios ao notification do Ant Design.
 * Renderize uma vez dentro do <App> provider.
 */
export function ApiErrorBridge() {
  const { notification } = App.useApp()

  useEffect(() => {
    setApiErrorNotifier((message, description) =>
      notification.error({ message, description }),
    )
    return () => setApiErrorNotifier(null)
  }, [notification])

  return null
}
