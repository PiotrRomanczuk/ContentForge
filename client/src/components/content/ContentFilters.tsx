import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Search, X } from 'lucide-react'
import { ContentStatus, ContentType } from '@/types'
import type { BotInfoDto } from '@/types'

interface ContentFiltersProps {
  status: string
  onStatusChange: (status: string) => void
  contentType: string
  onContentTypeChange: (type: string) => void
  botName: string
  onBotNameChange: (name: string) => void
  search: string
  onSearchChange: (search: string) => void
  bots?: BotInfoDto[]
}

export function ContentFilters({
  status,
  onStatusChange,
  contentType,
  onContentTypeChange,
  botName,
  onBotNameChange,
  search,
  onSearchChange,
  bots,
}: ContentFiltersProps) {
  const hasFilters = status !== 'all' || contentType !== 'all' || botName !== 'all' || search !== ''

  return (
    <div className="flex flex-wrap items-center gap-3">
      <Select value={status} onValueChange={onStatusChange}>
        <SelectTrigger className="w-[140px] border-none bg-fm-surface-container-high text-fm-on-surface">
          <SelectValue placeholder="Status" />
        </SelectTrigger>
        <SelectContent className="border-fm-outline-variant/15 bg-fm-surface-container-highest shadow-ambient backdrop-blur-xl">
          <SelectItem value="all">All Statuses</SelectItem>
          {Object.values(ContentStatus).map((s) => (
            <SelectItem key={s} value={s}>
              {s}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select value={contentType} onValueChange={onContentTypeChange}>
        <SelectTrigger className="w-[140px] border-none bg-fm-surface-container-high text-fm-on-surface">
          <SelectValue placeholder="Type" />
        </SelectTrigger>
        <SelectContent className="border-fm-outline-variant/15 bg-fm-surface-container-highest shadow-ambient backdrop-blur-xl">
          <SelectItem value="all">All Types</SelectItem>
          {Object.values(ContentType).map((t) => (
            <SelectItem key={t} value={t}>
              {t}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      {bots && bots.length > 0 && (
        <Select value={botName} onValueChange={onBotNameChange}>
          <SelectTrigger className="w-[160px] border-none bg-fm-surface-container-high text-fm-on-surface">
            <SelectValue placeholder="Bot" />
          </SelectTrigger>
          <SelectContent className="border-fm-outline-variant/15 bg-fm-surface-container-highest shadow-ambient backdrop-blur-xl">
            <SelectItem value="all">All Bots</SelectItem>
            {bots.map((bot) => (
              <SelectItem key={bot.name} value={bot.name}>
                {bot.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      )}

      <div className="relative">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-fm-on-surface-variant" />
        <Input
          placeholder="Search content..."
          value={search}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => onSearchChange(e.target.value)}
          className="w-[220px] border-none bg-fm-surface-container-high pl-9 text-fm-on-surface placeholder:text-fm-on-surface-variant/60"
        />
      </div>

      {hasFilters && (
        <Button
          variant="ghost"
          size="sm"
          onClick={() => {
            onStatusChange('all')
            onContentTypeChange('all')
            onBotNameChange('all')
            onSearchChange('')
          }}
          className="h-8 gap-1 text-xs text-fm-on-surface-variant hover:text-fm-on-surface"
        >
          <X className="h-3 w-3" />
          Clear
        </Button>
      )}
    </div>
  )
}
