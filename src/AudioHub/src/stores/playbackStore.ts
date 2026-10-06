import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { MediaMetaData, PlaybackState } from '@/types'
import { useApiClient } from '@/composables/useApiClient'
import { useServerEvents, type EventPayload } from '@/composables/useServerEvents'

export interface ExtendedPlaybackState extends Omit<PlaybackState, 'track'> {
  track: MediaMetaData | null
}

export const usePlaybackStore = defineStore('playback', () => {
  const api = useApiClient()

  const state = ref<ExtendedPlaybackState>({
    playing: false,
    activeBackend: '',
    track: null,
    position: 0,
    isLive: false,
  })

  const sse = useServerEvents({
    autoReconnect: true,
    immediate: true,
  })

  const applyStateUpdates = (incomingState: Partial<PlaybackState>) => {
    const hasTrackUri = incomingState.track?.uri?.type
    const isLive = hasTrackUri ? hasTrackUri === 'station' : (state.value.isLive ?? false)

    state.value = {
      ...state.value,
      ...incomingState,
      track: incomingState.track !== undefined ? (incomingState.track ?? null) : state.value.track,
      isLive,
    }
  }

  const handleTrackEvent = (trackPayload: MediaMetaData | undefined, playingState?: boolean) => {
    const track = trackPayload || null
    const isLive = track?.uri?.type === 'station'

    state.value = {
      ...state.value,
      track,
      isLive,
      ...(playingState !== undefined && { playing: playingState }),
    }
  }

  sse.on('status', (payload: EventPayload) => {
    try {
      if (payload.message) {
        const data = JSON.parse(payload.message) as PlaybackState
        applyStateUpdates(data)
      }
    } catch (err) {
      console.error('Failed to parse status event message:', err)
    }
  })

  const trackEventTypes = ['play', 'pause', 'previous', 'next', 'trackchanged']
  trackEventTypes.forEach((eventType) => {
    sse.on(eventType, (payload: EventPayload) => {
      try {
        const data = payload.message ? (JSON.parse(payload.message) as MediaMetaData) : undefined

        const playingState = eventType === 'play' ? true : eventType === 'pause' ? false : undefined

        handleTrackEvent(data, playingState)
      } catch (err) {
        console.error(`Failed to parse ${eventType} event message:`, err)
      }
    })
  })

  async function syncState() {
    const res = await api.media.getPlaybackState()
    if (res) {
      applyStateUpdates(res)
    } else {
      state.value = {
        playing: false,
        activeBackend: '',
        track: null,
        position: 0,
        isLive: false,
      }
    }
  }

  async function togglePlayPause() {
    if (state.value.playing) {
      await api.media.pause()
    } else {
      await api.media.resume()
    }
  }

  async function next() {
    await api.media.next()
  }

  async function previous() {
    await api.media.previous()
  }

  async function playUri(uri: string) {
      await api.queue.enqueue({uri})
//    await api.media.play({ uri })
  }

  return {
    state,
    sse,
    syncState,
    togglePlayPause,
    next,
    previous,
    playUri,
  }
})
