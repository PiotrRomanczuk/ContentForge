import { api } from './client'
import type {
  SocialAccountDto,
  CreateSocialAccountDto,
  UpdateSocialAccountDto,
  ValidateAccountResultDto,
  Platform,
} from '@/types'

export const socialAccountsApi = {
  getAll: (platform?: Platform) =>
    api
      .get('api/social-accounts', {
        searchParams: platform ? { platform } : {},
      })
      .json<SocialAccountDto[]>(),

  getById: (id: string) => api.get(`api/social-accounts/${id}`).json<SocialAccountDto>(),

  create: (data: CreateSocialAccountDto) =>
    api.post('api/social-accounts', { json: data }).json<SocialAccountDto>(),

  update: (id: string, data: UpdateSocialAccountDto) =>
    api.put(`api/social-accounts/${id}`, { json: data }).json<SocialAccountDto>(),

  deactivate: (id: string) => api.delete(`api/social-accounts/${id}`),

  validate: (id: string) =>
    api.post(`api/social-accounts/${id}/validate`).json<ValidateAccountResultDto>(),
}
