import { useApiTransport } from './useApiTransport';
import type {
  PlayRequest,
  SeekRequest,
  SetVolumeRequest,
  AudioItem,
  VolumeState,
} from '../types';

export function useMediaPlayerApi(transport = useApiTransport('/api/v1/media-player')) {
  return {
    play: (data: PlayRequest) => transport.request<any>('play', { method: 'POST', body: data }),
    pause: () => transport.request<any>('pause', { method: 'POST' }),
    resume: () => transport.request<any>('resume', { method: 'POST' }),
    stop: () => transport.request<any>('stop', { method: 'POST' }),
    seek: (data: SeekRequest) => transport.request<any>('seek', { method: 'POST', body: data }),
    next: () => transport.request<any>('next', { method: 'POST' }),
    previous: () => transport.request<any>('previous', { method: 'POST' }),
    getVolume: () => transport.request<number>('volume', { method: 'GET' }),
    setVolume: (data: SetVolumeRequest) => transport.request<Record<string, VolumeState>>('volume', { method: 'PUT', body: data }),
    getActive: () => transport.request<any>('active', { method: 'GET' }),
    getCurrentTrack: () => transport.request<AudioItem>('current', { method: 'GET' }),
  };
}
