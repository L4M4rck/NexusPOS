import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Pencil, Plus, Power, Search } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { getApiError } from '../../api/client'
import { catalogApi } from '../../api/services'
import { ErrorState, LoadingState } from '../../components/AsyncState'
import type { Product } from '../../types'
import { formatCop } from '../../utils/format'

type ProductInput = { sku: string; name: string; description: string; price: number; stock: number; categoryId: number; imageUrl: string | null }

export function AdminProductsPage() {
  const [search, setSearch] = useState('')
  const [editing, setEditing] = useState<Product | null | undefined>(undefined)
  const [error, setError] = useState('')
  const queryClient = useQueryClient()
  const products = useQuery({ queryKey: ['admin-products', search], queryFn: () => catalogApi.products({ page: 1, pageSize: 50, search, includeInactive: 'true' }) })
  const categories = useQuery({ queryKey: ['admin-categories'], queryFn: () => catalogApi.categories(true) })
  const save = useMutation({
    mutationFn: async (data: ProductInput) => editing ? catalogApi.updateProduct(editing.id, data) : catalogApi.createProduct(data),
    onSuccess: async () => { await queryClient.invalidateQueries({ queryKey: ['admin-products'] }); setEditing(undefined); setError('') },
    onError: (reason) => setError(getApiError(reason)),
  })
  const toggle = useMutation({ mutationFn: ({ id, active }: { id: number; active: boolean }) => catalogApi.setProductStatus(id, active), onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-products'] }) })
  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const form = new FormData(event.currentTarget)
    save.mutate({ sku: String(form.get('sku')), name: String(form.get('name')), description: String(form.get('description')), price: Number(form.get('price')), stock: Number(form.get('stock')), categoryId: Number(form.get('categoryId')), imageUrl: String(form.get('imageUrl')) || null })
  }
  return <><div className="admin-heading"><div><span className="eyebrow">Catálogo</span><h1>Productos</h1><p>Crea, actualiza y desactiva productos sin perder historia.</p></div><button className="button button-primary" onClick={() => setEditing(null)}><Plus />Nuevo producto</button></div><div className="table-toolbar"><label className="search-field"><Search /><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Buscar producto o SKU" /></label></div>{products.isLoading && <LoadingState />}{products.isError && <ErrorState message="No fue posible cargar los productos." />}<div className="table-wrap"><table className="data-table"><thead><tr><th>Producto</th><th>SKU</th><th>Categoría</th><th>Precio</th><th>Stock</th><th>Estado</th><th>Acciones</th></tr></thead><tbody>{products.data?.items.map((product) => <tr key={product.id}><td><div className="table-product"><img src={product.imageUrl ?? '/placeholder-product.svg'} alt="" /><strong>{product.name}</strong></div></td><td>{product.sku}</td><td>{product.categoryName}</td><td>{formatCop(product.price)}</td><td><span className={product.stock <= 5 ? 'badge badge-warning' : 'badge'}>{product.stock}</span></td><td><span className={product.isActive ? 'badge badge-success' : 'badge badge-muted'}>{product.isActive ? 'Activo' : 'Inactivo'}</span></td><td><div className="table-actions"><button title="Editar" onClick={() => setEditing(product)}><Pencil /></button><button title={product.isActive ? 'Desactivar' : 'Activar'} onClick={() => toggle.mutate({ id: product.id, active: !product.isActive })}><Power /></button></div></td></tr>)}</tbody></table></div>{editing !== undefined && <div className="modal-backdrop" onMouseDown={() => setEditing(undefined)}><form className="modal" onSubmit={submit} onMouseDown={(event) => event.stopPropagation()}><div><h2>{editing ? 'Editar producto' : 'Nuevo producto'}</h2><button type="button" onClick={() => setEditing(undefined)}>×</button></div><div className="form-row"><label>SKU<input name="sku" defaultValue={editing?.sku} required /></label><label>Nombre<input name="name" defaultValue={editing?.name} required /></label></div><label>Descripción<textarea name="description" defaultValue={editing?.description} required /></label><div className="form-row"><label>Precio COP<input name="price" type="number" min="1" defaultValue={editing?.price} required /></label><label>Stock<input name="stock" type="number" min="0" defaultValue={editing?.stock} required /></label></div><label>Categoría<select name="categoryId" defaultValue={editing?.categoryId || ''} required><option value="" disabled>Seleccionar</option>{categories.data?.filter((category) => category.isActive).map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select></label><label>URL de imagen<input name="imageUrl" type="url" defaultValue={editing?.imageUrl ?? ''} /></label>{error && <p className="form-error">{error}</p>}<div className="modal-actions"><button type="button" className="button button-secondary" onClick={() => setEditing(undefined)}>Cancelar</button><button className="button button-primary" disabled={save.isPending}>{save.isPending ? 'Guardando...' : 'Guardar producto'}</button></div></form></div>}</>
}
