import { CheckCircle2, CreditCard, ShieldCheck } from 'lucide-react'
import { useState } from 'react'
import { Link, Navigate } from 'react-router-dom'
import { getApiError } from '../api/client'
import { salesApi } from '../api/services'
import { useCart } from '../features/cart/CartContext'
import type { Sale } from '../types'
import { formatCop } from '../utils/format'

export function CheckoutPage() {
  const { items, estimatedSubtotal, clear } = useCart()
  const [result, setResult] = useState<Sale | null>(null)
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [reject, setReject] = useState(false)
  if (!items.length && !result) return <Navigate to="/carrito" replace />
  if (result) return <section className="success-page"><CheckCircle2 /><span className="eyebrow">Pago confirmado</span><h1>¡Tu compra está lista!</h1><p>Generamos la factura <strong>{result.invoiceNumber}</strong>.</p><strong className="success-total">{formatCop(result.total)}</strong><div><Link className="button button-primary" to="/compras">Ver mis compras</Link><Link className="button button-secondary" to="/catalogo">Seguir comprando</Link></div></section>

  const checkout = async () => {
    setError(''); setSubmitting(true)
    try {
      const sale = await salesApi.checkout(items.map(({ product, quantity }) => ({ productId: product.id, quantity })), crypto.randomUUID(), reject ? 'mock-rejected' : 'mock-approved')
      setResult(sale); clear()
    } catch (reason) { setError(getApiError(reason)) } finally { setSubmitting(false) }
  }
  return <section className="page-container"><div className="section-heading"><div><span className="eyebrow">Compra segura</span><h1>Finalizar compra</h1></div></div><div className="checkout-layout"><div className="payment-card"><CreditCard /><h2>Pago simulado</h2><p>Para esta prueba técnica no solicitamos ni almacenamos datos de tarjeta. El gateway local permite validar ambos resultados.</p><label className="switch-row"><input type="checkbox" checked={reject} onChange={(event) => setReject(event.target.checked)} /><span>Simular pago rechazado</span></label><div className="security-note"><ShieldCheck />El monto real se calcula exclusivamente en backend.</div>{error && <p className="form-error" role="alert">{error}</p>}<button className="button button-primary button-full" disabled={submitting} onClick={checkout}>{submitting ? 'Procesando...' : 'Confirmar y pagar'}</button></div><aside className="order-summary"><h2>Tu pedido</h2>{items.map(({ product, quantity }) => <div key={product.id}><span>{quantity} × {product.name}</span><strong>{formatCop(product.price * quantity)}</strong></div>)}<hr /><div><span>Subtotal estimado</span><strong>{formatCop(estimatedSubtotal)}</strong></div><p>El total final con IVA aparecerá después de la validación del servidor.</p></aside></div></section>
}
