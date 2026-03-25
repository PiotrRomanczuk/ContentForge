import ky from 'ky'
import type { TokenRequest, TokenResponse } from '@/types'
import { setAuthToken } from './client'

const baseUrl = import.meta.env.VITE_API_URL || ''

export const authApi = {
  login: async (apiKey: string): Promise<TokenResponse> => {
    const response = await ky
      .post(`${baseUrl}/auth/token`, {
        json: { apiKey } satisfies TokenRequest,
      })
      .json<TokenResponse>()

    setAuthToken(response.token)
    localStorage.setItem('cf_token_expires', response.expiresAt)

    return response
  },
}
