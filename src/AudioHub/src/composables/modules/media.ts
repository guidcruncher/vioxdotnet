import type { useApiTransport } from '../useApiTransport'
import type {
  GetVolumeResponse,
  MediaMetaData,
  PagedList,
  PlaybackState,
  PlayRequest,
  SeekRequest,
  SetVolumeRequest,
  VolumeState,
} from '@/types'

export function createMediaModule(transport: ReturnType<typeof useApiTransport>) {
  return {
    getPlaybackState: () => transport.request<PlaybackState>('/api/v1/media-player/current'),

    getActiveBackend: () => transport.request<string>('/api/v1/media-player/active'),

    play: (payload: PlayRequest) =>
      transport.request<void>('/api/v1/media-player/play', {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    pause: () =>
      transport.request<void>('/api/v1/media-player/pause', {
        method: 'POST',
      }),

    resume: () =>
      transport.request<void>('/api/v1/media-player/resume', {
        method: 'POST',
      }),

    stop: () =>
      transport.request<void>('/api/v1/media-player/stop', {
        method: 'POST',
      }),

    next: () =>
      transport.request<void>('/api/v1/media-player/next', {
        method: 'POST',
      }),

    previous: () =>
      transport.request<void>('/api/v1/media-player/previous', {
        method: 'POST',
      }),

    seek: (payload: SeekRequest) =>
      transport.request<void>('/api/v1/media-player/seek', {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    getVolume: () => transport.request<GetVolumeResponse>('/api/v1/media-player/volume'),

    setVolume: (payload: SetVolumeRequest) =>
      transport.request<VolumeState>('/api/v1/media-player/volume', {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    searchAll: (query: string, pageNumber = 1, limit = 20) =>
      transport.request<Record<string, PagedList<MediaMetaData>>>('/api/v1/media/search', {
        params: {
          query,
          pageNumber,
          limit,
        },
      }),

    searchSource: (sourceKey: string, query: string, pageNumber = 1, limit = 20) =>
      transport.request<PagedList<MediaMetaData>>(`/api/v1/media/search/${sourceKey}`, {
        params: {
          query,
          pageNumber,
          limit,
        },
      }),
  }
}
