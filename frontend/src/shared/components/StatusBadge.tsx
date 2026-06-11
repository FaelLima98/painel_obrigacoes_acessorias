import { Tag } from 'antd'
import {
  CheckCircleOutlined,
  ClockCircleOutlined,
  MinusCircleOutlined,
  WarningOutlined,
} from '@ant-design/icons'
import type { ReactNode } from 'react'
import {
  StatusObrigacao,
  statusColors,
  statusLabels,
} from '../types/enums'

const icons: Record<StatusObrigacao, ReactNode> = {
  [StatusObrigacao.Pendente]: <ClockCircleOutlined />,
  [StatusObrigacao.Atrasada]: <WarningOutlined />,
  [StatusObrigacao.Entregue]: <CheckCircleOutlined />,
  [StatusObrigacao.NaoAplicavel]: <MinusCircleOutlined />,
}

export function StatusBadge({ status }: { status: StatusObrigacao }) {
  return (
    <Tag color={statusColors[status]} icon={icons[status]}>
      {statusLabels[status]}
    </Tag>
  )
}
