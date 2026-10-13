<script setup lang="ts">
import { computed } from 'vue'
import { usePlaybackStore } from '@/stores/playbackStore'
import { useApiClient } from '@/composables/useApiClient'

const store = usePlaybackStore()
const api = useApiClient()

const progressPercent = computed(() => {
  const duration = store.state.track?.duration
  const position = store.state.position ?? 0

  if (!duration || duration <= 0) return 0
  return Math.min(100, (position / duration) * 100)
})

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

async function handleSeek(e: Event) {
  const target = e.target as HTMLInputElement
  const val = Number(target.value)
  const duration = store.state.track?.duration ?? 0
  const newPosition = (val / 100) * duration

  try {
    // If store has seek method: store.seek(newPosition)
    await api.media.seek({ position: `${newPosition}` })
  } catch (error) {
    console.error('Failed to seek track position:', error)
  }
}
</script>

<template>
  <div class="px-8 py-4 space-y-2">
    <div class="relative group flex items-center">
      <input
        type="range"
        min="0"
        max="100"
        :value="progressPercent"
        @input="handleSeek"
        class="absolute inset-0 w-full opacity-0 cursor-pointer z-10 h-2"
        aria-label="Seek track position"
      />
      <div class="w-full bg-zinc-800 h-1.5 rounded-full overflow-hidden">
        <div
          class="bg-indigo-500 h-full rounded-full transition-all duration-100 relative"
          :style="{ width: `${progressPercent}%` }"
        >
          <div
            class="absolute right-0 top-1/2 -translate-y-1/2 w-3 h-3 bg-white rounded-full shadow-md opacity-0 group-hover:opacity-100 transition-opacity"
          ></div>
        </div>
      </div>
    </div>

    <div class="flex justify-between text-xs font-medium text-zinc-500 font-mono">
      <span>{{ convertSeconds(store.state.position ?? 0) }}</span>
      <span v-if="store.state.isLive" class="text-indigo-400 uppercase tracking-wider">Live</span>
      <span v-else>{{ convertSeconds(store.state.track?.duration ?? 0) }}</span>
    </div>
  </div>
</template>
