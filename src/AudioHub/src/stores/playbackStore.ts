import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { PlaybackState } from '@/types/api'
import { useApiClient } from '@/composables/useApiClient'

export const usePlaybackStore = defineStore('playback', () => {
  const api = useApiClient()
  const state = ref<PlaybackState>({
    playing: false,
    activeBackend: '',
    track: null,
    position: 0,
    isLive: false,
  })
  let pollingTimer: number | null = null

  async function pollState() {
    const res = await api.media.getPlaybackState()
    if (res) {
      if (res.track && res.track.uri) {
        if (res.track.uri.type == 'station') {
          res.isLive = true
        } else {
          res.isLive = false
        }
      }

      state.value = res
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

  function startPolling() {
    pollState()
    if (!pollingTimer) {
      pollingTimer = window.setInterval(pollState, 3000)
    }
  }

  async function togglePlayPause() {
    if (state.value.playing) {
      await api.media.pause()
    } else {
      await api.media.resume()
    }
    await pollState()
  }

  async function next() {
    await api.media.next()
    await pollState()
  }

  async function previous() {
    await api.media.previous()
    await pollState()
  }

  async function playUri(uri: string) {
    await api.media.play({ uri })
    await pollState()
  }

  return {
    state,
    startPolling,
    togglePlayPause,
    next,
    previous,
    playUri,
  }
})
