import { Image, LayoutGrid, Film, BookOpen, Type } from 'lucide-react'
import { cn } from '@/lib/utils'
import type { ContentType } from '@/types'

const iconMap = {
  Image: Image,
  Carousel: LayoutGrid,
  Video: Film,
  Story: BookOpen,
  Text: Type,
} as const

interface ContentTypeBadgeProps {
  contentType: ContentType
  className?: string
  showLabel?: boolean
}

export function ContentTypeBadge({ contentType, className, showLabel = true }: ContentTypeBadgeProps) {
  const Icon = iconMap[contentType] ?? Type

  return (
    <span
      className={cn(
        'inline-flex items-center gap-1 rounded-full border border-fm-outline-variant/15 px-2 py-0.5 text-xs text-fm-on-surface-variant',
        className,
      )}
    >
      <Icon className="h-3 w-3" />
      {showLabel && contentType}
    </span>
  )
}
