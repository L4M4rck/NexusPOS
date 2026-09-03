import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Pencil, Plus } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { getApiError } from '../../api/client'
import { catalogApi } from '../../api/services'
import { ErrorState, LoadingState } from '../../components/AsyncState'
import type { Category } from '../../types'

export function AdminCategoriesPage() {
  const queryClient = useQueryClient()
  const [editing, setEditing] = useState<Category | null | undefined>(undefined)
  const [error, setError] = useState('')
  const query = useQuery({ queryKey: ['admin-categories'], queryFn: () => catalogApi.categories(true) })
  const save = useMutation({ mutationFn: ({ data, id }: { data: Omit<Category, 'id'>; id?: number }) => catalogApi.saveCategory(data, id), onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['admin-categories'] }); setEditing(undefined) }, onError: (reason) => setError(getApiError(reason)) })
  const submit = (event: FormEvent<HTMLFormElement>) => { event.preventDefault(); const form = new FormData(event.currentTarget); save.mutate({ id: editing?.id, data: { name: String(form.get('name')), description: String(form.get('description')), imageUrl: String(form.get('imageUrl')), isActive: form.get('isActive') === 'on' } }) }
  return <><div className="admin-heading"><div><span className="eyebrow">Organización</span><h1>Categorías</h1><p>Gestiona las secciones e imágenes visibles del catálogo.</p></div><button className="button button-primary" onClick={() => { setError(''); setEditing(null) }}><Plus />Nueva categoría</button></div>{query.isLoading && <LoadingState />}{query.isError && <ErrorState message="No fue posible cargar las categorías." />}<div className="category-grid">{query.data?.map((category) => <article className="panel admin-category-card" key={category.id}><img src={category.imageUrl ?? '/placeholder-product.svg'} alt={category.name} /><div><span className={category.isActive ? 'badge badge-success' : 'badge badge-muted'}>{category.isActive ? 'Activa' : 'Inactiva'}</span><h2>{category.name}</h2><p>{category.description || 'Sin descripción'}</p><button className="button button-secondary" onClick={() => { setError(''); setEditing(category) }}><Pencil />Editar</button></div></article>)}</div>{editing !== undefined && <div className="modal-backdrop" onMouseDown={() => setEditing(undefined)}><form className="modal modal-small" onSubmit={submit} onMouseDown={(event) => event.stopPropagation()}><div><h2>{editing ? 'Editar categoría' : 'Nueva categoría'}</h2><button type="button" onClick={() => setEditing(undefined)}>×</button></div><label>Nombre<input name="name" defaultValue={editing?.name} required /></label><label>Descripción<textarea name="description" defaultValue={editing?.description ?? ''} /></label><label>URL de imagen<input name="imageUrl" type="url" defaultValue={editing?.imageUrl ?? ''} placeholder="https://..." /></label><label className="switch-row"><input name="isActive" type="checkbox" defaultChecked={editing?.isActive ?? true} />Categoría activa</label>{error && <p className="form-error">{error}</p>}<button className="button button-primary button-full" disabled={save.isPending}>{save.isPending ? 'Guardando...' : 'Guardar categoría'}</button></form></div>}</>
}
