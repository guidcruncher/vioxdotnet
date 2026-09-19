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

const { data: outlines, loading, execute } = api.createApiState<MediaMetaData[]>()

const hasResults = computed(() => {
  if (!outlines.value) return false
  return outlines.value.length > 0
})

async function viewLink(uri: string) {
  // Navigation or detail view logic for podcasts
  router.push(`/tunein?id=${encodeURIComponent(uri)}`)
}

async function loadOutline(id: string) {
  const idToLoad = id ?? 'r0'
  await execute(() => api.library.getLibrary('tunein', { id: idToLoad }))
}

watch(
  () => route.query.id,
  (newId) => {
    if (typeof newId === 'string') loadOutline(newId)
  }
)

onMounted(() => {
  if (typeof route.query.id === 'string') loadOutline(route.query.id as string)
  else loadOutline('r0')
})
</script>

<template>
  <div>
    <h2 class="text-xl font-bold mb-4"></h2>
    <div v-if="loading" class="text-slate-400 animate-pulse">Searching media providers...</div>

    <div v-else-if="hasResults" class="space-y-8">
      <div class="space-y-3">
        <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div
            v-for="item in outlines"
            :key="item.rawUri"
            class="bg-slate-900 p-3 rounded-lg border border-slate-800 flex flex-col justify-between"
          >
            <div>
              <img
                :src="item.imageUrl || '/tuneinregion.png'"
                class="w-full h-32 object-cover rounded mb-2"
              />
              <p class="font-bold text-sm truncate">{{ item.title }}</p>
              <p class="text-xs text-slate-400 truncate">{{ item.artist }}</p>
            </div>
            <button
              v-if="item.uri?.type === 'link' && item.rawUri"
              @click="viewLink(item.uri.id)"
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
    </div>

    <div v-else class="text-slate-400">No media found matching query.</div>
  </div>
</template>
