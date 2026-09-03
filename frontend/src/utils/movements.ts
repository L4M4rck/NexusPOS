import type { Invoice } from '../types'

export type MovementSort = 'date_desc' | 'date_asc' | 'number_asc' | 'number_desc' | 'total_desc' | 'total_asc'

const invoiceNumberCollator = new Intl.Collator('es', { numeric: true, sensitivity: 'base' })

const normalizeSearchValue = (value: string) =>
  value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase('es').trim()

export function filterAndSortMovements(invoices: Invoice[], search: string, sort: MovementSort): Invoice[] {
  const normalizedSearch = normalizeSearchValue(search)
  const filtered = normalizedSearch
    ? invoices.filter((invoice) => normalizeSearchValue(
      `${invoice.customerName} ${invoice.customerDocument} ${invoice.number}`,
    ).includes(normalizedSearch))
    : invoices

  return [...filtered].sort((left, right) => {
    switch (sort) {
      case 'date_asc':
        return new Date(left.issuedAt).getTime() - new Date(right.issuedAt).getTime()
      case 'number_asc':
        return invoiceNumberCollator.compare(left.number, right.number)
      case 'number_desc':
        return invoiceNumberCollator.compare(right.number, left.number)
      case 'total_asc':
        return left.total - right.total
      case 'total_desc':
        return right.total - left.total
      default:
        return new Date(right.issuedAt).getTime() - new Date(left.issuedAt).getTime()
    }
  })
}
