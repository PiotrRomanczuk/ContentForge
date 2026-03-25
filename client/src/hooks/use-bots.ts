import { useQuery } from '@tanstack/react-query'
import { botsApi } from '@/api/bots'
import { queryKeys } from '@/lib/query-keys'

export function useBots() {
  return useQuery({
    queryKey: queryKeys.bots.all,
    queryFn: () => botsApi.getAll(),
  })
}

export function usePromptTemplate(botName: string, contentType = 'Image', language = 'en') {
  return useQuery({
    queryKey: queryKeys.bots.prompt(botName, contentType, language),
    queryFn: () => botsApi.getPromptTemplate(botName, contentType, language),
    enabled: !!botName,
  })
}
