// ==========================================
// Generic Interfaces
// ==========================================

export interface EqualizerBand {
  index: number
  controlName: string
  frequencyLabel: string
  leftPercentage: number
  rightPercentage: number
}

export interface ClientConfig {
  defaultCountry: string
  playLists: Record<string, string>
}

export interface PagedList<T> {
  offset?: number | string
  pageNumber?: number | string
  totalPages?: number | string
  items?: T[] | null
  limit?: number | string
  totalCount?: number | string
  hasPreviousPage?: boolean
  hasNextPage?: boolean
}

export interface SpotifyPagedResult<T> {
  href?: string
  limit?: number | string
  next?: string | null
  offset?: number | string
  previous?: string | null
  total?: number | string
  items?: T[]
}

export interface SpotifyResponse<T> {
  isSuccess: boolean
  statusCode: number | string
  data?: T | null
  error?: SpotifyErrorObject | null
}

export interface TuneInResponse<T> {
  [key: string]: any
}

// ==========================================
// Unknown / Referenced External Types
// ==========================================

export type JsonElement = any
export type VolumeState = any
export type ValidationProblemDetails = any
export type StationOrder = any
export type TuneInOutline = any
export type StationElement = any
export type SpotifySavedAlbum = any
export type SpotifySavedEpisode = any
export type SpotifySavedShow = any
export type SpotifySavedTrack = any
export type SpotifyTrack = any
export type SpotifyUserProfile = any

// ==========================================
// Core Models
// ==========================================

export interface ApiPlayRequest {
  uri: string
  skip_to_uri?: string | null
  paused?: boolean | null
  position?: number | null
}

export interface ApiRepeatContextRequest {
  repeat_context: boolean
}

export interface ApiRepeatTrackRequest {
  repeat_track: boolean
}

export interface ApiSeekRequest {
  position: number
  relative?: boolean
}

export interface ApiServer {
  ip?: string | null
  name?: string | null
}

export interface ApiShuffleContextRequest {
  shuffle_context: boolean
}

export interface AudioElement {
  element?: string
  url?: string
  reliability?: number | string
  bitrate?: number | string
  media_type?: string
  position?: number
  player_width?: number | string
  player_height?: number | string
  is_hls_advanced?: string
  live_seek_stream?: string
  guide_id?: string
  is_ad_clipped_content_enabled?: string
  is_direct?: boolean
}

export interface ClickResult {
  ok?: any
  message?: string | null
  stationuuid?: string | null
  name?: string | null
  url?: string | null
}

export interface ClientConfig {
  instance: number | string
  latency: number | string
  name: string
  volume: VolumeState
}

export interface ClientHost {
  arch: string
  ip: string
  mac: string
  name: string
  os: string
}

export interface MediaUri {
  source: string
  type: string
  id: string
  secondaryId?: string | null
}

export interface MediaMetaData {
  album: string
  title: string
  artist: string
  url: string
  imageUrl: string
  uri?: MediaUri | null
  rawUri: string
  duration: number
  favourite: boolean
  releaseDate: string
}

export interface MediaAlbum {
  album: string
  title: string
  artist: string
  url: string
  imageUrl: string
  tracks?: MediaMetaData[]
  uri?: MediaUri | null
  rawUri?: string | null
  duration?: number | string | null
}

export interface MpdCommandRequest {
  command: string
}

export interface MpdPlayRequest {
  fileOrUrl: string
}

export interface PlaybackState {
  activeBackend?: string
  track?: MediaMetaData | null
  position?: number
  playing?: boolean
  isLive?: boolean
}

export interface PlayRequest {
  uri: string
}

export interface ProblemDetails {
  type?: string | null
  title?: string | null
  status?: number | string | null
  detail?: string | null
  instance?: string | null
}

export interface RpcVersion {
  major: number | string
  minor: number | string
  patch: number | string
}

export interface SeekRequest {
  position: string
}

export interface ServerStats {
  supported_version?: number
  software_version?: string | null
  status?: string | null
  stations?: number
  stations_broken?: number
  tags?: number
  clicks_last_hour?: number
  clicks_last_day?: number
  languages?: number
  countries?: number
}

export interface SetClientLatencyRequest {
  latency: number | string
}

export interface SetClientNameRequest {
  name: string
}

export interface SetGroupClientsRequest {
  clientIds: string[]
}

export interface SetGroupMuteRequest {
  mute: boolean
}

export interface SetGroupStreamRequest {
  streamId: string
}

export interface SetVolumeRequest {
  volumePercent: number
  muted?: boolean
}

export interface SetVolumeResponse {
  percent: number
  muted?: boolean
}

export interface GetVolumeResponse {
  volumePercent: number
  muted?: boolean
}

export interface SnapTime {
  sec: number | string
  usec: number | string
}

export interface SnapClient {
  id: string
  connected: boolean
  config: ClientConfig
  host: ClientHost
  lastSeen: SnapTime
}

export interface SnapGroup {
  id: string
  name: string
  muted: boolean
  stream_id: string
  clients: SnapClient[]
}

export interface SnapStream {
  id: string
  status: string
  uri: JsonElement
}

export interface SnapServer {
  host: ClientHost
  groups: SnapGroup[]
  streams: SnapStream[]
}

// ==========================================
// Spotify Specific Models
// ==========================================

export interface SpotifyExternalUrls {
  spotify?: string
}

export interface SpotifyFollowers {
  href?: string | null
  total?: number | string
}

export interface SpotifyImage {
  url?: string
  height?: number | string | null
  width?: number | string | null
}

export interface SpotifyErrorObject {
  status?: number | string
  message?: string
}

export interface SpotifyArtist {
  id?: string
  name?: string
  external_urls?: SpotifyExternalUrls | null
  followers?: SpotifyFollowers | null
  genres?: string[]
  href?: string
  images?: SpotifyImage[]
  popularity?: number | string
  type?: string
  uri?: string
}

export interface SpotifyEpisode {
  id?: string
  name?: string
  description?: string
  duration_ms?: number | string
  explicit?: boolean
  external_urls?: SpotifyExternalUrls | null
  href?: string
  images?: SpotifyImage[]
  is_playable?: boolean
  release_date?: string
  type?: string
  uri?: string
}

export interface SpotifyAlbum {
  id?: string
  name?: string
  album_type?: string
  total_tracks?: number | string
  available_markets?: string[]
  external_urls?: SpotifyExternalUrls | null
  href?: string
  images?: SpotifyImage[]
  release_date?: string
  release_date_precision?: string
  type?: string
  uri?: string
  artists?: SpotifyArtist[]
  tracks?: SpotifyPagedResult<SpotifyTrack> | null
}

export interface SpotifyPlaylistItem {
  added_at?: string | null
  added_by?: SpotifyUserProfile | null
  is_local?: boolean
  track?: SpotifyTrack | null
}

export interface SpotifyPlaylist {
  id?: string
  name?: string
  collaborative?: boolean
  description?: string | null
  external_urls?: SpotifyExternalUrls | null
  followers?: SpotifyFollowers | null
  href?: string
  images?: SpotifyImage[]
  owner?: SpotifyUserProfile | null
  public?: boolean | null
  snapshot_id?: string
  tracks?: SpotifyPagedResult<SpotifyPlaylistItem> | null
  type?: string
  uri?: string
}
