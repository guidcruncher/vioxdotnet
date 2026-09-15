import { useApiTransport } from './useApiTransport';

export function useTuneInApi(transport = useApiTransport('/api/v1/media/tunein')) {
  return {
    search: (query: string) =>
      transport.request<unknown>('search', { method: 'GET', params: { query } }),
    getStation: (id: string) =>
      transport.request<unknown>(`stations/${encodeURIComponent(id)}`, { method: 'GET' }),
  };
}
