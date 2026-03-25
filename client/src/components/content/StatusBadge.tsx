import { cn } from '@/lib/utils'
import { STATUS_CONFIG } from '@/lib/constants'
import type { ContentStatus } from '@/types'

interface StatusBadgeProps {
  status: ContentStatus
  className?: string
}

export function StatusBadge({ status, className }: StatusBadgeProps) {
  const config = STATUS_CONFIG[status]

  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-medium',
        config.bg,
        config.color,
        className,
      )}
    >
      {status === 'Publishing' && (
        <span className="h-1.5 w-1.5 animate-pulse rounded-full bg-current" />
      )}
      {config.label}
    </span>
  )
}
