import { useApiTransport, type TransportOptions } from './useApiTransport'
import type {
  EqualizerBand,
  ApiPlayRequest,
  ApiRepeatContextRequest,
  ApiRepeatTrackRequest,
  ApiSeekRequest,
  ApiServer,
  ApiShuffleContextRequest,
  AudioElement,
  ClickResult,
  GetVolumeResponse,
  MediaMetaData,
  MpdCommandRequest,
  MpdPlayRequest,
  PagedList,
  PlaybackState,
  ProblemDetails,
  ServerStats,
  SetClientLatencyRequest,
  SetClientNameRequest,
  SetGroupClientsRequest,
  SetGroupMuteRequest,
  SetGroupStreamRequest,
  SetVolumeRequest,
  SetVolumeResponse,
  SnapServer,
  SpotifyAlbum,
  SpotifyArtist,
  SpotifyEpisode,
  SpotifyPagedResult,
  SpotifyPlaylist,
  SpotifyPlaylistItem,
  SpotifyResponse,
  SpotifySavedAlbum,
  SpotifySavedEpisode,
  SpotifySavedShow,
  SpotifySavedTrack,
  SpotifyUserProfile,
  StationElement,
  TuneInOutline,
  TuneInResponse,
  ClientConfig,
} from '../types/api'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export function useApiClient(baseUrl: string = apiBaseUrl) {
  const transport = useApiTransport(baseUrl)

  // ==========================================
  // Core API Module
  // ==========================================
  const core = {
    getCountries: (): Promise<Record<string, string>> =>
      transport.request<Record<string, string>>('/api/v1/core/countrys'),
  }

  // ==========================================
  // Equalizer API Module
  // ==========================================
  const equalizer = {
    getEqualizerBands: (): Promise<EqualizerBand[]> =>
      transport.request<EqualizerBand[]>('/api/v1/audiocontrol/equalizer/bands'),
    setEqualizerBand: (index: number, value: number) =>
      transport.request<void>(`/api/v1/audiocontrol/equalizer/bands/{index}`, {
        method: 'PUT',
        body: JSON.stringify({ percentage: value }),
      }),
    setEqualizerBands: (values: number[]) =>
      transport.request<void>(`/api/v1/audiocontrol/equalizer/bands`, {
        method: 'PUT',
        body: JSON.stringify({ percentages: values }),
      }),
  }

  // ==========================================
  // Client Config API Module
  // ==========================================
  const config = {
    get: (): Promise<ClientConfig> => transport.request<ClientConfig>('/api/v1/client/config'),
    update: (item: ClientConfig): Promise<void> =>
      transport.request<void>('/api/v1/client/config', {
        method: 'POST',
        body: JSON.stringify(item),
      }),
  }

  // ==========================================
  // Favourites API Module
  // ==========================================
  const favourites = {
    getAll: (): Promise<MediaMetaData[]> =>
      transport.request<MediaMetaData[]>('/api/v1/favourites'),
    add: (item: MediaMetaData): Promise<void> =>
      transport.request<void>('/api/v1/favourites', {
        method: 'POST',
        body: JSON.stringify(item),
      }),
    remove: (uri: string): Promise<void> =>
      transport.request<void>(`/api/v1/favourites?rawUri=${encodeURIComponent(uri)}`, {
        method: 'DELETE',
      }),
    find: (uri: string): Promise<void> =>
      transport.request<void>(`/api/v1/favourites/find?rawUri=${encodeURIComponent(uri)}`),
  }

  // ==========================================
  // Snapcast API Module
  // ==========================================
  const snapcast = {
    getServer: (): Promise<SnapServer> =>
      transport.request<SnapServer>('/api/v1/mixer/snapcast/status'),

    deleteClient: (clientId: string): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/clients/${clientId}`, {
        method: 'DELETE',
      }),

    setClientLatency: (clientId: string, payload: SetClientLatencyRequest): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/clients/${clientId}/latency`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setClientName: (clientId: string, payload: SetClientNameRequest): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/clients/${clientId}/name`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setClientVolume: (clientId: string, payload: SetVolumeRequest): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/clients/${clientId}/volume`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setGroupClients: (groupId: string, payload: SetGroupClientsRequest): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/groups/${groupId}/clients`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setGroupMute: (groupId: string, payload: SetGroupMuteRequest): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/groups/${groupId}/mute`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setGroupStream: (groupId: string, payload: SetGroupStreamRequest): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/groups/${groupId}/stream`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),
  }

  // ==========================================
  // Library API Module
  // ==========================================
  const library = {
    getLibrary: (source: string, params: Record<string, string>): Promise<MediaMetaData[]> =>
      transport.request<MediaMetaData[]>(`/api/v1/library/${source}`, { params }),
  }

  // ==========================================
  // Media & Playback API Module
  // ==========================================
  const media = {
    getPlaybackState: (): Promise<PlaybackState> =>
      transport.request<PlaybackState>('/api/v1/media-player/current'),

    play: (payload: ApiPlayRequest): Promise<void> =>
      transport.request<void>('/api/v1/media-player/play', {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    pause: (): Promise<void> =>
      transport.request<void>('/api/v1/media-player/pause', { method: 'POST' }),

    resume: (): Promise<void> =>
      transport.request<void>('/api/v1/media-player/resume', { method: 'POST' }),

    stop: (): Promise<void> =>
      transport.request<void>('/api/v1/media-player/stop', { method: 'POST' }),

    next: (): Promise<void> =>
      transport.request<void>('/api/v1/media-player/next', { method: 'POST' }),

    previous: (): Promise<void> =>
      transport.request<void>('/api/v1/media-player/previous', { method: 'POST' }),

    seek: (payload: ApiSeekRequest): Promise<void> =>
      transport.request<void>('/api/v1/media-player/seek', {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    setRepeatContext: (payload: ApiRepeatContextRequest): Promise<void> =>
      transport.request<void>('/api/v1/media-player/repeat-context', {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    setRepeatTrack: (payload: ApiRepeatTrackRequest): Promise<void> =>
      transport.request<void>('/api/v1/media-player/repeat-track', {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    setVolume: (payload: SetVolumeRequest): Promise<SetVolumeResponse> =>
      transport.request<SetVolumeResponse>('/api/v1/media-player/volume', {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    getVolume: (): Promise<GetVolumeResponse> =>
      transport.request<GetVolumeResponse>('/api/v1/media-player/volume', {
        method: 'GET',
      }),

    setShuffleContext: (payload: ApiShuffleContextRequest): Promise<void> =>
      transport.request<void>('/api/v1/media-player/shuffle-context', {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    searchSource: (
      source: string,
      query: string,
      page: number = 1,
      pageSize: number = 20
    ): Promise<PagedList<MediaMetaData>> =>
      transport.request<PagedList<MediaMetaData>>(`/api/v1/media/search/${source}`, {
        params: { query, page, pageSize },
      }),

    searchAll: (
      query: string,
      page: number = 1,
      pageSize: number = 20
    ): Promise<Record<string, PagedList<MediaMetaData>>> =>
      transport.request<Record<string, PagedList<MediaMetaData>>>(`/api/v1/media/search`, {
        params: { query, page, pageSize },
      }),
  }

  // ==========================================
  // MPD Backend API Module
  // ==========================================
  const mpd = {
    sendCommand: (payload: MpdCommandRequest): Promise<void> =>
      transport.request<void>('/api/v1/mpd/command', {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    playFileOrUrl: (payload: MpdPlayRequest): Promise<void> =>
      transport.request<void>('/api/v1/mpd/play', {
        method: 'POST',
        body: JSON.stringify(payload),
      }),
  }

  // ==========================================
  // Podverse Backend API Module
  // ==========================================
  const podverse = {
    getPodcast: (id: string): Promise<MediaMetaData> =>
      transport.request<MediaMetaData>(
        `/api/v1/media/podverse/podcasts/${encodeURIComponent(id)}`,
        {
          method: 'GET',
        }
      ),
    getEpisodes: (id: string): Promise<MediaMetaData[]> =>
      transport.request<MediaMetaData[]>(
        `/api/v1/media/podverse/podcast/${encodeURIComponent(id)}/episodes`,
        {
          method: 'GET',
        }
      ),
    getEpisode: (id: string): Promise<MediaMetaData> =>
      transport.request<MediaMetaData>(`/api/v1/media/podverse/episodes/${id}`, {
        method: 'GET',
      }),
  }

  // ==========================================
  // Radio Browser API Module
  // ==========================================
  const radioBrowser = {
    getServers: (): Promise<ApiServer[]> =>
      transport.request<ApiServer[]>('/api/v1/media/radiobrowser/servers'),

    getStats: (): Promise<ServerStats> =>
      transport.request<ServerStats>('/api/v1/radiobrowser/stats'),

    getByCountry: (country: string): Promise<MediaMetaData[]> =>
      transport.request<MediaMetaData[]>(`/api/v1/media/radiobrowser/stations/country/${country}`),

    clickStation: (stationUuid: string): Promise<ClickResult> =>
      transport.request<ClickResult>(`/api/v1/media/radiobrowser/click/${stationUuid}`, {
        method: 'POST',
      }),
  }

  // ==========================================
  // Spotify API Module
  // ==========================================
  const spotify = {
    getProfile: (): Promise<SpotifyResponse<SpotifyUserProfile>> =>
      transport.request<SpotifyResponse<SpotifyUserProfile>>('/api/v1/media/spotify/me'),

    getSavedAlbums: (
      offset?: number,
      limit?: number
    ): Promise<SpotifyResponse<SpotifyPagedResult<SpotifySavedAlbum>>> =>
      transport.request<SpotifyResponse<SpotifyPagedResult<SpotifySavedAlbum>>>(
        '/api/v1/media/spotify/me/albums',
        { params: { offset, limit } }
      ),

    getSavedEpisodes: (
      offset?: number,
      limit?: number
    ): Promise<SpotifyResponse<SpotifyPagedResult<SpotifySavedEpisode>>> =>
      transport.request<SpotifyResponse<SpotifyPagedResult<SpotifySavedEpisode>>>(
        '/api/v1/media/spotify/me/episodes',
        { params: { offset, limit } }
      ),

    getSavedShows: (
      offset?: number,
      limit?: number
    ): Promise<SpotifyResponse<SpotifyPagedResult<SpotifySavedShow>>> =>
      transport.request<SpotifyResponse<SpotifyPagedResult<SpotifySavedShow>>>(
        '/api/v1/media/spotify/me/shows',
        { params: { offset, limit } }
      ),

    getSavedTracks: (
      offset?: number,
      limit?: number
    ): Promise<SpotifyResponse<SpotifyPagedResult<SpotifySavedTrack>>> =>
      transport.request<SpotifyResponse<SpotifyPagedResult<SpotifySavedTrack>>>(
        '/api/v1/media/spotify/me/tracks',
        { params: { offset, limit } }
      ),

    getUserPlaylists: (
      offset?: number,
      limit?: number
    ): Promise<SpotifyResponse<SpotifyPagedResult<SpotifyPlaylist>>> =>
      transport.request<SpotifyResponse<SpotifyPagedResult<SpotifyPlaylist>>>(
        '/api/v1/media/spotify/me/playlists',
        { params: { offset, limit } }
      ),

    getPlaylist: (playlistId: string): Promise<MediaMetaData> =>
      transport.request<MediaMetaData>(`/api/v1/media/spotify/playlists/${playlistId}`),

    getPlaylistItems: (playlistId: string): Promise<MediaMetaData[]> =>
      transport.request<MediaMetaData[]>(`/api/v1/media/spotify/playlists/${playlistId}/items`),

    getAlbum: (albumId: string): Promise<MediaMetaData> =>
      transport.request<MediaMetaData>(`/api/v1/media/spotify/albums/${albumId}`),

    getAlbumTracks: (albumId: string): Promise<MediaMetaData[]> =>
      transport.request<MediaMetaData[]>(`/api/v1/media/spotify/albums/${albumId}/tracks`),

    getArtist: (artistId: string): Promise<SpotifyArtist> =>
      transport.request<SpotifyArtist>(`/api/v1/media/spotify/artists/${artistId}`),

    getShow: (id: string): Promise<MediaMetaData> =>
      transport.request<MediaMetaData>(`/api/v1/media/spotify/shows/${id}`),

    getEpisodes: (showId: string): Promise<MediaMetaData[]> =>
      transport.request<MediaMetaData[]>(`/api/v1/media/spotify/shows/${showId}/episodes`),

    getEpisode: (episodeId: string): Promise<MediaMetaData> =>
      transport.request<MediaMetaData>(`/api/v1/media/spotify/episodes/${episodeId}`),
  }

  // =================a=========================
  // TuneIn API Module
  // ==========================================
  const tuneIn = {
    getBrowse: (guideId?: string): Promise<TuneInResponse<TuneInOutline>> =>
      transport.request<TuneInResponse<TuneInOutline>>('/api/v1/tunein/browse', {
        params: { guideId },
      }),

    getStation: (stationId: string): Promise<TuneInResponse<StationElement>> =>
      transport.request<TuneInResponse<StationElement>>(`/api/v1/tunein/station/${stationId}`),

    getStreamUrl: (guideId: string): Promise<TuneInResponse<AudioElement>> =>
      transport.request<TuneInResponse<AudioElement>>(`/api/v1/tunein/stream/${guideId}`),
  }

  return {
    createApiState: transport.createApiState,
    core,
    snapcast,
    media,
    mpd,
    radioBrowser,
    spotify,
    tuneIn,
    podverse,
    library,
    favourites,
    config,
    equalizer,
  }
}
