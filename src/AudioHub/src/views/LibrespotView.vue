<script setup lang="ts">
import { ref } from 'vue'
import { useApiClient } from '@/composables/useApiClient'

const api = useApiClient()
const libreUri = ref('')

async function playContext() {
  if (libreUri.value.trim()) {
    await api.media.play({ uri: libreUri.value.trim() })
  }
}

async function setShuffle(enabled: boolean) {
  await api.media.setShuffleContext({ shuffle_context: enabled })
}
</script>

<template>
  <div
    class="w-full min-h-full flex flex-col items-center justify-center p-4 sm:p-6 lg:p-8 overscroll-none touch-none select-none text-slate-100"
  >
    <div class="w-full max-w-lg">
      <h2 class="text-lg sm:text-xl font-bold mb-3 sm:mb-4 text-white">
        Librespot Control Console
      </h2>

      <div class="bg-slate-900 border border-slate-800 rounded-2xl p-4 sm:p-6 space-y-4 shadow-2xl">
        <div>
          <label class="block text-xs sm:text-sm font-medium mb-2 text-slate-300"
            >Spotify Context URI</label
          >
          <input
            v-model="libreUri"
            type="text"
            placeholder="spotify:playlist:37i9dQZF1DXcBWIGoYBM5M"
            class="w-full bg-slate-800 border border-slate-700 rounded-lg p-2.5 text-xs sm:text-sm text-slate-100 focus:outline-none focus:border-indigo-500 transition"
          />
        </div>

        <div class="flex flex-col sm:flex-row gap-2 sm:gap-3">
          <button
            @click="playContext"
            class="flex-1 bg-indigo-600 hover:bg-indigo-500 active:scale-95 text-xs sm:text-sm font-semibold py-2.5 px-4 rounded-lg text-white shadow-sm shadow-indigo-600/30 transition shrink-0"
          >
            Play Context
          </button>
          <button
            @click="setShuffle(true)"
            class="bg-slate-800 hover:bg-slate-700 active:scale-95 text-xs font-medium px-4 py-2.5 rounded-lg border border-slate-700 text-slate-200 transition shrink-0"
          >
            Shuffle On
          </button>
          <button
            @click="setShuffle(false)"
            class="bg-slate-800 hover:bg-slate-700 active:scale-95 text-xs font-medium px-4 py-2.5 rounded-lg border border-slate-700 text-slate-200 transition shrink-0"
          >
            Shuffle Off
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
