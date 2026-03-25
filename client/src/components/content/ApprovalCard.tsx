import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { Textarea } from '@/components/ui/textarea'
import { ContentTypeBadge } from './ContentTypeBadge'
import { Check, X, Pencil } from 'lucide-react'
import { cn } from '@/lib/utils'
import { format } from 'date-fns'
import type { ContentItemDto } from '@/types'

interface ApprovalCardProps {
  item: ContentItemDto
  isSelected: boolean
  isFocused: boolean
  editedText?: string
  onToggleSelect: () => void
  onApprove: () => void
  onReject: () => void
  onEditText: (text: string) => void
  onClick: () => void
}

export function ApprovalCard({
  item,
  isSelected,
  isFocused,
  editedText,
  onToggleSelect,
  onApprove,
  onReject,
  onEditText,
  onClick,
}: ApprovalCardProps) {
  const [isEditing, setIsEditing] = useState(false)
  const hasBeenEdited = editedText !== undefined

  return (
    <div
      className={cn(
        'cursor-pointer rounded-xl bg-fm-surface-container p-4 transition-all',
        'hover:bg-fm-surface-container-high',
        isFocused && 'ring-1 ring-fm-primary-dim/60',
        isSelected && 'shadow-[inset_0_0_0_1px] shadow-fm-primary/30 bg-fm-surface-container-high',
        hasBeenEdited && 'shadow-[inset_0_0_0_1px] shadow-amber-500/30',
      )}
      onClick={onClick}
    >
      <div className="flex items-start gap-3">
        <Checkbox
          checked={isSelected}
          onCheckedChange={() => onToggleSelect()}
          onClick={(e: React.MouseEvent) => e.stopPropagation()}
          className="mt-0.5"
        />

        <div className="min-w-0 flex-1">
          <div className="mb-2 flex items-center gap-2">
            <span className="text-sm font-medium text-fm-on-surface">{item.botName}</span>
            <ContentTypeBadge contentType={item.contentType} showLabel={false} />
            {hasBeenEdited && (
              <Pencil className="h-3 w-3 text-amber-400" />
            )}
            {item.scheduledAt && (
              <span className="ml-auto text-xs text-fm-on-surface-variant">
                {format(new Date(item.scheduledAt), 'MMM d, HH:mm')}
              </span>
            )}
          </div>

          {isEditing ? (
            <Textarea
              value={editedText ?? item.textContent}
              onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => onEditText(e.target.value)}
              onClick={(e: React.MouseEvent) => e.stopPropagation()}
              onBlur={() => setIsEditing(false)}
              className="min-h-[80px] border-none bg-fm-surface-container-highest text-sm text-fm-on-surface"
              autoFocus
            />
          ) : (
            <p className="line-clamp-3 text-sm leading-relaxed text-fm-on-surface-variant">{editedText ?? item.textContent}</p>
          )}

          <div className="mt-3 flex items-center gap-2">
            <Button
              size="sm"
              variant="ghost"
              className="h-7 gap-1 text-xs text-emerald-400 hover:bg-emerald-500/10 hover:text-emerald-300"
              onClick={(e: React.MouseEvent) => {
                e.stopPropagation()
                onApprove()
              }}
            >
              <Check className="h-3 w-3" />
              Approve
            </Button>
            <Button
              size="sm"
              variant="ghost"
              className="h-7 gap-1 text-xs text-fm-error hover:bg-fm-error/10 hover:text-fm-error"
              onClick={(e: React.MouseEvent) => {
                e.stopPropagation()
                onReject()
              }}
            >
              <X className="h-3 w-3" />
              Reject
            </Button>
            <Button
              size="sm"
              variant="ghost"
              className="h-7 gap-1 text-xs text-fm-primary hover:bg-fm-primary/10 hover:text-fm-primary"
              onClick={(e: React.MouseEvent) => {
                e.stopPropagation()
                setIsEditing(!isEditing)
              }}
            >
              <Pencil className="h-3 w-3" />
              Edit
            </Button>
          </div>
        </div>
      </div>
    </div>
  )
}
