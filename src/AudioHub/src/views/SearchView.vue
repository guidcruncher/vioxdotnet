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

async function viewItem(item: MediaMetaData) {
  if (!item.uri) return

  if (item.uri.source == 'podverse' && item.uri.type == 'podcast') {
    router.push(`/podverse/podcast/${encodeURIComponent(item.rawUri)}`)
  }

  if (item.uri.source == 'spotify') {
    switch (item.uri.type) {
      case 'show':
        router.push(`/spotify/show/${encodeURIComponent(item.uri.id)}`)
        break
      case 'album':
        router.push(`/spotify/album/${encodeURIComponent(item.uri.id)}`)
        break
      case 'playlist':
        router.push(`/spotify/playlist/${encodeURIComponent(item.uri.id)}`)
        break
    }
  }
}

async function playItem(item: MediaMetaData) {
  await store.playUri(item.rawUri)
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

async function refresh() {
  if (typeof route.query.q === 'string') performSearch(route.query.q)
}

onMounted(() => {
  if (typeof route.query.q === 'string') performSearch(route.query.q)
})
</script>

<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
    <!-- Search Query Header -->
    <h2 class="text-xl sm:text-2xl font-bold mb-4 text-slate-100 tracking-tight">
      Search Results for "{{ route.query.q }}"
    </h2>

    <!-- Loading Skeleton Placeholder -->
    <div v-if="loading" class="text-slate-400 animate-pulse py-8 text-center sm:text-left">
      Searching media providers...
    </div>

    <!-- Results Section -->
    <div v-else-if="hasResults" class="space-y-8">
      <template v-for="(sourceResults, source) in searchResults" :key="source">
        <div v-if="sourceResults?.items && sourceResults.items.length" class="space-y-3">
          <h3 class="text-lg font-semibold text-indigo-400 capitalize">
            {{ source }}
          </h3>

          <!-- Responsive Grid: 1 col (xs), 2 cols (sm), 3 cols (md), 4 cols (lg), 6 cols (xl) -->
          <MediaCardGrid>
            <template v-for="(item, index) in sourceResults.items" :key="item.rawUri">
              <MediaCard
                v-model:item="sourceResults.items[index]"
                @view="viewItem"
                @play="playItem"
              />
            </template>
          </MediaCardGrid>
        </div>
      </template>
    </div>

    <!-- Empty State -->
    <div v-else class="text-slate-400 py-8 text-center sm:text-left">
      No media found matching query.
    </div>
  </div>
</template>
