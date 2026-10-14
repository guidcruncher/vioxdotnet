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
    await api.media.seek({ position: `${newPosition}` })
  } catch (error) {
    console.error('Failed to seek track position:', error)
  }
}
</script>

<template>
  <div class="w-full space-y-2 mt-2">
    <div class="relative flex items-center">
      <input
        type="range"
        min="0"
        max="100"
        step="0.1"
        :value="progressPercent"
        @input="handleSeek"
        class="w-full h-1.5 rounded-lg appearance-none cursor-pointer"
        aria-label="Seek track position"
      />
    </div>
    <div class="flex justify-between text-xs font-semibold text-slate-400">
      <span>{{ convertSeconds(store.state.position ?? 0) }}</span>
      <span v-if="store.state.isLive" class="text-purple-400 uppercase tracking-wider">Live</span>
      <span v-else>{{ convertSeconds(store.state.track?.duration ?? 0) }}</span>
    </div>
  </div>
</template>

<style scoped>
/* Custom range slider styling matching the mockup */
input[type='range'] {
  -webkit-appearance: none;
  appearance: none;
  background: transparent;
  cursor: pointer;
}
input[type='range']::-webkit-slider-runnable-track {
  background: rgba(255, 255, 255, 0.15);
  height: 6px;
  border-radius: 9999px;
}
input[type='range']::-webkit-slider-thumb {
  -webkit-appearance: none;
  appearance: none;
  margin-top: -3px;
  background-color: #a78bfa;
  height: 12px;
  width: 12px;
  border-radius: 50%;
  box-shadow: 0 0 10px rgba(167, 139, 250, 0.8);
  transition: transform 0.1s ease;
}
input[type='range']::-webkit-slider-thumb:hover {
  transform: scale(1.3);
}

/* Firefox compatibility tracks/thumbs */
input[type='range']::-moz-range-track {
  background: rgba(255, 255, 255, 0.15);
  height: 6px;
  border-radius: 9999px;
}
input[type='range']::-moz-range-thumb {
  background-color: #a78bfa;
  height: 12px;
  width: 12px;
  border-radius: 50%;
  border: none;
  box-shadow: 0 0 10px rgba(167, 139, 250, 0.8);
  transition: transform 0.1s ease;
}
input[type='range']::-moz-range-thumb:hover {
  transform: scale(1.3);
}
</style>
