import { useApiTransport } from './useApiTransport';
import type {
  SpotifyResponseOfSpotifyAlbum,
  SpotifyResponseOfSpotifyAlbumList,
  SpotifyResponseOfSpotifyPagedResultOfSpotifyTrack,
  SpotifyResponseOfSpotifyArtist,
  SpotifyResponseOfSpotifyArtistList,
  SpotifyResponseOfSpotifyPagedResultOfSpotifyAlbum,
  SpotifyResponseOfSpotifyTrackList,
  SpotifyResponseOfSpotifyShow,
  SpotifyResponseOfSpotifyPagedResultOfSpotifyEpisode,
  SpotifyResponseOfSpotifyEpisode,
  SpotifyResponseOfSpotifyAudiobook,
  SpotifyResponseOfSpotifyPagedResultOfSpotifyChapter,
  SpotifyResponseOfSpotifyPagedResultOfSpotifyAudiobook,
  SpotifyResponseOfSpotifyChapter,
  SpotifyResponseOfSpotifyTrack,
  SpotifyResponseOfSpotifySearchResult,
  SpotifyResponseOfSpotifyUserProfile,
  SpotifyResponseOfSpotifyPlaylist,
  SpotifyResponseOfObject,
  SpotifyResponseOfSpotifyPagedResultOfSpotifyPlaylistItem,
  SpotifyResponseOfSpotifySnapshotResult,
  ChangePlaylistDetailsRequest,
  AddItemsToPlaylistRequest,
  ReorderOrReplacePlaylistItemsRequest,
  RemovePlaylistItemsRequest,
} from '../types';

