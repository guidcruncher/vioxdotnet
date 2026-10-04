import { useApiTransport } from './useApiTransport'

import type {
  ClientConfiguration,
  EmitEventRequest,
  EmitEventResponse,
  EqualizerBand,
  MediaMetaData,
  MediaMetaDataPlaylist,
  PagedList,
  PlaybackState,
  PlayRequest,
  RpcVersion,
  SeekRequest,
  SetClientLatencyRequest,
  SetClientNameRequest,
  SetGroupClientsRequest,
  SetGroupMuteRequest,
  SetGroupStreamRequest,
  SetVolumeRequest,
  GetVolumeResponse,
  SnapClient,
  SnapServer,
  Sources,
  SpotifyResponse,
  SpotifyUserProfile,
  SubscriberCountResponse,
  VolumeState,
} from '@/types'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export function useApiClient(baseUrl: string = apiBaseUrl) {
  const transport = useApiTransport(baseUrl)

  /* ==========================================
     Core
  ========================================== */

  const core = {
    getCountries: () => transport.request<Record<string, string>>('/api/v1/core/countrys'),
  }

  /* ==========================================
     Client Config
  ========================================== */

  const config = {
    get: () => transport.request<ClientConfiguration>('/api/v1/client/config'),

    update: (payload: ClientConfiguration) =>
      transport.request<void>('/api/v1/client/config', {
        method: 'POST',
        body: JSON.stringify(payload),
      }),
  }

  /* ==========================================
     Media Player
  ========================================== */

  const media = {
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

  /* ==========================================
     Playlists
  ========================================== */

  const playlists = {
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

  /* ==========================================
     Favourites
  ========================================== */

  const favourites = {
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

  /* ==========================================
     Library
  ========================================== */

  const library = {
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

  /* ==========================================
     Equalizer
  ========================================== */

  const equalizer = {
    getPresets: () =>
      transport.request<Record<string, number[]>>('/api/v1/audiocontrol/equalizer/presets'),

    getBands: () => transport.request<EqualizerBand[]>('/api/v1/audiocontrol/equalizer/bands'),

    setBand: (bandIndex: number, percentage: number) =>
      transport.request<void>(`/api/v1/audiocontrol/equalizer/bands/${bandIndex}`, {
        method: 'PUT',
        body: JSON.stringify({
          percentage,
        }),
      }),

    setBands: (percentages: number[]) =>
      transport.request<void>('/api/v1/audiocontrol/equalizer/bands', {
        method: 'PUT',
        body: JSON.stringify({
          percentages,
        }),
      }),
  }

  /* ==========================================
     Snapcast
  ========================================== */

  const snapcast = {
    connect: () =>
      transport.request<void>('/api/v1/mixer/snapcast/connect', {
        method: 'POST',
      }),

    disconnect: () =>
      transport.request<void>('/api/v1/mixer/snapcast/disconnect', {
        method: 'POST',
      }),

    getRpcVersion: () => transport.request<RpcVersion>('/api/v1/mixer/snapcast/rpc-version'),

    getServer: () => transport.request<SnapServer>('/api/v1/mixer/snapcast/status'),

    getClients: () => transport.request<SnapClient[]>('/api/v1/mixer/snapcast/clients'),

    deleteClient: (clientId: string) =>
      transport.request<void>(`/api/v1/mixer/snapcast/clients/${clientId}`, {
        method: 'DELETE',
      }),

    setClientLatency: (clientId: string, payload: SetClientLatencyRequest) =>
      transport.request<number>(`/api/v1/mixer/snapcast/clients/${clientId}/latency`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setClientName: (clientId: string, payload: SetClientNameRequest) =>
      transport.request<string>(`/api/v1/mixer/snapcast/clients/${clientId}/name`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setClientVolume: (clientId: string, payload: SetVolumeRequest) =>
      transport.request<VolumeState>(`/api/v1/mixer/snapcast/clients/${clientId}/volume`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setAllClientVolume: (payload: SetVolumeRequest) =>
      transport.request<Record<string, VolumeState>>('/api/v1/mixer/snapcast/clients/volume', {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setGroupMute: (groupId: string, payload: SetGroupMuteRequest) =>
      transport.request<boolean>(`/api/v1/mixer/snapcast/groups/${groupId}/mute`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setGroupStream: (groupId: string, payload: SetGroupStreamRequest) =>
      transport.request<string>(`/api/v1/mixer/snapcast/groups/${groupId}/stream`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setGroupClients: (groupId: string, payload: SetGroupClientsRequest) =>
      transport.request<string[]>(`/api/v1/mixer/snapcast/groups/${groupId}/clients`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),
  }

  /* ==========================================
     Auth
  ========================================== */

  const auth = {
    me: () => transport.request<SpotifyResponse<SpotifyUserProfile>>('/api/v1/auth/me'),

    loginUrl: () => `${baseUrl}/api/v1/auth/login`,
  }

  /* ==========================================
     Events (non-SSE endpoints only)
  ========================================== */

  const events = {
    getSubscribers: () => transport.request<SubscriberCountResponse>('/api/v1/events/subscribers'),

    emit: (payload: EmitEventRequest) =>
      transport.request<EmitEventResponse>('/api/v1/events/emit', {
        method: 'POST',
        body: JSON.stringify(payload),
      }),
  }

  const radioBrowser = {
    getByCountry: (country: string): Promise<MediaMetaData[]> =>
      transport.request<MediaMetaData[]>(`/api/v1/media/radiobrowser/stations/country/${country}`),
  }

  // ==========================================
  // File Playlist API Module
  // ==========================================
  const filePlaylists = {
    getPlaylists: (): Promise<Record<string, MediaMetaData[]>> =>
      transport.request<Record<string, MediaMetaData[]>>('/api/v1/media/playlists'),
    getPlaylist: (key: string): Promise<MediaMetaData[]> =>
      transport.request<MediaMetaData[]>(`/api/v1/media/playlists/${encodeURIComponent(key)}`),
    find: (uri: string): Promise<MediaMetaData> =>
      transport.request<MediaMetaData>(
        `/api/v1/media/playlists/find?uri=${encodeURIComponent(uri)}`
      ),
  }

  return {
    createApiState: transport.createApiState,

    core,
    config,
    media,
    playlists,
    filePlaylists,
    radioBrowser,
    favourites,
    library,
    equalizer,
    snapcast,
    auth,
    events,
  }
}
