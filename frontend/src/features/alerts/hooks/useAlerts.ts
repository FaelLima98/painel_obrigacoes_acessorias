import { useQuery } from '@tanstack/react-query'
import { apiClient } from '../../../shared/services/apiClient'
import type { Alert } from '../types'

export function useAlerts(diasAdiante = 30) {
  return useQuery({
    queryKey: ['alerts', diasAdiante],
    queryFn: () =>
      apiClient
        .get<Alert[]>('/alerts', { params: { diasAdiante } })
        .then((r) => r.data),
  })
}
