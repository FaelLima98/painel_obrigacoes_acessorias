import { colors } from '../theme/colors'

/* Enums espelham os do backend. Usamos objetos `const` (em vez de `enum`)
   por compatibilidade com o modo erasableSyntaxOnly do TypeScript. */

export const RegimeTributario = {
  SimplesNacional: 1,
  LucroPresumido: 2,
  LucroReal: 3,
  ImunidadeIsencao: 4,
} as const
export type RegimeTributario =
  (typeof RegimeTributario)[keyof typeof RegimeTributario]

export const StatusObrigacao = {
  Pendente: 1,
  Atrasada: 2,
  Entregue: 3,
  NaoAplicavel: 4,
} as const
export type StatusObrigacao =
  (typeof StatusObrigacao)[keyof typeof StatusObrigacao]

export const Periodicidade = {
  Mensal: 1,
  Anual: 2,
} as const
export type Periodicidade = (typeof Periodicidade)[keyof typeof Periodicidade]

export const regimeLabels: Record<RegimeTributario, string> = {
  [RegimeTributario.SimplesNacional]: 'Simples Nacional',
  [RegimeTributario.LucroPresumido]: 'Lucro Presumido',
  [RegimeTributario.LucroReal]: 'Lucro Real',
  [RegimeTributario.ImunidadeIsencao]: 'Imunidade/Isenção',
}

export const regimeColors: Record<RegimeTributario, string> = {
  [RegimeTributario.SimplesNacional]: colors.accent,
  [RegimeTributario.LucroPresumido]: colors.primaryHover,
  [RegimeTributario.LucroReal]: colors.primary,
  [RegimeTributario.ImunidadeIsencao]: '#8C8C8C',
}

export const statusLabels: Record<StatusObrigacao, string> = {
  [StatusObrigacao.Pendente]: 'Pendente',
  [StatusObrigacao.Atrasada]: 'Atrasada',
  [StatusObrigacao.Entregue]: 'Entregue',
  [StatusObrigacao.NaoAplicavel]: 'Não Aplicável',
}

export const statusColors: Record<StatusObrigacao, string> = {
  [StatusObrigacao.Pendente]: colors.primaryHover,
  [StatusObrigacao.Atrasada]: colors.danger,
  [StatusObrigacao.Entregue]: colors.success,
  [StatusObrigacao.NaoAplicavel]: '#8C8C8C',
}

export const periodicidadeLabels: Record<Periodicidade, string> = {
  [Periodicidade.Mensal]: 'Mensal',
  [Periodicidade.Anual]: 'Anual',
}
