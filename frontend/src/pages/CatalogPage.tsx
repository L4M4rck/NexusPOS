import { useQuery } from '@tanstack/react-query'
import { ArrowLeft, ArrowRight, Search, SlidersHorizontal, Sparkles } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { catalogApi } from '../api/services'
import { EmptyState, ErrorState, LoadingState } from '../components/AsyncState'
import { ProductCard } from '../components/ProductCard'
import { useCart } from '../features/cart/CartContext'

export function CatalogPage({ home = false }: { home?: boolean }) {
  const categoryParam = useParams<{ categoryId: string }>().categoryId
  const categoryId = Number(categoryParam)
  const isCategoryView = Boolean(categoryParam)
  const [search, setSearch] = useState('')
  const [sort, setSort] = useState('name_asc')
  const [page, setPage] = useState(1)
  const { add } = useCart()
  const categories = useQuery({ queryKey: ['categories'], queryFn: () => catalogApi.categories() })
  const selectedCategory = categories.data?.find((category) => category.id === categoryId)
  const products = useQuery({
    queryKey: ['products', page, search, categoryId, sort],
    queryFn: () => catalogApi.products({ page, pageSize: 12, search: search || undefined, categoryId, sort }),
    enabled: isCategoryView && Number.isInteger(categoryId) && categoryId > 0,
  })

  useEffect(() => {
    setPage(1)
    setSearch('')
  }, [categoryId])

  return (
    <>
      {home && (
        <section className="hero">
          <div className="hero-copy">
            <span className="eyebrow"><Sparkles size={16} /> Tecnología elegida para ti</span>
            <h1>Tu setup.<br /><em>A otro nivel.</em></h1>
            <p>Periféricos y accesorios seleccionados para trabajar, crear y jugar mejor.</p>
            <a href="#catalogo" className="button button-primary">Explorar categorías</a>
          </div>
          <div className="hero-visual"><div className="hero-orbit" /><strong>NEXUS</strong><span>GEAR / 2026</span></div>
        </section>
      )}
      <section className="catalog-section" id="catalogo">
        {!isCategoryView ? (
          <>
            <div className="section-heading">
              <div><span className="eyebrow">Explora por categoría</span><h1>{home ? 'Encuentra tu próximo equipo' : 'Catálogo Nexus'}</h1></div>
              <p>Selecciona una categoría para ver sus productos</p>
            </div>
            {categories.isLoading && <LoadingState label="Preparando las categorías..." />}
            {categories.isError && <ErrorState message="No fue posible cargar las categorías." retry={() => categories.refetch()} />}
            {categories.data?.length === 0 && <EmptyState message="No hay categorías disponibles." />}
            <div className="category-showcase-grid">
              {categories.data?.map((category) => (
                <Link className="category-showcase-card" to={`/catalogo/categoria/${category.id}`} key={category.id}>
                  <img src={category.imageUrl ?? '/placeholder-product.svg'} alt={category.name} loading="lazy" />
                  <span className="category-showcase-overlay" />
                  <div><span>Explorar categoría</span><h2>{category.name}</h2><p>{category.description}</p><strong>Ver productos <ArrowRight /></strong></div>
                </Link>
              ))}
            </div>
          </>
        ) : (
          <>
            <Link to="/catalogo" className="back-link"><ArrowLeft size={18} />Todas las categorías</Link>
            {categories.isLoading && <LoadingState label="Cargando categoría..." />}
            {!categories.isLoading && !selectedCategory && <ErrorState message="La categoría solicitada no existe o no está disponible." />}
            {selectedCategory && (
              <>
                <div className="category-products-heading">
                  <img src={selectedCategory.imageUrl ?? '/placeholder-product.svg'} alt="" />
                  <div><span className="eyebrow">Categoría</span><h1>{selectedCategory.name}</h1><p>{selectedCategory.description}</p></div>
                  <strong>{products.data?.totalItems ?? 0} productos</strong>
                </div>
                <div className="catalog-toolbar">
                  <label className="search-field"><Search size={19} /><span className="sr-only">Buscar productos</span><input value={search} onChange={(event) => { setSearch(event.target.value); setPage(1) }} placeholder={`Buscar en ${selectedCategory.name}...`} /></label>
                  <label><SlidersHorizontal size={18} /><span className="sr-only">Ordenar productos</span><select value={sort} onChange={(event) => { setSort(event.target.value); setPage(1) }}><option value="name_asc">Nombre A–Z</option><option value="price_asc">Menor precio</option><option value="price_desc">Mayor precio</option><option value="newest">Más recientes</option></select></label>
                </div>
                {products.isLoading && <LoadingState label={`Preparando ${selectedCategory.name}...`} />}
                {products.isError && <ErrorState message="No fue posible cargar los productos." retry={() => products.refetch()} />}
                {products.data?.items.length === 0 && <EmptyState message="No se encontraron productos con esa búsqueda." />}
                <div className="product-grid">{products.data?.items.map((product) => <ProductCard key={product.id} product={product} onAdd={add} />)}</div>
                {products.data && products.data.totalPages > 1 && (
                  <nav className="pagination" aria-label="Paginación">
                    <button disabled={!products.data.hasPreviousPage} onClick={() => setPage(page - 1)}>Anterior</button>
                    <span>Página {page} de {products.data.totalPages}</span>
                    <button disabled={!products.data.hasNextPage} onClick={() => setPage(page + 1)}>Siguiente</button>
                  </nav>
                )}
              </>
            )}
          </>
        )}
      </section>
    </>
  )
}
