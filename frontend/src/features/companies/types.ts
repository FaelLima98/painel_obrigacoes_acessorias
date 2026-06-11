import type { RegimeTributario } from '../../shared/types/enums'

export interface Company {
  id: string
  nomeFantasia: string
  cnpj: string
  regime: RegimeTributario
  createdAt: string
}

export interface CreateCompanyDto {
  nomeFantasia: string
  cnpj: string
  regime: RegimeTributario
}
