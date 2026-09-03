export type Role = 'Guest' | 'Customer' | 'Admin'

export interface AuthResponse {
  accessToken: string
  expiresAt: string
  role: Role
  displayName: string | null
}

export interface Product {
  id: number
  sku: string
  name: string
  description: string
  price: number
  stock: number
  categoryId: number
  categoryName: string
  imageUrl: string | null
  isActive: boolean
}

export interface Category {
  id: number
  name: string
  description: string | null
  imageUrl: string | null
  isActive: boolean
}

export interface PagedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface CartItem {
  product: Product
  quantity: number
}

export interface SaleItem {
  productId: number
  productName: string
  quantity: number
  unitPrice: number
  subtotal: number
}

export interface Sale {
  id: number
  invoiceNumber: string
  invoiceId: number
  subtotal: number
  tax: number
  discount: number
  total: number
  status: string
  createdAt: string
  items: SaleItem[]
}

export interface Invoice {
  id: number
  number: string
  customerName: string
  customerDocument: string
  issuedAt: string
  subtotal: number
  tax: number
  discount: number
  total: number
  items: SaleItem[]
}

export interface Customer {
  id: number
  firstName: string
  lastName: string
  email: string
  documentNumber: string
  phone: string | null
  address: string | null
  isActive: boolean
  createdAt: string
}

export interface Dashboard {
  period: string
  revenue: number
  salesCount: number
  customersCount: number
  unitsSold: number
  averageTicket: number
  series: { label: string; sales: number; revenue: number }[]
  topProducts: { name: string; units: number; revenue: number }[]
  lowStockProducts: { id: number; name: string; stock: number }[]
  recentSales: { id: number; invoiceNumber: string; customer: string; total: number; createdAt: string }[]
}
