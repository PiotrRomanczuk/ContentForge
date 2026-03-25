import { useState } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '@/hooks/use-auth'
import { Zap, Loader2 } from 'lucide-react'

export function LoginPage() {
  const { isAuthenticated, login } = useAuth()
  const [apiKey, setApiKey] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  if (isAuthenticated) {
    return <Navigate to="/" replace />
  }

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    if (!apiKey.trim()) return

    setError('')
    setIsSubmitting(true)

    try {
      await login(apiKey)
    } catch {
      setError('Invalid API key. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div
      className="flex min-h-screen items-center justify-center p-4"
      style={{
        background: 'radial-gradient(ellipse at top left, var(--fm-surface-bright), var(--fm-surface) 70%)',
      }}
    >
      <div className="w-full max-w-sm">
        {/* Glassmorphism card */}
        <div
          className="rounded-xl p-8"
          style={{
            background: 'color-mix(in srgb, var(--fm-surface-container) 80%, transparent)',
            backdropFilter: 'blur(24px)',
            WebkitBackdropFilter: 'blur(24px)',
            boxShadow: '0 20px 80px rgba(0, 0, 0, 0.45)',
            border: '1px solid color-mix(in srgb, var(--fm-outline-variant) 15%, transparent)',
          }}
        >
          {/* Branding */}
          <div className="mb-8 flex flex-col items-center text-center">
            <div className="mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-fm-primary/15">
              <Zap className="h-7 w-7 text-fm-primary" />
            </div>
            <h1 className="font-display text-2xl font-bold tracking-tight text-fm-on-background">
              ContentForge
            </h1>
            <p className="mt-1 text-sm text-fm-on-surface-variant">
              Content Management Platform
            </p>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-5">
            <div className="space-y-2">
              <label htmlFor="api-key" className="block text-xs font-medium text-fm-on-surface-variant">
                API Key
              </label>
              <input
                id="api-key"
                type="password"
                placeholder="Enter your API key"
                value={apiKey}
                onChange={(e) => setApiKey(e.target.value)}
                disabled={isSubmitting}
                autoFocus
                className="h-10 w-full rounded-lg bg-[#000000] px-3 text-sm text-fm-on-background placeholder:text-fm-outline outline-none transition-shadow focus:ring-2 focus:ring-fm-primary-dim/50 disabled:cursor-not-allowed disabled:opacity-50"
              />
            </div>

            <button
              type="submit"
              disabled={isSubmitting || !apiKey.trim()}
              className="relative flex h-10 w-full items-center justify-center rounded-lg text-sm font-semibold text-fm-on-background transition-all disabled:cursor-not-allowed disabled:opacity-50"
              style={{
                background: 'linear-gradient(135deg, var(--fm-primary-dim), var(--fm-primary))',
              }}
              onMouseEnter={(e) => {
                if (!e.currentTarget.disabled) {
                  e.currentTarget.style.boxShadow = '0 0 20px color-mix(in srgb, var(--fm-primary) 20%, transparent)'
                }
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.boxShadow = 'none'
              }}
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Connecting...
                </>
              ) : (
                'Sign In'
              )}
            </button>

            {error && (
              <p className="text-center text-sm text-fm-error" role="alert">
                {error}
              </p>
            )}
          </form>
        </div>
      </div>
    </div>
  )
}
