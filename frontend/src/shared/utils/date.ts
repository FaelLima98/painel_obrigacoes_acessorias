import dayjs from 'dayjs'

/** Formata uma data ISO (YYYY-MM-DD) para DD/MM/YYYY. */
export function formatDate(iso?: string | null): string {
  return iso ? dayjs(iso).format('DD/MM/YYYY') : '—'
}
