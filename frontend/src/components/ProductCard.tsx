import { ShoppingCart } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { Product } from '../types'
import { formatCop } from '../utils/format'

export function ProductCard({ product, onAdd }: { product: Product; onAdd: (product: Product) => void }) {
  return (
    <article className="product-card">
      <Link to={`/catalogo/${product.id}`} className="product-preview-link" aria-label={`Ver ${product.name}`}>
        <div className="product-image-wrap">
          <img src={product.imageUrl ?? '/placeholder-product.svg'} alt={product.name} className="product-image" loading="lazy" />
          <span className="product-category">{product.categoryName}</span>
        </div>
        <h2 className="product-preview-name">{product.name}</h2>
      </Link>
      <div className="product-body">
        <span className="product-sku">{product.sku}</span>
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
