import type { Periodicidade, StatusObrigacao } from '../../shared/types/enums'

export interface Obligation {
  tipo: number
  nome: string
  periodicidade: Periodicidade
  competenciaAno: number
  competenciaMes: number
  vencimento: string
  status: StatusObrigacao
  dataEntrega: string | null
  entregaId: string | null
}
