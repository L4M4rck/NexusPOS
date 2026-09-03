import { useQuery } from '@tanstack/react-query'
import { FileText } from 'lucide-react'
import { Link } from 'react-router-dom'
import { salesApi } from '../api/services'
import { EmptyState, ErrorState, LoadingState } from '../components/AsyncState'
import { formatCop, formatDate } from '../utils/format'

export function PurchasesPage() {
  const query = useQuery({ queryKey: ['sales'], queryFn: salesApi.sales })
  return <section className="page-container"><div className="section-heading"><div><span className="eyebrow">Tu historial</span><h1>Mis compras</h1></div></div>{query.isLoading && <LoadingState />}{query.isError && <ErrorState message="No fue posible cargar tus compras." />}{query.data?.length === 0 && <EmptyState message="Aún no tienes compras registradas." />}<div className="orders-list">{query.data?.map((sale) => <article key={sale.id}><div><span>{sale.invoiceNumber}</span><h2>{formatDate(sale.createdAt)}</h2><p>{sale.items.length} productos · {sale.status}</p></div><strong>{formatCop(sale.total)}</strong><Link className="button button-secondary" to={`/facturas/${sale.invoiceId}`}><FileText />Ver y descargar</Link></article>)}</div></section>
}
