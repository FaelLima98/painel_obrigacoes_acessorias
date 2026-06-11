import { useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '../services/apiClient'
import type { CreateDeliveryDto } from '../types/api'

/** Invalida tudo que depende do estado das entregas. */
function invalidateRelated(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: ['obligations'] })
  queryClient.invalidateQueries({ queryKey: ['dashboard'] })
  queryClient.invalidateQueries({ queryKey: ['alerts'] })
}

export function useCreateDelivery() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateDeliveryDto) =>
      apiClient.post('/deliveries', data).then((r) => r.data),
    onSuccess: () => invalidateRelated(queryClient),
  })
}

export function useDeleteDelivery() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (entregaId: string) => apiClient.delete(`/deliveries/${entregaId}`),
    onSuccess: () => invalidateRelated(queryClient),
  })
}
