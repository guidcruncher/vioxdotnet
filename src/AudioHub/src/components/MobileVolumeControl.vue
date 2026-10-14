<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import type { GetVolumeResponse } from '@/types'

const api = useApiClient()
const masterVol = ref(50)
const isMuted = ref(false)
const previousVolume = ref(50)
let debounceTimer: ReturnType<typeof setTimeout> | null = null

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

function toggleMute() {
  if (isMuted.value) {
    isMuted.value = false
    masterVol.value = previousVolume.value
  } else {
    previousVolume.value = masterVol.value
    isMuted.value = true
    masterVol.value = 0
  }
  onVolumeChange()
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
  <div
    class="w-full flex items-center space-x-3 mt-4 pt-4 border-t border-slate-800/60 text-slate-400"
  >
    <button
      @click="toggleMute"
      class="hover:text-white transition-colors focus:outline-none"
      aria-label="Mute or unmute volume"
      title="Mute / Unmute"
    >
      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M15.536 8.464a5 5 0 010 7.072m2.828-9.9a9 9 0 010 12.728M5.586 15H4a1 1 0 01-1-1v-4a1 1 0 011-1h1.586l4.707-4.707C10.923 3.663 12 4.109 12 5v14c0 .891-1.077 1.337-1.707.707L5.586 15z"
        />
      </svg>
    </button>

    <div class="relative flex-1 flex items-center">
      <input
        type="range"
        min="0"
        max="100"
        v-model="masterVol"
        @input="onVolumeChange"
        class="w-full h-1.5 rounded-lg appearance-none cursor-pointer"
        aria-label="Volume slider"
      />
    </div>

    <button
      @click="toggleMute"
      class="hover:text-white transition-colors focus:outline-none"
      aria-label="Max volume"
      title="Volume"
    >
      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M15.536 8.464a5 5 0 010 7.072m2.828-9.9a9 9 0 010 12.728M5.586 15H4a1 1 0 01-1-1v-4a1 1 0 011-1h1.586l4.707-4.707C10.923 3.663 12 4.109 12 5v14c0 .891-1.077 1.337-1.707.707L5.586 15z"
        />
      </svg>
    </button>
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
