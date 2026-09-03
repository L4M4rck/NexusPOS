import { beforeEach, expect, it, vi } from 'vitest'
import type { Invoice } from '../types'
import { downloadInvoicePdf } from './invoicePdf'

const { documentMock, jsPdfMock } = vi.hoisted(() => {
  const mock = {
    internal: { pageSize: { getWidth: () => 210, getHeight: () => 297 } },
    addPage: vi.fn(),
    line: vi.fn(),
    rect: vi.fn(),
    roundedRect: vi.fn(),
    save: vi.fn(),
    setDrawColor: vi.fn(),
    setFillColor: vi.fn(),
    setFont: vi.fn(),
    setFontSize: vi.fn(),
    setTextColor: vi.fn(),
    splitTextToSize: vi.fn((value: string) => [value]),
    text: vi.fn(),
  }
  return { documentMock: mock, jsPdfMock: vi.fn(() => mock) }
})

vi.mock('jspdf', () => ({ jsPDF: jsPdfMock }))

beforeEach(() => vi.clearAllMocks())

it('genera una factura PDF descargable con IVA y un nombre seguro', async () => {
  const invoice: Invoice = {
    id: 1,
    number: 'FV/2026-000001',
    customerName: 'Laura Gómez',
    customerDocument: '1001001001',
    issuedAt: '2026-09-02T12:00:00Z',
    subtotal: 100_000,
    tax: 19_000,
    discount: 0,
    total: 119_000,
    items: [{ productId: 1, productName: 'Producto de prueba', quantity: 1, unitPrice: 100_000, subtotal: 100_000 }],
  }

  await downloadInvoicePdf(invoice)

  expect(jsPdfMock).toHaveBeenCalledOnce()
  expect(documentMock.text).toHaveBeenCalledWith('IVA', 128, expect.any(Number))
  expect(documentMock.save).toHaveBeenCalledWith('Factura-FV_2026-000001.pdf')
})
