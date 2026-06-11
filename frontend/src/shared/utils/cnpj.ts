/** Utilitários de CNPJ no frontend: máscara, normalização e validação de DV. */

export function onlyDigits(value: string): string {
  return (value ?? '').replace(/\D/g, '')
}

/** Aplica a máscara XX.XXX.XXX/XXXX-XX progressivamente (para inputs). */
export function maskCnpj(value: string): string {
  const d = onlyDigits(value).slice(0, 14)
  return d
    .replace(/^(\d{2})(\d)/, '$1.$2')
    .replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/\.(\d{3})(\d)/, '.$1/$2')
    .replace(/(\d{4})(\d)/, '$1-$2')
}

/** Formata um CNPJ completo (14 dígitos). Se incompleto, devolve o que houver. */
export function formatCnpj(value: string): string {
  return maskCnpj(value)
}

/** Valida os dois dígitos verificadores do CNPJ (mesmo algoritmo do backend). */
export function isValidCnpj(value: string): boolean {
  const d = onlyDigits(value)
  if (d.length !== 14) return false
  if (/^(\d)\1{13}$/.test(d)) return false

  const calc = (len: number) => {
    const pesos =
      len === 12
        ? [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]
        : [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]
    let soma = 0
    for (let i = 0; i < len; i++) soma += Number(d[i]) * pesos[i]
    const resto = soma % 11
    return resto < 2 ? 0 : 11 - resto
  }

  return calc(12) === Number(d[12]) && calc(13) === Number(d[13])
}
