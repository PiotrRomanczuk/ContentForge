import { api } from './client'
import type { LogPageResult, LogStats, LogTraceResult } from '@/types/logs'

interface LogsQuery {
  date?: string
  level?: string
  correlationId?: string
  source?: string
  search?: string
  skip?: number
  take?: number
}

export const logsApi = {
  getLogs: (params: LogsQuery = {}) => {
    const searchParams: Record<string, string> = {}
    if (params.date) searchParams.date = params.date
    if (params.level) searchParams.level = params.level
    if (params.correlationId) searchParams.correlationId = params.correlationId
    if (params.source) searchParams.source = params.source
    if (params.search) searchParams.search = params.search
    if (params.skip !== undefined) searchParams.skip = String(params.skip)
    if (params.take !== undefined) searchParams.take = String(params.take)

    return api.get('api/logs', { searchParams }).json<LogPageResult>()
  },

  getStats: (date?: string) =>
    api
      .get('api/logs/stats', {
        searchParams: date ? { date } : {},
      })
      .json<LogStats>(),

  trace: (correlationId: string, date?: string) =>
    api
      .get(`api/logs/trace/${correlationId}`, {
        searchParams: date ? { date } : {},
      })
      .json<LogTraceResult>(),
}
