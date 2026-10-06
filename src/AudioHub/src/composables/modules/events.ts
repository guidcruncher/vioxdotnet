import type { useApiTransport } from '../useApiTransport'
import type { EmitEventRequest, EmitEventResponse, SubscriberCountResponse } from '@/types'

export function createEventsModule(transport: ReturnType<typeof useApiTransport>) {
  return {
    getSubscribers: () => transport.request<SubscriberCountResponse>('/api/v1/events/subscribers'),

    emit: (payload: EmitEventRequest) =>
      transport.request<EmitEventResponse>('/api/v1/events/emit', {
        method: 'POST',
        body: JSON.stringify(payload),
      }),
  }
}
