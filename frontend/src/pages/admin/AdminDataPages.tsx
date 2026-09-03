import { useQuery } from '@tanstack/react-query'
import { Download, Eraser, Eye, Search, SlidersHorizontal } from 'lucide-react'
import { useDeferredValue, useState } from 'react'
import { Link } from 'react-router-dom'
import { adminApi, catalogApi, salesApi } from '../../api/services'
import { EmptyState, ErrorState, LoadingState } from '../../components/AsyncState'
import { formatCop, formatDate } from '../../utils/format'
import { downloadInvoicePdf } from '../../utils/invoicePdf'
import type { MovementSort } from '../../utils/movements'

const movementsPageSize = 5

export function AdminInventoryPage() {
  const query = useQuery({ queryKey: ['inventory'], queryFn: () => catalogApi.products({ page: 1, pageSize: 50, includeInactive: 'true', sort: 'name_asc' }) })
  return <AdminTablePage title="Inventario" eyebrow="Existencias" description="Disponibilidad actual validada por el servidor." loading={query.isLoading} error={query.isError}><table className="data-table"><thead><tr><th>SKU</th><th>Producto</th><th>Categoría</th><th>Stock</th><th>Estado</th></tr></thead><tbody>{query.data?.items.map((item) => <tr key={item.id}><td>{item.sku}</td><td><strong>{item.name}</strong></td><td>{item.categoryName}</td><td><span className={item.stock <= 5 ? 'badge badge-warning' : 'badge badge-success'}>{item.stock} und.</span></td><td>{item.isActive ? 'Activo' : 'Inactivo'}</td></tr>)}</tbody></table></AdminTablePage>
}

export function AdminCustomersPage() {
  const query = useQuery({ queryKey: ['customers'], queryFn: adminApi.customers })
  return <AdminTablePage title="Clientes" eyebrow="Relaciones" description="Perfiles registrados en NexusPOS." loading={query.isLoading} error={query.isError}><table className="data-table"><thead><tr><th>Cliente</th><th>Documento</th><th>Contacto</th><th>Ciudad / Dirección</th><th>Registro</th></tr></thead><tbody>{query.data?.map((item) => <tr key={item.id}><td><strong>{item.firstName} {item.lastName}</strong><small>{item.email}</small></td><td>{item.documentNumber}</td><td>{item.phone ?? '—'}</td><td>{item.address ?? '—'}</td><td>{formatDate(item.createdAt)}</td></tr>)}</tbody></table></AdminTablePage>
}

export function AdminMovementsPage() {
  const [search, setSearch] = useState('')
  const [sort, setSort] = useState<MovementSort>('date_desc')
  const [page, setPage] = useState(1)
  const deferredSearch = useDeferredValue(search.trim())
  const query = useQuery({
    queryKey: ['admin-movements', page, deferredSearch, sort],
    queryFn: () => salesApi.movements({ page, pageSize: movementsPageSize, search: deferredSearch || undefined, sort }),
    placeholderData: (previousData) => previousData,
  })
  const movements = query.data?.items ?? []
  const clearFilters = () => { setSearch(''); setSort('date_desc'); setPage(1) }
  const filtersActive = search.trim() !== '' || sort !== 'date_desc'
  const firstResult = query.data?.totalItems ? (query.data.page - 1) * query.data.pageSize + 1 : 0
  const lastResult = query.data?.totalItems ? firstResult + query.data.items.length - 1 : 0

  return <AdminTablePage title="Movimientos" eyebrow="Facturación" description="Consulta, organiza y descarga todos los movimientos facturados." loading={query.isLoading} error={query.isError} wrap={false}>
    <div className="movements-toolbar">
      <label className="search-field"><Search size={19} /><span className="sr-only">Buscar movimientos</span><input value={search} onChange={(event) => { setSearch(event.target.value); setPage(1) }} placeholder="Buscar por cliente, documento o FV-2026-000006..." /></label>
      <label className="movement-sort"><SlidersHorizontal size={18} /><span className="sr-only">Ordenar movimientos</span><select value={sort} onChange={(event) => { setSort(event.target.value as MovementSort); setPage(1) }}><option value="date_desc">Más recientes</option><option value="date_asc">Más antiguos</option><option value="number_asc">Identificador ascendente</option><option value="number_desc">Identificador descendente</option><option value="total_desc">Precio: mayor a menor</option><option value="total_asc">Precio: menor a mayor</option></select></label>
      <button type="button" className="button button-secondary clear-filters" disabled={!filtersActive} onClick={clearFilters}><Eraser size={18} />Limpiar filtros</button>
    </div>
    <div className="movements-summary"><span>Mostrando {firstResult}–{lastResult} de {query.data?.totalItems ?? 0} movimientos</span>{filtersActive && <small>Filtros activos</small>}</div>
    <div className="table-wrap"><table className="data-table"><thead><tr><th>Número</th><th>Cliente</th><th>Documento</th><th>Emisión</th><th>Total</th><th>Acciones</th></tr></thead><tbody>{movements.length > 0 ? movements.map((item) => <tr key={item.id}><td><strong>{item.number}</strong></td><td>{item.customerName}</td><td>{item.customerDocument}</td><td>{formatDate(item.issuedAt)}</td><td><strong>{formatCop(item.total)}</strong></td><td><div className="table-actions"><Link to={`/admin/movimientos/${item.id}`} title="Ver movimiento" aria-label={`Ver movimiento ${item.number}`}><Eye /></Link><button type="button" title="Descargar PDF" aria-label={`Descargar factura ${item.number}`} onClick={() => void downloadInvoicePdf(item)}><Download /></button></div></td></tr>) : <tr><td colSpan={6}><EmptyState message="No se encontraron movimientos con esos filtros." /></td></tr>}</tbody></table></div>
    {query.data && query.data.totalPages > 1 && <nav className="pagination movements-pagination" aria-label="Paginación de movimientos"><button disabled={!query.data.hasPreviousPage || query.isFetching} onClick={() => setPage(query.data.page - 1)}>Anterior</button><span>Página {query.data.page} de {query.data.totalPages}</span><button disabled={!query.data.hasNextPage || query.isFetching} onClick={() => setPage(query.data.page + 1)}>Siguiente</button></nav>}
  </AdminTablePage>
}

function AdminTablePage({ title, eyebrow, description, loading, error, children, wrap = true }: { title: string; eyebrow: string; description: string; loading: boolean; error: boolean; children: React.ReactNode; wrap?: boolean }) {
  return <><div className="admin-heading"><div><span className="eyebrow">{eyebrow}</span><h1>{title}</h1><p>{description}</p></div></div>{loading && <LoadingState />}{error && <ErrorState message={`No fue posible cargar ${title.toLowerCase()}.`} />}{!loading && !error && (wrap ? <div className="table-wrap">{children}</div> : children)}</>
}
