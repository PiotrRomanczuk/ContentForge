import ky from 'ky'

let authToken: string | null = null

export function setAuthToken(token: string | null) {
  authToken = token
  if (token) {
    localStorage.setItem('cf_token', token)
  } else {
    localStorage.removeItem('cf_token')
    localStorage.removeItem('cf_token_expires')
  }
}

export function getAuthToken(): string | null {
  if (authToken) return authToken
  const stored = localStorage.getItem('cf_token')
  if (stored) {
    const expires = localStorage.getItem('cf_token_expires')
    if (expires && new Date(expires) < new Date()) {
      localStorage.removeItem('cf_token')
      localStorage.removeItem('cf_token_expires')
      return null
    }
    authToken = stored
    return stored
  }
  return null
}

export function clearAuth() {
  authToken = null
  localStorage.removeItem('cf_token')
  localStorage.removeItem('cf_token_expires')
}

export function isAuthenticated(): boolean {
  return getAuthToken() !== null
}

export const api = ky.create({
  prefixUrl: import.meta.env.VITE_API_URL || '',
  hooks: {
    beforeRequest: [
      (request) => {
        const token = getAuthToken()
        if (token) {
          request.headers.set('Authorization', `Bearer ${token}`)
        }
      },
    ],
    afterResponse: [
      (_request, _options, response) => {
        if (response.status === 401) {
          clearAuth()
          if (window.location.pathname !== '/login') {
            window.location.href = '/login'
          }
        }
      },
    ],
  },
  retry: {
    limit: 2,
    statusCodes: [500, 502, 503],
  },
})
