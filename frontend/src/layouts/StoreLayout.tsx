import { LogIn, LogOut, Menu, ShoppingBag, UserRound, X } from 'lucide-react'
import { useState } from 'react'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../features/auth/AuthContext'
import { useCart } from '../features/cart/CartContext'

export function StoreLayout() {
  const { role, session, logout } = useAuth()
  const { count } = useCart()
  const navigate = useNavigate()
  const [open, setOpen] = useState(false)
  const close = () => setOpen(false)

  return (
    <div className="app-shell">
      <header className="store-header">
        <Link to="/" className="brand" onClick={close}><span>N</span>NEXUS<strong>POS</strong></Link>
        <button className="menu-toggle" aria-label="Abrir menú" onClick={() => setOpen(!open)}>{open ? <X /> : <Menu />}</button>
        <nav className={open ? 'store-nav open' : 'store-nav'} aria-label="Navegación principal">
          <NavLink to="/" onClick={close}>Inicio</NavLink>
          <NavLink to="/catalogo" onClick={close}>Catálogo</NavLink>
          {role === 'Customer' && <NavLink to="/compras" onClick={close}>Mis compras</NavLink>}
          {role === 'Admin' && <NavLink to="/admin" onClick={close}>Administración</NavLink>}
        </nav>
        <div className="header-actions">
          <Link to="/carrito" className="cart-link" aria-label={`Carrito con ${count} productos`}>
            <ShoppingBag /><span>{count}</span>
          </Link>
          {role === 'Guest' ? (
            <Link className="account-link" to="/login"><LogIn size={18} /> Ingresar</Link>
          ) : (
            <button className="account-link" onClick={async () => { await logout(); navigate('/') }}>
              <UserRound size={18} /><span>{session?.displayName}</span><LogOut size={16} />
            </button>
          )}
        </div>
      </header>
      <main><Outlet /></main>
      <footer className="store-footer">
        <div><div className="brand brand-light"><span>N</span>NEXUS<strong>POS</strong></div><p>Tecnología que conecta contigo.</p></div>
        <div><strong>Compra segura</strong><p>Precios e inventario validados siempre por nuestro servidor.</p></div>
        <div><strong>Soporte</strong><p>soporte@nexuspos.local</p></div>
      </footer>
    </div>
  )
}
