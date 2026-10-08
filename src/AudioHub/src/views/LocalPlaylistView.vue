<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData, MediaMetaDataPlaylist } from '@/types'
import { useRoute, useRouter } from 'vue-router'
import MediaCard from '@/components/MediaCard.vue'
import MediaCardGrid from '@/components/MediaCardGrid.vue'
import MediaTrackList from '@/components/MediaTrackList.vue'
import { Play, Grid3x3, Rows3 } from '@lucide/vue'
import { playAllTracks } from '@/utilities'

const STORAGE_KEY = 'library_view_mode'

const router = useRouter()
const route = useRoute()
const api = useApiClient()
const store = usePlaybackStore()

const selectedPlaylistId = ref<string>('')
const playlist = ref<MediaMetaDataPlaylist>()
const playlists = ref<Record<string, string>>()
const viewMode = ref<'grid' | 'list'>('grid')
const loading = ref<boolean>(false)

onMounted(() => {
  const savedMode = localStorage.getItem(STORAGE_KEY)
  if (savedMode === 'grid' || savedMode === 'list') {
    viewMode.value = savedMode
  }
})

function toggleViewMode(mode: 'grid' | 'list') {
  viewMode.value = mode
  localStorage.setItem(STORAGE_KEY, mode)
}

async function playAll() {
  if (!playlist.value || !playlist.value.items) {
    return
  }

  await playAllTracks(playlist.value.items)
}

async function loadPlaylistById(id: string) {
  if (!id) return
  loading.value = true
  try {
    const result = await api.playlists.getById(id)
    if (result) {
      playlist.value = result
    }
  } finally {
    loading.value = false
  }
}

async function fetchPlaylists() {
  loading.value = true
  try {
    const res = await api.playlists.getAll()
    if (res && Object.keys(res).length > 0) {
      playlists.value = res

      // Prefer route param/query ID, fallback to first available playlist key
      const routeId = (route.params.id as string) || (route.query.id as string)
      const targetId = routeId && res[routeId] ? routeId : Object.keys(res)[0]

      if (targetId) {
        selectedPlaylistId.value = targetId
        await loadPlaylistById(targetId)
      }
    }
  } finally {
    loading.value = false
  }
}

function handlePlaylistSelect(event: Event) {
  const target = event.target as HTMLSelectElement
  const id = target.value
  selectedPlaylistId.value = id
  loadPlaylistById(id)
}

async function playItem(station: MediaMetaData) {
  if (station.rawUri) {
    await store.playUri(station.rawUri)
  }
}

// Re-fetch playlist if navigating between route params on the same component instance
watch(
  () => route.params.id,
  async (newId) => {
    if (newId && typeof newId === 'string') {
      selectedPlaylistId.value = newId
      await loadPlaylistById(newId)
    }
  }
)

onMounted(() => {
  fetchPlaylists()
})
</script>

<template>
  <div class="w-full min-h-full flex flex-col justify-start overscroll-y-contain touch-pan-y">
    <div class="max-w-7xl mx-auto w-full px-4 sm:px-6 lg:px-8 py-6">
      <!-- Header with Title and View Switcher Button -->
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-xl sm:text-2xl font-bold text-slate-100 tracking-tight">Playlists</h2>

        <ViewSelector v-model="viewMode" v-if="!loading" />
      </div>

      <div
        class="mb-2 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 bg-slate-800/40 border border-slate-800 p-4 rounded-xl"
      >
        <label for="playlist-select" class="text-sm font-medium text-slate-300">
          Select Playlist
        </label>
        <div class="relative w-full sm:w-72">
          <select
            @change="handlePlaylistSelect"
            id="playlist-select"
            v-model="selectedPlaylistId"
            class="w-full bg-slate-900 border border-slate-700 text-slate-100 text-sm rounded-lg focus:ring-indigo-500 focus:border-indigo-500 block p-2.5 transition appearance-none cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <option v-for="(value, key) in playlists" :key="key" :value="key">
              {{ value }}
            </option>
          </select>
          <div
            class="pointer-events-none absolute inset-y-0 right-0 flex items-center px-2 text-slate-400"
          >
            <svg class="w-4 h-4 fill-current" viewBox="0 0 20 20">
              <path
                d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z"
              />
            </svg>
          </div>
        </div>
        <div>
          <button
            @click="playAll()"
            class="inline-flex items-center gap-2 px-5 py-2.5 rounded-full bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-sm shadow-lg hover:scale-105 active:scale-95 transition shrink-0"
            title="Play Album"
          >
            <Play class="w-5 h-5" />
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="text-slate-400 animate-pulse py-8 text-center sm:text-left">
        Loading playlists...
      </div>

      <!-- Content Display -->
      <div v-else-if="playlist && playlist.items" class="space-y-8">
        <div class="space-y-3">
          <!-- Grid View -->
          <MediaCardGrid v-if="viewMode === 'grid'">
            <MediaCard
              v-for="(s, index) in playlist.items"
              :key="s.rawUri || index"
              v-model:item="playlist.items[index]"
              @play="playItem"
            />
          </MediaCardGrid>

          <!-- List View -->
          <MediaTrackList
            :showFavorite="true"
            :showPlaylist="false"
            v-else-if="viewMode === 'list'"
            v-model="playlist.items"
            @play="playItem"
          />
        </div>
      </div>
    </div>
  </div>
</template>
