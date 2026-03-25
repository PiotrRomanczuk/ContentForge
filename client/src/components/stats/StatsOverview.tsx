import { Link } from 'react-router-dom'
import { Skeleton } from '@/components/ui/skeleton'
import { STATUS_CONFIG } from '@/lib/constants'
import { CONTENT_STATUS_ORDER } from '@/types'
import type { ContentStatus } from '@/types'
import {
  FileEdit,
  Sparkles,
  ImageIcon,
  Clock,
  Send,
  CheckCircle2,
  XCircle,
} from 'lucide-react'
import type { LucideIcon } from 'lucide-react'

/** Map icon string names from STATUS_CONFIG to actual Lucide components */
const ICON_MAP: Record<string, LucideIcon> = {
  FileEdit,
  Sparkles,
  ImageIcon,
  Clock,
  Send,
  CheckCircle2,
  XCircle,
}

interface StatsOverviewProps {
  stats: Record<ContentStatus, number> | undefined
  isLoading: boolean
}

export function StatsOverview({ stats, isLoading }: StatsOverviewProps) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-7">
        {Array.from({ length: 7 }).map((_, i) => (
          <Skeleton key={i} className="h-[88px] rounded-xl bg-fm-surface-container" />
        ))}
      </div>
    )
  }

  return (
    <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-7">
      {CONTENT_STATUS_ORDER.map((status) => {
        const config = STATUS_CONFIG[status]
        const count = stats?.[status] ?? 0
        const Icon = ICON_MAP[config.icon]

        return (
          <Link key={status} to={`/content?status=${status}`}>
            <div
              className="group relative rounded-xl bg-fm-surface-container p-4 transition-colors hover:bg-fm-surface-container-high"
            >
              {/* Colored top accent line */}
              <div
                className="absolute inset-x-0 top-0 h-[2px] rounded-t-xl opacity-60"
                style={{ backgroundColor: config.hex }}
              />

              <div className="flex items-center gap-2">
                {Icon && (
                  <Icon
                    className="h-3.5 w-3.5"
                    style={{ color: config.hex }}
                  />
                )}
                <p className="text-xs font-medium text-fm-on-surface-variant">
                  {config.label}
                </p>
              </div>
              <p className="mt-2 font-display text-2xl font-bold text-fm-on-surface">
                {count}
              </p>
            </div>
          </Link>
        )
      })}
    </div>
  )
}
