import { Boxes, ChartNoAxesCombined, FileText, LogOut, Package, ReceiptText, Tags, UsersRound } from 'lucide-react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../features/auth/AuthContext'

const links = [
  { to: '/admin', label: 'Dashboard', icon: ChartNoAxesCombined, end: true },
  { to: '/admin/productos', label: 'Productos', icon: Package },
  { to: '/admin/categorias', label: 'Categorías', icon: Tags },
  { to: '/admin/inventario', label: 'Inventario', icon: Boxes },
  { to: '/admin/clientes', label: 'Clientes', icon: UsersRound },
  { to: '/admin/ventas', label: 'Ventas', icon: ReceiptText },
  { to: '/admin/facturas', label: 'Facturas', icon: FileText },
]

export function AdminLayout() {
  const { logout, session } = useAuth()
  const navigate = useNavigate()
  return (
    <div className="admin-shell">
      <aside className="admin-sidebar">
        <div className="brand brand-light"><span>N</span>NEXUS<strong>POS</strong></div>
        <small>Panel administrativo</small>
        <nav>{links.map(({ to, label, icon: Icon, end }) => <NavLink key={to} to={to} end={end}><Icon size={19} />{label}</NavLink>)}</nav>
        <button onClick={async () => { await logout(); navigate('/') }}><LogOut size={19} />Cerrar sesión</button>
      </aside>
      <section className="admin-workspace">
        <header className="admin-topbar"><div><span>Administración</span><strong>{session?.displayName}</strong></div></header>
        <main className="admin-content"><Outlet /></main>
      </section>
    </div>
  )
}
