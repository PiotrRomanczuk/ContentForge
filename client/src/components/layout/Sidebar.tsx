import { Link, useLocation } from 'react-router-dom'
import { cn } from '@/lib/utils'
import {
  LayoutDashboard,
  CheckCircle2,
  GitBranch,
  Upload,
  Users,
  Clock,
  Bot,
  PanelLeftClose,
  PanelLeft,
  Zap,
  LogOut,
  ScrollText,
} from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip'
import { useAuth } from '@/hooks/use-auth'
import { useContentStats } from '@/hooks/use-content'

interface SidebarProps {
  collapsed: boolean
  onToggle: () => void
}

const navGroups = [
  {
    label: 'Main',
    items: [
      { path: '/', icon: LayoutDashboard, label: 'Dashboard' },
      { path: '/approval', icon: CheckCircle2, label: 'Approval Queue', showBadge: true },
    ],
  },
  {
    label: 'Content',
    items: [
      { path: '/content', icon: GitBranch, label: 'Pipeline' },
      { path: '/content/import', icon: Upload, label: 'Import' },
    ],
  },
  {
    label: 'Publishing',
    items: [
      { path: '/social-accounts', icon: Users, label: 'Social Accounts' },
      { path: '/schedules', icon: Clock, label: 'Schedules' },
    ],
  },
  {
    label: 'Configuration',
    items: [
      { path: '/bots', icon: Bot, label: 'Bot Explorer' },
      { path: '/logs', icon: ScrollText, label: 'Logs' },
    ],
  },
]

export function Sidebar({ collapsed, onToggle }: SidebarProps) {
  const location = useLocation()
  const { logout } = useAuth()
  const { data: stats } = useContentStats()
  const pendingCount = stats?.Generated ?? 0

  return (
    <aside
      className={cn(
        'flex h-screen flex-col bg-fm-surface-container-low text-foreground transition-all duration-200',
        collapsed ? 'w-16' : 'w-60',
      )}
    >
      {/* Logo -- background shift separates from nav, no hard border */}
      <div className="flex h-14 shrink-0 items-center gap-2.5 px-4">
        <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-fm-primary">
          <Zap className="h-4 w-4 text-fm-background" />
        </div>
        {!collapsed && (
          <span className="font-display text-[15px] font-bold tracking-tight text-fm-on-background">
            ContentForge
          </span>
        )}
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto px-2 pt-4">
        {navGroups.map((group) => (
          <div key={group.label} className="mb-5">
            {!collapsed && (
              <p className="mb-1.5 px-3 text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant/60">
                {group.label}
              </p>
            )}
            {group.items.map((item) => {
              const isActive =
                item.path === '/'
                  ? location.pathname === '/'
                  : location.pathname.startsWith(item.path)

              const linkContent = (
                <Link
                  key={item.path}
                  to={item.path}
                  className={cn(
                    'flex items-center gap-3 rounded-lg px-3 py-2 text-sm transition-colors',
                    isActive
                      ? 'bg-fm-primary/10 font-medium text-fm-primary'
                      : 'text-fm-on-surface-variant hover:bg-fm-surface-bright/60 hover:text-fm-on-surface',
                    collapsed && 'justify-center px-2',
                  )}
                >
                  <item.icon className="h-4 w-4 shrink-0" />
                  {!collapsed && (
                    <>
                      <span className="flex-1">{item.label}</span>
                      {item.showBadge && pendingCount > 0 && (
                        <span className="flex h-5 min-w-5 items-center justify-center rounded-full bg-fm-primary/15 px-1.5 text-[11px] font-medium text-fm-primary">
                          {pendingCount}
                        </span>
                      )}
                    </>
                  )}
                </Link>
              )

              if (collapsed) {
                return (
                  <Tooltip key={item.path} delayDuration={0}>
                    <TooltipTrigger asChild>{linkContent}</TooltipTrigger>
                    <TooltipContent side="right" className="flex items-center gap-2">
                      {item.label}
                      {item.showBadge && pendingCount > 0 && (
                        <span className="rounded-full bg-fm-primary/15 px-1.5 text-[11px] font-medium text-fm-primary">
                          {pendingCount}
                        </span>
                      )}
                    </TooltipContent>
                  </Tooltip>
                )
              }

              return linkContent
            })}
          </div>
        ))}
      </nav>

      {/* Bottom -- background shift from nav area, no border */}
      <div className="bg-fm-surface-container-low p-2">
        <div className="flex items-center gap-1">
          <Tooltip delayDuration={0}>
            <TooltipTrigger asChild>
              <Button
                variant="ghost"
                size="icon"
                onClick={onToggle}
                className="h-8 w-8 shrink-0 text-fm-on-surface-variant hover:bg-fm-surface-bright/60 hover:text-fm-on-surface"
                aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
              >
                {collapsed ? (
                  <PanelLeft className="h-4 w-4" />
                ) : (
                  <PanelLeftClose className="h-4 w-4" />
                )}
              </Button>
            </TooltipTrigger>
            <TooltipContent side="right">
              {collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
            </TooltipContent>
          </Tooltip>
          {!collapsed && (
            <Button
              variant="ghost"
              size="sm"
              onClick={logout}
              className="ml-auto h-8 gap-2 text-fm-on-surface-variant hover:bg-fm-surface-bright/60 hover:text-fm-on-surface"
            >
              <LogOut className="h-3.5 w-3.5" />
              Logout
            </Button>
          )}
          {collapsed && (
            <Tooltip delayDuration={0}>
              <TooltipTrigger asChild>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={logout}
                  className="h-8 w-8 text-fm-on-surface-variant hover:bg-fm-surface-bright/60 hover:text-fm-on-surface"
                  aria-label="Logout"
                >
                  <LogOut className="h-4 w-4" />
                </Button>
              </TooltipTrigger>
              <TooltipContent side="right">Logout</TooltipContent>
            </Tooltip>
          )}
        </div>
      </div>
    </aside>
  )
}
