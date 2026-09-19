<script setup lang="ts">
import { watch, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { PagedList, MediaMetaData } from '@/types/api'

const router = useRouter()
const route = useRoute()
const api = useApiClient()
const store = usePlaybackStore()

// State typed as a dictionary where keys are sources and values are paged results
const {
  data: searchResults,
  loading,
  execute,
} = api.createApiState<Record<string, PagedList<MediaMetaData>>>()

const hasResults = computed(() => {
  if (!searchResults.value) return false
  return Object.values(searchResults.value).some((list) => list?.items && list.items.length > 0)
})

async function viewPodcast(uri: string) {
  // Navigation or detail view logic for podcasts
  router.push(`/podverse/podcast/${encodeURIComponent(uri)}`)
}

async function performSearch(query: string) {
  if (!query) return
  await execute(() => api.media.searchAll(query))
}

watch(
  () => route.query.q,
  (newQuery) => {
    if (typeof newQuery === 'string') performSearch(newQuery)
  }
)

onMounted(() => {
  if (typeof route.query.q === 'string') performSearch(route.query.q)
})
</script>

<template>
  <div>
    <h2 class="text-xl font-bold mb-4">Search Results for "{{ route.query.q }}"</h2>
    <div v-if="loading" class="text-slate-400 animate-pulse">Searching media providers...</div>

    <div v-else-if="hasResults" class="space-y-8">
      <template v-for="(sourceResults, source) in searchResults" :key="source">
        <div v-if="sourceResults?.items && sourceResults.items.length" class="space-y-3">
          <h3 class="text-lg font-semibold text-indigo-400 capitalize">
            {{ source }}
          </h3>
          <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div
              v-for="item in sourceResults.items"
              :key="item.rawUri || item.title"
              class="bg-slate-900 p-3 rounded-lg border border-slate-800 flex flex-col justify-between"
            >
              <div>
                <img
                  :src="item.imageUrl || 'https://via.placeholder.com/150'"
                  class="w-full h-32 object-cover rounded mb-2"
                />
                <p class="font-bold text-sm truncate">{{ item.title }}</p>
                <p class="text-xs text-slate-400 truncate">{{ item.artist }}</p>
              </div>
              <button
                v-if="item.uri?.source === 'podverse' && item.rawUri"
                @click="viewPodcast(item.rawUri)"
                class="mt-2 w-full bg-indigo-600 text-xs py-1.5 rounded font-medium hover:bg-indigo-500 transition"
              >
                View
              </button>
              <button
                v-else-if="item.rawUri"
                @click="store.playUri(item.rawUri)"
                class="mt-2 w-full bg-indigo-600 text-xs py-1.5 rounded font-medium hover:bg-indigo-500 transition"
              >
                Play Now
              </button>
            </div>
          </div>
        </div>
      </template>
    </div>

    <div v-else class="text-slate-400">No media found matching query.</div>
  </div>
</template>
