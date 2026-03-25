import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  CartesianGrid,
  ResponsiveContainer,
  Cell,
} from 'recharts'
import { STATUS_CONFIG } from '@/lib/constants'
import { CONTENT_STATUS_ORDER } from '@/types'
import type { ContentStatus } from '@/types'

interface PipelineChartProps {
  stats: Record<ContentStatus, number> | undefined
}

/** Custom tooltip styled with Forge Midnight surface hierarchy */
function ChartTooltip({
  active,
  payload,
  label,
}: {
  active?: boolean
  payload?: Array<{ value: number; payload: { fill: string } }>
  label?: string
}) {
  if (!active || !payload?.length) return null

  return (
    <div className="rounded-lg bg-fm-surface-container-highest px-3 py-2 shadow-ambient">
      <p className="text-xs font-medium text-fm-on-surface">{label}</p>
      <p
        className="mt-0.5 font-display text-sm font-bold"
        style={{ color: payload[0].payload.fill }}
      >
        {payload[0].value} items
      </p>
    </div>
  )
}

export function PipelineChart({ stats }: PipelineChartProps) {
  const data = CONTENT_STATUS_ORDER.map((status) => ({
    name: status,
    count: stats?.[status] ?? 0,
    fill: STATUS_CONFIG[status].hex,
  }))

  return (
    <div className="rounded-xl bg-fm-surface-container p-5">
      <h3 className="mb-4 font-display text-sm font-semibold text-fm-on-surface">
        Content Pipeline
      </h3>
      <ResponsiveContainer width="100%" height={220}>
        <BarChart data={data} barCategoryGap="20%">
          <CartesianGrid
            strokeDasharray="3 3"
            stroke="var(--fm-outline-variant)"
            strokeOpacity={0.15}
            vertical={false}
          />
          <XAxis
            dataKey="name"
            tick={{ fontSize: 11, fill: 'var(--fm-on-surface-variant)' }}
            axisLine={false}
            tickLine={false}
          />
          <YAxis
            allowDecimals={false}
            tick={{ fontSize: 11, fill: 'var(--fm-on-surface-variant)' }}
            axisLine={false}
            tickLine={false}
            width={32}
          />
          <Tooltip
            content={<ChartTooltip />}
            cursor={{ fill: 'rgba(64, 72, 93, 0.1)' }}
          />
          <Bar dataKey="count" radius={[6, 6, 0, 0]}>
            {data.map((entry) => (
              <Cell key={entry.name} fill={entry.fill} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  )
}
