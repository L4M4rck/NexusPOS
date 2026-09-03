import { useQuery } from '@tanstack/react-query'
import { ArrowLeft, Download, Printer } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import { salesApi } from '../api/services'
import { ErrorState, LoadingState } from '../components/AsyncState'
import { useAuth } from '../features/auth/AuthContext'
import { formatCop, formatDate } from '../utils/format'
import { downloadInvoicePdf } from '../utils/invoicePdf'

export function InvoicePage() {
  const id = Number(useParams().id)
  const { role } = useAuth()
  const query = useQuery({ queryKey: ['invoice', id], queryFn: () => salesApi.invoice(id) })
  if (query.isLoading) return <LoadingState />
  if (!query.data || query.isError) return <ErrorState message="No fue posible cargar esta factura." />
  const invoice = query.data
  const backTo = role === 'Admin' ? '/admin/facturas' : '/compras'
  return <section className="invoice-page"><div className="invoice-actions"><Link to={backTo}><ArrowLeft />Volver</Link><div><button className="button button-secondary" onClick={() => window.print()}><Printer />Imprimir</button><button className="button button-primary" onClick={() => void downloadInvoicePdf(invoice)}><Download />Descargar PDF</button></div></div><article className="invoice"><header><div className="brand"><span>N</span>NEXUS<strong>POS</strong></div><div><strong>FACTURA</strong><span>{invoice.number}</span></div></header><div className="invoice-customer"><div><span>Facturado a</span><strong>{invoice.customerName}</strong><small>Documento {invoice.customerDocument}</small></div><div><span>Fecha de emisión</span><strong>{formatDate(invoice.issuedAt)}</strong></div></div><table><thead><tr><th>Producto</th><th>Cantidad</th><th>Precio unitario</th><th>Subtotal</th></tr></thead><tbody>{invoice.items.map((item) => <tr key={item.productId}><td>{item.productName}</td><td>{item.quantity}</td><td>{formatCop(item.unitPrice)}</td><td>{formatCop(item.subtotal)}</td></tr>)}</tbody></table><div className="invoice-totals"><div><span>Subtotal</span><strong>{formatCop(invoice.subtotal)}</strong></div><div><span>IVA</span><strong>{formatCop(invoice.tax)}</strong></div><div><span>Descuento</span><strong>{formatCop(invoice.discount)}</strong></div><div className="invoice-total"><span>Total</span><strong>{formatCop(invoice.total)}</strong></div></div><footer>Gracias por elegir NexusPOS · Tecnología que conecta contigo</footer></article></section>
}
