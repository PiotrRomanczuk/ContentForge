import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { contentApi } from '@/api/content'
import { queryKeys } from '@/lib/query-keys'
import type {
  ContentStatus,
  ImportContentRequest,
  BulkApproveRequest,
  RenderRequestDto,
  PublishRequestDto,
} from '@/types'

export function useContentByStatus(status?: ContentStatus) {
  return useQuery({
    queryKey: queryKeys.content.list(status),
    queryFn: () => contentApi.getByStatus(status),
  })
}

export function useContentById(id: string) {
  return useQuery({
    queryKey: queryKeys.content.detail(id),
    queryFn: () => contentApi.getById(id),
    enabled: !!id,
  })
}

export function usePendingContent(skip = 0, take = 50) {
  return useQuery({
    queryKey: queryKeys.content.pending(skip, take),
    queryFn: () => contentApi.getPending(skip, take),
  })
}

export function useContentStats() {
  return useQuery({
    queryKey: queryKeys.content.stats(),
    queryFn: () => contentApi.getStats(),
    refetchInterval: 30_000,
  })
}

export function useContentTemplates(contentType?: string) {
  return useQuery({
    queryKey: queryKeys.content.templates(contentType),
    queryFn: () => contentApi.getTemplates(contentType),
  })
}

export function useImportContent() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (request: ImportContentRequest) => contentApi.import(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.content.all })
    },
  })
}

export function useBulkApprove() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (request: BulkApproveRequest) => contentApi.bulkApprove(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.content.all })
    },
  })
}

export function useRenderContent() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, request }: { id: string; request?: RenderRequestDto }) =>
      contentApi.render(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.content.all })
    },
  })
}

export function usePublishContent() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: PublishRequestDto }) =>
      contentApi.publish(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.content.all })
    },
  })
}
