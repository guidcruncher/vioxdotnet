import type { useApiTransport } from '../useApiTransport'

export function createCoreModule(transport: ReturnType<typeof useApiTransport>) {
  return {
    getCountries: () => transport.request<Record<string, string>>('/api/v1/core/countrys'),
  }
}
