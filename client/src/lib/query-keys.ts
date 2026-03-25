export const queryKeys = {
  content: {
    all: ['content'] as const,
    lists: () => [...queryKeys.content.all, 'list'] as const,
    list: (status?: string) => [...queryKeys.content.lists(), { status }] as const,
    details: () => [...queryKeys.content.all, 'detail'] as const,
    detail: (id: string) => [...queryKeys.content.details(), id] as const,
    pending: (skip: number, take: number) =>
      [...queryKeys.content.all, 'pending', { skip, take }] as const,
    stats: () => [...queryKeys.content.all, 'stats'] as const,
    templates: (contentType?: string) =>
      [...queryKeys.content.all, 'templates', { contentType }] as const,
  },
  socialAccounts: {
    all: ['social-accounts'] as const,
    list: (platform?: string) => [...queryKeys.socialAccounts.all, { platform }] as const,
    detail: (id: string) => [...queryKeys.socialAccounts.all, id] as const,
  },
  schedules: {
    all: ['schedules'] as const,
    list: (active?: boolean) => [...queryKeys.schedules.all, { active }] as const,
    detail: (id: string) => [...queryKeys.schedules.all, id] as const,
    jobs: () => [...queryKeys.schedules.all, 'jobs'] as const,
  },
  bots: {
    all: ['bots'] as const,
    prompt: (botName: string, contentType: string, language: string) =>
      [...queryKeys.bots.all, botName, 'prompt', { contentType, language }] as const,
  },
} as const
