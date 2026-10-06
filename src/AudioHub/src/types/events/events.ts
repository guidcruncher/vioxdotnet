export interface EmitEventRequest {
  eventType: string
  message: string
}

export interface EmitEventResponse {
  status: string
  activeSubscribers: number
}

export interface SubscriberCountResponse {
  activeSubscribers: number
}

export interface EventPayload<T = any> {
  eventId: string
  eventType: string
  message: string
  timestamp: string
  data?: T
}

export type EventHandler<T = any> = (payload: EventPayload<T>) => void

export interface UseServerEventsOptions {
  autoReconnect?: boolean
  maxRetries?: number
  initialRetryIntervalMs?: number
  maxRetryIntervalMs?: number
  withCredentials?: boolean
  immediate?: boolean
}
