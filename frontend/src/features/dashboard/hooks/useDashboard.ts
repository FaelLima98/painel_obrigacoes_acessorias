import { useQuery } from '@tanstack/react-query'
import { apiClient } from '../../../shared/services/apiClient'
import type { DashboardData } from '../types'

export function useDashboard(year: number, month: number) {
  return useQuery({
    queryKey: ['dashboard', year, month],
    queryFn: () =>
      apiClient
        .get<DashboardData>('/dashboard', { params: { year, month } })
        .then((r) => r.data),
  })
}
