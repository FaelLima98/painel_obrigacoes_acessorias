import { Tag } from 'antd'
import {
  regimeColors,
  regimeLabels,
  type RegimeTributario,
} from '../types/enums'

export function RegimeBadge({ regime }: { regime: RegimeTributario }) {
  return <Tag color={regimeColors[regime]}>{regimeLabels[regime]}</Tag>
}
