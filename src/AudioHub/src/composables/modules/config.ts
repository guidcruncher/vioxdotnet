import type { useApiTransport } from '../useApiTransport'
import type { ClientConfiguration } from '@/types'

export function createConfigModule(transport: ReturnType<typeof useApiTransport>) {
  return {
    get: () => transport.request<ClientConfiguration>('/api/v1/client/config'),

    update: (payload: ClientConfiguration) =>
      transport.request<void>('/api/v1/client/config', {
        method: 'POST',
        body: JSON.stringify(payload),
      }),
  }
}