export function useSpotifyApi(transport = useApiTransport('/api/v1/media/spotify')) {
  return {
    getAlbum: (id: string, market?: string) =>
      transport.request<SpotifyResponseOfSpotifyAlbum>(`albums/${encodeURIComponent(id)}`, { method: 'GET', params: { market } }),
    getAlbumsBatch: (ids: string[], market?: string) =>
      transport.request<SpotifyResponseOfSpotifyAlbumList>('albums/batch', { method: 'POST', body: ids, params: { market } }),
    getAlbumTracks: (id: string, params?: { market?: string; limit?: number; offset?: number }) =>
      transport.request<SpotifyResponseOfSpotifyPagedResultOfSpotifyTrack>(`albums/${encodeURIComponent(id)}/tracks`, { method: 'GET', params }),
    getArtist: (id: string) =>
      transport.request<SpotifyResponseOfSpotifyArtist>(`artists/${encodeURIComponent(id)}`, { method: 'GET' }),
    getArtistsBatch: (ids: string[]) =>
      transport.request<SpotifyResponseOfSpotifyArtistList>('artists/batch', { method: 'POST', body: ids }),
    getArtistAlbums: (id: string, params?: { includeGroups?: string; market?: string; limit?: number; offset?: number }) =>
      transport.request<SpotifyResponseOfSpotifyPagedResultOfSpotifyAlbum>(`artists/${encodeURIComponent(id)}/albums`, { method: 'GET', params }),
    getArtistTopTracks: (id: string, market?: string) =>
      transport.request<SpotifyResponseOfSpotifyTrackList>(`artists/${encodeURIComponent(id)}/top-tracks`, { method: 'GET', params: { market } }),
    getRelatedArtists: (id: string) =>
      transport.request<SpotifyResponseOfSpotifyArtistList>(`artists/${encodeURIComponent(id)}/related-artists`, { method: 'GET' }),
    getShow: (id: string, market?: string) =>
      transport.request<SpotifyResponseOfSpotifyShow>(`shows/${encodeURIComponent(id)}`, { method: 'GET', params: { market } }),
    getShowEpisodes: (id: string, params?: { market?: string; limit?: number; offset?: number }) =>
      transport.request<SpotifyResponseOfSpotifyPagedResultOfSpotifyEpisode>(`shows/${encodeURIComponent(id)}/episodes`, { method: 'GET', params }),
    getEpisode: (id: string, market?: string) =>
      transport.request<SpotifyResponseOfSpotifyEpisode>(`episodes/${encodeURIComponent(id)}`, { method: 'GET', params: { market } }),
    getAudiobook: (id: string, market?: string) =>
      transport.request<SpotifyResponseOfSpotifyAudiobook>(`audiobooks/${encodeURIComponent(id)}`, { method: 'GET', params: { market } }),
    getAudiobookChapters: (id: string, params?: { market?: string; limit?: number; offset?: number }) =>
      transport.request<SpotifyResponseOfSpotifyPagedResultOfSpotifyChapter>(`audiobooks/${encodeURIComponent(id)}/chapters`, { method: 'GET', params }),
    getSavedAudiobooks: (params?: { limit?: number; offset?: number }) =>
      transport.request<SpotifyResponseOfSpotifyPagedResultOfSpotifyAudiobook>('me/audiobooks', { method: 'GET', params }),
    getChapter: (id: string, market?: string) =>
      transport.request<SpotifyResponseOfSpotifyChapter>(`chapters/${encodeURIComponent(id)}`, { method: 'GET', params: { market } }),
    getTrack: (id: string, market?: string) =>
      transport.request<SpotifyResponseOfSpotifyTrack>(`tracks/${encodeURIComponent(id)}`, { method: 'GET', params: { market } }),
    search: (params: { query: string; types: string[]; market?: string; limit?: number; offset?: number; includeExternal?: string }) =>
      transport.request<SpotifyResponseOfSpotifySearchResult>('search', { method: 'GET', params: params as any as Record<string, any> }),
    getMe: () =>
      transport.request<SpotifyResponseOfSpotifyUserProfile>('me', { method: 'GET' }),
    getPlaylist: (playlistId: string, params?: { market?: string; fields?: string; additionalTypes?: string }) =>
      transport.request<SpotifyResponseOfSpotifyPlaylist>(`playlists/${encodeURIComponent(playlistId)}`, { method: 'GET', params }),
    changePlaylistDetails: (playlistId: string, data: ChangePlaylistDetailsRequest) =>
      transport.request<SpotifyResponseOfObject>(`playlists/${encodeURIComponent(playlistId)}`, { method: 'PUT', body: data }),
    getPlaylistTracks: (playlistId: string, params?: { market?: string; fields?: string; limit?: number; offset?: number; additionalTypes?: string }) =>
      transport.request<SpotifyResponseOfSpotifyPagedResultOfSpotifyPlaylistItem>(`playlists/${encodeURIComponent(playlistId)}/tracks`, { method: 'GET', params }),
    addItemsToPlaylist: (playlistId: string, data: AddItemsToPlaylistRequest, params?: { position?: number; uris?: string[] }) =>
      transport.request<SpotifyResponseOfSpotifySnapshotResult>(`playlists/${encodeURIComponent(playlistId)}/tracks`, { method: 'POST', body: data, params }),
    reorderOrReplacePlaylistItems: (playlistId: string, data: ReorderOrReplacePlaylistItemsRequest, uris?: string[]) =>
      transport.request<SpotifyResponseOfSpotifySnapshotResult>(`playlists/${encodeURIComponent(playlistId)}/tracks`, { method: 'PUT', body: data, params: { uris } }),
    removePlaylistItems: (playlistId: string, data: RemovePlaylistItemsRequest) =>
      transport.request<SpotifyResponseOfSpotifySnapshotResult>(`playlists/${encodeURIComponent(playlistId)}/tracks`, { method: 'DELETE', body: data }),
    getUserPlaylists: (params?: { limit?: number; offset?: number }) =>
      transport.request<any>('me/playlists', { method: 'GET', params }),
    login: () => window.location.href = '/api/v1/auth/login'
  };
}
