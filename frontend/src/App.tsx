import { lazy, Suspense } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { LoadingState } from './components/AsyncState'
import { ProtectedRoute } from './components/ProtectedRoute'

const StoreLayout = lazy(() => import('./layouts/StoreLayout').then((module) => ({ default: module.StoreLayout })))
const AdminLayout = lazy(() => import('./layouts/AdminLayout').then((module) => ({ default: module.AdminLayout })))
const CatalogPage = lazy(() => import('./pages/CatalogPage').then((module) => ({ default: module.CatalogPage })))
const ProductDetailPage = lazy(() => import('./pages/ProductDetailPage').then((module) => ({ default: module.ProductDetailPage })))
const CartPage = lazy(() => import('./pages/CartPage').then((module) => ({ default: module.CartPage })))
const CheckoutPage = lazy(() => import('./pages/CheckoutPage').then((module) => ({ default: module.CheckoutPage })))
const PurchasesPage = lazy(() => import('./pages/PurchasesPage').then((module) => ({ default: module.PurchasesPage })))
const InvoicePage = lazy(() => import('./pages/InvoicePage').then((module) => ({ default: module.InvoicePage })))
const LoginPage = lazy(() => import('./pages/LoginPage').then((module) => ({ default: module.LoginPage })))
const RegisterPage = lazy(() => import('./pages/RegisterPage').then((module) => ({ default: module.RegisterPage })))
const AdminDashboardPage = lazy(() => import('./pages/admin/AdminDashboardPage').then((module) => ({ default: module.AdminDashboardPage })))
const AdminProductsPage = lazy(() => import('./pages/admin/AdminProductsPage').then((module) => ({ default: module.AdminProductsPage })))
const AdminCategoriesPage = lazy(() => import('./pages/admin/AdminCategoriesPage').then((module) => ({ default: module.AdminCategoriesPage })))
const AdminInventoryPage = lazy(() => import('./pages/admin/AdminDataPages').then((module) => ({ default: module.AdminInventoryPage })))
const AdminCustomersPage = lazy(() => import('./pages/admin/AdminDataPages').then((module) => ({ default: module.AdminCustomersPage })))
const AdminMovementsPage = lazy(() => import('./pages/admin/AdminDataPages').then((module) => ({ default: module.AdminMovementsPage })))

export default function App() {
  return <Suspense fallback={<LoadingState label="Cargando NexusPOS..." />}><Routes>
    <Route element={<StoreLayout />}>
      <Route index element={<CatalogPage home />} />
      <Route path="catalogo" element={<CatalogPage />} />
      <Route path="catalogo/categoria/:categoryId" element={<CatalogPage />} />
      <Route path="catalogo/:id" element={<ProductDetailPage />} />
      <Route path="carrito" element={<CartPage />} />
      <Route element={<ProtectedRoute roles={['Customer']} />}>
        <Route path="checkout" element={<CheckoutPage />} />
        <Route path="compras" element={<PurchasesPage />} />
        <Route path="facturas/:id" element={<InvoicePage />} />
      </Route>
    </Route>
    <Route path="login" element={<LoginPage />} />
    <Route path="registro" element={<RegisterPage />} />
    <Route element={<ProtectedRoute roles={['Admin']} />}>
      <Route path="admin" element={<AdminLayout />}>
        <Route index element={<AdminDashboardPage />} />
        <Route path="productos" element={<AdminProductsPage />} />
        <Route path="categorias" element={<AdminCategoriesPage />} />
        <Route path="inventario" element={<AdminInventoryPage />} />
        <Route path="clientes" element={<AdminCustomersPage />} />
        <Route path="movimientos" element={<AdminMovementsPage />} />
        <Route path="movimientos/:id" element={<InvoicePage />} />
        <Route path="ventas" element={<Navigate to="/admin/movimientos" replace />} />
        <Route path="facturas" element={<Navigate to="/admin/movimientos" replace />} />
        <Route path="facturas/:id" element={<InvoicePage />} />
      </Route>
    </Route>
    <Route path="*" element={<Navigate to="/" replace />} />
  </Routes></Suspense>
}
