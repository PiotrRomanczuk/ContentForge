import { useState, useCallback, useEffect, useMemo } from 'react'
import { usePendingContent, useBulkApprove } from '@/hooks/use-content'
import { ApprovalCard } from '@/components/content/ApprovalCard'
import { StatusBadge } from '@/components/content/StatusBadge'
import { ContentTypeBadge } from '@/components/content/ContentTypeBadge'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { Skeleton } from '@/components/ui/skeleton'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Check, X, CheckCircle2 } from 'lucide-react'
import { toast } from 'sonner'
import { format } from 'date-fns'
import type { ContentItemDto, ApprovalDecisionDto } from '@/types'

export function ApprovalQueuePage() {
  const { data: items, isLoading } = usePendingContent()
  const bulkApprove = useBulkApprove()

  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [focusedIndex, setFocusedIndex] = useState(0)
  const [decisions, setDecisions] = useState<Map<string, ApprovalDecisionDto>>(new Map())
  const [editedTexts, setEditedTexts] = useState<Map<string, string>>(new Map())

  const activeItems = useMemo(
    () => items?.filter((item) => !decisions.has(item.id)) ?? [],
    [items, decisions],
  )
  const focusedItem = activeItems[focusedIndex]

  const toggleSelect = useCallback((id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }, [])

  const selectAll = useCallback(() => {
    setSelectedIds(new Set(activeItems.map((item) => item.id)))
  }, [activeItems])

  const clearSelection = useCallback(() => {
    setSelectedIds(new Set())
  }, [])

  const addDecision = useCallback(
    (id: string, approved: boolean) => {
      const edited = editedTexts.get(id)
      setDecisions((prev) => {
        const next = new Map(prev)
        next.set(id, {
          contentItemId: id,
          approved,
          editedText: edited,
        })
        return next
      })
      setSelectedIds((prev) => {
        const next = new Set(prev)
        next.delete(id)
        return next
      })
    },
    [editedTexts],
  )

  const approveSelected = useCallback(() => {
    selectedIds.forEach((id) => addDecision(id, true))
  }, [selectedIds, addDecision])

  const rejectSelected = useCallback(() => {
    selectedIds.forEach((id) => addDecision(id, false))
  }, [selectedIds, addDecision])

  const submitDecisions = useCallback(async () => {
    if (decisions.size === 0) return
    try {
      const result = await bulkApprove.mutateAsync({
        decisions: Array.from(decisions.values()),
      })
      toast.success(
        `${result.approved} approved, ${result.rejected} rejected${result.edited > 0 ? `, ${result.edited} edited` : ''}`,
      )
      setDecisions(new Map())
      setEditedTexts(new Map())
      setSelectedIds(new Set())
    } catch {
      toast.error('Failed to submit decisions')
    }
  }, [decisions, bulkApprove])

  // Keyboard shortcuts
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.target instanceof HTMLTextAreaElement || e.target instanceof HTMLInputElement) return

      switch (e.key) {
        case 'j':
          setFocusedIndex((prev) => Math.min(prev + 1, activeItems.length - 1))
          break
        case 'k':
          setFocusedIndex((prev) => Math.max(prev - 1, 0))
          break
        case ' ':
          e.preventDefault()
          if (focusedItem) toggleSelect(focusedItem.id)
          break
        case 'a':
          if (focusedItem) addDecision(focusedItem.id, true)
          break
        case 'r':
          if (focusedItem) addDecision(focusedItem.id, false)
          break
      }

      if (e.metaKey && e.key === 'Enter') {
        e.preventDefault()
        submitDecisions()
      }
    }

    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [activeItems.length, focusedItem, toggleSelect, addDecision, submitDecisions])

  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-32 rounded-xl bg-fm-surface-container" />
        ))}
      </div>
    )
  }

  if (!items || items.length === 0) {
    return (
      <div className="flex flex-col items-center gap-4 pt-20">
        <CheckCircle2 className="h-16 w-16 text-emerald-400" />
        <h2 className="font-display text-xl font-semibold text-fm-on-surface">All clear!</h2>
        <p className="text-fm-on-surface-variant">No content awaiting review.</p>
      </div>
    )
  }

  return (
    <div className="flex gap-6">
      {/* Left: Card List (~60%) */}
      <div className="min-w-0 flex-[3] space-y-4">
        <div className="flex items-center justify-between">
          <p className="text-sm text-fm-on-surface-variant">{activeItems.length} items pending</p>
          <div className="flex items-center gap-2">
            <Button
              variant="ghost"
              size="sm"
              onClick={selectAll}
              className="h-7 text-xs text-fm-on-surface-variant hover:text-fm-on-surface"
            >
              Select All
            </Button>
            {selectedIds.size > 0 && (
              <Button
                variant="ghost"
                size="sm"
                onClick={clearSelection}
                className="h-7 text-xs text-fm-on-surface-variant hover:text-fm-on-surface"
              >
                Clear
              </Button>
            )}
          </div>
        </div>

        <ScrollArea className="h-[calc(100vh-220px)]">
          <div className="space-y-2 pr-4">
            {activeItems.map((item, index) => (
              <ApprovalCard
                key={item.id}
                item={item}
                isSelected={selectedIds.has(item.id)}
                isFocused={index === focusedIndex}
                editedText={editedTexts.get(item.id)}
                onToggleSelect={() => toggleSelect(item.id)}
                onApprove={() => addDecision(item.id, true)}
                onReject={() => addDecision(item.id, false)}
                onEditText={(text) => {
                  setEditedTexts((prev) => {
                    const next = new Map(prev)
                    next.set(item.id, text)
                    return next
                  })
                }}
                onClick={() => setFocusedIndex(index)}
              />
            ))}
          </div>
        </ScrollArea>

        {/* Floating Action Bar */}
        {(selectedIds.size > 0 || decisions.size > 0) && (
          <div className="fixed bottom-6 left-1/2 z-50 flex -translate-x-1/2 items-center gap-3 rounded-xl bg-fm-surface-container-highest/95 px-5 py-3 shadow-ambient backdrop-blur-xl">
            {selectedIds.size > 0 && (
              <>
                <div className="flex items-center gap-2">
                  <Checkbox
                    checked={selectedIds.size === activeItems.length}
                    onCheckedChange={() =>
                      selectedIds.size === activeItems.length ? clearSelection() : selectAll()
                    }
                  />
                  <span className="text-sm font-medium text-fm-on-surface">
                    {selectedIds.size} of {activeItems.length} selected
                  </span>
                </div>
                <div className="mx-1 h-5 w-px bg-fm-outline-variant/30" />
                <Button
                  size="sm"
                  onClick={approveSelected}
                  className="gap-1 bg-emerald-600 text-white hover:bg-emerald-500 hover:shadow-[0_0_16px_rgba(16,185,129,0.25)]"
                >
                  <Check className="h-3 w-3" />
                  Approve Selected
                </Button>
                <Button
                  size="sm"
                  variant="outline"
                  onClick={rejectSelected}
                  className="gap-1 border-fm-error/30 text-fm-error hover:bg-fm-error/10"
                >
                  <X className="h-3 w-3" />
                  Reject Selected
                </Button>
              </>
            )}
            {decisions.size > 0 && (
              <>
                {selectedIds.size > 0 && <div className="mx-1 h-5 w-px bg-fm-outline-variant/30" />}
                <span className="text-sm text-fm-on-surface-variant">
                  {decisions.size} decision(s) pending
                </span>
                <Button
                  size="sm"
                  onClick={submitDecisions}
                  disabled={bulkApprove.isPending}
                  className="gap-1 bg-gradient-to-br from-fm-primary-dim to-fm-primary text-fm-background hover:glow-primary"
                >
                  {bulkApprove.isPending ? 'Submitting...' : 'Submit (\u2318\u21B5)'}
                </Button>
              </>
            )}
          </div>
        )}
      </div>

      {/* Right: Preview Panel (~40%, sticky) */}
      <div className="hidden w-[380px] shrink-0 lg:block">
        {focusedItem ? (
          <PreviewPanel item={focusedItem} editedText={editedTexts.get(focusedItem.id)} />
        ) : (
          <div className="flex h-48 items-center justify-center rounded-xl bg-fm-surface-container-low text-sm text-fm-on-surface-variant">
            Select an item to preview
          </div>
        )}
      </div>
    </div>
  )
}

