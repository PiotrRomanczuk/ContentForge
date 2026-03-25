import { useState } from 'react'
import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '@/hooks/use-auth'
import { Sidebar } from './Sidebar'
import { Header } from './Header'
import { Loader2 } from 'lucide-react'

export function AppLayout() {
  const { isAuthenticated, isLoading } = useAuth()
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false)

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center bg-fm-background">
        <Loader2 className="h-8 w-8 animate-spin text-fm-primary" />
      </div>
    )
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  return (
    <div className="flex h-screen overflow-hidden bg-fm-surface">
      <Sidebar collapsed={sidebarCollapsed} onToggle={() => setSidebarCollapsed(!sidebarCollapsed)} />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Header />
        <main className="flex-1 overflow-y-auto bg-fm-background p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
