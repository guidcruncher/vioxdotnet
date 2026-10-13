// useServerEvents.ts
import { onUnmounted, readonly } from 'vue'
import { sseSingleton } from './sseSingleton'
import type { MediaEvent, EventHandler, UseServerEventsOptions } from '@/types'
import { MediaEventTypes } from '@/types'
import { useAlert } from '@/composables/useAlert'

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

  // Track listeners registered by this specific composable instance for automatic cleanup on unmount
  const registeredHandlers: Array<{ eventName: string; handler: EventHandler }> = []
  const registeredGlobalHandlers = new Set<EventHandler>()

  const on = (eventName: string, handler: EventHandler) => {
    if (!MediaEventTypes.includes(eventName)) {
      useAlert(`Event type "${eventName}" is not a recognized MediaEvent type.`)
      return
    }

    sseSingleton.on(eventName, handler)
    registeredHandlers.push({ eventName, handler })
    return () => off(eventName, handler)
  }

  const off = (eventName: string, handler: EventHandler) => {
    if (!MediaEventTypes.includes(eventName)) {
      return
    }

    sseSingleton.off(eventName, handler)
    const index = registeredHandlers.findIndex(
      (item) => item.eventName === eventName && item.handler === handler
    )
    if (index !== -1) {
      registeredHandlers.splice(index, 1)
    }
  }

  const onMessage = (handler: EventHandler) => {
    sseSingleton.onMessage(handler)
    registeredGlobalHandlers.add(handler)
    return () => offMessage(handler)
  }

  const offMessage = (handler: EventHandler) => {
    sseSingleton.offMessage(handler)
    registeredGlobalHandlers.delete(handler)
  }

  onUnmounted(() => {
    // Only remove this composable instance's handlers without disconnecting global SSE
    registeredHandlers.forEach(({ eventName, handler }) => {
      sseSingleton.off(eventName, handler)
    })
    registeredHandlers.length = 0

    registeredGlobalHandlers.forEach((handler) => {
      sseSingleton.offMessage(handler)
    })
    registeredGlobalHandlers.clear()
  })

  return {
    isConnected: readonly(sseSingleton.isConnected),
    isReconnecting: readonly(sseSingleton.isReconnecting),
    error: readonly(sseSingleton.error),
    retryCount: readonly(sseSingleton.retryCount),
    lastEvent: readonly(sseSingleton.lastEvent),

    on,
    off,
    onMessage,
    offMessage,

    connect: () => sseSingleton.connect(url, { withCredentials }, reconnectCfg),
    disconnect: () => sseSingleton.disconnect(),
  }
}