function PreviewPanel({
  item,
  editedText,
}: {
  item: ContentItemDto
  editedText?: string
}) {
  return (
    <div className="sticky top-0 space-y-4 rounded-xl bg-fm-surface-container-low p-5">
      <div className="space-y-2">
        <div className="flex items-center gap-2">
          <StatusBadge status={item.status} />
          <ContentTypeBadge contentType={item.contentType} />
        </div>
        <h3 className="font-display text-sm font-semibold text-fm-on-surface">{item.botName}</h3>
        <p className="text-xs text-fm-on-surface-variant">{item.category}</p>
      </div>

      <div>
        <p className="mb-1.5 text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
          Content
        </p>
        <p className="whitespace-pre-wrap text-sm leading-relaxed text-fm-on-surface">
          {editedText ?? item.textContent}
        </p>
      </div>

      {item.scheduledAt && (
        <div>
          <p className="mb-1 text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
            Scheduled
          </p>
          <p className="text-sm text-fm-on-surface">{format(new Date(item.scheduledAt), 'PPp')}</p>
        </div>
      )}

      <div>
        <p className="mb-1 text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
          Created
        </p>
        <p className="text-sm text-fm-on-surface">{format(new Date(item.createdAt), 'PPp')}</p>
      </div>

      <div className="rounded-lg bg-fm-surface-container p-3">
        <p className="text-[11px] font-medium uppercase tracking-wider text-fm-on-surface-variant">
          Keyboard Shortcuts
        </p>
        <div className="mt-2 space-y-1.5 text-xs text-fm-on-surface-variant">
          <p>
            <kbd className="rounded bg-fm-surface-container-high px-1.5 py-0.5 font-mono text-[11px] text-fm-on-surface">
              j/k
            </kbd>{' '}
            Navigate
          </p>
          <p>
            <kbd className="rounded bg-fm-surface-container-high px-1.5 py-0.5 font-mono text-[11px] text-fm-on-surface">
              Space
            </kbd>{' '}
            Toggle select
          </p>
          <p>
            <kbd className="rounded bg-fm-surface-container-high px-1.5 py-0.5 font-mono text-[11px] text-fm-on-surface">
              a
            </kbd>{' '}
            Approve
          </p>
          <p>
            <kbd className="rounded bg-fm-surface-container-high px-1.5 py-0.5 font-mono text-[11px] text-fm-on-surface">
              r
            </kbd>{' '}
            Reject
          </p>
          <p>
            <kbd className="rounded bg-fm-surface-container-high px-1.5 py-0.5 font-mono text-[11px] text-fm-on-surface">
              {'\u2318\u21B5'}
            </kbd>{' '}
            Submit all
          </p>
        </div>
      </div>
    </div>
  )
}
