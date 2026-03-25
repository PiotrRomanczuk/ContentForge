import { Globe, Camera, Play, Video } from 'lucide-react'
import { cn } from '@/lib/utils'
import { PLATFORM_CONFIG } from '@/lib/constants'
import type { Platform } from '@/types'

interface PlatformIconProps {
  platform: Platform
  className?: string
  showLabel?: boolean
}

export function PlatformIcon({ platform, className, showLabel = false }: PlatformIconProps) {
  const config = PLATFORM_CONFIG[platform]

  const iconClass = cn('h-4 w-4', className)

  const icon = (() => {
    switch (platform) {
      case 'Facebook':
        return <Globe className={iconClass} style={{ color: config.color }} />
      case 'Instagram':
        return <Camera className={iconClass} style={{ color: config.color }} />
      case 'YouTube':
        return <Play className={iconClass} style={{ color: config.color }} />
      case 'TikTok':
        return <Video className={iconClass} style={{ color: config.color }} />
    }
  })()

  return (
    <span className="inline-flex items-center gap-1.5">
      {icon}
      {showLabel && <span className="text-xs">{config.label}</span>}
    </span>
  )
}
