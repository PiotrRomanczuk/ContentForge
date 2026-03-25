export interface LogEntry {
  '@t': string
  '@mt': string
  '@l'?: string
  '@x'?: string
  CorrelationId?: string
  SourceContext?: string
  Application?: string
  ElapsedMs?: number
  [key: string]: unknown
}

export interface LogPageResult {
  entries: LogEntry[]
  total: number
  availableDates: string[]
}

export interface LogStats {
  total: number
  debug: number
  information: number
  warning: number
  error: number
}

export interface LogTraceResult {
  entries: LogEntry[]
  correlationId: string
}

export type LogLevel = 'Debug' | 'Information' | 'Warning' | 'Error'
