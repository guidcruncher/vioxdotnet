import type { useApiTransport } from '../useApiTransport'
import type { MediaMetaData } from '@/types'

export function createFavouritesModule(transport: ReturnType<typeof useApiTransport>) {
  return {
    getAll: () => transport.request<MediaMetaData[]>('/api/v1/favourites'),

    add: (item: MediaMetaData) =>
      transport.request<void>('/api/v1/favourites', {
        method: 'POST',
        body: JSON.stringify(item),
      }),

    remove: (rawUri: string) =>
      transport.request<void>('/api/v1/favourites', {
        method: 'DELETE',
        params: {
          rawUri,
        },
      }),

    find: (rawUri: string) =>
      transport.request<MediaMetaData>('/api/v1/favourites/find', {
        params: {
          rawUri,
        },
      }),

    search: (query: string) =>
      transport.request<MediaMetaData[]>('/api/v1/favourites/search', {
        params: {
          query,
        },
      }),

    clear: () =>
      transport.request<void>('/api/v1/favourites/clear', {
        method: 'DELETE',
      }),
  }
}
