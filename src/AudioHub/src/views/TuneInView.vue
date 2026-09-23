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

const { data: outlines, loading, execute } = api.createApiState<MediaMetaData[]>()

const hasResults = computed(() => {
  if (!outlines.value) return false
  return outlines.value.length > 0
})

async function viewItem(item: MediaMetaData) {
  if (!item.uri) return

  if (item.uri.source == 'tunein' && item.uri.type != 'station') {
    router.push(`/tunein?id=${encodeURIComponent(item.uri.id)}`)
  }
}

async function playItem(item: MediaMetaData) {
  await store.playUri(item.rawUri)
}

async function loadOutline(id: string) {
  const idToLoad = id ?? 'r0'
  await execute(() => api.library.getLibrary('tunein', { id: idToLoad }))
}

watch(
  () => route.query.id,
  (newId) => {
    const idToLoad = typeof newId === 'string' ? newId : 'r0'
    loadOutline(idToLoad)
  },
  { immediate: true }
)

async function refresh() {
  if (typeof route.query.id === 'string') loadOutline(route.query.id as string)
  else loadOutline('r0')
}

onMounted(() => {
  if (typeof route.query.id === 'string') loadOutline(route.query.id as string)
  else loadOutline('r0')
})
</script>

<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
    <!-- Section Header -->
    <h2 class="text-xl sm:text-2xl font-bold mb-4 text-slate-100 tracking-tight">TuneIn Radio</h2>

    <!-- Loading State -->
    <div v-if="loading" class="text-slate-400 animate-pulse py-8 text-center sm:text-left">
      Searching media providers...
    </div>

    <!-- Results Outline Grid -->
    <div v-else-if="hasResults" class="space-y-8">
      <div class="space-y-3">
        <!-- Progressive grid layout scaling from 1 to 6 columns -->
        <MediaCardGrid>
          <template v-if="outlines" v-for="(item, index) in outlines" :key="item.rawUri">
            <MediaCard v-model:item="outlines[index]" @view="viewItem" @play="playItem" />
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
