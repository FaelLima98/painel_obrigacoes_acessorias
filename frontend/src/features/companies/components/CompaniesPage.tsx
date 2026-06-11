import { useState } from 'react'
import { App, Button, Card, Popconfirm, Space, Table, Typography } from 'antd'
import type { TableProps } from 'antd'
import { PlusOutlined } from '@ant-design/icons'
import { formatCnpj } from '../../../shared/utils/cnpj'
import { RegimeBadge } from '../../../shared/components/RegimeBadge'
import { useCompanies, useDeleteCompany } from '../hooks/useCompanies'
import { CompanyFormModal } from './CompanyFormModal'
import type { Company } from '../types'

const PAGE_SIZE = 10

export function CompaniesPage() {
  const [page, setPage] = useState(1)
  const [modalOpen, setModalOpen] = useState(false)
  const { data, isLoading } = useCompanies(page, PAGE_SIZE)
  const deleteCompany = useDeleteCompany()
  const { message } = App.useApp()

  const handleDelete = (id: string) =>
    deleteCompany.mutate(id, {
      onSuccess: () => message.success('Empresa removida.'),
      onError: () => message.error('Não foi possível remover a empresa.'),
    })

  const columns: TableProps<Company>['columns'] = [
    { title: 'Nome Fantasia', dataIndex: 'nomeFantasia', key: 'nomeFantasia' },
    {
      title: 'CNPJ',
      dataIndex: 'cnpj',
      key: 'cnpj',
      render: (value: string) => formatCnpj(value),
    },
    {
      title: 'Regime',
      dataIndex: 'regime',
      key: 'regime',
      render: (_, record) => <RegimeBadge regime={record.regime} />,
    },
    {
      title: 'Ações',
      key: 'acoes',
      align: 'right',
      width: 130,
      render: (_, record) => (
        <Popconfirm
          title="Remover empresa?"
          description="As entregas vinculadas também serão removidas."
          okText="Remover"
          cancelText="Cancelar"
          okButtonProps={{ danger: true }}
          onConfirm={() => handleDelete(record.id)}
        >
          <Button danger size="small">
            Remover
          </Button>
        </Popconfirm>
      ),
    },
  ]

  return (
    <Card>
      <Space style={{ width: '100%', justifyContent: 'space-between', marginBottom: 16 }}>
        <Typography.Text type="secondary">{data?.total ?? 0} empresa(s)</Typography.Text>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalOpen(true)}>
          Nova Empresa
        </Button>
      </Space>

      <Table
        rowKey="id"
        columns={columns}
        dataSource={data?.items}
        loading={isLoading}
        pagination={{
          current: page,
          pageSize: PAGE_SIZE,
          total: data?.total ?? 0,
          onChange: setPage,
          showSizeChanger: false,
        }}
      />

      <CompanyFormModal open={modalOpen} onClose={() => setModalOpen(false)} />
    </Card>
  )
}
