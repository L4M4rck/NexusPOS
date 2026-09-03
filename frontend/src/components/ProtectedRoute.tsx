import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../features/auth/AuthContext'
import type { Role } from '../types'
import { LoadingState } from './AsyncState'

export function ProtectedRoute({ roles }: { roles: Role[] }) {
  const { role, loading } = useAuth()
  const location = useLocation()
  if (loading) return <LoadingState />
  if (!role || !roles.includes(role)) return <Navigate to="/login" state={{ from: location.pathname }} replace />
  return <Outlet />
}
