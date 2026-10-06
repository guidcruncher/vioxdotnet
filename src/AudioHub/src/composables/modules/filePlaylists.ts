import type { useApiTransport } from '../useApiTransport'
import type { MediaMetaData } from '@/types'

export function createFilePlaylistsModule(transport: ReturnType<typeof useApiTransport>) {
  return {
    getPlaylists: (): Promise<Record<string, MediaMetaData[]>> =>
      transport.request<Record<string, MediaMetaData[]>>('/api/v1/media/playlists'),

    getPlaylist: (key: string): Promise<MediaMetaData[]> =>
      transport.request<MediaMetaData[]>(`/api/v1/media/playlists/${encodeURIComponent(key)}`),

    find: (uri: string): Promise<MediaMetaData> =>
      transport.request<MediaMetaData>(
        `/api/v1/media/playlists/find?uri=${encodeURIComponent(uri)}`
      ),
  }
}
