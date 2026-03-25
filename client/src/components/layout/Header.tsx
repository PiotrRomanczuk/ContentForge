import { useLocation } from 'react-router-dom'

const pageTitles: Record<string, string> = {
  '/': 'Dashboard',
  '/approval': 'Approval Queue',
  '/content': 'Content Pipeline',
  '/content/import': 'Import Content',
  '/social-accounts': 'Social Accounts',
  '/schedules': 'Schedules',
  '/bots': 'Bot Explorer',
  '/logs': 'Logs',
}

export function Header() {
  const location = useLocation()

  const title =
    pageTitles[location.pathname] ??
    (location.pathname.startsWith('/content/') ? 'Content Detail' : 'ContentForge')

  return (
    <header
      className="flex h-11 shrink-0 items-center bg-fm-surface-container-low px-6"
      style={{ borderBottom: '1px solid color-mix(in srgb, var(--fm-outline-variant) 15%, transparent)' }}
    >
      <h1 className="font-display text-sm font-semibold text-fm-on-background">{title}</h1>
    </header>
  )
}
