<script setup lang="ts">
import { watch, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '@/types/api'

const router = useRouter()
const route = useRoute()
const api = useApiClient()
const store = usePlaybackStore()

const { data: items, loading, execute } = api.createApiState<MediaMetaData[]>()

const hasResults = computed(() => {
  if (!items.value) return false
  return items.value.length > 0
})

async function viewItem(item: MediaMetaData) {
  if (!item.uri) return

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

async function loadLibrary() {
  await execute(() => api.library.getLibrary('spotify', {}))
}

async function refresh() {
  loadLibrary()
}

onMounted(() => {
  loadLibrary()
})
</script>

<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
    <!-- Section Header -->
    <h2 class="text-xl sm:text-2xl font-bold mb-4 text-slate-100 tracking-tight">
      Spotify Library
    </h2>

    <!-- Loading State -->
    <div v-if="loading" class="text-slate-400 animate-pulse py-8 text-center sm:text-left">
      Searching media providers...
    </div>

    <!-- Library Grid -->
    <div v-else-if="hasResults" class="space-y-8">
      <div class="space-y-3">
        <!-- Multi-tier responsive grid: 1 col (xs) -> 2 cols (sm) -> 3 cols (md) -> 4 cols (lg) -> 6 cols (xl) -->
        <MediaCardGrid>
          <template v-if="items" v-for="(item, index) in items" :key="item.rawUri">
            <MediaCard v-model:item="items[index]" @view="viewItem" @play="playItem" />
          </template>
        </MediaCardGrid>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-slate-400 py-8 text-center sm:text-left">
      No media found matching query.
    </div>
  </div>
</template>
