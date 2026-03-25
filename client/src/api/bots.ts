import { api } from './client'
import type { BotInfoDto, PromptTemplateResponse } from '@/types'

export const botsApi = {
  getAll: () => api.get('api/bots').json<BotInfoDto[]>(),

  getPromptTemplate: (
    botName: string,
    contentType = 'Image',
    language = 'en',
  ) =>
    api
      .get(`api/bots/${botName}/prompt`, {
        searchParams: { contentType, language },
      })
      .json<PromptTemplateResponse>(),
}
