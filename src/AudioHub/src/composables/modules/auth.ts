import type { useApiTransport } from '../useApiTransport'
import type { SpotifyResponse, SpotifyUserProfile } from '@/types'

export function createAuthModule(transport: ReturnType<typeof useApiTransport>, baseUrl: string) {
  return {
    me: () => transport.request<SpotifyResponse<SpotifyUserProfile>>('/api/v1/auth/me'),

    loginUrl: () => `${baseUrl}/api/v1/auth/login`,
  }
}
