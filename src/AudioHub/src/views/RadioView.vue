<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData, PagedList } from '../types/api'

const api = useApiClient()
const store = usePlaybackStore()

// Pass the generic type so execute() correctly infers res as PagedList<MediaMetaData>
const { loading, execute } = api.createApiState<PagedList<MediaMetaData>>()
const stations = ref<MediaMetaData[]>([])

async function playStation(station: MediaMetaData) {
  if (station.rawUri) {
    await store.playUri(station.rawUri)
  }
}

onMounted(async () => {
  const res = await execute(() => api.media.searchSource('radiobrowser', 'LBC UK', 1, 12))
  if (res?.items) {
    stations.value = res.items
  }
})
</script>

<template>
  <div>
    <h2 class="text-xl font-bold mb-4">Top Radio Stations</h2>
    <div v-if="loading" class="text-slate-400 animate-pulse">Loading stations...</div>
    <div v-else-if="stations.length" class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
      <div
        v-for="s in stations"
        :key="s.rawUri || s.title"
        class="bg-slate-900 p-4 rounded-xl border border-slate-800 flex flex-col justify-between"
      >
        <div>
          <img
            :src="s.imageUrl || 'https://via.placeholder.com/80'"
            class="w-12 h-12 rounded bg-slate-800 mb-3 object-cover"
          />
          <h4 class="font-bold text-sm truncate">{{ s.title }}</h4>
          <p class="text-xs text-slate-400 truncate">{{ s.album || 'Internet Radio' }}</p>
        </div>
        <button
          @click="playStation(s)"
          class="mt-4 w-full bg-slate-800 hover:bg-indigo-600 text-xs py-2 rounded-lg transition border border-slate-700"
        >
          Tune Station
        </button>
      </div>
    </div>
    <div v-else class="text-slate-400">No radio stations available.</div>
  </div>
</template>
