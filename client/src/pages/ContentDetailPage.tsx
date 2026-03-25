import { useParams, Link } from 'react-router-dom'
import { useContentById } from '@/hooks/use-content'
import { StatusBadge } from '@/components/content/StatusBadge'
import { ContentTypeBadge } from '@/components/content/ContentTypeBadge'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { ArrowLeft } from 'lucide-react'
import { format } from 'date-fns'

export function ContentDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: item, isLoading, error } = useContentById(id ?? '')

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-48 rounded-lg bg-fm-surface-container" />
        <Skeleton className="h-64 rounded-xl bg-fm-surface-container" />
      </div>
    )
  }

  if (error || !item) {
    return (
      <div className="flex flex-col items-center gap-4 pt-12">
        <p className="text-fm-on-surface-variant">Content item not found.</p>
        <Button asChild variant="ghost" className="text-fm-primary hover:text-fm-primary">
          <Link to="/content">Back to Pipeline</Link>
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Back button + badges */}
      <div className="flex items-center gap-3">
        <Button
          asChild
          variant="ghost"
          size="icon"
          className="h-8 w-8 text-fm-on-surface-variant hover:bg-fm-surface-container-high hover:text-fm-on-surface"
        >
          <Link to="/content">
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div className="flex items-center gap-2">
          <StatusBadge status={item.status} />
          <ContentTypeBadge contentType={item.contentType} />
        </div>
      </div>

      {/* Two-column layout: 65% content / 35% details */}
      <div className="grid gap-6 lg:grid-cols-[65fr_35fr]">
        {/* Content card */}
        <div className="rounded-xl bg-fm-surface-container p-6">
          <h2 className="mb-4 text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
            Content
          </h2>
          <p className="whitespace-pre-wrap text-sm leading-relaxed text-fm-on-surface">
            {item.textContent}
          </p>
        </div>

        {/* Details card */}
        <div className="rounded-xl bg-fm-surface-container p-6">
          <h2 className="mb-4 text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
            Details
          </h2>
          <div className="space-y-4">
            <DetailRow label="Bot" value={item.botName} />
            <DetailRow label="Category" value={item.category} />
            <DetailRow label="Created" value={format(new Date(item.createdAt), 'PPp')} />
            {item.scheduledAt && (
              <DetailRow label="Scheduled" value={format(new Date(item.scheduledAt), 'PPp')} />
            )}
            {item.publishedAt && (
              <DetailRow label="Published" value={format(new Date(item.publishedAt), 'PPp')} />
            )}
            {item.mediaPath && (
              <DetailRow label="Media Path" value={item.mediaPath} truncate />
            )}
          </div>
        </div>
      </div>
    </div>
  )
}

function DetailRow({
  label,
  value,
  truncate = false,
}: {
  label: string
  value: string
  truncate?: boolean
}) {
  return (
    <div>
      <p className="text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
        {label}
      </p>
      <p className={`mt-0.5 text-sm text-fm-on-surface ${truncate ? 'truncate' : ''}`}>{value}</p>
    </div>
  )
}
