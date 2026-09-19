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
  <div>
    <h2 class="text-xl font-bold mb-4">Librespot Control Console</h2>
    <div class="bg-slate-900 border border-slate-800 rounded-xl p-6 max-w-lg space-y-4">
      <div>
        <label class="block text-sm font-medium mb-1">Spotify Context URI</label>
        <input
          v-model="libreUri"
          type="text"
          placeholder="spotify:playlist:37i9dQZF1DXcBWIGoYBM5M"
          class="w-full bg-slate-800 border border-slate-700 rounded-lg p-2.5 text-sm focus:outline-none focus:border-indigo-500"
        />
      </div>
      <div class="flex space-x-3">
        <button
          @click="playContext"
          class="flex-1 bg-indigo-600 hover:bg-indigo-500 text-sm font-semibold py-2 rounded-lg transition"
        >
          Play Context
        </button>
        <button
          @click="setShuffle(true)"
          class="bg-slate-800 hover:bg-slate-700 text-xs px-4 py-2 rounded-lg border border-slate-700"
        >
          Shuffle On
        </button>
        <button
          @click="setShuffle(false)"
          class="bg-slate-800 hover:bg-slate-700 text-xs px-4 py-2 rounded-lg border border-slate-700"
        >
          Shuffle Off
        </button>
      </div>
    </div>
  </div>
</template>
