import type { Invoice } from '../types'

const pdfCurrency = new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  maximumFractionDigits: 0,
})

export async function downloadInvoicePdf(invoice: Invoice) {
  const { jsPDF } = await import('jspdf')
  const document = new jsPDF({ unit: 'mm', format: 'a4' })
  const pageWidth = document.internal.pageSize.getWidth()
  const pageHeight = document.internal.pageSize.getHeight()
  const margin = 18
  let y = 20

  const drawPageHeader = () => {
    document.setFillColor(215, 25, 32)
    document.roundedRect(margin, 15, 13, 13, 2, 2, 'F')
    document.setTextColor(255, 255, 255)
    document.setFont('helvetica', 'bold')
    document.setFontSize(15)
    document.text('N', margin + 6.5, 24.2, { align: 'center' })

    document.setTextColor(17, 17, 17)
    document.setFontSize(18)
    document.text('NEXUS', margin + 17, 24.5)
    document.setTextColor(215, 25, 32)
    document.text('POS', margin + 40.5, 24.5)

    document.setTextColor(215, 25, 32)
    document.setFontSize(18)
    document.text('FACTURA', pageWidth - margin, 21, { align: 'right' })
    document.setTextColor(70, 70, 70)
    document.setFont('helvetica', 'normal')
    document.setFontSize(10)
    document.text(invoice.number, pageWidth - margin, 27, { align: 'right' })
    document.setDrawColor(225, 225, 225)
    document.line(margin, 34, pageWidth - margin, 34)
    y = 43
  }

  const addPage = () => {
    document.addPage()
    drawPageHeader()
  }

  const ensureSpace = (requiredHeight: number) => {
    if (y + requiredHeight > pageHeight - 20) addPage()
  }

  drawPageHeader()
  document.setTextColor(120, 120, 120)
  document.setFontSize(8)
  document.text('FACTURADO A', margin, y)
  document.text('FECHA DE EMISIÓN', pageWidth - margin, y, { align: 'right' })
  y += 6
  document.setTextColor(17, 17, 17)
  document.setFont('helvetica', 'bold')
  document.setFontSize(11)
  document.text(invoice.customerName, margin, y)
  document.text(new Date(invoice.issuedAt).toLocaleDateString('es-CO'), pageWidth - margin, y, { align: 'right' })
  y += 5
  document.setFont('helvetica', 'normal')
  document.setFontSize(9)
  document.setTextColor(85, 85, 85)
  document.text(`Documento: ${invoice.customerDocument}`, margin, y)
  y += 12

  const columns = [margin, 112, 139, pageWidth - margin]
  const drawTableHeader = () => {
    document.setFillColor(247, 247, 247)
    document.rect(margin, y - 5, pageWidth - margin * 2, 9, 'F')
    document.setTextColor(90, 90, 90)
    document.setFont('helvetica', 'bold')
    document.setFontSize(8)
    document.text('PRODUCTO', columns[0], y)
    document.text('CANTIDAD', columns[1], y, { align: 'right' })
    document.text('PRECIO UNITARIO', columns[2], y, { align: 'right' })
    document.text('SUBTOTAL', columns[3], y, { align: 'right' })
    y += 8
  }

  drawTableHeader()
  for (const item of invoice.items) {
    const productLines = document.splitTextToSize(item.productName, 78) as string[]
    const rowHeight = Math.max(10, productLines.length * 4.5 + 3)
    if (y + rowHeight > pageHeight - 20) {
      addPage()
      drawTableHeader()
    }
    document.setTextColor(30, 30, 30)
    document.setFont('helvetica', 'normal')
    document.setFontSize(9)
    document.text(productLines, columns[0], y)
    document.text(String(item.quantity), columns[1], y, { align: 'right' })
    document.text(pdfCurrency.format(item.unitPrice), columns[2], y, { align: 'right' })
    document.text(pdfCurrency.format(item.subtotal), columns[3], y, { align: 'right' })
    y += rowHeight
    document.setDrawColor(235, 235, 235)
    document.line(margin, y - 4, pageWidth - margin, y - 4)
  }

  ensureSpace(45)
  y += 3
  const totalsLabelX = 128
  const totalsValueX = pageWidth - margin
  const totalLine = (label: string, value: number, emphasized = false) => {
    if (emphasized) {
      document.setDrawColor(30, 30, 30)
      document.line(totalsLabelX, y - 4, totalsValueX, y - 4)
      document.setFont('helvetica', 'bold')
      document.setFontSize(12)
    } else {
      document.setFont('helvetica', 'normal')
      document.setFontSize(9)
    }
    document.setTextColor(30, 30, 30)
    document.text(label, totalsLabelX, y)
    document.text(pdfCurrency.format(value), totalsValueX, y, { align: 'right' })
    y += emphasized ? 9 : 7
  }

  totalLine('Subtotal', invoice.subtotal)
  totalLine('IVA', invoice.tax)
  totalLine('Descuento', invoice.discount)
  totalLine('Total', invoice.total, true)

  document.setFont('helvetica', 'normal')
  document.setFontSize(8)
  document.setTextColor(125, 125, 125)
  document.text('Gracias por elegir NexusPOS · Tecnología que conecta contigo', pageWidth / 2, pageHeight - 12, { align: 'center' })

  const safeNumber = invoice.number.replace(/[^a-zA-Z0-9-_]/g, '_')
  document.save(`Factura-${safeNumber}.pdf`)
}
