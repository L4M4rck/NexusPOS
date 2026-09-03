import { CheckCircle2, CreditCard, LockKeyhole, ShieldCheck, Wifi } from 'lucide-react'
import { type FormEvent, useState } from 'react'
import { Link, Navigate } from 'react-router-dom'
import { getApiError } from '../api/client'
import { salesApi } from '../api/services'
import { useCart } from '../features/cart/CartContext'
import type { Sale } from '../types'
import { formatCop } from '../utils/format'
import { formatCardExpiry, formatCardNumber, getCardBrand, onlyDigits } from '../utils/paymentCard'

type CardType = 'credit' | 'debit'

export function CheckoutPage() {
  const { items, estimatedSubtotal, clear } = useCart()
  const [result, setResult] = useState<Sale | null>(null)
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [reject, setReject] = useState(false)
  const [cardType, setCardType] = useState<CardType>('credit')
  const [cardholder, setCardholder] = useState('')
  const [cardNumber, setCardNumber] = useState('')
  const [expiry, setExpiry] = useState('')
  const [cvv, setCvv] = useState('')
  const [installments, setInstallments] = useState('1')
  if (!items.length && !result) return <Navigate to="/carrito" replace />
  if (result) return <section className="success-page"><CheckCircle2 /><span className="eyebrow">Pago confirmado</span><h1>¡Tu compra está lista!</h1><p>Generamos la factura <strong>{result.invoiceNumber}</strong>.</p><strong className="success-total">{formatCop(result.total)}</strong><div><Link className="button button-primary" to="/compras">Ver mis compras</Link><Link className="button button-secondary" to="/catalogo">Seguir comprando</Link></div></section>

  const checkout = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError(''); setSubmitting(true)
    try {
      const sale = await salesApi.checkout(items.map(({ product, quantity }) => ({ productId: product.id, quantity })), crypto.randomUUID(), reject ? 'mock-rejected' : 'mock-approved')
      setResult(sale); clear()
    } catch (reason) { setError(getApiError(reason)) } finally { setSubmitting(false) }
  }

  const cardBrand = getCardBrand(cardNumber)
  const previewNumber = cardNumber || '•••• •••• •••• ••••'
  const previewHolder = cardholder.trim() || 'NOMBRE DEL TITULAR'

  return <section className="page-container">
    <div className="section-heading"><div><span className="eyebrow">Compra segura</span><h1>Finalizar compra</h1></div></div>
    <div className="checkout-layout">
      <form className="payment-card" onSubmit={checkout}>
        <div className="payment-card-header"><span><CreditCard /></span><div><h2>Pago con tarjeta</h2><p>Completa los datos para simular el procesamiento de una tarjeta.</p></div></div>

        <fieldset className="payment-method-tabs">
          <legend>Tipo de tarjeta</legend>
          <label className={cardType === 'credit' ? 'active' : ''}><input type="radio" name="cardType" value="credit" checked={cardType === 'credit'} onChange={() => setCardType('credit')} /><CreditCard />Crédito</label>
          <label className={cardType === 'debit' ? 'active' : ''}><input type="radio" name="cardType" value="debit" checked={cardType === 'debit'} onChange={() => { setCardType('debit'); setInstallments('1') }} /><CreditCard />Débito</label>
        </fieldset>

        <div className={`card-preview card-preview-${cardType}`} aria-hidden="true">
          <div className="card-preview-top"><span className="card-chip" /><Wifi /><strong>{cardBrand}</strong></div>
          <span className="card-preview-number">{previewNumber}</span>
          <div className="card-preview-bottom"><span><small>Titular</small><strong>{previewHolder}</strong></span><span><small>Vence</small><strong>{expiry || 'MM/AA'}</strong></span></div>
        </div>

        <div className="simulation-notice"><ShieldCheck /><p><strong>Entorno de simulación</strong><span>Usa información ficticia. Estos datos no se guardan ni se envían al backend.</span></p></div>

        <div className="card-form-fields">
          <label className="card-field card-field-full"><span>Número de tarjeta</span><div className="card-input-shell"><CreditCard /><input required inputMode="numeric" autoComplete="off" minLength={19} maxLength={19} value={cardNumber} onChange={(event) => setCardNumber(formatCardNumber(event.target.value))} placeholder="4242 4242 4242 4242" aria-describedby="card-number-help" /><strong>{cardBrand}</strong></div><small id="card-number-help">Ingresa 16 dígitos ficticios.</small></label>
          <label className="card-field card-field-full"><span>Nombre del titular</span><input required autoComplete="off" maxLength={50} value={cardholder} onChange={(event) => setCardholder(event.target.value.toUpperCase())} placeholder="NOMBRE COMO APARECE EN LA TARJETA" /></label>
          <label className="card-field"><span>Fecha de vencimiento</span><input required inputMode="numeric" autoComplete="off" minLength={5} maxLength={5} value={expiry} onChange={(event) => setExpiry(formatCardExpiry(event.target.value))} placeholder="MM/AA" /></label>
          <label className="card-field"><span>Código de seguridad</span><div className="card-input-shell"><LockKeyhole /><input required type="password" inputMode="numeric" autoComplete="off" minLength={3} maxLength={4} value={cvv} onChange={(event) => setCvv(onlyDigits(event.target.value, 4))} placeholder="CVV" /></div></label>
          {cardType === 'credit' && <label className="card-field card-field-full"><span>Número de cuotas</span><select value={installments} onChange={(event) => setInstallments(event.target.value)}><option value="1">1 cuota</option><option value="3">3 cuotas</option><option value="6">6 cuotas</option><option value="12">12 cuotas</option></select></label>}
        </div>

        <fieldset className="simulation-result">
          <legend>Resultado de la simulación</legend>
          <label className={!reject ? 'selected' : ''}><input type="radio" name="simulationResult" checked={!reject} onChange={() => setReject(false)} /><span><strong>Aprobar pago</strong><small>La compra y la factura serán generadas.</small></span></label>
          <label className={reject ? 'selected danger' : ''}><input type="radio" name="simulationResult" checked={reject} onChange={() => setReject(true)} /><span><strong>Rechazar pago</strong><small>Permite probar el flujo de error.</small></span></label>
        </fieldset>

        <div className="security-note"><ShieldCheck />El monto final y la disponibilidad se validan exclusivamente en el backend.</div>
        {error && <p className="form-error" role="alert">{error}</p>}
        <button type="submit" className="button button-primary button-full" disabled={submitting}>{submitting ? 'Procesando pago...' : `Pagar ${formatCop(estimatedSubtotal)} (simulado)`}</button>
      </form>
      <aside className="order-summary"><h2>Tu pedido</h2>{items.map(({ product, quantity }) => <div key={product.id}><span>{quantity} × {product.name}</span><strong>{formatCop(product.price * quantity)}</strong></div>)}<hr /><div><span>Subtotal estimado</span><strong>{formatCop(estimatedSubtotal)}</strong></div><p>El total final con IVA aparecerá después de la validación del servidor.</p></aside>
    </div>
  </section>
}
