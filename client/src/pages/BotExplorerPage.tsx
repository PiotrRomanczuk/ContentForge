import { useState } from 'react'
import { useBots, usePromptTemplate } from '@/hooks/use-bots'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Bot, Copy, Check } from 'lucide-react'
import { ContentTypeBadge } from '@/components/content/ContentTypeBadge'
import { toast } from 'sonner'
import { cn } from '@/lib/utils'
import type { BotInfoDto } from '@/types'

/** Category color mapping for bot category badges */
const CATEGORY_COLORS: Record<string, { text: string; bg: string }> = {
  Education: { text: 'text-blue-400', bg: 'bg-blue-500/10' },
  Lifestyle: { text: 'text-purple-400', bg: 'bg-purple-500/10' },
  Science: { text: 'text-emerald-400', bg: 'bg-emerald-500/10' },
  Technology: { text: 'text-cyan-400', bg: 'bg-cyan-500/10' },
}

function getCategoryColor(category: string) {
  return CATEGORY_COLORS[category] ?? { text: 'text-fm-on-surface-variant', bg: 'bg-fm-surface-container-high' }
}

export function BotExplorerPage() {
  const { data: bots, isLoading } = useBots()
  const [selectedBot, setSelectedBot] = useState<BotInfoDto | null>(null)

  if (isLoading) {
    return (
      <div className="flex gap-6">
        <div className="grid flex-1 gap-4 sm:grid-cols-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-44 rounded-xl bg-fm-surface-container" />
          ))}
        </div>
        <div className="hidden w-[420px] shrink-0 lg:block">
          <Skeleton className="h-96 rounded-xl bg-fm-surface-container-low" />
        </div>
      </div>
    )
  }

  if (!bots || bots.length === 0) {
    return (
      <div className="flex flex-col items-center gap-4 pt-16">
        <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-fm-surface-container">
          <Bot className="h-8 w-8 text-fm-on-surface-variant" />
        </div>
        <p className="text-fm-on-surface-variant">No bots registered.</p>
      </div>
    )
  }

  return (
    <div className="flex gap-6">
      {/* LEFT: Bot cards grid (~55%) */}
      <div className="grid flex-[1.2] gap-4 self-start sm:grid-cols-2">
        {bots.map((bot) => {
          const isSelected = selectedBot?.name === bot.name
          const catColor = getCategoryColor(bot.category)

          return (
            <button
              key={bot.name}
              type="button"
              className={cn(
                'group rounded-xl bg-fm-surface-container p-5 text-left transition-all hover:bg-fm-surface-container-high',
                isSelected && 'ring-2 ring-fm-primary/40 bg-fm-surface-container-high',
              )}
              onClick={() => setSelectedBot(bot)}
            >
              {/* Bot icon + name */}
              <div className="flex items-center gap-2.5">
                <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-fm-surface-bright">
                  <Bot className="h-4 w-4 text-fm-primary" />
                </div>
                <h3 className="font-display text-sm font-bold text-fm-on-surface">
                  {bot.name}
                </h3>
              </div>

              {/* Category badge */}
              <span
                className={cn(
                  'mt-3 inline-flex rounded-full px-2.5 py-0.5 text-[11px] font-medium',
                  catColor.text,
                  catColor.bg,
                )}
              >
                {bot.category}
              </span>

              {/* Description */}
              <p className="mt-2 text-sm leading-relaxed text-fm-on-surface-variant">
                {bot.description}
              </p>

              {/* Content type badges */}
              <div className="mt-3 flex flex-wrap gap-2">
                {bot.supportedContentTypes.map((type) => (
                  <ContentTypeBadge key={type} contentType={type} />
                ))}
              </div>
            </button>
          )
        })}
      </div>

      {/* RIGHT: Prompt Template Viewer (~45%, sticky) */}
      <div className="hidden w-[420px] shrink-0 lg:block">
        {selectedBot ? (
          <PromptViewer bot={selectedBot} />
        ) : (
          <div className="sticky top-0 flex h-56 items-center justify-center rounded-xl bg-fm-surface-container-low">
            <p className="text-sm text-fm-on-surface-variant">
              Select a bot to view prompt templates
            </p>
          </div>
        )}
      </div>
    </div>
  )
}

function PromptViewer({ bot }: { bot: BotInfoDto }) {
  const [contentType, setContentType] = useState<string>(bot.supportedContentTypes[0] ?? 'Image')
  const [language, setLanguage] = useState('en')
  const [copied, setCopied] = useState(false)

  const { data: template, isLoading } = usePromptTemplate(bot.name, contentType, language)

  const handleCopy = async () => {
    if (template?.promptTemplate) {
      await navigator.clipboard.writeText(template.promptTemplate)
      setCopied(true)
      toast.success('Copied to clipboard')
      setTimeout(() => setCopied(false), 2000)
    }
  }

  return (
    <div className="sticky top-0 rounded-xl bg-fm-surface-container-low p-5">
      {/* Bot title */}
      <h3 className="font-display text-sm font-bold text-fm-on-surface">
        {bot.name}
      </h3>
      <p className="mt-0.5 text-xs text-fm-on-surface-variant">Prompt Template</p>

      {/* Controls row */}
      <div className="mt-4 flex items-center gap-2">
        <Select value={contentType} onValueChange={setContentType}>
          <SelectTrigger className="h-8 bg-fm-surface-container text-xs text-fm-on-surface">
            <SelectValue />
          </SelectTrigger>
          <SelectContent className="bg-fm-surface-container-highest">
            {bot.supportedContentTypes.map((t) => (
              <SelectItem key={t} value={t}>{t}</SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select value={language} onValueChange={setLanguage}>
          <SelectTrigger className="h-8 w-[80px] bg-fm-surface-container text-xs text-fm-on-surface">
            <SelectValue />
          </SelectTrigger>
          <SelectContent className="bg-fm-surface-container-highest">
            <SelectItem value="en">EN</SelectItem>
            <SelectItem value="pl">PL</SelectItem>
          </SelectContent>
        </Select>
        <button
          type="button"
          onClick={handleCopy}
          className="ml-auto flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-fm-on-surface-variant transition-colors hover:bg-fm-surface-bright hover:text-fm-on-surface"
          aria-label="Copy prompt template"
        >
          {copied ? (
            <Check className="h-3.5 w-3.5 text-emerald-400" />
          ) : (
            <Copy className="h-3.5 w-3.5" />
          )}
        </button>
      </div>

      {/* Code block */}
      <div className="mt-4">
        {isLoading ? (
          <Skeleton className="h-48 rounded-lg bg-fm-surface-container" />
        ) : template ? (
          <ScrollArea className="h-[calc(100vh-320px)]">
            <pre className="whitespace-pre-wrap rounded-lg bg-[#000000] p-4 font-mono text-xs leading-relaxed text-fm-on-surface">
              {template.promptTemplate}
            </pre>
          </ScrollArea>
        ) : (
          <div className="flex h-32 items-center justify-center rounded-lg bg-fm-surface-container">
            <p className="text-sm text-fm-on-surface-variant">
              No template available for this combination.
            </p>
          </div>
        )}
      </div>
    </div>
  )
}
