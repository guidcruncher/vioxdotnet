<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import type { GetVolumeResponse } from '@/types'
import { Volume2 } from '@lucide/vue'

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
  <div class="px-8 pt-4 pb-6 flex items-center gap-4 text-zinc-400">
    <button
      @click="toggleMute"
      class="hover:text-zinc-200 transition-colors focus:outline-none"
      aria-label="Mute or unmute volume"
    >
      <Volume2 class="w-5 h-5" />
    </button>

    <div class="relative flex-1 flex items-center">
      <input
        type="range"
        min="0"
        max="100"
        v-model="masterVol"
        @input="onVolumeChange"
        class="absolute inset-0 w-full opacity-0 cursor-pointer z-10 h-2"
        aria-label="Volume slider"
      />
      <div class="w-full bg-zinc-800 h-1.5 rounded-full overflow-hidden">
        <div
          class="bg-zinc-400 hover:bg-indigo-500 h-full rounded-full transition-all duration-100"
          :style="{ width: `${masterVol}%` }"
        ></div>
      </div>
    </div>
  </div>
</template>
