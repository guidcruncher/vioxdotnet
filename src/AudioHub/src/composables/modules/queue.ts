import type { useApiTransport } from '../useApiTransport'
import type { PlayRequest, MediaMetaData, QueueStatusResponse, PlaybackRepeatMode } from '@/types'

export function createQueueModule(transport: ReturnType<typeof useApiTransport>) {
  return {
    getStatus: () => transport.request<QueueStatusResponse>('/api/v1/queue'),

    clear: () =>
      transport.request<void>('/api/v1/queue', {
        method: 'DELETE',
      }),

    enqueue: (item: PlayRequest) =>
      transport.request<void>('/api/v1/queue/enqueue', {
        method: 'POST',
        body: JSON.stringify(item),
      }),

    enqueueBatch: (items: PlayRequest[]) =>
      transport.request<void>('/api/v1/queue/enqueue-batch', {
        method: 'POST',
        body: JSON.stringify(items),
      }),

    next: () =>
      transport.request<boolean>('/api/v1/queue/next', {
        method: 'POST',
      }),

    previous: () =>
      transport.request<boolean>('/api/v1/queue/previous', {
        method: 'POST',
      }),

    skipTo: (index: number) =>
      transport.request<void>(`/api/v1/queue/skip/${encodeURIComponent(index)}`, {
        method: 'POST',
      }),

    removeItem: (index: number) =>
      transport.request<void>(`/api/v1/queue/${encodeURIComponent(index)}`, {
        method: 'DELETE',
      }),

    setShuffle: (enabled: boolean) =>
      transport.request<void>('/api/v1/queue/shuffle', {
        method: 'POST',
        body: JSON.stringify(enabled),
      }),

    setRepeatMode: (mode: PlaybackRepeatMode) =>
      transport.request<void>('/api/v1/queue/repeat', {
        method: 'POST',
        body: JSON.stringify(mode),
      }),
  }
}
