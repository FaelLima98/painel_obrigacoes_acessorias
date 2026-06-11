import { App, Form, Input, Modal, Select } from 'antd'
import type { AxiosError } from 'axios'
import { isValidCnpj, maskCnpj, onlyDigits } from '../../../shared/utils/cnpj'
import { regimeLabels, type RegimeTributario } from '../../../shared/types/enums'
import { useCreateCompany } from '../hooks/useCompanies'
import type { CreateCompanyDto } from '../types'

interface Props {
  open: boolean
  onClose: () => void
}

interface FormValues {
  nomeFantasia: string
  cnpj: string
  regime: RegimeTributario
}

export function CompanyFormModal({ open, onClose }: Props) {
  const [form] = Form.useForm<FormValues>()
  const { message } = App.useApp()
  const createCompany = useCreateCompany()

  const close = () => {
    form.resetFields()
    onClose()
  }

  const handleOk = async () => {
    const values = await form.validateFields()
    const dto: CreateCompanyDto = {
      nomeFantasia: values.nomeFantasia.trim(),
      cnpj: onlyDigits(values.cnpj),
      regime: values.regime,
    }

    createCompany.mutate(dto, {
      onSuccess: () => {
        message.success('Empresa cadastrada com sucesso.')
        close()
      },
      onError: (err) => {
        const ax = err as AxiosError<{ errors?: Record<string, string[]> }>
        if (ax.response?.status === 409) {
          message.error('CNPJ já cadastrado.')
        } else if (ax.response?.status === 400 && ax.response.data?.errors) {
          form.setFields(
            Object.entries(ax.response.data.errors).map(([name, errors]) => ({
              name: (name.charAt(0).toLowerCase() + name.slice(1)) as keyof FormValues,
              errors,
            })),
          )
        } else {
          message.error('Não foi possível cadastrar a empresa.')
        }
      },
    })
  }

  return (
    <Modal
      title="Nova Empresa"
      open={open}
      onOk={handleOk}
      onCancel={close}
      okText="Salvar"
      cancelText="Cancelar"
      confirmLoading={createCompany.isPending}
      destroyOnHidden
    >
      <Form form={form} layout="vertical" requiredMark>
        <Form.Item
          name="nomeFantasia"
          label="Nome Fantasia"
          rules={[{ required: true, message: 'Informe o nome fantasia.' }]}
        >
          <Input maxLength={200} placeholder="Ex.: Padaria Pão Quente" />
        </Form.Item>

        <Form.Item
          name="cnpj"
          label="CNPJ"
          normalize={maskCnpj}
          rules={[
            { required: true, message: 'Informe o CNPJ.' },
            {
              validator: (_, value: string) =>
                isValidCnpj(value ?? '')
                  ? Promise.resolve()
                  : Promise.reject(new Error('CNPJ inválido.')),
            },
          ]}
        >
          <Input placeholder="00.000.000/0000-00" inputMode="numeric" />
        </Form.Item>

        <Form.Item
          name="regime"
          label="Regime Tributário"
          rules={[{ required: true, message: 'Selecione o regime.' }]}
        >
          <Select
            placeholder="Selecione"
            options={Object.entries(regimeLabels).map(([value, label]) => ({
              value: Number(value),
              label,
            }))}
          />
        </Form.Item>
      </Form>
    </Modal>
  )
}
