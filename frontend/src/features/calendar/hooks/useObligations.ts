import { useQuery } from '@tanstack/react-query'
import { apiClient } from '../../../shared/services/apiClient'
import type { Obligation } from '../types'

export function useObligations(
  empresaId: string | undefined,
  year: number,
  month: number,
) {
  return useQuery({
    queryKey: ['obligations', empresaId, year, month],
    queryFn: () =>
      apiClient
        .get<Obligation[]>('/obligations', { params: { empresaId, year, month } })
        .then((r) => r.data),
    enabled: Boolean(empresaId),
  })
}
