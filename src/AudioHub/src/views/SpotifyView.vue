<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '../types/api'

const api = useApiClient()
const store = usePlaybackStore()

// Pass the generic response structure so execute() correctly types the return payload
const { loading, execute } = api.createApiState<MediaMetaData[]>()

const tracks = ref<MediaMetaData[]>([])

onMounted(async () => {
  const res = await execute(() => api.library.getLibrary('spotify', {}))

  if (res) {
    tracks.value = res
  }
})
</script>

<template>
  <div>
    <h2 class="text-xl font-bold mb-4">Spotify Library</h2>
    <div v-if="loading" class="text-slate-400 animate-pulse">Loading Spotify library...</div>
    <div
      v-else-if="tracks.length"
      class="bg-slate-900 rounded-lg border border-slate-800 overflow-hidden"
    >
      <table class="w-full text-left text-sm">
        <thead class="bg-slate-800/50 text-slate-400 border-b border-slate-700">
          <tr>
            <th class="p-3">Title</th>
            <th class="p-3">Artist</th>
            <th class="p-3">Actions</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-slate-800">
          <tr v-for="t in tracks" :key="t.rawUri" class="hover:bg-slate-800/30">
            <td class="p-3 font-medium">{{ t.title || 'Unknown' }}</td>
            <td class="p-3 text-slate-400">{{ t.artist || 'Unknown' }}</td>
            <td class="p-3">
              <button
                v-if="t.rawUri"
                @click="store.playUri(t.rawUri)"
                class="bg-slate-800 hover:bg-slate-700 text-indigo-400 px-3 py-1 rounded text-xs border border-slate-700"
              >
                Play
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <div v-else class="text-slate-400">No library available or unauthenticated session.</div>
  </div>
</template>
