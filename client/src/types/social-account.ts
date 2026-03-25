import type { Platform } from './enums'

export interface SocialAccountDto {
  id: string
  name: string
  platform: Platform
  externalId: string
  isActive: boolean
  tokenExpiresAt: string | null
}

export interface CreateSocialAccountDto {
  name: string
  platform: Platform
  externalId: string
  accessToken: string
  tokenExpiresAt?: string
  metadata?: Record<string, string>
}

export interface UpdateSocialAccountDto {
  name?: string
  accessToken?: string
  tokenExpiresAt?: string
  isActive?: boolean
}

export interface ValidateAccountResultDto {
  accountId: string
  isValid: boolean
  errorMessage: string | null
  tokenExpiresAt: string | null
}
