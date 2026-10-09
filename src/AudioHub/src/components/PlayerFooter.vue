<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { usePlaybackStore } from '@/stores/playbackStore'
import { useApiClient } from '@/composables/useApiClient'
import type { GetVolumeResponse, SetVolumeRequest, VolumeState } from '@/types'
import {
  Volume2,
  Repeat,
  SkipForward,
  SkipBack,
  Shuffle,
  RotateCcw,
  RotateCw,
  Play,
  Trash,
  Pause,
} from '@lucide/vue'

const store = usePlaybackStore()
const api = useApiClient()

const masterVol = ref(50)
let debounceTimer: ReturnType<typeof setTimeout> | null = null

// Cached progress percentage calculation
const progressPercent = computed(() => {
  const duration = store.state.track?.duration
  const position = store.state.position ?? 0

  if (!duration || duration <= 0) return 0
  return Math.min(100, (position / duration) * 100)
})

// Dynamic time formatter (M:SS / H:MM:SS)
function convertSeconds(totalSeconds?: number): string {
  if (!totalSeconds || totalSeconds <= 0) {
    return '0:00'
  }
  const hours = Math.floor(totalSeconds / 3600)
  const minutes = Math.floor((totalSeconds % 3600) / 60)
  const seconds = Math.floor(totalSeconds % 60)
  const pad = (num: number): string => num.toString().padStart(2, '0')

  if (hours > 0) {
    return `${hours}:${pad(minutes)}:${pad(seconds)}`
  }
  return `${minutes}:${pad(seconds)}`
}

// Debounced volume dispatch to eliminate API spam on drag/change
function onVolumeChange() {
  if (debounceTimer) {
    clearTimeout(debounceTimer)
  }

  debounceTimer = setTimeout(async () => {
    try {
      await api.media.setVolume({ volumePercent: masterVol.value })
    } catch (error) {
      console.error('Failed to set volume:', error)
    }
  }, 200)
}

onMounted(async () => {
  try {
    const vol: GetVolumeResponse = await api.media.getVolume()
    masterVol.value = vol.volumePercent
  } catch (error) {
    console.error('Failed to load initial volume state:', error)
  }
})
</script>

<template>
  <footer
    class="relative h-20 sm:h-24 border-t border-slate-800 bg-slate-900/95 backdrop-blur-md px-3 sm:px-6 flex items-center justify-between sticky bottom-0 z-50 gap-2 sm:gap-4"
  >
    <!-- Top Progress Line (Visible on Mobile `< sm`) -->
    <div class="absolute top-0 left-0 right-0 h-1 bg-slate-800 sm:hidden">
      <div
        class="bg-indigo-500 h-full transition-all duration-300"
        :style="{ width: `${progressPercent}%` }"
      ></div>
    </div>

    <!-- Track Metadata Section -->
    <div class="flex items-center space-x-2.5 sm:space-x-4 min-w-0 flex-1 sm:flex-initial sm:w-1/4">
      <img
        :src="
          store.state.track?.imageUrl ||
          'data:image/svg+xml,%3Csvg xmlns=\'http://www.w3.org/2000/svg\' width=\'100\' height=\'100\'%3E%3Crect width=\'100\' height=\'100\' fill=\'%231e293b\'/%3E%3C/svg%3E'
        "
        alt="Album Artwork"
        class="w-11 h-11 sm:w-14 sm:h-14 rounded-lg bg-slate-800 object-cover shadow-md shrink-0 border border-slate-700/50"
      />
      <div class="min-w-0 flex-1">
        <h4 class="font-semibold text-xs sm:text-sm truncate text-slate-100">
          {{ store.state.track?.title || 'No media playing' }}
        </h4>
        <p class="text-[11px] sm:text-xs text-slate-400 truncate mt-0.5">
          {{ store.state.track?.album || 'Viox Core Server' }}
        </p>
        <span
          class="hidden sm:inline-block mt-1 px-1.5 py-0.5 text-[10px] rounded bg-slate-800 text-slate-400 border border-slate-700/80 font-medium"
        >
          {{ store.state.activeBackend || 'Idle' }}
        </span>
      </div>
    </div>

    <!-- Transport Controls & Timeline Bar -->
    <div
      class="flex flex-col items-center justify-center space-y-1.5 sm:space-y-2 shrink-0 sm:w-2/4 sm:max-w-xl"
    >
      <!-- Media Transport Buttons -->
      <div class="flex items-center space-x-3 sm:space-x-5">
        <button
          @click="store.previous"
          class="text-slate-400 hover:text-white transition p-2 sm:p-1.5 active:scale-95 focus:outline-none"
          title="Previous Track"
        >
          <SkipBack class="w-5 h-5 sm:w-5 sm:h-5" />
        </button>

        <button
          @click="store.togglePlayPause"
          class="bg-white text-slate-900 p-2.5 sm:p-3 rounded-full hover:scale-105 active:scale-95 transition shadow-lg shadow-white/10 flex items-center justify-center focus:outline-none"
          title="Play / Pause"
        >
          <Play v-if="!store.state.playing" id="playIcon" class="w-5 h-5 fill-current" />
          <Pause v-else id="pauseIcon" class="w-5 h-5 fill-current" />
        </button>

        <button
          @click="store.next"
          class="text-slate-400 hover:text-white transition p-2 sm:p-1.5 active:scale-95 focus:outline-none"
          title="Next Track"
        >
          <SkipForward class="w-5 h-5 sm:w-5 sm:h-5" />
        </button>
      </div>

      <!-- Desktop Timeline & Position Display -->
      <div class="hidden sm:flex w-full items-center space-x-3 text-xs text-slate-400 font-mono">
        <span class="w-10 text-right">{{ convertSeconds(store.state.position ?? 0) }}</span>
        <div class="w-full bg-slate-800 h-1.5 rounded-full overflow-hidden">
          <div
            class="bg-indigo-500 h-full rounded-full transition-all duration-300"
            :style="{ width: `${progressPercent}%` }"
          ></div>
        </div>
        <span
          v-if="store.state.isLive"
          class="inline-block px-1.5 py-0.5 text-[10px] rounded bg-indigo-950/80 text-indigo-300 border border-indigo-800/60 uppercase tracking-wider"
        >
          Live
        </span>
        <span v-else class="w-10">{{ convertSeconds(store.state.track?.duration ?? 0) }}</span>
      </div>
    </div>

    <!-- Volume Control Section (Hidden on narrow mobile `< sm`) -->
    <div class="hidden sm:flex items-center justify-end space-x-3 sm:w-1/4">
      <Volume2 class="w-4 h-4 text-slate-400 shrink-0" />
      <input
        type="range"
        min="0"
        max="100"
        v-model="masterVol"
        @input="onVolumeChange"
        class="w-20 lg:w-24 accent-indigo-500 bg-slate-700 h-1.5 rounded-lg cursor-pointer"
      />
    </div>
  </footer>
</template>
