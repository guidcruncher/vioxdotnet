import { useApiTransport } from './useApiTransport';
import type {
  ApiPlayRequest,
  ApiSeekRequest,
  ApiShuffleContextRequest,
  ApiRepeatContextRequest,
  ApiRepeatTrackRequest,
} from '../types';

export function useLibrespotApi(transport = useApiTransport('/api/v1/players/librespot')) {
  return {
    getState: () => transport.request<any>('state', { method: 'GET' }),
    getEvents: () => transport.request<any>('events', { method: 'GET' }),
    play: (data: ApiPlayRequest) => transport.request<any>('play', { method: 'POST', body: data }),
    pause: () => transport.request<any>('pause', { method: 'POST' }),
    resume: () => transport.request<any>('resume', { method: 'POST' }),
    seek: (data: ApiSeekRequest) => transport.request<any>('seek', { method: 'POST', body: data }),
    shuffle: (data: ApiShuffleContextRequest) => transport.request<any>('shuffle', { method: 'POST', body: data }),
    repeatContext: (data: ApiRepeatContextRequest) => transport.request<any>('repeat-context', { method: 'POST', body: data }),
    repeatTrack: (data: ApiRepeatTrackRequest) => transport.request<any>('repeat-track', { method: 'POST', body: data }),
  };
}
