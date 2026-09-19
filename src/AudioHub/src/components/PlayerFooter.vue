<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { usePlaybackStore } from '@/stores/playbackStore'
import { useApiClient } from '@/composables/useApiClient'
import type { SetVolumeRequest, SetVolumeResponse, GetVolumeResponse } from '../types/api'

const store = usePlaybackStore()
const api = useApiClient()

const seekVal = ref(0)
const masterVol = ref(50)

async function onSeekChange() {
  const totalSeconds = Math.floor((seekVal.value / 100) * 240)
  const formatted = new Date(totalSeconds * 1000).toISOString().substring(11, 19)
}

function convertSeconds(totalSeconds: number): string {
  if (!totalSeconds || totalSeconds <= 0) {
    return '00:00:00'
  }
  const hours = Math.floor(totalSeconds / 3600)
  const minutes = Math.floor((totalSeconds % 3600) / 60)
  const seconds = Math.floor(totalSeconds % 60)
  const pad = (num: number): string => num.toString().padStart(2, '0')
  return `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`
}

async function onVolumeChange() {
  const res: SetVolumeResponse = await api.media.setVolume({ volumePercent: masterVol.value })
}

onMounted(async () => {
  const vol: GetVolumeResponse = await api.media.getVolume()
  masterVol.value = vol.volumePercent
})
</script>

<template>
  <footer
    class="h-24 border-t border-slate-800 bg-slate-900/90 backdrop-blur px-6 flex items-center justify-between sticky bottom-0 z-50"
  >
    <!-- Track Metadata -->
    <div class="flex items-center space-x-4 w-1/4">
      <img
        :src="
          store.state.track?.imageUrl ||
          'data:image/svg+xml,%3Csvg xmlns=\'http://www.w3.org/2000/svg\' width=\'100\' height=\'100\'%3E%3Crect width=\'100\' height=\'100\' fill=\'%231e293b\'/%3E%3C/svg%3E'
        "
        class="w-14 h-14 rounded bg-slate-800 object-cover shadow-md"
      />
      <div class="overflow-hidden">
        <h4 class="font-semibold text-sm truncate text-slate-100">
          {{ store.state.track?.title || 'No media playing' }}
        </h4>
        <p class="text-xs text-slate-400 truncate">
          {{ store.state.track?.album || 'Viox Core Server' }}
        </p>
        <span
          class="inline-block mt-1 px-1.5 py-0.5 text-[10px] rounded bg-slate-800 text-slate-400 border border-slate-700"
          >{{ store.state.activeBackend || 'Idle' }}</span
        >
      </div>
    </div>

    <!-- Transport Controls -->
    <div class="flex flex-col items-center space-y-2 w-2/4 max-w-xl">
      <div class="flex items-center space-x-4">
        <button @click="store.previous" class="text-slate-400 hover:text-white transition p-1.5">
          <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
            <path d="M6 6h2v12H6zm3.5 6l8.5 6V6z" />
          </svg>
        </button>
        <button
          @click="store.togglePlayPause"
          class="bg-white text-slate-900 p-3 rounded-full hover:scale-105 transition shadow-lg shadow-white/10 flex items-center justify-center"
        >
          <svg
            v-if="!store.state.playing"
            id="playIcon"
            class="w-5 h-5 fill-current"
            viewBox="0 0 24 24"
          >
            <path d="M8 5v14l11-7z" />
          </svg>
          <svg v-else id="pauseIcon" class="w-5 h-5 fill-current" viewBox="0 0 24 24">
            <path d="M6 19h4V5H6v14zm8-14v14h4V5h-4z" />
          </svg>
        </button>
        <button @click="store.next" class="text-slate-400 hover:text-white transition p-1.5">
          <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
            <path d="M6 18l8.5-6L6 6v12zM16 6v12h2V6h-2z" />
          </svg>
        </button>
      </div>
      <div class="w-full flex items-center space-x-3 text-xs text-slate-400 font-mono">
        <span>{{ convertSeconds(store.state.position ?? 0) }}</span>
        <input
          type="range"
          min="0"
          max="100"
          v-model="seekVal"
          @change="onSeekChange"
          class="w-full accent-indigo-500 bg-slate-700 h-1.5 rounded-lg cursor-pointer"
        />
        <span
          v-if="store.state.isLive"
          class="inline-block mt-1 px-1.5 py-0.5 text-[10px] rounded bg-slate-800 text-slate-400 border border-slate-700"
          >Live</span
        >
        <span v-else>{{ convertSeconds(store.state.track?.duration ?? 0) }}</span>
      </div>
    </div>

    <!-- Volume Control -->
    <div class="flex items-center justify-end space-x-3 w-1/4">
      <svg class="w-4 h-4 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M15.536 8.464a5 5 0 010 7.072m2.828-9.9a9 9 0 010 12.728M5.586 15H4a1 1 0 01-1-1v-4a1 1 0 011-1h1.586l4.707-4.707C10.923 3.663 12 4.109 12 5v14c0 .891-1.077 1.337-1.707.707L5.586 15z"
        />
      </svg>
      <input
        type="range"
        min="0"
        max="100"
        v-model="masterVol"
        @change="onVolumeChange"
        class="w-28 accent-indigo-500 bg-slate-700 h-1.5 rounded-lg cursor-pointer"
      />
    </div>
  </footer>
</template>
