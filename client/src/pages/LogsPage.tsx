import { useState, useMemo } from 'react'
import { useLogs, useLogStats, useLogTrace } from '@/hooks/use-logs'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  ChevronLeft,
  ChevronRight,
  Search,
  X,
  Link2,
  Bug,
  Info,
  AlertTriangle,
  XCircle,
  RefreshCw,
  Activity,
} from 'lucide-react'
import { cn } from '@/lib/utils'
import { format } from 'date-fns'
import type { LogEntry, LogLevel } from '@/types/logs'

const LOG_LEVEL_CONFIG: Record<string, { icon: typeof Info; color: string; bg: string }> = {
  Debug: { icon: Bug, color: 'text-slate-400', bg: 'bg-slate-500/10' },
  Information: { icon: Info, color: 'text-blue-400', bg: 'bg-blue-500/10' },
  Warning: { icon: AlertTriangle, color: 'text-amber-400', bg: 'bg-amber-500/10' },
  Error: { icon: XCircle, color: 'text-red-400', bg: 'bg-red-500/10' },
}

/** Stats card icon + color mapping */
const STATS_CARD_CONFIG: Record<string, { icon: typeof Activity; color: string; bg: string }> = {
  Total: { icon: Activity, color: 'text-fm-on-surface', bg: 'bg-fm-surface-container-high' },
  Debug: { icon: Bug, color: 'text-slate-400', bg: 'bg-slate-500/10' },
  Info: { icon: Info, color: 'text-blue-400', bg: 'bg-blue-500/10' },
  Warning: { icon: AlertTriangle, color: 'text-amber-400', bg: 'bg-amber-500/10' },
  Error: { icon: XCircle, color: 'text-red-400', bg: 'bg-red-500/10' },
}

const PAGE_SIZE = 50

