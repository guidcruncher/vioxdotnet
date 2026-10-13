<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import type { MediaMetaData } from '@/types'
import MobilePlayerHeader from '../components/MobilePlayerHeader.vue'
import MobileAlbumArt from '../components/MobileAlbumArt.vue'
import MobileTrackInfo from '../components/MobileTrackInfo.vue'
import MobileProgressBar from '../components/MobileProgressBar.vue'
import MobilePlayerControls from '../components/MobilePlayerControls.vue'
import MobileVolumeControl from '../components/MobileVolumeControl.vue'
import MobilePlaylistDrawer from '../components/MobilePlaylistDrawer.vue'

// TODO: Import your actual Pinia store or custom composable here
// e.g., const playerStore = usePlayerStore();
// e.g., const { currentTrack, isPlaying, playlist, currentTime, ... } = useAudioPlayer();

// Mock initial data as placeholders matching expected API interfaces
const playlist = ref<MediaMetaData[]>([])

const currentIndex = ref(0)
const currentTrack = ref<MediaMetaData>(playlist.value[0])
const isPlaying = ref(false)
const currentTime = ref(0)
const isShuffle = ref(false)
const isQueueOpen = ref(false)

let timer: number | null = null

// TODO: Replace with store actions / composable methods
function togglePlay() {
  isPlaying.value = !isPlaying.value
  if (isPlaying.value) {
    startTimer()
  } else {
    stopTimer()
  }
}

function startTimer() {
  if (timer) return
  timer = window.setInterval(() => {
    if (currentTime.value < (currentTrack.value.duration ?? 0)) {
      currentTime.value += 1
    } else {
      nextTrack()
    }
  }, 1000)
}

function stopTimer() {
  if (timer) {
    clearInterval(timer)
    timer = null
  }
}

function nextTrack() {
  if (isShuffle.value) {
    let nextIdx
    do {
      nextIdx = Math.floor(Math.random() * playlist.value.length)
    } while (nextIdx === currentIndex.value && playlist.value.length > 1)
    currentIndex.value = nextIdx
  } else {
    currentIndex.value = (currentIndex.value + 1) % playlist.value.length
  }
  currentTrack.value = playlist.value[currentIndex.value]
  currentTime.value = 0
}

function prevTrack() {
  currentIndex.value = (currentIndex.value - 1 + playlist.value.length) % playlist.value.length
  currentTrack.value = playlist.value[currentIndex.value]
  currentTime.value = 0
}

function seek(newTime: number) {
  currentTime.value = newTime
}

function toggleShuffle() {
  isShuffle.value = !isShuffle.value
}

function selectTrack(track: MediaMetaData) {
  const index = playlist.value.findIndex((t: any) => t.rawUri === track.rawUri)
  if (index !== -1) {
    currentIndex.value = index
    currentTrack.value = playlist.value[index]
    currentTime.value = 0
    isPlaying.value = true
    startTimer()
  }
  isQueueOpen.value = false
}

onMounted(() => {
  // TODO: Initialize player state or fetch initial playlist from API here
  // e.g., playerStore.fetchPlaylist();
})

onUnmounted(() => {
  stopTimer()
})
</script>

<template>
  <div class="min-h-screen bg-zinc-950 flex items-center justify-center p-4">
    <!-- Responsive Mobile Container -->
    <div
      class="relative w-full max-w-sm bg-zinc-900 rounded-[2.5rem] border border-zinc-800 shadow-2xl overflow-hidden flex flex-col justify-between"
    >
      <!-- Top Header -->
      <MobilePlayerHeader
        :playlist-count="playlist.length"
        @toggle-queue="isQueueOpen = true"
        @minimize="console.log('Minimized')"
      />

      <!-- Main Content Area -->
      <div class="flex-1 flex flex-col justify-center">
        <!-- Album Artwork -->
        <MobileAlbumArt :track="currentTrack" :is-playing="isPlaying" />

        <!-- Track Title & Artist -->
        <MobileTrackInfo :track="currentTrack" />

        <!-- Progress Bar -->
        <MobileProgressBar
          :current-time="currentTime"
          :duration="currentTrack.duration"
          @seek="seek"
        />

        <!-- Playback Controls -->
        <MobilePlayerControls
          :is-playing="isPlaying"
          :is-shuffle="isShuffle"
          @toggle-play="togglePlay"
          @next="nextTrack"
          @prev="prevTrack"
          @toggle-shuffle="toggleShuffle"
        />
      </div>

      <!-- Volume Control Bottom Bar -->
      <MobileVolumeControl />

      <!-- Playlist Queue Drawer -->
      <MobilePlaylistDrawer
        :is-open="isQueueOpen"
        :playlist="playlist"
        :current-track-id="currentTrack.rawUri"
        @close="isQueueOpen = false"
        @select-track="selectTrack"
      />
    </div>
  </div>
</template>
