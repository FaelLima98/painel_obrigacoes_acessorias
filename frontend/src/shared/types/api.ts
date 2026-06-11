export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  total: number
}

export interface CreateDeliveryDto {
  empresaId: string
  tipoObrigacao: number
  competenciaAno: number
  competenciaMes: number
  dataEntrega: string
}
