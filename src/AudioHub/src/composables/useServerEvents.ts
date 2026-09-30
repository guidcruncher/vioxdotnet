import { ref, onUnmounted, readonly, type Ref } from 'vue'

export interface EventPayload<T = unknown> {
  eventId: string
  eventType: string
  message: string
  timestamp: string
  data?: T
}

export type EventHandler<T = unknown> = (payload: EventPayload<T>) => void

export interface UseServerEventsOptions {
  autoReconnect?: boolean
  maxRetries?: number
  initialRetryIntervalMs?: number
  maxRetryIntervalMs?: number
  withCredentials?: boolean
  immediate?: boolean
}

export function useServerEvents<T = unknown>(options: UseServerEventsOptions = {}) {
  const url = `${window.location.protocol}//${window.location.host}/api/v1/events`

  const {
    autoReconnect = true,
    maxRetries = 10,
    initialRetryIntervalMs = 2000,
    maxRetryIntervalMs = 30000,
    withCredentials = false,
    immediate = true,
  } = options

  const isConnected = ref(false)
  const isReconnecting = ref(false)
  const error = ref<Error | null>(null)
  const retryCount = ref(0)
  const lastEvent = ref<EventPayload<T> | null>(null)

  const listeners = new Map<string, Set<EventHandler<T>>>()

  let eventSource: EventSource | null = null
  let reconnectTimer: ReturnType<typeof setTimeout> | null = null

  const getUrl = (): string => {
    return url
  }

  const clearTimer = () => {
    if (reconnectTimer !== null) {
      clearTimeout(reconnectTimer)
      reconnectTimer = null
    }
  }

  const on = (eventName: string, handler: EventHandler<T>): (() => void) => {
    if (!listeners.has(eventName)) {
      listeners.set(eventName, new Set())

      if (eventSource && isConnected.value) {
        eventSource.addEventListener(eventName, handleCustomEvent)
      }
    }

    listeners.get(eventName)!.add(handler)

    return () => off(eventName, handler)
  }

  const off = (eventName: string, handler: EventHandler<T>) => {
    const handlerSet = listeners.get(eventName)
    if (handlerSet) {
      handlerSet.delete(handler)
      if (handlerSet.size === 0) {
        listeners.delete(eventName)
        if (eventSource) {
          eventSource.removeEventListener(eventName, handleCustomEvent)
        }
      }
    }
  }

  const emit = (eventName: string, payload: EventPayload<T>) => {
    const handlerSet = listeners.get(eventName)
    if (handlerSet) {
      handlerSet.forEach((handler) => {
        try {
          handler(payload)
        } catch (err) {
          console.error(`Error in SSE listener for event '${eventName}':`, err)
        }
      })
    }
  }

  const processPayload = (rawJson: string): EventPayload<T> | null => {
    try {
      const parsedPayload = JSON.parse(rawJson) as EventPayload<T>
      lastEvent.value = parsedPayload
      error.value = null
      return parsedPayload
    } catch (err) {
      error.value = err instanceof Error ? err : new Error('Failed to parse event message JSON')
      return null
    }
  }

  const handleMessage = (event: MessageEvent) => {
    const payload = processPayload(event.data)
    if (payload) {
      emit('message', payload)
      if (payload.eventType && payload.eventType !== 'message') {
        emit(payload.eventType, payload)
      }
    }
  }

  const handleCustomEvent = (event: MessageEvent) => {
    const payload = processPayload(event.data)
    if (payload) {
      emit(event.type, payload)
    }
  }

  const disconnect = () => {
    clearTimer()
    if (eventSource) {
      eventSource.close()
      eventSource = null
    }
    isConnected.value = false
    isReconnecting.value = false
  }

  const scheduleReconnect = () => {
    disconnect()

    if (!autoReconnect) {
      return
    }

    if (retryCount.value >= maxRetries) {
      error.value = new Error(`Maximum reconnect attempts reached (${maxRetries})`)
      isReconnecting.value = false
      return
    }

    isReconnecting.value = true
    retryCount.value += 1

    const delay = Math.min(
      initialRetryIntervalMs * Math.pow(1.5, retryCount.value - 1),
      maxRetryIntervalMs
    )

    clearTimer()
    reconnectTimer = setTimeout(() => {
      connect()
    }, delay)
  }

  const connect = () => {
    disconnect()

    const targetUrl = getUrl()
    if (!targetUrl) {
      error.value = new Error('Invalid URL provided')
      return
    }

    try {
      eventSource = new EventSource(targetUrl, { withCredentials })

      eventSource.onopen = () => {
        isConnected.value = true
        isReconnecting.value = false
        retryCount.value = 0
        error.value = null

        listeners.forEach((_, eventName) => {
          if (eventName !== 'message') {
            eventSource?.addEventListener(eventName, handleCustomEvent)
          }
        })
      }

      eventSource.onmessage = handleMessage

      eventSource.onerror = () => {
        isConnected.value = false
        scheduleReconnect()
      }
    } catch (err) {
      error.value =
        err instanceof Error ? err : new Error('Failed to establish EventSource connection')
      scheduleReconnect()
    }
  }

  if (immediate) {
    connect()
  }

  onUnmounted(() => {
    listeners.clear()
    disconnect()
  })

  return {
    isConnected: readonly(isConnected),
    isReconnecting: readonly(isReconnecting),
    error: readonly(error),
    retryCount: readonly(retryCount),
    lastEvent: readonly(lastEvent),
    on,
    off,
    connect,
    disconnect,
  }
}
