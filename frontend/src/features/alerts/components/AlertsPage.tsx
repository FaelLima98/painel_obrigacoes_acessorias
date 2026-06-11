import { useState } from 'react'
import {
  Alert as AntAlert,
  Badge,
  Card,
  Collapse,
  Empty,
  List,
  Space,
  Typography,
} from 'antd'
import { ClockCircleOutlined, WarningOutlined } from '@ant-design/icons'
import { RegimeBadge } from '../../../shared/components/RegimeBadge'
import { MarkDeliveryButton } from '../../../shared/components/MarkDeliveryButton'
import { CompanySelect } from '../../../shared/components/CompanySelect'
import { formatDate } from '../../../shared/utils/date'
import { StatusObrigacao } from '../../../shared/types/enums'
import { colors } from '../../../shared/theme/colors'
import { useAlerts } from '../hooks/useAlerts'
import type { Alert } from '../types'

const { Text } = Typography
const PAGE_SIZE = 10

function diasLabel(dias: number): string {
  if (dias < 0) return `Há ${Math.abs(dias)} dia(s)`
  if (dias === 0) return 'Vence hoje'
  return `Em ${dias} dia(s)`
}

function AlertItem({ alert, urgente }: { alert: Alert; urgente: boolean }) {
  const cor = alert.status === StatusObrigacao.Atrasada
    ? colors.danger
    : urgente
      ? colors.warning
      : undefined

  return (
    <List.Item
      actions={[
        <MarkDeliveryButton
          key="mark"
          empresaId={alert.empresaId}
          tipo={alert.tipo}
          competenciaAno={alert.competenciaAno}
          competenciaMes={alert.competenciaMes}
        />,
      ]}
    >
      <List.Item.Meta
        title={
          <Space wrap>
            <Text strong>{alert.empresaNome}</Text>
            <RegimeBadge regime={alert.regime} />
          </Space>
        }
        description={
          <Space wrap>
            <Text>{alert.nome}</Text>
            <Text type="secondary">·</Text>
            <Text type="secondary">vence {formatDate(alert.vencimento)}</Text>
            <Text type="secondary">·</Text>
            <Text style={{ color: cor, fontWeight: cor ? 600 : 400 }}>
              {diasLabel(alert.diasRestantes)}
            </Text>
          </Space>
        }
      />
    </List.Item>
  )
}

export function AlertsPage() {
  const [empresaId, setEmpresaId] = useState<string>()
  const { data, isLoading, isError } = useAlerts(30)

  const filtrados = (data ?? []).filter((a) => !empresaId || a.empresaId === empresaId)
  const atrasadas = filtrados.filter((a) => a.status === StatusObrigacao.Atrasada)
  const proximas = filtrados.filter((a) => a.status !== StatusObrigacao.Atrasada)

  if (isError) {
    return (
      <Card>
        <AntAlert type="error" showIcon message="Não foi possível carregar os alertas." />
      </Card>
    )
  }

  const items = [
    {
      key: 'atrasadas',
      label: (
        <Space>
          <WarningOutlined style={{ color: colors.danger }} />
          <Text strong style={{ color: colors.danger }}>
            Obrigações Atrasadas
          </Text>
          <Badge count={atrasadas.length} color={colors.danger} showZero />
        </Space>
      ),
      children:
        atrasadas.length === 0 ? (
          <Empty description="Nenhuma obrigação atrasada." />
        ) : (
          <List
            loading={isLoading}
            dataSource={atrasadas}
            pagination={atrasadas.length > PAGE_SIZE ? { pageSize: PAGE_SIZE, size: 'small' } : false}
            renderItem={(a) => <AlertItem alert={a} urgente={false} />}
          />
        ),
    },
    {
      key: 'proximas',
      label: (
        <Space>
          <ClockCircleOutlined style={{ color: colors.warning }} />
          <Text strong style={{ color: colors.warning }}>
            Vencendo em 30 dias
          </Text>
          <Badge count={proximas.length} color={colors.warning} showZero />
        </Space>
      ),
      children:
        proximas.length === 0 ? (
          <Empty description="Nenhuma obrigação vencendo nos próximos 30 dias." />
        ) : (
          <List
            loading={isLoading}
            dataSource={proximas}
            pagination={proximas.length > PAGE_SIZE ? { pageSize: PAGE_SIZE, size: 'small' } : false}
            renderItem={(a) => <AlertItem alert={a} urgente={a.diasRestantes < 7} />}
          />
        ),
    },
  ]

  return (
    <Card loading={isLoading && !data}>
      <Space style={{ marginBottom: 16 }} wrap>
        <CompanySelect
          style={{ minWidth: 280 }}
          placeholder="Filtrar por empresa"
          value={empresaId}
          onChange={setEmpresaId}
          allowClear
        />
      </Space>
      <Collapse items={items} />
    </Card>
  )
}
