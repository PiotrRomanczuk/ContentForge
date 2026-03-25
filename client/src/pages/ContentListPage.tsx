import { useState, useMemo } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useContentByStatus } from '@/hooks/use-content'
import { useBots } from '@/hooks/use-bots'
import { ContentTable } from '@/components/content/ContentTable'
import { ContentFilters } from '@/components/content/ContentFilters'
import { Skeleton } from '@/components/ui/skeleton'
import type { ContentStatus, ContentType } from '@/types'

export function ContentListPage() {
  const [searchParams] = useSearchParams()
  const initialStatus = searchParams.get('status') ?? 'all'

  const [statusFilter, setStatusFilter] = useState(initialStatus)
  const [contentTypeFilter, setContentTypeFilter] = useState('all')
  const [botFilter, setBotFilter] = useState('all')
  const [search, setSearch] = useState('')

  const { data: bots } = useBots()
  const { data: content, isLoading } = useContentByStatus(
    statusFilter !== 'all' ? (statusFilter as ContentStatus) : undefined,
  )

  const filteredContent = useMemo(() => {
    if (!content) return []
    return content.filter((item) => {
      if (contentTypeFilter !== 'all' && item.contentType !== (contentTypeFilter as ContentType))
        return false
      if (botFilter !== 'all' && item.botName !== botFilter) return false
      if (search && !item.textContent.toLowerCase().includes(search.toLowerCase())) return false
      return true
    })
  }, [content, contentTypeFilter, botFilter, search])

  return (
    <div className="space-y-5">
      <ContentFilters
        status={statusFilter}
        onStatusChange={setStatusFilter}
        contentType={contentTypeFilter}
        onContentTypeChange={setContentTypeFilter}
        botName={botFilter}
        onBotNameChange={setBotFilter}
        search={search}
        onSearchChange={setSearch}
        bots={bots}
      />

      {isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full rounded-lg bg-fm-surface-container" />
          ))}
        </div>
      ) : (
        <ContentTable data={filteredContent} />
      )}
    </div>
  )
}
