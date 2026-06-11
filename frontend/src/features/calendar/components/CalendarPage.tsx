import { useState } from 'react'
import {
  App,
  Button,
  Card,
  DatePicker,
  Dropdown,
  Empty,
  Popconfirm,
  Space,
  Tag,
  Typography,
} from 'antd'
import type { TableProps } from 'antd'
import { Table } from 'antd'
import { DownloadOutlined } from '@ant-design/icons'
import dayjs, { type Dayjs } from 'dayjs'
import { StatusBadge } from '../../../shared/components/StatusBadge'
import { CompanySelect } from '../../../shared/components/CompanySelect'
import { formatDate } from '../../../shared/utils/date'
import { downloadCsv, downloadPdf } from '../../../shared/utils/export'
import {
  Periodicidade,
  StatusObrigacao,
  periodicidadeLabels,
  statusLabels,
} from '../../../shared/types/enums'
import { colors } from '../../../shared/theme/colors'
import { MarkDeliveryButton } from '../../../shared/components/MarkDeliveryButton'
import { useDeleteDelivery } from '../../../shared/hooks/useDeliveries'
import { useObligations } from '../hooks/useObligations'
import type { Obligation } from '../types'

export function CalendarPage() {
  const [empresaId, setEmpresaId] = useState<string>()
  const [empresaNome, setEmpresaNome] = useState<string>()
  const [competencia, setCompetencia] = useState<Dayjs>(dayjs())
  const year = competencia.year()
  const month = competencia.month() + 1
  const competenciaLabel = competencia.format('MM/YYYY')

  const { message } = App.useApp()
  const { data, isLoading } = useObligations(empresaId, year, month)
  const deleteDelivery = useDeleteDelivery()

  const exportRows = (): { headers: string[]; rows: string[][] } => {
    const headers = [
      'Empresa',
      'Obrigação',
      'Periodicidade',
      'Competência',
      'Vencimento',
      'Status',
      'Data de Entrega',
    ]
    const rows = (data ?? []).map((o) => [
      empresaNome ?? '',
      o.nome,
      periodicidadeLabels[o.periodicidade],
      `${String(o.competenciaMes).padStart(2, '0')}/${o.competenciaAno}`,
      formatDate(o.vencimento),
      statusLabels[o.status],
      formatDate(o.dataEntrega),
    ])
    return { headers, rows }
  }

  const slug = (empresaNome ?? 'calendario')
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .replace(/[^a-zA-Z0-9]+/g, '-')
    .toLowerCase()

  const handleExport = async (formato: 'csv' | 'pdf') => {
    const { headers, rows } = exportRows()
    const base = `calendario_${slug}_${competencia.format('MM-YYYY')}`
    try {
      if (formato === 'csv') {
        downloadCsv(`${base}.csv`, headers, rows)
      } else {
        await downloadPdf(
          `${base}.pdf`,
          'Calendário de Obrigações',
          `${empresaNome ?? ''} — competência ${competenciaLabel}`,
          headers,
          rows,
        )
      }
    } catch {
      message.error('Não foi possível gerar o arquivo.')
    }
  }

  const handleUndo = (entregaId: string) =>
    deleteDelivery.mutate(entregaId, {
      onSuccess: () => message.success('Entrega desfeita.'),
      onError: () => message.error('Não foi possível desfazer a entrega.'),
    })

  const columns: TableProps<Obligation>['columns'] = [
    { title: 'Obrigação', dataIndex: 'nome', key: 'nome' },
    {
      title: 'Periodicidade',
      dataIndex: 'periodicidade',
      key: 'periodicidade',
      render: (p: Periodicidade) => (
        <Tag color={p === Periodicidade.Anual ? colors.accent : 'default'}>
          {periodicidadeLabels[p]}
        </Tag>
      ),
    },
    {
      title: 'Vencimento',
      dataIndex: 'vencimento',
      key: 'vencimento',
      render: (v: string, record) => (
        <span style={{ fontWeight: record.status === StatusObrigacao.Atrasada ? 700 : 400 }}>
          {formatDate(v)}
        </span>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: StatusObrigacao) => <StatusBadge status={status} />,
    },
    {
      title: 'Data de Entrega',
      dataIndex: 'dataEntrega',
      key: 'dataEntrega',
      render: (d: string | null) => formatDate(d),
    },
    {
      title: 'Ação',
      key: 'acao',
      align: 'right',
      width: 160,
      render: (_, record) => {
        if (record.status === StatusObrigacao.Entregue && record.entregaId) {
          return (
            <Popconfirm
              title="Desfazer entrega?"
              okText="Desfazer"
              cancelText="Cancelar"
              okButtonProps={{ danger: true }}
              onConfirm={() => handleUndo(record.entregaId!)}
            >
              <Button type="link" size="small" danger>
                Desfazer
              </Button>
            </Popconfirm>
          )
        }
        if (
          record.status === StatusObrigacao.Pendente ||
          record.status === StatusObrigacao.Atrasada
        ) {
          return (
            <MarkDeliveryButton
              empresaId={empresaId!}
              tipo={record.tipo}
              competenciaAno={record.competenciaAno}
              competenciaMes={record.competenciaMes}
            />
          )
        }
        return null
      },
    },
  ]

  return (
    <Card>
      <Space style={{ marginBottom: 16 }} wrap>
        <CompanySelect
          style={{ minWidth: 280 }}
          value={empresaId}
          onChange={(id, label) => {
            setEmpresaId(id)
            setEmpresaNome(label)
          }}
        />
        <DatePicker
          picker="month"
          value={competencia}
          onChange={(d) => d && setCompetencia(d)}
          allowClear={false}
          format="MM/YYYY"
        />
        <Dropdown
          disabled={!data || data.length === 0}
          menu={{
            items: [
              { key: 'csv', label: 'Exportar CSV' },
              { key: 'pdf', label: 'Exportar PDF' },
            ],
            onClick: ({ key }) => handleExport(key as 'csv' | 'pdf'),
          }}
        >
          <Button icon={<DownloadOutlined />}>Exportar</Button>
        </Dropdown>
      </Space>

      {!empresaId ? (
        <Empty description="Selecione uma empresa para visualizar as obrigações da competência." />
      ) : (
        <Table
          rowKey={(r) => `${r.tipo}-${r.competenciaAno}-${r.competenciaMes}`}
          columns={columns}
          dataSource={data}
          loading={isLoading}
          pagination={false}
          rowClassName={(r) => (r.status === StatusObrigacao.Atrasada ? 'row-atrasada' : '')}
        />
      )}

      {empresaId && !isLoading && data?.length === 0 && (
        <Typography.Text type="secondary">
          Nenhuma obrigação para esta competência.
        </Typography.Text>
      )}
    </Card>
  )
}
