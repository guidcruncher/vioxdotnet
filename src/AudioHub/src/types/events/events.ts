export interface EventPayload {
  eventId: string

  eventType: string

  message: string

  timestamp: string
}

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
