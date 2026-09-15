export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  [key: string]: unknown;
}

export interface VolumeState {
  volume?: number;
  muted?: boolean;
}

export interface ApiPlayRequest {
  uri?: string;
}

export interface ApiSeekRequest {
  positionMs?: number;
}

export interface ApiShuffleContextRequest {
  shuffle?: boolean;
}

export interface ApiRepeatContextRequest {
  repeat?: boolean;
}

export interface ApiRepeatTrackRequest {
  repeat?: boolean;
}

export interface PlayRequest {
  uri?: string;
  contextUri?: string;
}

export interface SeekRequest {
  positionMs?: number;
}

export interface SetVolumeRequest {
  volume?: number;
  muted?: boolean;
}

export interface MpdPlayRequest {
  file?: string;
}

export interface MpdCommandRequest {
  command?: string;
  args?: string[];
}

export interface Podcast {
  id?: string;
  title?: string;
  author?: string;
  description?: string;
  imageUrl?: string;
}

export interface Episode {
  id?: string;
  podcastId?: string;
  title?: string;
  description?: string;
  audioUrl?: string;
  duration?: number;
}

export interface User {
  id?: string;
  name?: string;
  email?: string;
}

export type StationOrder = 
  | 'name'
  | 'url'
  | 'homepage'
  | 'favicon'
  | 'tags'
  | 'country'
  | 'state'
  | 'language'
  | 'votes'
  | 'negativevotes'
  | 'clickcount'
  | 'clicktrend'
  | 'bitrate'
  | 'lastcheckok'
  | 'lastchecktime'
  | 'lastcheckoktime'
  | 'lastlocalchecktime'
  | 'clicktimestamp'
  | 'clicktimestamp_time'
  | 'random';

export interface StationSearchOptions {
  name?: string;
  nameExact?: boolean;
  country?: string;
  countryExact?: boolean;
  countrycode?: string;
  state?: string;
  stateExact?: boolean;
  language?: string;
  languageExact?: boolean;
  tag?: string;
  tagExact?: boolean;
  tagList?: string;
  codec?: string;
  bitrateMin?: number;
  bitrateMax?: number;
  order?: StationOrder;
  reverse?: boolean;
  offset?: number;
  limit?: number;
  hidebroken?: boolean;
}

export interface Station {
  changeid?: string;
  stationuuid?: string;
  name?: string;
  url?: string;
  url_resolved?: string;
  homepage?: string;
  favicon?: string;
  tags?: string;
  country?: string;
  countrycode?: string;
  iso_3166_2?: string;
  state?: string;
  language?: string;
  languagecodes?: string;
  votes?: number;
  lastchangetime?: string;
  codec?: string;
  bitrate?: number;
  hls?: number;
  lastcheckok?: number;
  lastchecktime?: string;
  lastcheckoktime?: string;
  lastlocalchecktime?: string;
  clicktimestamp?: string;
  clickcount?: number;
  clicktrend?: number;
  ssl_error?: number;
  geo_lat?: number;
  geo_long?: number;
  has_extended_info?: boolean;
}

export interface AddStationRequest {
  name: string;
  url: string;
  homepage?: string;
  favicon?: string;
  countrycode?: string;
  state?: string;
  language?: string;
  tags?: string;
}

export interface AddStationResult {
  ok?: boolean;
  message?: string;
  stationuuid?: string;
}

export interface ServerStats {
  supported_version?: number;
  software_version?: string;
  status?: string;
  stations?: number;
  stations_broken?: number;
  tags?: number;
  clicks_time?: number;
  languages?: number;
  countries?: number;
}

export interface ApiServer {
  name?: string;
  ip?: string;
}

export interface CountryInfo {
  name?: string;
  stationcount?: number;
}

export interface CountryCodeInfo {
  name?: string;
  stationcount?: number;
}

export interface StateInfo {
  name?: string;
  country?: string;
  stationcount?: number;
}

export interface LanguageInfo {
  name?: string;
  stationcount?: number;
}

export interface TagInfo {
  name?: string;
  stationcount?: number;
}

export interface CodecInfo {
  name?: string;
  stationcount?: number;
}

export interface ClickResult {
  ok?: boolean;
  message?: string;
  stationuuid?: string;
  name?: string;
  url?: string;
}

export interface VoteResult {
  ok?: boolean;
  message?: string;
}

export interface StationClick {
  stationuuid?: string;
  clicktimestamp?: string;
}

export interface StationCheck {
  stationuuid?: string;
  checktimestamp?: string;
  ok?: boolean;
}

export interface RpcVersion {
  major?: number;
  minor?: number;
  patch?: number;
}

export interface SnapServer {
  server?: Record<string, unknown>;
  groups?: unknown[];
  streams?: unknown[];
}

export interface SnapClient {
  id?: string;
  host?: Record<string, unknown>;
  config?: Record<string, unknown>;
  lastSeen?: Record<string, unknown>;
  connected?: boolean;
}

export interface SetClientNameRequest {
  name?: string;
}

export interface SetClientLatencyRequest {
  latency?: number;
}

export interface SetGroupMuteRequest {
  mute?: boolean;
}

export interface SetGroupStreamRequest {
  streamId?: string;
}

export interface SetGroupClientsRequest {
  clients?: string[];
}

export interface SpotifyResponse<T> {
  data?: T;
  error?: ProblemDetails;
}

export type SpotifyResponseOfSpotifyAlbum = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyAlbumList = SpotifyResponse<Record<string, unknown>[]>;
export type SpotifyResponseOfSpotifyArtist = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyArtistList = SpotifyResponse<Record<string, unknown>[]>;
export type SpotifyResponseOfSpotifyTrack = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyTrackList = SpotifyResponse<Record<string, unknown>[]>;
export type SpotifyResponseOfSpotifyShow = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyEpisode = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyAudiobook = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyChapter = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifySearchResult = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyUserProfile = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyPlaylist = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifySnapshotResult = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfObject = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyPagedResultOfSpotifyTrack = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyPagedResultOfSpotifyAlbum = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyPagedResultOfSpotifyEpisode = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyPagedResultOfSpotifyChapter = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyPagedResultOfSpotifyAudiobook = SpotifyResponse<Record<string, unknown>>;
export type SpotifyResponseOfSpotifyPagedResultOfSpotifyPlaylistItem = SpotifyResponse<Record<string, unknown>>;

export interface ChangePlaylistDetailsRequest {
  name?: string;
  public?: boolean;
  collaborative?: boolean;
  description?: string;
}

export interface AddItemsToPlaylistRequest {
  uris?: string[];
  position?: number;
}

export interface ReorderOrReplacePlaylistItemsRequest {
  uris?: string[];
  rangeStart?: number;
  insertBefore?: number;
  length?: number;
  snapshotId?: string;
}

export interface RemovePlaylistItemsRequest {
  tracks?: Array<{ uri: string }>;
  snapshotId?: string;
}