export function LogsPage() {
  const [date, setDate] = useState<string>('')
  const [level, setLevel] = useState<string>('all')
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')
  const [source, setSource] = useState('')
  const [page, setPage] = useState(0)
  const [expandedRow, setExpandedRow] = useState<number | null>(null)
  const [traceDialog, setTraceDialog] = useState<string | null>(null)

  // Debounce search
  const handleSearchChange = (value: string) => {
    setSearch(value)
    setTimeout(() => setDebouncedSearch(value), 300)
  }

  const queryParams = useMemo(
    () => ({
      date: date || undefined,
      level: level !== 'all' ? level : undefined,
      search: debouncedSearch || undefined,
      source: source || undefined,
      skip: page * PAGE_SIZE,
      take: PAGE_SIZE,
    }),
    [date, level, debouncedSearch, source, page],
  )

  const { data, isLoading, refetch } = useLogs(queryParams)
  const { data: stats } = useLogStats(date || undefined)

  const totalPages = data ? Math.ceil(data.total / PAGE_SIZE) : 0

  return (
    <div className="space-y-5">
      {/* Stats Cards */}
      {stats && (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-5">
          <StatsCard label="Total" count={stats.total} configKey="Total" />
          <StatsCard label="Debug" count={stats.debug} configKey="Debug" />
          <StatsCard label="Info" count={stats.information} configKey="Info" />
          <StatsCard label="Warning" count={stats.warning} configKey="Warning" />
          <StatsCard label="Error" count={stats.error} configKey="Error" />
        </div>
      )}

      {/* Filter bar */}
      <div className="flex flex-wrap items-center gap-3">
        {data?.availableDates && data.availableDates.length > 0 && (
          <Select value={date || 'today'} onValueChange={(v: string) => { setDate(v === 'today' ? '' : v); setPage(0) }}>
            <SelectTrigger className="h-9 w-[150px] bg-fm-surface-container text-sm text-fm-on-surface">
              <SelectValue placeholder="Today" />
            </SelectTrigger>
            <SelectContent className="bg-fm-surface-container-highest">
              <SelectItem value="today">Today</SelectItem>
              {data.availableDates.map((d) => (
                <SelectItem key={d} value={d}>
                  {formatDateLabel(d)}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        )}

        <Select value={level} onValueChange={(v: string) => { setLevel(v); setPage(0) }}>
          <SelectTrigger className="h-9 w-[140px] bg-fm-surface-container text-sm text-fm-on-surface">
            <SelectValue placeholder="All Levels" />
          </SelectTrigger>
          <SelectContent className="bg-fm-surface-container-highest">
            <SelectItem value="all">All Levels</SelectItem>
            <SelectItem value="Debug">Debug</SelectItem>
            <SelectItem value="Information">Information</SelectItem>
            <SelectItem value="Warning">Warning</SelectItem>
            <SelectItem value="Error">Error</SelectItem>
          </SelectContent>
        </Select>

        <div className="relative">
          <Search className="absolute left-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-fm-outline" />
          <Input
            placeholder="Search messages..."
            value={search}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => handleSearchChange(e.target.value)}
            className="h-9 w-[220px] bg-fm-surface-container pl-8 text-sm text-fm-on-surface placeholder:text-fm-outline focus:bg-fm-surface-container-high"
          />
        </div>

        <Input
          placeholder="Source context..."
          value={source}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => { setSource(e.target.value); setPage(0) }}
          className="h-9 w-[180px] bg-fm-surface-container text-sm text-fm-on-surface placeholder:text-fm-outline focus:bg-fm-surface-container-high"
        />

        {(level !== 'all' || search || source || date) && (
          <button
            type="button"
            onClick={() => {
              setLevel('all')
              setSearch('')
              setDebouncedSearch('')
              setSource('')
              setDate('')
              setPage(0)
            }}
            className="inline-flex h-8 items-center gap-1 rounded-lg px-2.5 text-xs text-fm-on-surface-variant transition-colors hover:bg-fm-surface-bright hover:text-fm-on-surface"
          >
            <X className="h-3 w-3" />
            Clear
          </button>
        )}

        <button
          type="button"
          onClick={() => refetch()}
          className="ml-auto flex h-8 w-8 items-center justify-center rounded-lg border border-fm-outline-variant/30 bg-transparent text-fm-on-surface-variant transition-colors hover:bg-fm-surface-bright hover:text-fm-on-surface"
          aria-label="Refresh logs"
        >
          <RefreshCw className="h-3.5 w-3.5" />
        </button>
      </div>

      {/* Log Table */}
      {isLoading ? (
        <div className="space-y-1">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="h-10 w-full rounded-lg bg-fm-surface-container" />
          ))}
        </div>
      ) : data && data.entries.length > 0 ? (
        <>
          <div className="overflow-hidden rounded-xl bg-fm-surface-container">
            <Table>
              <TableHeader>
                <TableRow className="border-b border-fm-outline-variant/15 hover:bg-transparent">
                  <TableHead className="w-[80px] text-xs font-medium text-fm-on-surface-variant">Level</TableHead>
                  <TableHead className="w-[90px] text-xs font-medium text-fm-on-surface-variant">Time</TableHead>
                  <TableHead className="text-xs font-medium text-fm-on-surface-variant">Message</TableHead>
                  <TableHead className="w-[200px] text-xs font-medium text-fm-on-surface-variant">Source</TableHead>
                  <TableHead className="w-[80px] text-xs font-medium text-fm-on-surface-variant">Duration</TableHead>
                  <TableHead className="w-[60px] text-xs font-medium text-fm-on-surface-variant">Trace</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.entries.map((entry, index) => {
                  const logLevel = getLogLevel(entry)
                  const config = LOG_LEVEL_CONFIG[logLevel] ?? LOG_LEVEL_CONFIG.Information
                  const LevelIcon = config.icon
                  const isExpanded = expandedRow === index
                  const sourceContext = getShortSource(entry.SourceContext)
                  const correlationId = entry.CorrelationId as string | undefined

                  return (
                    <TableRow
                      key={index}
                      className={cn(
                        'cursor-pointer border-b border-fm-outline-variant/8 transition-colors',
                        logLevel === 'Error' && 'bg-red-500/[0.03]',
                        logLevel === 'Warning' && 'bg-amber-500/[0.03]',
                        isExpanded
                          ? 'bg-fm-surface-container-high'
                          : index % 2 === 0
                            ? 'bg-fm-surface'
                            : 'bg-fm-surface-container-low',
                        !isExpanded && 'hover:bg-fm-surface-container',
                      )}
                      onClick={() => setExpandedRow(isExpanded ? null : index)}
                    >
                      <TableCell>
                        <span className={cn(
                          'inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 text-[11px] font-medium',
                          config.color,
                          config.bg,
                        )}>
                          <LevelIcon className="h-3 w-3" />
                          {logLevel === 'Information' ? 'INFO' : logLevel.slice(0, 4).toUpperCase()}
                        </span>
                      </TableCell>
                      <TableCell className="font-mono text-xs text-fm-on-surface-variant">
                        {formatTime(entry['@t'])}
                      </TableCell>
                      <TableCell>
                        <div>
                          <p className={cn('text-sm text-fm-on-surface', isExpanded ? '' : 'max-w-[400px] truncate')}>
                            {renderMessage(entry)}
                          </p>
                          {isExpanded && (
                            <ExpandedDetails entry={entry} />
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <span className="truncate text-xs text-fm-on-surface-variant" title={entry.SourceContext as string}>
                          {sourceContext}
                        </span>
                      </TableCell>
                      <TableCell>
                        {entry.ElapsedMs != null && (
                          <span
                            className={cn(
                              'font-mono text-xs',
                              (entry.ElapsedMs as number) > 500 ? 'text-amber-400' : 'text-fm-on-surface-variant',
                            )}
                          >
                            {Math.round(entry.ElapsedMs as number)}ms
                          </span>
                        )}
                      </TableCell>
                      <TableCell>
                        {correlationId && (
                          <button
                            type="button"
                            className="flex h-6 w-6 items-center justify-center rounded text-fm-on-surface-variant transition-colors hover:bg-fm-surface-bright hover:text-fm-primary"
                            onClick={(e: React.MouseEvent) => {
                              e.stopPropagation()
                              setTraceDialog(correlationId)
                            }}
                            aria-label="View request trace"
                          >
                            <Link2 className="h-3 w-3" />
                          </button>
                        )}
                      </TableCell>
                    </TableRow>
                  )
                })}
              </TableBody>
            </Table>
          </div>

          {/* Pagination */}
          <div className="flex items-center justify-between">
            <p className="text-sm text-fm-on-surface-variant">
              Showing {page * PAGE_SIZE + 1}&ndash;{Math.min((page + 1) * PAGE_SIZE, data.total)} of {data.total}
            </p>
            <div className="flex items-center gap-2">
              <button
                type="button"
                className="flex h-8 w-8 items-center justify-center rounded-lg border border-fm-outline-variant/30 bg-transparent text-fm-on-surface-variant transition-colors hover:bg-fm-surface-bright hover:text-fm-on-surface disabled:cursor-not-allowed disabled:opacity-40"
                onClick={() => setPage((p) => Math.max(0, p - 1))}
                disabled={page === 0}
                aria-label="Previous page"
              >
                <ChevronLeft className="h-4 w-4" />
              </button>
              <span className="text-sm text-fm-on-surface-variant">
                Page {page + 1} of {totalPages}
              </span>
              <button
                type="button"
                className="flex h-8 w-8 items-center justify-center rounded-lg border border-fm-outline-variant/30 bg-transparent text-fm-on-surface-variant transition-colors hover:bg-fm-surface-bright hover:text-fm-on-surface disabled:cursor-not-allowed disabled:opacity-40"
                onClick={() => setPage((p) => p + 1)}
                disabled={page >= totalPages - 1}
                aria-label="Next page"
              >
                <ChevronRight className="h-4 w-4" />
              </button>
            </div>
          </div>
        </>
      ) : (
        <div className="flex h-40 items-center justify-center rounded-xl bg-fm-surface-container">
          <p className="text-sm text-fm-on-surface-variant">No log entries found.</p>
        </div>
      )}

      {/* Trace Dialog */}
      {traceDialog && (
        <TraceDialog
          correlationId={traceDialog}
          date={date || undefined}
          onClose={() => setTraceDialog(null)}
        />
      )}
    </div>
  )
}

