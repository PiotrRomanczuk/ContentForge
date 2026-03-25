import type { ContentStatus, ContentType, Platform } from '@/types'

export const STATUS_CONFIG: Record<
  ContentStatus,
  { label: string; color: string; bg: string; hex: string; icon: string }
> = {
  Draft: { label: 'Draft', color: 'text-blue-400', bg: 'bg-blue-500/10', hex: '#3b82f6', icon: 'FileEdit' },
  Generated: { label: 'Generated', color: 'text-indigo-400', bg: 'bg-indigo-500/10', hex: '#6366f1', icon: 'Sparkles' },
  Rendered: { label: 'Rendered', color: 'text-purple-400', bg: 'bg-purple-500/10', hex: '#a855f7', icon: 'ImageIcon' },
  Queued: { label: 'Queued', color: 'text-amber-400', bg: 'bg-amber-500/10', hex: '#f59e0b', icon: 'Clock' },
  Publishing: { label: 'Publishing', color: 'text-orange-400', bg: 'bg-orange-500/10', hex: '#f97316', icon: 'Send' },
  Published: { label: 'Published', color: 'text-emerald-400', bg: 'bg-emerald-500/10', hex: '#10b981', icon: 'CheckCircle2' },
  Failed: { label: 'Failed', color: 'text-red-400', bg: 'bg-red-500/10', hex: '#ef4444', icon: 'XCircle' },
}

export const CONTENT_TYPE_CONFIG: Record<ContentType, { label: string; icon: string }> = {
  Image: { label: 'Image', icon: 'Image' },
  Carousel: { label: 'Carousel', icon: 'LayoutGrid' },
  Video: { label: 'Video', icon: 'Film' },
  Story: { label: 'Story', icon: 'BookOpen' },
  Text: { label: 'Text', icon: 'Type' },
}

export const PLATFORM_CONFIG: Record<Platform, { label: string; color: string }> = {
  Facebook: { label: 'Facebook', color: '#1877F2' },
  Instagram: { label: 'Instagram', color: '#E4405F' },
  TikTok: { label: 'TikTok', color: '#000000' },
  YouTube: { label: 'YouTube', color: '#FF0000' },
}
