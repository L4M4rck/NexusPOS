import { useQuery } from '@tanstack/react-query'
import { Download, Eye } from 'lucide-react'
import { Link } from 'react-router-dom'
import { adminApi, catalogApi, salesApi } from '../../api/services'
import { EmptyState, ErrorState, LoadingState } from '../../components/AsyncState'
import { formatCop, formatDate } from '../../utils/format'
import { downloadInvoicePdf } from '../../utils/invoicePdf'

export function AdminInventoryPage() {
  const query = useQuery({ queryKey: ['inventory'], queryFn: () => catalogApi.products({ page: 1, pageSize: 50, includeInactive: 'true', sort: 'name_asc' }) })
  return <AdminTablePage title="Inventario" eyebrow="Existencias" description="Disponibilidad actual validada por el servidor." loading={query.isLoading} error={query.isError}><table className="data-table"><thead><tr><th>SKU</th><th>Producto</th><th>Categoría</th><th>Stock</th><th>Estado</th></tr></thead><tbody>{query.data?.items.map((item) => <tr key={item.id}><td>{item.sku}</td><td><strong>{item.name}</strong></td><td>{item.categoryName}</td><td><span className={item.stock <= 5 ? 'badge badge-warning' : 'badge badge-success'}>{item.stock} und.</span></td><td>{item.isActive ? 'Activo' : 'Inactivo'}</td></tr>)}</tbody></table></AdminTablePage>
}

export function AdminCustomersPage() {
  const query = useQuery({ queryKey: ['customers'], queryFn: adminApi.customers })
  return <AdminTablePage title="Clientes" eyebrow="Relaciones" description="Perfiles registrados en NexusPOS." loading={query.isLoading} error={query.isError}><table className="data-table"><thead><tr><th>Cliente</th><th>Documento</th><th>Contacto</th><th>Ciudad / Dirección</th><th>Registro</th></tr></thead><tbody>{query.data?.map((item) => <tr key={item.id}><td><strong>{item.firstName} {item.lastName}</strong><small>{item.email}</small></td><td>{item.documentNumber}</td><td>{item.phone ?? '—'}</td><td>{item.address ?? '—'}</td><td>{formatDate(item.createdAt)}</td></tr>)}</tbody></table></AdminTablePage>
}

export function AdminSalesPage() {
  const query = useQuery({ queryKey: ['admin-sales'], queryFn: salesApi.sales })
  return <AdminTablePage title="Ventas" eyebrow="Operación" description="Histórico consolidado de ventas." loading={query.isLoading} error={query.isError}><table className="data-table"><thead><tr><th>Factura</th><th>Fecha</th><th>Productos</th><th>Estado</th><th>Total</th></tr></thead><tbody>{query.data?.map((item) => <tr key={item.id}><td><strong>{item.invoiceNumber}</strong></td><td>{formatDate(item.createdAt)}</td><td>{item.items.reduce((sum, line) => sum + line.quantity, 0)} unidades</td><td><span className="badge badge-success">{item.status}</span></td><td><strong>{formatCop(item.total)}</strong></td></tr>)}</tbody></table></AdminTablePage>
}

export function AdminInvoicesPage() {
  const query = useQuery({ queryKey: ['admin-invoices'], queryFn: salesApi.invoices })
  return <AdminTablePage title="Facturas" eyebrow="Facturación" description="Consulta y descarga los documentos emitidos automáticamente." loading={query.isLoading} error={query.isError}><table className="data-table"><thead><tr><th>Número</th><th>Cliente</th><th>Documento</th><th>Emisión</th><th>Total</th><th>Acciones</th></tr></thead><tbody>{query.data?.map((item) => <tr key={item.id}><td><strong>{item.number}</strong></td><td>{item.customerName}</td><td>{item.customerDocument}</td><td>{formatDate(item.issuedAt)}</td><td><strong>{formatCop(item.total)}</strong></td><td><div className="table-actions"><Link to={`/admin/facturas/${item.id}`} title="Ver factura" aria-label={`Ver factura ${item.number}`}><Eye /></Link><button type="button" title="Descargar PDF" aria-label={`Descargar factura ${item.number}`} onClick={() => void downloadInvoicePdf(item)}><Download /></button></div></td></tr>)}</tbody></table></AdminTablePage>
}

function AdminTablePage({ title, eyebrow, description, loading, error, children }: { title: string; eyebrow: string; description: string; loading: boolean; error: boolean; children: React.ReactNode }) {
  return <><div className="admin-heading"><div><span className="eyebrow">{eyebrow}</span><h1>{title}</h1><p>{description}</p></div></div>{loading && <LoadingState />}{error && <ErrorState message={`No fue posible cargar ${title.toLowerCase()}.`} />}{!loading && !error && <div className="table-wrap">{children || <EmptyState message="No hay información registrada." />}</div>}</>
}
