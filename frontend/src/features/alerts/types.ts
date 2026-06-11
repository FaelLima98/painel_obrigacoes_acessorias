import type { RegimeTributario, StatusObrigacao } from '../../shared/types/enums'

export interface Alert {
  empresaId: string
  empresaNome: string
  regime: RegimeTributario
  tipo: number
  nome: string
  competenciaAno: number
  competenciaMes: number
  vencimento: string
  status: StatusObrigacao
  diasRestantes: number
}
