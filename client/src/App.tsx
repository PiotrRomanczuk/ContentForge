import { RouterProvider } from 'react-router-dom'
import { TooltipProvider } from '@/components/ui/tooltip'
import { Toaster } from '@/components/ui/sonner'
import { AuthProvider } from '@/providers/AuthProvider'
import { QueryProvider } from '@/providers/QueryProvider'
import { router } from '@/routes'

export function App() {
  return (
    <QueryProvider>
      <AuthProvider>
        <TooltipProvider>
          <RouterProvider router={router} />
          <Toaster position="bottom-right" />
        </TooltipProvider>
      </AuthProvider>
    </QueryProvider>
  )
}
