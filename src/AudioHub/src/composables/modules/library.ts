import type { useApiTransport } from '../useApiTransport'
import type { MediaMetaData, Sources } from '@/types'

export function createLibraryModule(transport: ReturnType<typeof useApiTransport>) {
  return {
    getSourceProps: () => transport.request<Sources>('/api/v1/library/sources/props'),

    getInstalledSources: () =>
      transport.request<Record<string, string>>('/api/v1/library/sources/installed'),

    getLibrary: (source: string, params?: Record<string, string>) =>
      transport.request<MediaMetaData[]>(`/api/v1/library/${source}`, {
        params,
      }),

    getLibraryItem: (source: string, rawUri: string) =>
      transport.request<MediaMetaData>(`/api/v1/library/${source}/${encodeURIComponent(rawUri)}`),

    getLibraryItemChildItems: (source: string, rawUri: string) =>
      transport.request<MediaMetaData[]>(
        `/api/v1/library/${source}/${encodeURIComponent(rawUri)}/items`
      ),
  }
}
