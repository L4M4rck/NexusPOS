import { useQuery } from '@tanstack/react-query'
import { Banknote, Boxes, CircleDollarSign, ReceiptText, ShoppingCart, UsersRound } from 'lucide-react'
import { useState } from 'react'
import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { adminApi } from '../../api/services'
import { ErrorState, LoadingState } from '../../components/AsyncState'
import { formatCop, formatDate } from '../../utils/format'

export function AdminDashboardPage() {
  const [period, setPeriod] = useState('monthly')
  const query = useQuery({ queryKey: ['dashboard', period], queryFn: () => adminApi.dashboard(period) })
  if (query.isLoading) return <LoadingState label="Calculando indicadores..." />
  if (!query.data || query.isError) return <ErrorState message="No fue posible cargar el dashboard." retry={() => query.refetch()} />
  const data = query.data
  const kpis = [
    { label: 'Ingresos', value: formatCop(data.revenue), icon: CircleDollarSign },
    { label: 'Ventas', value: data.salesCount.toString(), icon: ReceiptText },
    { label: 'Clientes', value: data.customersCount.toString(), icon: UsersRound },
    { label: 'Unidades', value: data.unitsSold.toString(), icon: Boxes },
    { label: 'Ticket promedio', value: formatCop(data.averageTicket), icon: Banknote },
  ]
  return <><div className="admin-heading"><div><span className="eyebrow">Visión general</span><h1>Dashboard</h1><p>Indicadores calculados con datos reales de ventas.</p></div><select value={period} onChange={(event) => setPeriod(event.target.value)}><option value="weekly">Últimos 7 días</option><option value="monthly">Últimos 30 días</option><option value="yearly">Este año</option></select></div><div className="kpi-grid">{kpis.map(({ label, value, icon: Icon }) => <article key={label}><Icon /><span>{label}</span><strong>{value}</strong></article>)}</div><div className="dashboard-grid"><article className="panel chart-panel"><div className="panel-heading"><div><h2>Rendimiento de ventas</h2><p>Ingresos por periodo</p></div></div><ResponsiveContainer width="100%" height={300}><AreaChart data={data.series}><defs><linearGradient id="revenue" x1="0" y1="0" x2="0" y2="1"><stop offset="5%" stopColor="#d71920" stopOpacity={0.35}/><stop offset="95%" stopColor="#d71920" stopOpacity={0}/></linearGradient></defs><CartesianGrid strokeDasharray="3 3" vertical={false} /><XAxis dataKey="label" /><YAxis tickFormatter={(value) => `${Number(value) / 1000}k`} /><Tooltip formatter={(value) => formatCop(Number(value))} /><Area type="monotone" dataKey="revenue" stroke="#d71920" strokeWidth={3} fill="url(#revenue)" /></AreaChart></ResponsiveContainer></article><article className="panel"><div className="panel-heading"><div><h2>Stock bajo</h2><p>Requieren atención</p></div><Boxes /></div><div className="compact-list">{data.lowStockProducts.length ? data.lowStockProducts.map((product) => <div key={product.id}><span>{product.name}</span><strong>{product.stock} und.</strong></div>) : <p>Todo el inventario está saludable.</p>}</div></article></div><div className="dashboard-grid"><article className="panel"><div className="panel-heading"><div><h2>Productos destacados</h2><p>Por unidades vendidas</p></div><ShoppingCart /></div><div className="rank-list">{data.topProducts.map((product, index) => <div key={product.name}><span>{String(index + 1).padStart(2, '0')}</span><div><strong>{product.name}</strong><small>{product.units} unidades</small></div><b>{formatCop(product.revenue)}</b></div>)}</div></article><article className="panel"><div className="panel-heading"><div><h2>Ventas recientes</h2><p>Últimos movimientos</p></div></div><div className="compact-list">{data.recentSales.map((sale) => <div key={sale.id}><span><strong>{sale.invoiceNumber}</strong><small>{sale.customer} · {formatDate(sale.createdAt)}</small></span><b>{formatCop(sale.total)}</b></div>)}</div></article></div></>
}
