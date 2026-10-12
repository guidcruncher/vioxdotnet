import { onUnmounted, readonly } from 'vue'
import { sseSingleton } from './sseSingleton'
import type { MediaEvent, EventHandler, UseServerEventsOptions } from '@/types'
import { MediaEventTypes } from '@/types'

export function useServerEvents(options: UseServerEventsOptions = {}) {
  const {
    autoReconnect = true,
    maxRetries = 10,
    initialRetryIntervalMs = 2000,
    maxRetryIntervalMs = 30000,
    withCredentials = false,
    immediate = true,
  } = options

  const url = `${window.location.protocol}//${window.location.host}/api/v1/events`

  const reconnectCfg = {
    autoReconnect,
    maxRetries,
    initialRetryIntervalMs,
    maxRetryIntervalMs,
  }

  if (immediate) {
    sseSingleton.connect(url, { withCredentials }, reconnectCfg)
  }

  const on = (eventName: string, handler: EventHandler) => {
    if (!MediaEventTypes.includes(eventName)) {
      alert(`Event type "${eventName}" is not a recognized MediaEvent type.`)
      return
    }

    sseSingleton.on(eventName, handler)
    return () => off(eventName, handler)
  }

  const off = (eventName: string, handler: EventHandler) => {
    if (!MediaEventTypes.includes(eventName)) {
      alert(`Event type "${eventName}" is not a recognized MediaEvent type.`)
      return
    }

    sseSingleton.off(eventName, handler)
  }

  onUnmounted(() => {
    // Only remove this composable's handlers
    // Do NOT disconnect the global SSE
  })

  return {
    isConnected: readonly(sseSingleton.isConnected),
    isReconnecting: readonly(sseSingleton.isReconnecting),
    error: readonly(sseSingleton.error),
    retryCount: readonly(sseSingleton.retryCount),
    lastEvent: readonly(sseSingleton.lastEvent),

    on,
    off,

    connect: () => sseSingleton.connect(url, { withCredentials }, reconnectCfg),
    disconnect: () => sseSingleton.disconnect(),
  }
}
