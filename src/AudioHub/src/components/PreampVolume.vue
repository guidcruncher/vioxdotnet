<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import type { PreampVolumeState } from '@/types'

// Initialize API client and state
const api = useApiClient()
const preAmpVolume = ref<PreampVolumeState | null>(null)

onMounted(async () => {
  await fetchPreampVolume()
})

async function fetchPreampVolume(): Promise<void> {
  preAmpVolume.value = await api.equalizer.getPreampVolume()
}

async function resetLevels() {
  await api.equalizer.setPreampVolume(80)
  preAmpVolume.value = { volume: 80 }
}

async function handleVolumeChange(value: number) {
  await api.equalizer.setPreampVolume(value)
  if (!preAmpVolume.value) {
    preAmpVolume.value = { volume: value }
  } else {
    preAmpVolume.value.volume = value
  }
}
</script>

<template>
  <div
    class="w-full max-w-4xl rounded-xl border border-slate-800 bg-slate-900 p-4 sm:p-6 text-slate-100 shadow-2xl"
  >
    <!-- Header -->
    <div
      class="mb-6 flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-800 pb-4"
    >
      <div>
        <h2 class="text-xl font-bold tracking-wide text-white">Levels</h2>
      </div>
      <button
        type="button"
        class="inline-flex items-center gap-1.5 rounded-lg border border-slate-700 bg-slate-800 px-3 py-1.5 text-xs font-medium text-slate-300 hover:bg-slate-700 hover:text-white transition-colors"
        @click="resetLevels"
      >
        Reset Levels
      </button>
    </div>

    <!-- Volume Control -->
    <div v-if="preAmpVolume">
      <div>
        <p class="text-xs text-slate-400 mb-2">Input Volume Levels</p>
        <input
          type="range"
          min="0"
          max="100"
          :value="preAmpVolume.volume"
          class="w-[50vw] accent-indigo-500 bg-slate-700 h-1.5 rounded-lg cursor-pointer"
          @input="(e) => handleVolumeChange(Number((e.target as HTMLInputElement).value))"
        />
      </div>
    </div>
  </div>
</template>
