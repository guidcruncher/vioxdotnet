// sseSingleton.ts
import { ref } from 'vue'
import type { EventPayload, EventHandler } from '@/types'

interface ListenerMap {
  [eventName: string]: Set<EventHandler>
}

class SSESingleton {
  private static instance: SSESingleton

  private eventSource: EventSource | null = null
  private listeners: ListenerMap = {}
  private reconnectTimer: ReturnType<typeof setTimeout> | null = null

  isConnected = ref(false)
  isReconnecting = ref(false)
  error = ref<Error | null>(null)
  retryCount = ref(0)
  lastEvent = ref<EventPayload | null>(null)

  private constructor() {}

  static getInstance() {
    if (!SSESingleton.instance) {
      SSESingleton.instance = new SSESingleton()
    }
    return SSESingleton.instance
  }

  connect(url: string, opts: EventSourceInit, reconnectCfg: any) {
    if (this.eventSource) return

    try {
      this.eventSource = new EventSource(url, opts)

      this.eventSource.onopen = () => {
        this.isConnected.value = true
        this.isReconnecting.value = false
        this.retryCount.value = 0
        this.error.value = null

        Object.keys(this.listeners).forEach((eventName) => {
          if (eventName !== 'message') {
            this.eventSource?.addEventListener(eventName, this.handleCustomEvent)
          }
        })
      }

      this.eventSource.onmessage = this.handleMessage
      this.eventSource.onerror = () => {
        this.isConnected.value = false
        this.scheduleReconnect(url, opts, reconnectCfg)
      }
    } catch (err) {
      this.error.value = err instanceof Error ? err : new Error('Failed to establish EventSource')
      this.scheduleReconnect(url, opts, reconnectCfg)
    }
  }

  disconnect() {
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer)
      this.reconnectTimer = null
    }
    if (this.eventSource) {
      this.eventSource.close()
      this.eventSource = null
    }
    this.isConnected.value = false
    this.isReconnecting.value = false
  }

  private scheduleReconnect(url: string, opts: EventSourceInit, cfg: any) {
    const { autoReconnect, maxRetries, initialRetryIntervalMs, maxRetryIntervalMs } = cfg

    if (!autoReconnect) return
    if (this.retryCount.value >= maxRetries) {
      this.error.value = new Error(`Max reconnect attempts reached`)
      return
    }

    this.isReconnecting.value = true
    this.retryCount.value += 1

    const delay = Math.min(
      initialRetryIntervalMs * Math.pow(1.5, this.retryCount.value - 1),
      maxRetryIntervalMs
    )

    this.reconnectTimer = setTimeout(() => {
      this.connect(url, opts, cfg)
    }, delay)
  }

  private processPayload(raw: string): EventPayload | null {
    try {
      const parsed = JSON.parse(raw)
      this.lastEvent.value = parsed
      this.error.value = null
      return parsed
    } catch (err) {
      this.error.value = err instanceof Error ? err : new Error('Invalid JSON payload')
      return null
    }
  }

  private handleMessage = (event: MessageEvent) => {
    const payload = this.processPayload(event.data)
    if (!payload) return

    this.emit('message', payload)
    if (payload.eventType && payload.eventType !== 'message') {
      this.emit(payload.eventType, payload)
    }
  }

  private handleCustomEvent = (event: MessageEvent) => {
    const payload = this.processPayload(event.data)
    if (payload) {
      this.emit(event.type, payload)
    }
  }

  on(eventName: string, handler: EventHandler) {
    if (!this.listeners[eventName]) {
      this.listeners[eventName] = new Set()
      if (this.eventSource && this.isConnected.value) {
        this.eventSource.addEventListener(eventName, this.handleCustomEvent)
      }
    }
    this.listeners[eventName].add(handler)
  }

  off(eventName: string, handler: EventHandler) {
    const set = this.listeners[eventName]
    if (!set) return

    set.delete(handler)
    if (set.size === 0) {
      delete this.listeners[eventName]
      this.eventSource?.removeEventListener(eventName, this.handleCustomEvent)
    }
  }

  private emit(eventName: string, payload: EventPayload) {
    const set = this.listeners[eventName]
    if (!set) return

    set.forEach((handler) => {
      try {
        handler(payload)
      } catch (err) {
        console.error(`Error in SSE listener '${eventName}':`, err)
      }
    })
  }
}

export const sseSingleton = SSESingleton.getInstance()
