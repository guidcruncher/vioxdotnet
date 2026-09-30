import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { MediaMetaData, PlaybackState } from '@/types/api'
import { useApiClient } from '@/composables/useApiClient'
import { useServerEvents, type EventPayload } from '@/composables/useServerEvents'

export const usePlaybackStore = defineStore('playback', () => {
  const api = useApiClient()

  const state = ref<PlaybackState>({
    playing: false,
    activeBackend: '',
    track: null,
    position: 0,
    isLive: false,
  })

  // Initialize SSE composable with unknown type to allow flexible event payloads
  const sse = useServerEvents({
    autoReconnect: true,
    immediate: true,
  })

  // Helper to normalize state and determine isLive flag
  const applyStateUpdates = (incomingState: PlaybackState) => {
    const hasTrackUri = incomingState.track?.uri?.type
    const isLive = hasTrackUri ? hasTrackUri === 'station' : (state.value.isLive ?? false)

    state.value = {
      ...state.value,
      ...incomingState,
      isLive,
    }
  }

  // Helper to handle track-specific event payloads
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

  // Handle incoming status event (returns full PlaybackState)
  sse.on('status', (payload: EventPayload<unknown>) => {
    const data = payload.data as PlaybackState | undefined
    if (data) {
      applyStateUpdates(data)
    }
  })

  // Handle incoming track/media events (return MediaMetaData)
  const trackEventTypes = ['play', 'pause', 'previous', 'next', 'trackchanged']
  trackEventTypes.forEach((eventType) => {
    sse.on(eventType, (payload: EventPayload<unknown>) => {
      const data = payload.data as MediaMetaData | undefined
      const playingState = eventType === 'play' ? true : eventType === 'pause' ? false : undefined

      handleTrackEvent(data, playingState)
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
    await api.media.play({ uri })
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
