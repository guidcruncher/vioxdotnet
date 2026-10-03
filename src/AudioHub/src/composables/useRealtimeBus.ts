import { readonly, ref } from 'vue'

/* ==========================================
   Types
========================================== */

export interface RealtimeOptions {
  url?: string

  reconnect?: boolean

  initialReconnectDelay?: number

  maxReconnectDelay?: number

  maxRetries?: number

  withCredentials?: boolean
}

export interface Subscription {
  unsubscribe(): void
}

type EventHandler<T = unknown> = (payload: T) => void

/* ==========================================
   Constants
========================================== */

const DEFAULT_URL = '/api/v1/events'

/* ==========================================
   Singleton State
========================================== */

const source = ref<EventSource | null>(null)

const connected = ref(false)

const connecting = ref(false)

const retryCount = ref(0)

const lastError = ref<Event | null>(null)

const listeners = new Map<string, Set<EventHandler>>()

/**
 * Tracks event types already registered
 * against the active EventSource.
 */
const registeredEventTypes = new Set<string>()

let reconnectTimer: number | null = null

let manuallyDisconnected = false

let connectionOptions: RealtimeOptions = {}

/* ==========================================
   Helpers
========================================== */

function parsePayload(raw: string): unknown {
  try {
    return JSON.parse(raw)
  } catch {
    return raw
  }
}

function clearReconnectTimer() {
  if (reconnectTimer !== null) {
    clearTimeout(reconnectTimer)

    reconnectTimer = null
  }
}

function ensureListenerBucket(eventName: string) {
  if (!listeners.has(eventName)) {
    listeners.set(eventName, new Set())
  }
}

function dispatchEvent(eventName: string, payload: unknown) {
  listeners.get(eventName)?.forEach((handler) => {
    try {
      handler(payload)
    } catch (error) {
      console.error(`[Realtime] Error processing "${eventName}" event`, error)
    }
  })
}

function calculateReconnectDelay(attempt: number, initialDelay: number, maxDelay: number): number {
  const exponential = Math.min(initialDelay * Math.pow(2, attempt - 1), maxDelay)

  const jitter = exponential * 0.25 * Math.random()

  return Math.round(exponential + jitter)
}

/* ==========================================
   Event Registration
========================================== */

function attachEventType(eventSource: EventSource, eventName: string) {
  eventSource.addEventListener(eventName, (event) => {
    const message = event as MessageEvent

    dispatchEvent(eventName, parsePayload(message.data))
  })
}

function registerEventType(eventName: string) {
  if (registeredEventTypes.has(eventName)) {
    return
  }

  registeredEventTypes.add(eventName)

  if (source.value) {
    attachEventType(source.value, eventName)
  }
}

function attachRegisteredEvents(eventSource: EventSource) {
  registeredEventTypes.forEach((eventName) => {
    attachEventType(eventSource, eventName)
  })
}

/* ==========================================
   Reconnect Logic
========================================== */

function scheduleReconnect() {
  if (manuallyDisconnected) {
    return
  }

  if (connectionOptions.reconnect === false) {
    return
  }

  const maxRetries = connectionOptions.maxRetries ?? Number.POSITIVE_INFINITY

  if (retryCount.value >= maxRetries) {
    console.warn('[Realtime] Max reconnect attempts reached')

    return
  }

  retryCount.value += 1

  const delay = calculateReconnectDelay(
    retryCount.value,
    connectionOptions.initialReconnectDelay ?? 1000,
    connectionOptions.maxReconnectDelay ?? 30000
  )

  clearReconnectTimer()

  reconnectTimer = window.setTimeout(() => {
    connect(connectionOptions)
  }, delay)
}

/* ==========================================
   Connection Lifecycle
========================================== */

function connect(options: RealtimeOptions = {}) {
  if (source.value || connecting.value) {
    return
  }

  connectionOptions = {
    reconnect: true,
    initialReconnectDelay: 1000,
    maxReconnectDelay: 30000,
    maxRetries: Number.POSITIVE_INFINITY,
    ...options,
  }

  manuallyDisconnected = false

  connecting.value = true

  const eventSource = new EventSource(options.url ?? DEFAULT_URL, {
    withCredentials: options.withCredentials ?? false,
  })

  source.value = eventSource

  attachRegisteredEvents(eventSource)

  eventSource.onopen = (event) => {
    connected.value = true

    connecting.value = false

    retryCount.value = 0

    lastError.value = null

    console.info('[Realtime] Connected', event)
  }

  eventSource.onerror = (event) => {
    console.warn('[Realtime] Connection lost')

    connected.value = false

    connecting.value = false

    lastError.value = event

    source.value = null

    eventSource.close()

    scheduleReconnect()
  }

  eventSource.onmessage = (event) => {
    dispatchEvent('message', parsePayload(event.data))
  }
}

function disconnect() {
  manuallyDisconnected = true

  clearReconnectTimer()

  source.value?.close()

  source.value = null

  connected.value = false

  connecting.value = false

  retryCount.value = 0
}

/* ==========================================
   Subscriptions
========================================== */

function subscribe<T>(eventName: string, handler: (payload: T) => void): Subscription {
  ensureListenerBucket(eventName)

  registerEventType(eventName)

  listeners.get(eventName)!.add(handler as EventHandler)

  return {
    unsubscribe() {
      const bucket = listeners.get(eventName)

      bucket?.delete(handler as EventHandler)

      if (bucket && bucket.size === 0) {
        listeners.delete(eventName)
      }
    },
  }
}

/* ==========================================
   Public API
========================================== */

export function useRealtimeBus() {
  return {
    connect,

    disconnect,

    subscribe,

    connected: readonly(connected),

    connecting: readonly(connecting),

    retryCount: readonly(retryCount),

    lastError: readonly(lastError),
  }
}
