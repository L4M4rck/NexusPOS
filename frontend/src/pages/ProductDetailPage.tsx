import { useQuery } from '@tanstack/react-query'
import { ArrowLeft, ShieldCheck, ShoppingCart, Truck } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import { catalogApi } from '../api/services'
import { ErrorState, LoadingState } from '../components/AsyncState'
import { useCart } from '../features/cart/CartContext'
import { formatCop } from '../utils/format'

export function ProductDetailPage() {
  const id = Number(useParams().id)
  const { add } = useCart()
  const query = useQuery({ queryKey: ['product', id], queryFn: () => catalogApi.product(id), enabled: Number.isFinite(id) })
  if (query.isLoading) return <LoadingState />
  if (!query.data || query.isError) return <ErrorState message="No fue posible cargar este producto." />
  const product = query.data
  return (
    <section className="detail-page">
      <Link to={`/catalogo/categoria/${product.categoryId}`} className="back-link"><ArrowLeft size={18} />Volver a {product.categoryName}</Link>
      <div className="detail-grid">
        <div className="detail-image"><img src={product.imageUrl ?? '/placeholder-product.svg'} alt={product.name} /></div>
        <div className="detail-copy"><span className="eyebrow">{product.categoryName} · {product.sku}</span><h1>{product.name}</h1><p>{product.description}</p><strong className="detail-price">{formatCop(product.price)}</strong><span className={product.stock > 5 ? 'stock-ok' : 'stock-low'}>{product.stock} unidades disponibles</span><button className="button button-primary button-large" disabled={!product.stock} onClick={() => add(product)}><ShoppingCart />Agregar al carrito</button><div className="detail-benefits"><span><Truck />Envío nacional</span><span><ShieldCheck />Compra protegida</span></div></div>
      </div>
    </section>
  )
}
