import { ShoppingCart } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { Product } from '../types'
import { formatCop } from '../utils/format'

export function ProductCard({ product, onAdd }: { product: Product; onAdd: (product: Product) => void }) {
  return (
    <article className="product-card">
      <Link to={`/catalogo/${product.id}`} className="product-image-wrap" aria-label={`Ver ${product.name}`}>
        <img src={product.imageUrl ?? '/placeholder-product.svg'} alt={product.name} className="product-image" />
        <span className="product-category">{product.categoryName}</span>
      </Link>
      <div className="product-body">
        <span className="product-sku">{product.sku}</span>
        <Link to={`/catalogo/${product.id}`}><h2>{product.name}</h2></Link>
        <p className="product-description">{product.description}</p>
        <div className="product-meta">
          <strong>{formatCop(product.price)}</strong>
          <span className={product.stock > 5 ? 'stock-ok' : 'stock-low'}>
            {product.stock > 0 ? `${product.stock} disponibles` : 'Agotado'}
          </span>
        </div>
        <button className="button button-primary button-full" disabled={product.stock === 0} onClick={() => onAdd(product)}>
          <ShoppingCart size={18} /> Agregar al carrito
        </button>
      </div>
    </article>
  )
}
