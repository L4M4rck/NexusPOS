import { describe, expect, it } from 'vitest'
import type { Invoice } from '../types'
import { filterAndSortMovements } from './movements'

const movements: Invoice[] = [
  { id: 6, number: 'FV-2026-000006', customerName: 'Laura Gómez', customerDocument: '1001', issuedAt: '2026-09-03T12:00:00Z', subtotal: 100, tax: 19, discount: 0, total: 119, items: [] },
  { id: 12, number: 'FV-2026-000012', customerName: 'Carlos Rojas', customerDocument: '2002', issuedAt: '2026-09-04T12:00:00Z', subtotal: 300, tax: 57, discount: 0, total: 357, items: [] },
]

describe('filterAndSortMovements', () => {
  it('busca por nombre sin depender de mayúsculas o tildes', () => {
    expect(filterAndSortMovements(movements, 'laura gomez', 'date_desc')).toEqual([movements[0]])
  })

  it('busca por número de factura o documento', () => {
    expect(filterAndSortMovements(movements, 'FV-2026-000012', 'date_desc')).toEqual([movements[1]])
    expect(filterAndSortMovements(movements, '1001', 'date_desc')).toEqual([movements[0]])
  })

  it('ordena por fecha, identificador y precio', () => {
    expect(filterAndSortMovements(movements, '', 'date_desc').map((item) => item.id)).toEqual([12, 6])
    expect(filterAndSortMovements(movements, '', 'number_desc').map((item) => item.id)).toEqual([12, 6])
    expect(filterAndSortMovements(movements, '', 'total_asc').map((item) => item.id)).toEqual([6, 12])
  })
})
