import { createContext, useCallback, useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { authApi } from '@/api/auth'
import { clearAuth, isAuthenticated as checkAuth } from '@/api/client'

interface AuthContextValue {
  isAuthenticated: boolean
  isLoading: boolean
  login: (apiKey: string) => Promise<void>
  logout: () => void
}

// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext<AuthContextValue | null>(null)

function getInitialAuth(): boolean {
  // Check token + expiry at init time (no effect needed)
  const hasToken = checkAuth()
  if (!hasToken) return false

  const expires = localStorage.getItem('cf_token_expires')
  if (expires && new Date(expires).getTime() < Date.now()) {
    clearAuth()
    return false
  }
  return true
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(getInitialAuth)
  const [isLoading] = useState(false)

  // Auto-logout timer for token expiry
  useEffect(() => {
    if (!isAuthenticated) return

    const expires = localStorage.getItem('cf_token_expires')
    if (!expires) return

    const timeUntilExpiry = new Date(expires).getTime() - Date.now() - 60_000

    if (timeUntilExpiry <= 0) return // Already handled in getInitialAuth

    const timer = setTimeout(() => {
      clearAuth()
      setIsAuthenticated(false)
    }, timeUntilExpiry)

    return () => clearTimeout(timer)
  }, [isAuthenticated])

  const login = useCallback(async (apiKey: string) => {
    await authApi.login(apiKey)
    setIsAuthenticated(true)
  }, [])

  const logout = useCallback(() => {
    clearAuth()
    setIsAuthenticated(false)
  }, [])

  return (
    <AuthContext.Provider value={{ isAuthenticated, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}
