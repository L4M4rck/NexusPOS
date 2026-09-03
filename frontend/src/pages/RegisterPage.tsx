import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { getApiError } from '../api/client'
import { useAuth } from '../features/auth/AuthContext'
import { AuthShell } from './LoginPage'

export function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault(); setError(''); setSubmitting(true)
    const data = Object.fromEntries(new FormData(event.currentTarget).entries()) as Record<string, string>
    try { await register(data); navigate('/catalogo') } catch (reason) { setError(getApiError(reason)) } finally { setSubmitting(false) }
  }
  return <AuthShell title="Crea tu cuenta" subtitle="Tus compras, facturas y productos favoritos en un solo lugar."><form onSubmit={submit} className="auth-form"><div className="form-row"><label>Nombre<input name="firstName" required /></label><label>Apellido<input name="lastName" required /></label></div><label>Correo electrónico<input name="email" type="email" required /></label><div className="form-row"><label>Documento<input name="documentNumber" required /></label><label>Teléfono<input name="phone" /></label></div><label>Dirección<input name="address" /></label><label>Contraseña<input name="password" type="password" required minLength={8} aria-describedby="password-help" /><small id="password-help">Mínimo 8 caracteres, mayúscula, minúscula y número.</small></label>{error && <p className="form-error" role="alert">{error}</p>}<button className="button button-primary button-full" disabled={submitting}>{submitting ? 'Creando cuenta...' : 'Crear cuenta'}</button><p className="auth-switch">¿Ya tienes cuenta? <Link to="/login">Iniciar sesión</Link></p></form></AuthShell>
}
