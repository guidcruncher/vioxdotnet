<script setup lang="ts">
import { watch, ref, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '@/types'
import { useRoute, useRouter } from 'vue-router'
import MediaCard from '@/components/MediaCard.vue'
import CountryCard from '@/components/CountryCard.vue'
import MediaCardGrid from '@/components/MediaCardGrid.vue'
import MediaTrackList from '@/components/MediaTrackList.vue'

const STORAGE_KEY = 'radio_view_mode'

const router = useRouter()
const route = useRoute()
const api = useApiClient()
const store = usePlaybackStore()

const addToPlaylist = ref<boolean>(false)
const playlistItem = ref<MediaMetaData>()
const countries = ref<Record<string, string> | null>(null)
const stations = ref<MediaMetaData[]>([])
const viewMode = ref<'grid' | 'list'>('grid')

const { loading, execute } = api.createApiState<MediaMetaData[]>()

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

async function ensureCountriesLoaded() {
  stations.value = [] // Clear previous station results when returning to country view
  if (!countries.value) {
    countries.value = await api.core.getCountries()
  }
}

async function loadStationsForCountry(countryCode: string) {
  const result = await execute(() => api.radioBrowser.getByCountry(countryCode))
  if (result) {
    stations.value = result
  }
}

async function playItem(station: MediaMetaData) {
  if (station.rawUri) {
    await store.playUri(station.rawUri)
  }
}

async function addToPlaylistFunc(data: any) {
  playlistItem.value = data.item
  addToPlaylist.value = true
}

function viewItem(countryCode: string) {
  if (!countryCode) {
    router.push('/radio')
    return
  }
  router.push({ path: '/radio', query: { id: countryCode } })
}

// React to route query changes
watch(
  () => route.query.id,
  async (newId) => {
    if (typeof newId === 'string' && newId.trim() !== '') {
      await loadStationsForCountry(newId)
    } else {
      await ensureCountriesLoaded()
    }
  },
  { immediate: true }
)
</script>

<template>
  <div class="w-full min-h-full flex flex-col justify-start overscroll-y-contain touch-pan-y">
    <div class="max-w-7xl mx-auto w-full px-4 sm:px-6 lg:px-8 py-6">
      <!-- Header with Title and View Switcher Button -->
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-xl sm:text-2xl font-bold text-slate-100 tracking-tight">Radio Stations</h2>

        <!-- View Mode Toggle Button (Shown when viewing stations) -->
        <div
          v-if="route.query.id && !loading && stations.length > 0"
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

      <!-- Content Display -->
      <div v-else class="space-y-8">
        <div class="space-y-3">
          <!-- Station View Switcher -->
          <template v-if="route.query.id">
            <!-- Grid View -->
            <MediaCardGrid v-if="viewMode === 'grid'">
              <MediaCard
                v-for="(s, index) in stations"
                :key="s.rawUri || index"
                v-model:item="stations[index]"
                @play="playItem"
                @playlist="addToPlaylistFunc"
              />
            </MediaCardGrid>

            <!-- List View -->
            <MediaTrackList v-else-if="viewMode === 'list'" v-model="stations" @play="playItem" />
          </template>

          <!-- Country List View (No 'id' query parameter) -->
          <template v-else-if="countries">
            <MediaCardGrid>
              <CountryCard
                v-for="[key, value] in Object.entries(countries)"
                :key="key"
                :item="[key, value]"
                @view="viewItem"
              />
            </MediaCardGrid>
          </template>
        </div>
      </div>
    </div>
  </div>
  <AddToPlaylistModal v-model:IsOpen="addToPlaylist" :item="playlistItem" />
</template>
