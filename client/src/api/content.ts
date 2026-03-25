import { api } from './client'
import type {
  ContentItemDto,
  ContentBatchResultDto,
  ImportContentRequest,
  BulkApproveRequest,
  BulkApprovalResultDto,
  RenderRequestDto,
  RenderContentResultDto,
  PublishRequestDto,
  PublishContentResultDto,
  AvailableTemplateDto,
  ContentStatus,
} from '@/types'

export const contentApi = {
  getByStatus: (status?: ContentStatus) =>
    api
      .get('api/content', {
        searchParams: status ? { status } : {},
      })
      .json<ContentItemDto[]>(),

  getById: (id: string) => api.get(`api/content/${id}`).json<ContentItemDto>(),

  getPending: (skip = 0, take = 50) =>
    api
      .get('api/content/pending', {
        searchParams: { skip: String(skip), take: String(take) },
      })
      .json<ContentItemDto[]>(),

  import: (request: ImportContentRequest) =>
    api.post('api/content/import', { json: request }).json<ContentBatchResultDto>(),

  bulkApprove: (request: BulkApproveRequest) =>
    api.post('api/content/approve', { json: request }).json<BulkApprovalResultDto>(),

  getStats: () => api.get('api/content/stats').json<Record<ContentStatus, number>>(),

  render: (id: string, request?: RenderRequestDto) =>
    api.post(`api/content/${id}/render`, { json: request ?? {} }).json<RenderContentResultDto>(),

  publish: (id: string, request: PublishRequestDto) =>
    api.post(`api/content/${id}/publish`, { json: request }).json<PublishContentResultDto>(),

  getTemplates: (contentType?: string) =>
    api
      .get('api/content/templates', {
        searchParams: contentType ? { contentType } : {},
      })
      .json<AvailableTemplateDto[]>(),
}
