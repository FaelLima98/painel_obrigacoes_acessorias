import { useMemo, useState } from 'react'
import { Select, Spin } from 'antd'
import type { CSSProperties } from 'react'
import { useCompanies } from '../../features/companies/hooks/useCompanies'
import { useDebouncedValue } from '../hooks/useDebouncedValue'

interface Props {
  value?: string
  onChange: (id?: string, label?: string) => void
  placeholder?: string
  style?: CSSProperties
  allowClear?: boolean
}

const PAGE_SIZE = 20

/**
 * Select de empresa com busca server-side (debounced). Escala para milhares de
 * registros: busca no backend por nome/CNPJ e mantém o rótulo da empresa
 * selecionada mesmo quando ela sai da página de resultados atual.
 */
export function CompanySelect({
  value,
  onChange,
  placeholder = 'Selecione uma empresa',
  style,
  allowClear,
}: Props) {
  const [term, setTerm] = useState('')
  const debounced = useDebouncedValue(term, 300)
  const { data, isFetching } = useCompanies(1, PAGE_SIZE, debounced)
  const [selectedLabel, setSelectedLabel] = useState<string>()

  const options = useMemo(() => {
    const base = (data?.items ?? []).map((c) => ({ value: c.id, label: c.nomeFantasia }))
    // Garante que a opção selecionada apareça mesmo fora do resultado atual.
    if (value && selectedLabel && !base.some((o) => o.value === value)) {
      return [{ value, label: selectedLabel }, ...base]
    }
    return base
  }, [data, value, selectedLabel])

  return (
    <Select
      showSearch
      filterOption={false}
      onSearch={setTerm}
      loading={isFetching}
      value={value}
      placeholder={placeholder}
      style={style}
      allowClear={allowClear}
      options={options}
      notFoundContent={isFetching ? <Spin size="small" /> : null}
      onChange={(val, option) => {
        const opt = Array.isArray(option) ? option[0] : option
        setSelectedLabel(opt?.label)
        setTerm('')
        onChange(val, opt?.label)
      }}
    />
  )
}
