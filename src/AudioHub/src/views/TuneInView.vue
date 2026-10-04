<script setup lang="ts">
import { watch, onMounted, computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '@/types'
import MediaCardGrid from '@/components/MediaCardGrid.vue'
import MediaCard from '@/components/MediaCard.vue'
import MediaTrackList from '@/components/MediaTrackList.vue'

const STORAGE_KEY = 'tunein_view_mode'

const router = useRouter()
const route = useRoute()
const api = useApiClient()
const store = usePlaybackStore()

const addToPlaylist = ref<boolean>(false)
const playlistItem = ref<MediaMetaData>()
const viewMode = ref<'grid' | 'list'>('grid')

const { data: outlines, loading, execute } = api.createApiState<MediaMetaData[]>()

const hasResults = computed(() => {
  if (!outlines.value) return false
  return outlines.value.length > 0
})

// Writable computed property to support two-way v-model binding with MediaTrackList
const outlineList = computed<MediaMetaData[]>({
  get: () => outlines.value || [],
  set: (val) => {
    outlines.value = val
  },
})

async function addToPlaylistFunc(data: any) {
  playlistItem.value = data.item
  addToPlaylist.value = true
}

function toggleViewMode(mode: 'grid' | 'list') {
  viewMode.value = mode
  localStorage.setItem(STORAGE_KEY, mode)
}

async function viewItem(item: MediaMetaData) {
  if (!item.uri) return
  if (item.uri.source === 'tunein' && item.uri.type !== 'station') {
    router.push(`/tunein?id=${encodeURIComponent(item.uri.id)}`)
  }
}

async function playItem(item: MediaMetaData) {
  if (item.rawUri) {
    await store.playUri(item.rawUri)
  }
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
  const savedMode = localStorage.getItem(STORAGE_KEY)
  if (savedMode === 'grid' || savedMode === 'list') {
    viewMode.value = savedMode
  }
  if (typeof route.query.id === 'string') loadOutline(route.query.id as string)
  else loadOutline('r0')
})
</script>

<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
    <!-- Section Header with View Mode Controls -->
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-xl sm:text-2xl font-bold text-slate-100 tracking-tight">TuneIn Radio</h2>

      <!-- View Switcher Button Group -->
      <div
        v-if="!loading && hasResults"
        class="flex items-center bg-slate-800/80 p-1 rounded-lg border border-slate-700/60"
      >
        <button
          @click="toggleViewMode('grid')"
          :class="[
            'p-1.5 rounded-md transition text-slate-400 hover:text-slate-100',
            viewMode === 'grid' ? 'bg-slate-700 text-indigo-400 font-semibold shadow-sm' : '',
          ]"
          title="Grid View"
          aria-label="Switch to grid view"
        >
          <svg class="w-4 h-4 fill-current" viewBox="0 0 24 24">
            <path
              d="M4 4h4v4H4V4zm6 0h4v4h-4V4zm6 0h4v4h-4V4zM4 10h4v4H4v-4zm6 0h4v4h-4v-4zm6 0h4v4h-4v-4zM4 16h4v4H4v-4zm6 0h4v4h-4v-4zm6 0h4v4h-4v-4z"
            />
          </svg>
        </button>
        <button
          @click="toggleViewMode('list')"
          :class="[
            'p-1.5 rounded-md transition text-slate-400 hover:text-slate-100',
            viewMode === 'list' ? 'bg-slate-700 text-indigo-400 font-semibold shadow-sm' : '',
          ]"
          title="List View"
          aria-label="Switch to list view"
        >
          <svg class="w-4 h-4 fill-current" viewBox="0 0 24 24">
            <path d="M4 6h16v2H4V6zm0 5h16v2H4v-2zm0 5h16v2H4v-2z" />
          </svg>
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="text-slate-400 animate-pulse py-8 text-center sm:text-left">
      Searching media providers...
    </div>

    <!-- Content Views -->
    <div v-else-if="hasResults" class="space-y-8">
      <div class="space-y-3">
        <!-- Icon Grid View -->
        <MediaCardGrid v-if="viewMode === 'grid'">
          <template v-if="outlines" v-for="(item, index) in outlines" :key="item.rawUri || index">
            <MediaCard
              v-model:item="outlines[index]"
              @view="viewItem"
              @play="playItem"
              @playlist="addToPlaylistFunc"
            />
          </template>
        </MediaCardGrid>

        <!-- List View -->
        <MediaTrackList
          v-else-if="viewMode === 'list'"
          v-model="outlineList"
          @play="playItem"
          @view="viewItem"
        />
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-slate-400 py-8 text-center sm:text-left">
      No media found matching query.
    </div>
  </div>
  <AddToPlaylistModal v-model:IsOpen="addToPlaylist" :item="playlistItem" />
</template>
