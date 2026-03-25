import { useQuery } from '@tanstack/react-query'
import { logsApi } from '@/api/logs'

const LOG_KEYS = {
  all: ['logs'] as const,
  list: (params: Record<string, unknown>) => [...LOG_KEYS.all, 'list', params] as const,
  stats: (date?: string) => [...LOG_KEYS.all, 'stats', date] as const,
  trace: (correlationId: string, date?: string) =>
    [...LOG_KEYS.all, 'trace', correlationId, date] as const,
}

export function useLogs(params: {
  date?: string
  level?: string
  correlationId?: string
  source?: string
  search?: string
  skip?: number
  take?: number
}) {
  return useQuery({
    queryKey: LOG_KEYS.list(params),
    queryFn: () => logsApi.getLogs(params),
    refetchInterval: 10_000,
  })
}

export function useLogStats(date?: string) {
  return useQuery({
    queryKey: LOG_KEYS.stats(date),
    queryFn: () => logsApi.getStats(date),
    refetchInterval: 10_000,
  })
}

export function useLogTrace(correlationId: string, date?: string) {
  return useQuery({
    queryKey: LOG_KEYS.trace(correlationId, date),
    queryFn: () => logsApi.trace(correlationId, date),
    enabled: !!correlationId,
  })
}
