import type { ContentType } from './enums'

export interface BotInfoDto {
  name: string
  category: string
  description: string
  supportedContentTypes: ContentType[]
}

export interface PromptTemplateResponse {
  botName: string
  category: string
  contentType: string
  language: string
  promptTemplate: string
}
