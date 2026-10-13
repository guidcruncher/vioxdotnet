<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import { useServerEvents } from '@/composables/useServerEvents'
import type { MediaMetaData, MediaEvent, PlaybackState, QueueStatusResponse } from '@/types'
import MobilePlayerHeader from '../components/MobilePlayerHeader.vue'
import MobileAlbumArt from '../components/MobileAlbumArt.vue'
import MobileTrackInfo from '../components/MobileTrackInfo.vue'
import MobileProgressBar from '../components/MobileProgressBar.vue'
import MobilePlayerControls from '../components/MobilePlayerControls.vue'
import MobileVolumeControl from '../components/MobileVolumeControl.vue'
import MobilePlaylistDrawer from '../components/MobilePlaylistDrawer.vue'

const apiClient = useApiClient()
const queueApi = apiClient.queue

const sse = useServerEvents({
  autoReconnect: true,
  immediate: true,
})

const queue = ref<MediaMetaData[]>([])
const currentIndex = ref<number>(-1)
const currentItem = ref<MediaMetaData | null>(null)
const isShuffleEnabled = ref<boolean>(false)
const isQueueOpen = ref<boolean>(false)
const lastTrack = ref<string>('')

const playbackState = ref<PlaybackState>({
  playing: false,
  activeBackend: '',
  track: null,
  position: 0,
  isLive: false,
})

const activeTrack = computed<MediaMetaData>(() => {
  return (
    currentItem.value ||
    playbackState.value.track || {
      rawUri: '',
      title: 'Nothing Playing',
      artist: 'Select a track',
      album: '',
      url: '',
      imageUrl: '',
    }
  )
})

const isPlaying = computed(() => !!playbackState.value.playing)

sse.on('Status', async (payload: MediaEvent) => {
  playbackState.value = {
    activeBackend: payload.backend,
    track: payload.track,
    position: payload.data.position,
    playing: payload.data.playing,
    isLive: payload.data.isLive,
  }

  if (payload.track?.rawUri && payload.track.rawUri !== lastTrack.value) {
    lastTrack.value = payload.track.rawUri
    await fetchStatus()
  }
})

async function fetchStatus(): Promise<void> {
  try {
    const status: QueueStatusResponse = await queueApi.getStatus()
    queue.value = status.items || []
    currentIndex.value = status.currentIndex ?? -1
    currentItem.value = status.currentItem || playbackState.value.track || null
    isShuffleEnabled.value = status.isShuffleEnabled ?? false
  } catch (err) {
    console.error('Failed to fetch queue status:', err)
  }
}

async function togglePlay() {
  if (playbackState.value.playing) {
    await apiClient.media.pause()
  } else {
    await apiClient.media.resume()
  }
}

async function nextTrack() {
  await queueApi.next()
  await fetchStatus()
}

async function prevTrack() {
  await queueApi.previous()
  await fetchStatus()
}

async function toggleShuffle() {
  const nextState = !isShuffleEnabled.value
  await queueApi.setShuffle(nextState)
  isShuffleEnabled.value = nextState
  await fetchStatus()
}

async function seek(newTime: number) {
  if (apiClient.media.seek) {
    await apiClient.media.seek({ position: `${newTime}` })
  }
}

async function selectTrack(track: MediaMetaData) {
  const index = queue.value.findIndex((t: any) => t.rawUri === track.rawUri)
  if (index !== -1) {
    await queueApi.skipTo(index)
    await fetchStatus()
  }
  isQueueOpen.value = false
}

onMounted(async () => {
  await fetchStatus()
  const state = await apiClient.media.getPlaybackState()
  if (state) {
    playbackState.value = state
    if (state.track?.rawUri) {
      lastTrack.value = state.track.rawUri
    }
  }
})
</script>

<template>
  <div class="min-h-screen bg-zinc-950 flex items-center justify-center p-4">
    <!-- Responsive Mobile Container (Strictly Isolated Phone Shell) -->
    <div
      class="relative w-full max-w-sm h-[800px] bg-zinc-900 rounded-[2.5rem] border border-zinc-800 shadow-2xl overflow-hidden flex flex-col justify-between"
    >
      <!-- Top Header -->
      <MobilePlayerHeader
        :playlist-count="queue.length"
        @toggle-queue="isQueueOpen = true"
        @minimize="console.log('Minimized')"
      />

      <!-- Main Content Area -->
      <div class="flex-1 flex flex-col justify-center px-6">
        <!-- Album Artwork -->
        <MobileAlbumArt :track="activeTrack" :is-playing="isPlaying" />

        <!-- Track Title & Artist -->
        <MobileTrackInfo :track="activeTrack" />

        <!-- Progress Bar -->
        <MobileProgressBar
          :current-time="playbackState.position"
          :duration="activeTrack.duration"
          @seek="seek"
        />

        <!-- Playback Controls -->
        <MobilePlayerControls
          :is-playing="isPlaying"
          :is-shuffle="isShuffleEnabled"
          @toggle-play="togglePlay"
          @next="nextTrack"
          @prev="prevTrack"
          @toggle-shuffle="toggleShuffle"
        />
      </div>

      <!-- Volume Control Bottom Bar -->
      <MobileVolumeControl />

      <!-- Playlist Queue Drawer (Handles queue list conditionally as an overlay/drawer) -->
      <MobilePlaylistDrawer
        :is-open="isQueueOpen"
        :playlist="queue"
        :current-track-id="activeTrack.rawUri"
        @close="isQueueOpen = false"
        @select-track="selectTrack"
      />
    </div>
  </div>
</template>
