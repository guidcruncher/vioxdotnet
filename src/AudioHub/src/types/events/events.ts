import type { MediaMetaData } from '@/types'

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

export interface EventPayload {
  eventId: string
  eventType: string
  message: string
  timestamp: string
}

export interface MediaEvent<T = any> {
  type: string
  backend: string
  track?: MediaMetaData
  data?: T
  timestamp: string
}

export type EventHandler<T = any> = (payload: MediaEvent<T>) => void

export interface UseServerEventsOptions {
  autoReconnect?: boolean
  maxRetries?: number
  initialRetryIntervalMs?: number
  maxRetryIntervalMs?: number
  withCredentials?: boolean
  immediate?: boolean
}
