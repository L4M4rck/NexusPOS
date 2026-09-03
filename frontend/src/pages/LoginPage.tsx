import { ArrowRight, Eye, EyeOff } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom'
import { getApiError } from '../api/client'
import { useAuth } from '../features/auth/AuthContext'

export function LoginPage() {
  const { login, role } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [show, setShow] = useState(false)
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  if (role && role !== 'Guest') return <Navigate to={role === 'Admin' ? '/admin' : '/catalogo'} replace />

  const submit = async (event: FormEvent) => {
    event.preventDefault(); setError(''); setSubmitting(true)
    try { await login(email, password); navigate((location.state as { from?: string })?.from ?? '/catalogo') }
    catch (reason) { setError(getApiError(reason)) }
    finally { setSubmitting(false) }
  }

  return <AuthShell title="Bienvenido de nuevo" subtitle="Ingresa para continuar con tu experiencia Nexus."><form onSubmit={submit} className="auth-form"><label>Correo electrónico<input type="email" required value={email} onChange={(e) => setEmail(e.target.value)} placeholder="tu@correo.com" /></label><label>Contraseña<div className="password-field"><input type={show ? 'text' : 'password'} required value={password} onChange={(e) => setPassword(e.target.value)} placeholder="••••••••" /><button type="button" aria-label="Mostrar contraseña" onClick={() => setShow(!show)}>{show ? <EyeOff /> : <Eye />}</button></div></label>{error && <p className="form-error" role="alert">{error}</p>}<button className="button button-primary button-full" disabled={submitting}>{submitting ? 'Ingresando...' : <>Iniciar sesión <ArrowRight /></>}</button><p className="auth-switch">¿No tienes cuenta? <Link to="/registro">Crear cuenta</Link></p><Link className="guest-link" to="/catalogo">Continuar como invitado</Link></form></AuthShell>
}

export function AuthShell({ title, subtitle, children }: { title: string; subtitle: string; children: React.ReactNode }) {
  return <main className="auth-page"><section className="auth-brand-panel"><Link to="/" className="brand brand-light"><span>N</span>NEXUS<strong>POS</strong></Link><div><span className="eyebrow">La mejor tecnología</span><h1>Conecta con lo que sigue.</h1><p>Una experiencia segura, rápida y diseñada para ti.</p></div></section><section className="auth-card"><div><h1>{title}</h1><p>{subtitle}</p></div>{children}</section></main>
}
