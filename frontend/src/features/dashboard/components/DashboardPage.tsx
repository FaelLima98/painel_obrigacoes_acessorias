import { useState, type ReactNode } from 'react'
import { Alert, Card, Col, DatePicker, Row, Skeleton, Statistic } from 'antd'
import {
  BankOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  FileTextOutlined,
  WarningOutlined,
} from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { colors } from '../../../shared/theme/colors'
import { useDashboard } from '../hooks/useDashboard'

interface Kpi {
  title: string
  value: number
  color: string
  icon: ReactNode
}

export function DashboardPage() {
  const [competencia, setCompetencia] = useState<Dayjs>(dayjs())
  const year = competencia.year()
  const month = competencia.month() + 1

  const { data, isLoading, isError } = useDashboard(year, month)

  const kpis: Kpi[] = [
    { title: 'Total de Empresas', value: data?.totalEmpresas ?? 0, color: colors.primary, icon: <BankOutlined /> },
    { title: 'Obrigações do Mês', value: data?.obrigacoesMes ?? 0, color: colors.accent, icon: <FileTextOutlined /> },
    { title: 'Pendentes', value: data?.pendentes ?? 0, color: colors.primaryHover, icon: <ClockCircleOutlined /> },
    { title: 'Entregues', value: data?.entregues ?? 0, color: colors.success, icon: <CheckCircleOutlined /> },
    { title: 'Atrasadas', value: data?.atrasadas ?? 0, color: colors.danger, icon: <WarningOutlined /> },
  ]

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <DatePicker
          picker="month"
          value={competencia}
          onChange={(d) => d && setCompetencia(d)}
          allowClear={false}
          format="MM/YYYY"
        />
      </div>

      {isError && (
        <Alert
          type="error"
          showIcon
          message="Não foi possível carregar os indicadores."
          style={{ marginBottom: 16 }}
        />
      )}

      <Row gutter={[16, 16]}>
        {kpis.map((kpi) => (
          <Col key={kpi.title} xs={24} sm={12} lg={8} xl={Math.floor(24 / 5) || 4}>
            <Card>
              {isLoading ? (
                <Skeleton active paragraph={false} title={{ width: '60%' }} />
              ) : (
                <Statistic
                  title={kpi.title}
                  value={kpi.value}
                  valueStyle={{ color: kpi.color }}
                  prefix={<span style={{ color: kpi.color }}>{kpi.icon}</span>}
                />
              )}
            </Card>
          </Col>
        ))}
      </Row>
    </div>
  )
}
