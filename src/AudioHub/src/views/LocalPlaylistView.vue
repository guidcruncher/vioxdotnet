<script setup lang="ts">
import { computed, watch, ref, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData, MediaMetaDataPlaylist } from '@/types'
import { useRoute, useRouter } from 'vue-router'
import MediaCard from '@/components/MediaCard.vue'
import CountryCard from '@/components/CountryCard.vue'
import MediaCardGrid from '@/components/MediaCardGrid.vue'
import MediaTrackList from '@/components/MediaTrackList.vue'

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

async function fetchPlaylists() {
  const res = await api.playlists.getAll()
  if (res) {
    playlists.value = res
  }
}

async function loadPlaylist(event: Event) {
  const target = event.target as HTMLSelectElement
  const id = target.value
  const result = await api.playlists.getById(id)
  if (result) {
    playlist.value = result
  }
}

async function playItem(station: MediaMetaData) {
  if (station.rawUri) {
    await store.playUri(station.rawUri)
  }
}

async function addToPlaylistFunc(state: boolean, item: MediaMetaData) {}

function viewItem(id: string) {}

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

        <div
          v-if="!loading"
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

      <div
        class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 bg-slate-800/40 border border-slate-800 p-4 rounded-xl"
      >
        <label for="playlist-select" class="text-sm font-medium text-slate-300">
          Select Playlist
        </label>
        <div class="relative w-full sm:w-72">
          <select
            @change="loadPlaylist"
            id="playlist-select"
            v-model="selectedPlaylistId"
            class="w-full bg-slate-900 border border-slate-700 text-slate-100 text-sm rounded-lg focus:ring-indigo-500 focus:border-indigo-500 block p-2.5 transition appearance-none cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <option v-for="(value, key, index) in playlists" :key="key" :value="key">
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
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="text-slate-400 animate-pulse py-8 text-center sm:text-left">
        Searching media providers...
      </div>

      <!-- Content Display -->
      <div v-else class="space-y-8">
        <div class="space-y-3">
          <!-- Station View Switcher -->
          <template v-if="playlist && playlist.items">
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
              v-else-if="viewMode === 'list'"
              v-model="playlist.items"
              @play="playItem"
            />
          </template>
        </div>
      </div>
    </div>
  </div>
</template>
