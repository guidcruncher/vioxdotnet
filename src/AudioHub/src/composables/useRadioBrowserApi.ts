import { useApiTransport } from './useApiTransport';
import type {
  ServerStats,
  ApiServer,
  StationSearchOptions,
  Station,
  AddStationRequest,
  AddStationResult,
  CountryInfo,
  CountryCodeInfo,
  StateInfo,
  LanguageInfo,
  TagInfo,
  CodecInfo,
  ClickResult,
  VoteResult,
  StationClick,
  StationCheck,
  StationOrder,
} from '../types';

interface CommonQueryParams {
  Order?: StationOrder;
  Reverse?: boolean;
  Offset?: number;
  Limit?: number;
  HideBroken?: boolean;
}

export function useRadioBrowserApi(transport = useApiTransport('/api/v1/media/radiobrowser')) {
  return {
    getStats: () => transport.request<ServerStats>('stats', { method: 'GET' }),
    getServers: () => transport.request<ApiServer[]>('servers', { method: 'GET' }),
    searchStations: (options?: StationSearchOptions) =>
      transport.request<Station[]>('stations/search', { method: 'POST', body: options }),
    getStations: (params?: CommonQueryParams) =>
      transport.request<Station[]>('stations', { method: 'GET', params: params as Record<string, any> }),
    addStation: (data: AddStationRequest) =>
      transport.request<AddStationResult>('stations', { method: 'POST', body: data }),
    getStationsByUuids: (uuids: string[]) =>
      transport.request<Station[]>('stations/by-uuids', { method: 'POST', body: uuids }),
    getStationByUuid: (stationUuid: string) =>
      transport.request<Station>(`stations/${encodeURIComponent(stationUuid)}`, { method: 'GET' }),
    getStationsByName: (name: string, exact = false, params?: CommonQueryParams) =>
      transport.request<Station[]>('stations/by-name', { method: 'GET', params: { name, exact, ...params } }),
    getStationsByTag: (tag: string, exact = false, params?: CommonQueryParams) =>
      transport.request<Station[]>('stations/by-tag', { method: 'GET', params: { tag, exact, ...params } }),
    getStationsByCountry: (country: string, exact = false, params?: CommonQueryParams) =>
      transport.request<Station[]>('stations/by-country', { method: 'GET', params: { country, exact, ...params } }),
    getStationsByCountryCode: (countryCode: string, params?: CommonQueryParams) =>
      transport.request<Station[]>(`stations/by-country-code/${encodeURIComponent(countryCode)}`, { method: 'GET', params: params as Record<string, any> }),
    getStationsByState: (state: string, exact = false, params?: CommonQueryParams) =>
      transport.request<Station[]>('stations/by-state', { method: 'GET', params: { state, exact, ...params } }),
    getStationsByLanguage: (language: string, exact = false, params?: CommonQueryParams) =>
      transport.request<Station[]>('stations/by-language', { method: 'GET', params: { language, exact, ...params } }),
    getStationsByCodec: (codec: string, exact = false, params?: CommonQueryParams) =>
      transport.request<Station[]>('stations/by-codec', { method: 'GET', params: { codec, exact, ...params } }),
    getStationsByUrl: (url: string) =>
      transport.request<Station[]>('stations/by-url', { method: 'GET', params: { url } }),
    getTopClickedStations: (params?: CommonQueryParams) =>
      transport.request<Station[]>('stations/top-clicked', { method: 'GET', params: params as Record<string, any> }),
    getTopVotedStations: (params?: CommonQueryParams) =>
      transport.request<Station[]>('stations/top-voted', { method: 'GET', params: params as Record<string, any> }),
    getRecentlyClickedStations: (params?: CommonQueryParams) =>
      transport.request<Station[]>('stations/recently-clicked', { method: 'GET', params: params as Record<string, any> }),
    getRecentlyChangedStations: (params?: CommonQueryParams) =>
      transport.request<Station[]>('stations/recently-changed', { method: 'GET', params: params as Record<string, any> }),
    getBrokenStations: (params?: CommonQueryParams) =>
      transport.request<Station[]>('stations/broken', { method: 'GET', params: params as Record<string, any> }),
    getCountries: (filter?: string, params?: CommonQueryParams) =>
      transport.request<CountryInfo[]>('countries', { method: 'GET', params: { filter, ...params } }),
    getCountryCodes: (filter?: string, params?: CommonQueryParams) =>
      transport.request<CountryCodeInfo[]>('country-codes', { method: 'GET', params: { filter, ...params } }),
    getStates: (country?: string, filter?: string, params?: CommonQueryParams) =>
      transport.request<StateInfo[]>('states', { method: 'GET', params: { country, filter, ...params } }),
    getLanguages: (filter?: string, params?: CommonQueryParams) =>
      transport.request<LanguageInfo[]>('languages', { method: 'GET', params: { filter, ...params } }),
    getTags: (filter?: string, params?: CommonQueryParams) =>
      transport.request<TagInfo[]>('tags', { method: 'GET', params: { filter, ...params } }),
    getCodecs: (filter?: string, params?: CommonQueryParams) =>
      transport.request<CodecInfo[]>('codecs', { method: 'GET', params: { filter, ...params } }),
    clickStation: (stationUuid: string) =>
      transport.request<ClickResult>(`stations/${encodeURIComponent(stationUuid)}/click`, { method: 'POST' }),
    voteStation: (stationUuid: string) =>
      transport.request<VoteResult>(`stations/${encodeURIComponent(stationUuid)}/vote`, { method: 'POST' }),
    getClicks: (stationUuid?: string, seconds?: number) =>
      transport.request<StationClick[]>('clicks', { method: 'GET', params: { stationUuid, seconds } }),
    getChecks: (stationUuid?: string, lastCheckTime?: number, seconds?: number) =>
      transport.request<StationCheck[]>('checks', { method: 'GET', params: { stationUuid, lastCheckTime, seconds } }),
    getPlaylistUrl: (format: string, relativePathAndQuery: string) =>
      transport.request<string>('playlist-url', { method: 'GET', params: { format, relativePathAndQuery } }),
  };
}
