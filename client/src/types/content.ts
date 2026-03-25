import type { ContentStatus, ContentType } from './enums'

export interface ContentItemDto {
  id: string
  botName: string
  category: string
  contentType: ContentType
  status: ContentStatus
  textContent: string
  mediaPath: string | null
  scheduledAt: string | null
  publishedAt: string | null
  createdAt: string
}

export interface ImportContentItemRequest {
  botName: string
  category: string
  contentType: ContentType
  textContent: string
  scheduledAt?: string
  properties?: Record<string, string>
}

export interface ImportContentRequest {
  items: ImportContentItemRequest[]
}

export interface ContentBatchResultDto {
  totalGenerated: number
  succeeded: number
  failed: number
  items: ContentItemDto[]
  errors: string[]
}

export interface ApprovalDecisionDto {
  contentItemId: string
  approved: boolean
  editedText?: string
  rescheduleAt?: string
}

export interface BulkApproveRequest {
  decisions: ApprovalDecisionDto[]
}

export interface BulkApprovalResultDto {
  approved: number
  rejected: number
  edited: number
}

export interface RenderRequestDto {
  templateName?: string
  parameters?: Record<string, string>
}

export interface RenderContentResultDto {
  contentItemId: string
  mediaPath: string
  thumbnailPath: string | null
  templateName: string
}

export interface PublishRequestDto {
  socialAccountId: string
}

export interface PublishContentResultDto {
  contentItemId: string
  isSuccess: boolean
  externalPostId: string | null
  errorMessage: string | null
  attemptedAt: string
}

export interface AvailableTemplateDto {
  name: string
  description: string
  supportedContentTypes: string[]
}
