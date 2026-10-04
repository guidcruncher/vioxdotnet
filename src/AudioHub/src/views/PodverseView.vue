<script setup lang="ts">
import { watch, onMounted, computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '@/types'
import MediaCardGrid from '@/components/MediaCardGrid.vue'
import MediaCard from '@/components/MediaCard.vue'
import MediaTrackList from '@/components/MediaTrackList.vue'

const STORAGE_KEY = 'podverse_library_view_mode'

const router = useRouter()
const route = useRoute()
const api = useApiClient()
const store = usePlaybackStore()

const addToPlaylist = ref<boolean>(false)
const playlistItem = ref<MediaMetaData>()
const viewMode = ref<'grid' | 'list'>('grid')

const { data: items, loading, execute } = api.createApiState<MediaMetaData[]>()

const hasResults = computed(() => {
  if (!items.value) return false
  return items.value.length > 0
})

// Writable computed property to support two-way v-model binding with MediaTrackList
const itemList = computed<MediaMetaData[]>({
  get: () => items.value || [],
  set: (val) => {
    items.value = val
  },
})

async function addToPlaylistFunc(state: boolean, item: MediaMetaData) {
  if (state) {
    playlistItem.value = item
    addToPlaylist.value = true
  } else {
    await api.playlists.removeItem(item.rawUri)
  }
}

function toggleViewMode(mode: 'grid' | 'list') {
  viewMode.value = mode
  localStorage.setItem(STORAGE_KEY, mode)
}

async function viewItem(item: MediaMetaData) {
  if (!item.uri) return
  if (item.uri.source === 'podverse' && item.uri.type === 'podcast') {
    router.push(`/podverse/podcast/${encodeURIComponent(item.rawUri)}`)
  }
}

async function playItem(item: MediaMetaData) {
  if (item.rawUri) {
    await store.playUri(item.rawUri)
  }
}

async function loadLibrary() {
  await execute(() => api.library.getLibrary('podverse', {}))
}

async function refresh() {
  await loadLibrary()
}

onMounted(() => {
  const savedMode = localStorage.getItem(STORAGE_KEY)
  if (savedMode === 'grid' || savedMode === 'list') {
    viewMode.value = savedMode
  }
  loadLibrary()
})
</script>

<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
    <!-- Section Header with View Mode Controls -->
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-xl sm:text-2xl font-bold text-slate-100 tracking-tight">
        Subscribed Podcasts
      </h2>

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
          <template v-if="items" v-for="(item, index) in items" :key="item.rawUri || index">
            <MediaCard
              v-model:item="items[index]"
              @view="viewItem"
              @play="playItem"
              @playlist="addToPlaylistFunc"
            />
          </template>
        </MediaCardGrid>

        <!-- List View -->
        <MediaTrackList
          v-else-if="viewMode === 'list'"
          v-model="itemList"
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
  <AddToPlaylistModal v-model:isOpen="addToPlaylist" :item="playlistItem" />
</template>
