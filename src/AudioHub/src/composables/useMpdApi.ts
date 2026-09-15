import { useApiTransport } from './useApiTransport';
import type { MpdPlayRequest, MpdCommandRequest } from '../types';

export function useMpdApi(transport = useApiTransport('/api/Mpd')) {
  return {
    connect: () => transport.request<any>('connect', { method: 'POST' }),
    disconnect: () => transport.request<any>('disconnect', { method: 'POST' }),
    play: () => transport.request<any>('play', { method: 'POST' }),
    playFile: (data: MpdPlayRequest) => transport.request<any>('play-file', { method: 'POST', body: data }),
    pause: () => transport.request<any>('pause', { method: 'POST' }),
    stop: () => transport.request<any>('stop', { method: 'POST' }),
    next: () => transport.request<any>('next', { method: 'POST' }),
    previous: () => transport.request<any>('previous', { method: 'POST' }),
    clearPlaylist: () => transport.request<any>('playlist', { method: 'DELETE' }),
    sendCommand: (data: MpdCommandRequest) => transport.request<any>('command', { method: 'POST', body: data }),
  };
}
