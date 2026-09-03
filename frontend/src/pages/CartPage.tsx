import { Minus, Plus, ShoppingBag, Trash2 } from 'lucide-react'
import { Link } from 'react-router-dom'
import { EmptyState } from '../components/AsyncState'
import { useCart } from '../features/cart/CartContext'
import { formatCop } from '../utils/format'

export function CartPage() {
  const { items, estimatedSubtotal, setQuantity, remove } = useCart()
  return <section className="page-container"><div className="section-heading"><div><span className="eyebrow">Tu selección</span><h1>Carrito de compras</h1></div></div>{items.length === 0 ? <><EmptyState message="Tu carrito está vacío." /><Link className="button button-primary empty-action" to="/catalogo">Ver catálogo</Link></> : <div className="cart-layout"><div className="cart-items">{items.map(({ product, quantity }) => <article className="cart-item" key={product.id}><img src={product.imageUrl ?? '/placeholder-product.svg'} alt={product.name} /><div><span>{product.categoryName}</span><h2>{product.name}</h2><strong>{formatCop(product.price)}</strong></div><div className="quantity-control"><button aria-label="Disminuir" onClick={() => setQuantity(product.id, quantity - 1)}><Minus /></button><span>{quantity}</span><button aria-label="Aumentar" disabled={quantity >= product.stock} onClick={() => setQuantity(product.id, quantity + 1)}><Plus /></button></div><strong>{formatCop(product.price * quantity)}</strong><button className="icon-danger" aria-label="Eliminar" onClick={() => remove(product.id)}><Trash2 /></button></article>)}</div><aside className="order-summary"><ShoppingBag /><h2>Resumen</h2><div><span>Subtotal estimado</span><strong>{formatCop(estimatedSubtotal)}</strong></div><p>Impuestos y total definitivo serán recalculados de forma segura por el servidor.</p><Link className="button button-primary button-full" to="/checkout">Continuar al pago</Link></aside></div>}</section>
}
