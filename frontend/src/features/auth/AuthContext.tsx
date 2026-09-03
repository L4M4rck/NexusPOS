import { createContext, useContext, useEffect, useMemo, useState, type PropsWithChildren } from 'react'
import { authApi } from '../../api/services'
import { TOKEN_KEY } from '../../api/client'
import type { AuthResponse, Role } from '../../types'

const SESSION_KEY = 'nexuspos.session'

interface AuthContextValue {
  session: AuthResponse | null
  loading: boolean
  role: Role | null
  login: (email: string, password: string) => Promise<void>
  register: (data: Record<string, string>) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function persistSession(session: AuthResponse) {
  localStorage.setItem(TOKEN_KEY, session.accessToken)
  localStorage.setItem(SESSION_KEY, JSON.stringify(session))
}

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<AuthResponse | null>(() => {
    const saved = localStorage.getItem(SESSION_KEY)
    return saved ? (JSON.parse(saved) as AuthResponse) : null
  })
  const [loading, setLoading] = useState(!session)

  const becomeGuest = async () => {
    const guest = await authApi.guest()
    persistSession(guest)
    setSession(guest)
  }

  useEffect(() => {
    if (session) return
    becomeGuest().catch(() => setSession(null)).finally(() => setLoading(false))
  }, [session])

  const value = useMemo<AuthContextValue>(() => ({
    session,
    loading,
    role: session?.role ?? null,
    login: async (email, password) => {
      const result = await authApi.login(email, password)
      persistSession(result)
      setSession(result)
    },
    register: async (data) => {
      const result = await authApi.register(data)
      persistSession(result)
      setSession(result)
    },
    logout: async () => {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(SESSION_KEY)
      setLoading(true)
      try { await becomeGuest() } finally { setLoading(false) }
    },
  }), [loading, session])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth debe utilizarse dentro de AuthProvider')
  return context
}
