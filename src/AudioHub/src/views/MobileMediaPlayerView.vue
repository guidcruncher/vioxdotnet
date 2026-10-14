<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import { useServerEvents } from '@/composables/useServerEvents'
import { PlaybackRepeatMode } from '@/types'
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
const isRepeatEnabled = ref<boolean>(false)
const repeatMode = ref<PlaybackRepeatMode>(PlaybackRepeatMode.Off)
const isQueueOpen = ref<boolean>(false)
const lastTrack = ref<string>('')
const currentTimeClock = ref<string>('9:41')

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
      title: 'No media',
      artist: '',
      album: '',
      url: '',
      imageUrl: '/notrack.png',
      duration: 0,
    }
  )
})

const isPlaying = computed(() => !!playbackState.value.playing)

// Initialize Live Clock
function updateClock() {
  const now = new Date()
  let hours = now.getHours()
  let minutes = now.getMinutes()
  const formattedMinutes = minutes < 10 ? '0' + minutes : minutes
  currentTimeClock.value = `${hours}:${formattedMinutes}`
}

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
    repeatMode.value = status.repeatMode ?? PlaybackRepeatMode.Off
    isRepeatEnabled.value = repeatMode.value !== PlaybackRepeatMode.Off
  } catch (err) {
    console.error('Failed to fetch queue status:', err)
  }
}

async function toggleShuffle() {
  const nextState = !isShuffleEnabled.value
  await queueApi.setShuffle(nextState)
  isShuffleEnabled.value = nextState
  await fetchStatus()
}

async function toggleRepeat() {
  const nextMode =
    repeatMode.value === PlaybackRepeatMode.Off ? PlaybackRepeatMode.Track : PlaybackRepeatMode.Off
  await queueApi.setRepeatMode(nextMode)
  repeatMode.value = nextMode
  isRepeatEnabled.value = nextMode !== PlaybackRepeatMode.Off
  await fetchStatus()
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
  updateClock()
  setInterval(updateClock, 1000)
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
  <div
    class="h-full bg-slate-950 text-slate-100 flex items-center justify-center p-0 sm:p-4 overflow-x-hidden selection:bg-purple-500 selection:text-white"
  >
    <!-- Background Gradient Glow Orbs -->
    <div class="absolute inset-0 overflow-hidden pointer-events-none z-0">
      <div class="absolute -top-40 -left-40 w-96 h-96 bg-purple-900/20 rounded-full blur-3xl"></div>
      <div
        class="absolute -bottom-40 -right-40 w-96 h-96 bg-indigo-900/20 rounded-full blur-3xl"
      ></div>
    </div>

    <!-- Phone Shell Frame -->
    <div
      class="relative z-10 w-full sm:max-w-[400px] h-full sm:h-[840px] bg-slate-900 sm:rounded-[48px] sm:border-[8px] sm:border-slate-800 shadow-2xl flex flex-col overflow-hidden"
    >
      <!-- Mobile Status Bar -->
      <div
        class="pt-3 px-6 flex justify-between items-center text-xs font-medium text-slate-400 select-none z-30"
      >
        <span>{{ currentTimeClock }}</span>
        <div
          class="absolute left-1/2 -translate-x-1/2 top-2.5 w-28 h-5 bg-slate-950 rounded-full hidden sm:block"
        ></div>
        <div class="flex items-center space-x-1.5">
          <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24">
            <path
              d="M12 3c-4.97 0-9 4.03-9 9 0 2.12.74 4.07 1.97 5.61L4.35 19h3.3c1.32.61 2.82 1 4.35 1 4.97 0 9-4.03 9-9s-4.03-9-9-9zm0 16c-3.86 0-7-3.14-7-7s3.14-7 7-7 7 3.14 7 7-3.14 7-7 7z"
            />
          </svg>
          <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24">
            <path
              d="M12 3C6.95 3 3.15 4.85 0 7.23L12 22 24 7.23C20.85 4.85 17.05 3 12 3zm0 3.7c3.92 0 7.24 1.35 10.02 3.49L12 20.3 1.98 10.19C4.76 8.05 8.08 6.7 12 6.7z"
            />
          </svg>
          <svg class="w-5 h-3.5" fill="currentColor" viewBox="0 0 24 24">
            <path
              d="M16 4h-2V2h-4v2H8c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H8V6h8v14z"
            />
          </svg>
        </div>
      </div>

      <!-- Top Header Component -->
      <MobilePlayerHeader
        playlist-title=""
        :playlist-count="queue.length"
        @toggle-queue="isQueueOpen = !isQueueOpen"
        @minimize="console.log('Minimized')"
        @maximize="console.log('Maximized')"
      />

      <!-- Main Content Area -->
      <main class="flex-1 px-6 flex flex-col justify-between pb-8 z-20 relative overflow-hidden">
        <!-- Album Artwork -->
        <MobileAlbumArt :track="activeTrack" :is-playing="isPlaying" />

        <!-- Track Title & Artist with Like Button -->
        <MobileTrackInfo :track="activeTrack" />

        <!-- Progress Bar -->
        <MobileProgressBar />

        <!-- Playback Controls -->
        <MobilePlayerControls
          :is-shuffle="isShuffleEnabled"
          :is-repeat="isRepeatEnabled"
          @toggle-shuffle="toggleShuffle"
          @toggle-repeat="toggleRepeat"
        />

        <!-- Volume Control Bar -->
        <MobileVolumeControl />
      </main>

      <!-- Playlist Queue Drawer Overlay -->
      <div
        class="absolute inset-x-0 bottom-0 top-16 bg-slate-900/95 backdrop-blur-xl border-t border-slate-800 transform transition-transform duration-300 ease-in-out z-40 flex flex-col"
        :class="isQueueOpen ? 'translate-y-0' : 'translate-y-full'"
      >
        <MobilePlaylistDrawer
          :is-open="isQueueOpen"
          :playlist="queue"
          :current-track-id="activeTrack.rawUri"
          @close="isQueueOpen = false"
          @select-track="selectTrack"
        />
      </div>

      <!-- Mobile Home Indicator -->
      <div class="pb-2 pt-1 flex justify-center bg-slate-900 z-30 select-none">
        <div class="w-32 h-1 bg-slate-700 rounded-full"></div>
      </div>
    </div>
  </div>
</template>
