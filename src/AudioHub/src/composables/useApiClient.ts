import { useApiTransport, type TransportOptions } from './useApiTransport'
import type {
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
} from '../types/api'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export function useApiClient(baseUrl: string = apiBaseUrl) {
  const transport = useApiTransport(baseUrl)

  // ==========================================
  // Snapcast API Module
  // ==========================================
  const snapcast = {
    getServer: (): Promise<SnapServer> =>
      transport.request<SnapServer>('/api/v1/mixer/snapcast/status'),

    deleteClient: (clientId: string): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/client/${clientId}`, {
        method: 'DELETE',
      }),

    setClientLatency: (clientId: string, payload: SetClientLatencyRequest): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/client/${clientId}/latency`, {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    setClientName: (clientId: string, payload: SetClientNameRequest): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/client/${clientId}/name`, {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    setClientVolume: (clientId: string, payload: SetVolumeRequest): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/client/${clientId}/volume`, {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    setGroupClients: (groupId: string, payload: SetGroupClientsRequest): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/group/${groupId}/clients`, {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    setGroupMute: (groupId: string, payload: SetGroupMuteRequest): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/group/${groupId}/mute`, {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    setGroupStream: (groupId: string, payload: SetGroupStreamRequest): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/group/${groupId}/stream`, {
        method: 'POST',
        body: JSON.stringify(payload),
      }),

    setGroupVolume: (groupId: string, payload: SetVolumeRequest): Promise<void> =>
      transport.request<void>(`/api/v1/mixer/snapcast/group/${groupId}/volume`, {
        method: 'POST',
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
        method: 'POST',
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

    getPlaylist: (playlistId: string): Promise<SpotifyResponse<SpotifyPlaylist>> =>
      transport.request<SpotifyResponse<SpotifyPlaylist>>(
        `/api/v1/media/spotify/playlists/${playlistId}`
      ),

    getPlaylistItems: (
      playlistId: string,
      offset?: number,
      limit?: number
    ): Promise<SpotifyResponse<SpotifyPagedResult<SpotifyPlaylistItem>>> =>
      transport.request<SpotifyResponse<SpotifyPagedResult<SpotifyPlaylistItem>>>(
        `/api/v1/media/spotify/playlists/${playlistId}/tracks`,
        { params: { offset, limit } }
      ),

    getAlbum: (albumId: string): Promise<SpotifyAlbum> =>
      transport.request<SpotifyAlbum>(`/api/v1/media/spotify/albums/${albumId}`),

    getArtist: (artistId: string): Promise<SpotifyArtist> =>
      transport.request<SpotifyArtist>(`/api/v1/media/spotify/artists/${artistId}`),

    getEpisode: (episodeId: string): Promise<SpotifyEpisode> =>
      transport.request<SpotifyEpisode>(`/api/v1/media/spotify/episodes/${episodeId}`),
  }

  // ==========================================
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
    snapcast,
    media,
    mpd,
    radioBrowser,
    spotify,
    tuneIn,
    podverse,
    library,
  }
}
