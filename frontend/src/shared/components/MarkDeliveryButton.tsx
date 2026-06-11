import { useState } from 'react'
import { App, Button, DatePicker, Popover, Space } from 'antd'
import dayjs, { type Dayjs } from 'dayjs'
import { useCreateDelivery } from '../hooks/useDeliveries'

interface Props {
  empresaId: string
  tipo: number
  competenciaAno: number
  competenciaMes: number
}

/** Registra a entrega de uma obrigação via DatePicker inline (Popover). */
export function MarkDeliveryButton({ empresaId, tipo, competenciaAno, competenciaMes }: Props) {
  const [open, setOpen] = useState(false)
  const [date, setDate] = useState<Dayjs>(dayjs())
  const { message } = App.useApp()
  const createDelivery = useCreateDelivery()

  const confirm = () => {
    createDelivery.mutate(
      {
        empresaId,
        tipoObrigacao: tipo,
        competenciaAno,
        competenciaMes,
        dataEntrega: date.format('YYYY-MM-DD'),
      },
      {
        onSuccess: () => {
          message.success('Entrega registrada.')
          setOpen(false)
        },
        onError: () => message.error('Não foi possível registrar a entrega.'),
      },
    )
  }

  const content = (
    <Space direction="vertical">
      <DatePicker
        value={date}
        onChange={(d) => d && setDate(d)}
        allowClear={false}
        format="DD/MM/YYYY"
      />
      <Button type="primary" size="small" block loading={createDelivery.isPending} onClick={confirm}>
        Confirmar
      </Button>
    </Space>
  )

  return (
    <Popover
      open={open}
      onOpenChange={setOpen}
      trigger="click"
      title="Data de entrega"
      content={content}
      destroyOnHidden
    >
      <Button type="link" size="small">
        Marcar Entregue
      </Button>
    </Popover>
  )
}