function StatsCard({ label, count, configKey }: { label: string; count: number; configKey: string }) {
  const cfg = STATS_CARD_CONFIG[configKey] ?? STATS_CARD_CONFIG.Total
  const Icon = cfg.icon

  return (
    <div className="rounded-xl bg-fm-surface-container p-4 transition-colors hover:bg-fm-surface-container-high">
      <div className="flex items-center gap-2">
        <span className={cn('flex h-6 w-6 items-center justify-center rounded-md', cfg.bg)}>
          <Icon className={cn('h-3.5 w-3.5', cfg.color)} />
        </span>
        <p className="text-xs font-medium text-fm-on-surface-variant">{label}</p>
      </div>
      <p className={cn('mt-2 font-display text-2xl font-bold', cfg.color)}>
        {count.toLocaleString()}
      </p>
    </div>
  )
}

function ExpandedDetails({ entry }: { entry: LogEntry }) {
  const standardKeys = new Set(['@t', '@mt', '@l', '@x', 'CorrelationId', 'SourceContext', 'Application', 'ElapsedMs'])
  const extraProps = Object.entries(entry).filter(([key]) => !standardKeys.has(key))

  return (
    <div className="mt-3 space-y-2.5 rounded-lg bg-fm-surface-container-low p-3">
      {entry.CorrelationId && (
        <div className="flex items-center gap-2">
          <span className="text-[11px] font-medium text-fm-on-surface-variant">Correlation ID:</span>
          <code className="rounded bg-fm-surface-container px-1.5 py-0.5 font-mono text-[11px] text-fm-on-surface-variant">
            {entry.CorrelationId as string}
          </code>
        </div>
      )}
      {entry['@x'] && (
        <div>
          <span className="text-[11px] font-medium text-red-400">Exception:</span>
          <pre className="mt-1 max-h-40 overflow-auto rounded-lg bg-red-500/[0.05] p-2.5 font-mono text-[11px] leading-relaxed text-red-400">
            {entry['@x'] as string}
          </pre>
        </div>
      )}
      {extraProps.length > 0 && (
        <div>
          <span className="text-[11px] font-medium text-fm-on-surface-variant">Properties:</span>
          <div className="mt-1.5 grid grid-cols-2 gap-x-4 gap-y-1">
            {extraProps.map(([key, value]) => (
              <div key={key} className="flex items-baseline gap-1.5">
                <span className="text-[11px] font-medium text-fm-on-surface-variant/70">{key}:</span>
                <span className="truncate text-[11px] text-fm-on-surface">{String(value)}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

function TraceDialog({
  correlationId,
  date,
  onClose,
}: {
  correlationId: string
  date?: string
  onClose: () => void
}) {
  const { data, isLoading } = useLogTrace(correlationId, date)

  return (
    <Dialog open onOpenChange={onClose}>
      <DialogContent
        className="max-w-3xl border-ghost shadow-ambient bg-fm-surface-container-highest"
        style={{
          backdropFilter: 'blur(24px)',
          WebkitBackdropFilter: 'blur(24px)',
        }}
      >
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 font-display text-sm text-fm-on-background">
            <Link2 className="h-4 w-4 text-fm-primary" />
            Request Trace
            <code className="rounded bg-fm-surface-container px-2 py-0.5 font-mono text-[11px] font-normal text-fm-on-surface-variant">
              {correlationId}
            </code>
          </DialogTitle>
        </DialogHeader>
        <ScrollArea className="max-h-[60vh]">
          {isLoading ? (
            <div className="space-y-2 p-4">
              {Array.from({ length: 4 }).map((_, i) => (
                <Skeleton key={i} className="h-12 rounded-lg bg-fm-surface-container" />
              ))}
            </div>
          ) : data && data.entries.length > 0 ? (
            <div className="space-y-1 p-1">
              {data.entries.map((entry, i) => {
                const logLevel = getLogLevel(entry)
                const config = LOG_LEVEL_CONFIG[logLevel] ?? LOG_LEVEL_CONFIG.Information
                const LevelIcon = config.icon

                return (
                  <div
                    key={i}
                    className={cn(
                      'flex items-start gap-3 rounded-lg p-3 text-sm',
                      logLevel === 'Error' && 'bg-red-500/[0.03]',
                      logLevel === 'Warning' && 'bg-amber-500/[0.03]',
                    )}
                  >
                    <span className={cn('mt-0.5 shrink-0', config.color)}>
                      <LevelIcon className="h-3.5 w-3.5" />
                    </span>
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2">
                        <span className="font-mono text-xs text-fm-on-surface-variant">
                          {formatTime(entry['@t'])}
                        </span>
                        <span className="text-xs text-fm-on-surface-variant/70">
                          {getShortSource(entry.SourceContext)}
                        </span>
                        {entry.ElapsedMs != null && (
                          <span className="rounded bg-fm-surface-container-high px-1.5 py-0.5 text-[10px] font-medium text-fm-on-surface-variant">
                            {Math.round(entry.ElapsedMs as number)}ms
                          </span>
                        )}
                      </div>
                      <p className="mt-0.5 text-fm-on-surface">{renderMessage(entry)}</p>
                      {entry['@x'] && (
                        <pre className="mt-1.5 max-h-24 overflow-auto rounded-lg bg-red-500/[0.05] p-2.5 font-mono text-[11px] leading-relaxed text-red-400">
                          {entry['@x'] as string}
                        </pre>
                      )}
                    </div>
                  </div>
                )
              })}
            </div>
          ) : (
            <p className="p-4 text-center text-sm text-fm-on-surface-variant">
              No entries found for this trace.
            </p>
          )}
        </ScrollArea>
      </DialogContent>
    </Dialog>
  )
}

// Helper functions

function getLogLevel(entry: LogEntry): LogLevel {
  return (entry['@l'] as LogLevel) ?? 'Information'
}

function formatTime(timestamp: string): string {
  try {
    return format(new Date(timestamp), 'HH:mm:ss.SSS')
  } catch {
    return timestamp
  }
}

function getShortSource(source?: string | unknown): string {
  if (!source || typeof source !== 'string') return ''
  const parts = source.split('.')
  return parts.length > 1 ? parts.slice(-2).join('.') : source
}

function renderMessage(entry: LogEntry): string {
  let msg = entry['@mt'] ?? ''
  msg = msg.replace(/\{(\w+)(?::[\w.]+)?\}/g, (_, key: string) => {
    const val = entry[key]
    return val !== undefined ? String(val) : `{${key}}`
  })
  return msg
}

function formatDateLabel(dateStr: string): string {
  try {
    const year = dateStr.slice(0, 4)
    const month = dateStr.slice(4, 6)
    const day = dateStr.slice(6, 8)
    return format(new Date(`${year}-${month}-${day}`), 'MMM d, yyyy')
  } catch {
    return dateStr
  }
}
