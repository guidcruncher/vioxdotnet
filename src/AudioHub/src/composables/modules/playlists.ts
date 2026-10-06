import type { useApiTransport } from '../useApiTransport'
import type { MediaMetaData, MediaMetaDataPlaylist } from '@/types'

export function createPlaylistsModule(transport: ReturnType<typeof useApiTransport>) {
  return {
    getAll: () => transport.request<Record<string, string>>('/api/v1/playlists'),

    getById: (id: string) =>
      transport.request<MediaMetaDataPlaylist>(`/api/v1/playlists/${encodeURIComponent(id)}`),

    delete: (id: string) =>
      transport.request<void>(`/api/v1/playlists/${encodeURIComponent(id)}`, {
        method: 'DELETE',
      }),

    addItem: (id: string, item: MediaMetaData) =>
      transport.request<void>(`/api/v1/playlists/${encodeURIComponent(id)}/items`, {
        method: 'POST',
        body: JSON.stringify(item),
      }),

    createPlaylist: (title: string, item: MediaMetaData) =>
      transport.request<MediaMetaDataPlaylist>(
        `/api/v1/playlists/items?title=${encodeURIComponent(title)}`,
        {
          method: 'POST',
          body: JSON.stringify(item),
        }
      ),

    removeItem: (rawUri: string) =>
      transport.request<void>(`/api/v1/playlists/${encodeURIComponent(rawUri)}/items`, {
        method: 'DELETE',
        params: {
          rawUri,
        },
      }),
  }
}
