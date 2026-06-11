import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '../../../shared/services/apiClient'
import type { PagedResult } from '../../../shared/types/api'
import type { Company, CreateCompanyDto } from '../types'

export function useCompanies(page: number, pageSize: number, search?: string) {
  return useQuery({
    queryKey: ['companies', page, pageSize, search ?? ''],
    queryFn: () =>
      apiClient
        .get<PagedResult<Company>>('/companies', {
          params: { page, pageSize, search: search || undefined },
        })
        .then((r) => r.data),
  })
}

export function useCreateCompany() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateCompanyDto) =>
      apiClient.post<Company>('/companies', data).then((r) => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['companies'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}

export function useDeleteCompany() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => apiClient.delete(`/companies/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['companies'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
  })
}
