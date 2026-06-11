/** Utilitários genéricos de exportação (CSV imediato, PDF sob demanda). */

function triggerDownload(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  a.click()
  URL.revokeObjectURL(url)
}

/** Gera um CSV (separador `;` e BOM UTF-8 para abrir certo no Excel pt-BR). */
export function downloadCsv(filename: string, headers: string[], rows: string[][]) {
  const escape = (v: string) => `"${(v ?? '').replace(/"/g, '""')}"`
  const lines = [headers, ...rows].map((r) => r.map(escape).join(';'))
  const csv = '﻿' + lines.join('\r\n')
  triggerDownload(new Blob([csv], { type: 'text/csv;charset=utf-8;' }), filename)
}

/** Gera um PDF tabular. jsPDF é carregado sob demanda (dynamic import). */
export async function downloadPdf(
  filename: string,
  title: string,
  subtitle: string,
  headers: string[],
  rows: string[][],
) {
  const { jsPDF } = await import('jspdf')
  const autoTable = (await import('jspdf-autotable')).default

  const doc = new jsPDF()
  doc.setFontSize(14)
  doc.text(title, 14, 18)
  doc.setFontSize(10)
  doc.setTextColor(120)
  doc.text(subtitle, 14, 25)

  autoTable(doc, {
    startY: 30,
    head: [headers],
    body: rows,
    styles: { fontSize: 9 },
    headStyles: { fillColor: [21, 101, 192] }, // --primary
  })

  doc.save(filename)
}
