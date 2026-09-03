import { apiClient } from './client'
import type { AuthResponse, Category, Customer, Dashboard, Invoice, PagedResponse, Product, Sale } from '../types'

export const authApi = {
  guest: async () => (await apiClient.post<AuthResponse>('/auth/guest')).data,
  login: async (email: string, password: string) =>
    (await apiClient.post<AuthResponse>('/auth/login', { email, password })).data,
  register: async (data: Record<string, string>) =>
    (await apiClient.post<AuthResponse>('/auth/register', data)).data,
}

export const catalogApi = {
  products: async (params: Record<string, string | number | undefined>) =>
    (await apiClient.get<PagedResponse<Product>>('/products', { params })).data,
  product: async (id: number) => (await apiClient.get<Product>(`/products/${id}`)).data,
  categories: async (includeInactive = false) =>
    (await apiClient.get<Category[]>('/categories', { params: { includeInactive } })).data,
  createProduct: async (data: Omit<Product, 'id' | 'categoryName' | 'isActive'>) =>
    (await apiClient.post<Product>('/products', data)).data,
  updateProduct: async (id: number, data: Omit<Product, 'id' | 'categoryName' | 'isActive'>) =>
    (await apiClient.put<Product>(`/products/${id}`, data)).data,
  setProductStatus: async (id: number, isActive: boolean) => apiClient.patch(`/products/${id}/status`, { isActive }),
  saveCategory: async (data: Omit<Category, 'id'>, id?: number) =>
    id ? (await apiClient.put<Category>(`/categories/${id}`, data)).data : (await apiClient.post<Category>('/categories', data)).data,
}

export const salesApi = {
  checkout: async (items: { productId: number; quantity: number }[], idempotencyKey: string, paymentMethod: string) =>
    (await apiClient.post<Sale>('/checkout', { items, idempotencyKey, paymentMethod })).data,
  sales: async () => (await apiClient.get<Sale[]>('/sales')).data,
  invoices: async () => (await apiClient.get<Invoice[]>('/invoices')).data,
  invoice: async (id: number) => (await apiClient.get<Invoice>(`/invoices/${id}`)).data,
}

export const adminApi = {
  dashboard: async (period: string) => (await apiClient.get<Dashboard>('/admin/dashboard', { params: { period } })).data,
  customers: async () => (await apiClient.get<Customer[]>('/customers')).data,
}
