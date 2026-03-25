import { Link } from 'react-router-dom'
import { useContentStats } from '@/hooks/use-content'
import { useBots } from '@/hooks/use-bots'
import { StatsOverview } from '@/components/stats/StatsOverview'
import { PipelineChart } from '@/components/stats/PipelineChart'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { CheckCircle2, Upload, Bot, Cpu } from 'lucide-react'

export function DashboardPage() {
  const { data: stats, isLoading: statsLoading } = useContentStats()
  const { data: bots, isLoading: botsLoading } = useBots()

  const pendingCount = stats?.Generated ?? 0

  return (
    <div className="space-y-6">
      {/* Status Cards */}
      <StatsOverview stats={stats} isLoading={statsLoading} />

      {/* Quick Actions -- 3 prominent buttons in a row */}
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
        <Link
          to="/approval"
          className="group relative flex items-center gap-3 rounded-xl bg-fm-surface-container-high px-5 py-4 transition-all hover:bg-fm-surface-bright hover:glow-primary"
        >
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-fm-primary/10">
            <CheckCircle2 className="h-4.5 w-4.5 text-fm-primary" />
          </div>
          <div className="flex-1">
            <p className="text-sm font-semibold text-fm-primary">Review Queue</p>
            <p className="text-xs text-fm-on-surface-variant">Approve pending content</p>
          </div>
          {pendingCount > 0 && (
            <span className="flex h-6 min-w-6 items-center justify-center rounded-full bg-fm-primary/15 px-2 text-xs font-bold text-fm-primary">
              {pendingCount}
            </span>
          )}
        </Link>

        <Link
          to="/content/import"
          className="group flex items-center gap-3 rounded-xl bg-fm-surface-container-high px-5 py-4 transition-all hover:bg-fm-surface-bright hover:glow-primary"
        >
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-fm-primary/10">
            <Upload className="h-4.5 w-4.5 text-fm-primary" />
          </div>
          <div>
            <p className="text-sm font-semibold text-fm-primary">Import Content</p>
            <p className="text-xs text-fm-on-surface-variant">Add new content items</p>
          </div>
        </Link>

        <Link
          to="/bots"
          className="group flex items-center gap-3 rounded-xl bg-fm-surface-container-high px-5 py-4 transition-all hover:bg-fm-surface-bright hover:glow-primary"
        >
          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-fm-primary/10">
            <Bot className="h-4.5 w-4.5 text-fm-primary" />
          </div>
          <div>
            <p className="text-sm font-semibold text-fm-primary">Explore Bots</p>
            <p className="text-xs text-fm-on-surface-variant">Browse content generators</p>
          </div>
        </Link>
      </div>

      {/* Pipeline Chart -- full width */}
      <PipelineChart stats={stats} />

      {/* Connected Bots */}
      <div className="rounded-xl bg-fm-surface-container p-5">
        <h3 className="mb-4 font-display text-sm font-semibold text-fm-on-surface">
          Connected Bots
        </h3>

        {botsLoading ? (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 3 }).map((_, i) => (
              <Skeleton key={i} className="h-28 rounded-xl bg-fm-surface-container-high" />
            ))}
          </div>
        ) : bots && bots.length > 0 ? (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {bots.map((bot) => (
              <Link key={bot.name} to="/bots">
                <div className="group rounded-xl bg-fm-surface-container-high p-4 transition-colors hover:bg-fm-surface-bright">
                  {/* Bot header */}
                  <div className="flex items-center gap-2.5">
                    <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-fm-primary-dim/20">
                      <Cpu className="h-4 w-4 text-fm-primary" />
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="truncate font-display text-sm font-bold text-fm-on-surface">
                        {bot.name}
                      </p>
                    </div>
                  </div>

                  {/* Category badge */}
                  <div className="mt-2.5">
                    <Badge
                      variant="secondary"
                      className="bg-fm-secondary-container/30 text-fm-secondary text-[10px]"
                    >
                      {bot.category}
                    </Badge>
                  </div>

                  {/* Description */}
                  {bot.description && (
                    <p className="mt-2 line-clamp-2 text-xs text-fm-on-surface-variant">
                      {bot.description}
                    </p>
                  )}

                  {/* Content type badges */}
                  <div className="mt-3 flex flex-wrap gap-1">
                    {bot.supportedContentTypes.map((type) => (
                      <Badge
                        key={type}
                        variant="outline"
                        className="border-fm-outline-variant/30 text-fm-on-surface-variant text-[10px]"
                      >
                        {type}
                      </Badge>
                    ))}
                  </div>
                </div>
              </Link>
            ))}
          </div>
        ) : (
          <p className="text-sm text-fm-on-surface-variant">No bots registered yet.</p>
        )}
      </div>
    </div>
  )
}
