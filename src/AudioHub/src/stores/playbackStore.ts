import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { MediaMetaData, PlaybackState, MediaEvent } from '@/types'
import { useApiClient } from '@/composables/useApiClient'
import { useServerEvents } from '@/composables/useServerEvents'

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

  sse.on('PlaybackStarted', async (payload: MediaEvent) => {
    handleTrackEvent(payload.track, true)
  })

  sse.on('TrackFinished', async (payload: MediaEvent) => {
    if (!state.value.playing) {
      await api.queue.next()
    }
  })

  sse.on('Status', (payload: MediaEvent) => {
    const data: PlaybackState = {
      activeBackend: payload.backend,
      track: payload.track,
      position: payload.data.position,
      playing: payload.data.playing,
      isLive: payload.data.isLive,
    }
    applyStateUpdates(data)
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

  async function stop() {
    await api.media.stop()
  }

  async function next() {
    await api.media.next()
  }

  async function previous() {
    await api.media.previous()
  }

  async function playUri(uri: string) {
    await api.queue.enqueue({ uri })
    //    await api.media.play({ uri })
  }

  return {
    state,
    sse,
    syncState,
    togglePlayPause,
    stop,
    next,
    previous,
    playUri,
  }
})
