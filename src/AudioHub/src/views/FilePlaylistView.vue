<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '@/types'

const api = useApiClient()
const playbackStore = usePlaybackStore()

const playlistsMap = ref<Record<string, MediaMetaData[]>>({})
const selectedPlaylistName = ref<string>('')
const isLoading = ref<boolean>(true)
const errorMessage = ref<string | null>(null)
const addToPlaylist = ref<boolean>(false)
const playlistItem = ref<MediaMetaData>()

// Computed list of available playlist names for the dropdown
const playlistNames = computed<string[]>(() => Object.keys(playlistsMap.value))

// Writable computed property to support two-way v-model binding with MediaTrackList
const tracks = computed<MediaMetaData[]>({
  get: () => {
    if (!selectedPlaylistName.value || !playlistsMap.value[selectedPlaylistName.value]) {
      return []
    }
    return playlistsMap.value[selectedPlaylistName.value]
  },
  set: (updatedTracks) => {
    if (selectedPlaylistName.value) {
      playlistsMap.value[selectedPlaylistName.value] = updatedTracks
    }
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

async function fetchPlaylists() {
  isLoading.value = true
  errorMessage.value = null
  try {
    const data = await api.filePlaylists.getPlaylists()
    playlistsMap.value = data || {}
    const names = Object.keys(playlistsMap.value)
    if (names.length > 0) {
      selectedPlaylistName.value = names[0]
    }
  } catch (error) {
    console.error('Failed to load playlists:', error)
    errorMessage.value = 'Failed to load playlist information. Please try again.'
  } finally {
    isLoading.value = false
  }
}

async function handlePlayTrack(track: MediaMetaData) {
  if (track.rawUri) {
    await playbackStore.playUri(track.rawUri)
  }
}

function handleViewTrack(track: MediaMetaData) {
  console.log('Viewing track details:', track)
  // Handle detail viewing or navigation logic
}

onMounted(() => {
  fetchPlaylists()
})
</script>

<template>
  <div class="min-h-screen text-slate-100">
    <!-- Responsive outer padding wrapper -->
    <div class="p-4 sm:p-6 lg:p-8 max-w-7xl mx-auto space-y-6 sm:space-y-8">
      <!-- Playlist Switcher Dropdown Header -->
      <div
        class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 bg-slate-800/40 border border-slate-800 p-4 rounded-xl"
      >
        <label for="playlist-select" class="text-sm font-medium text-slate-300">
          Select Playlist
        </label>
        <div class="relative w-full sm:w-72">
          <select
            id="playlist-select"
            v-model="selectedPlaylistName"
            :disabled="isLoading || playlistNames.length === 0"
            class="w-full bg-slate-900 border border-slate-700 text-slate-100 text-sm rounded-lg focus:ring-indigo-500 focus:border-indigo-500 block p-2.5 transition appearance-none cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <option v-if="playlistNames.length === 0" value="" disabled>
              No playlists available
            </option>
            <option v-for="name in playlistNames" :key="name" :value="name">
              {{ name }} ({{ playlistsMap[name].length }})
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
      <div v-if="isLoading" class="flex items-center justify-center py-20">
        <div class="flex items-center space-x-3 text-slate-400">
          <svg class="animate-spin h-6 w-6 text-indigo-500" viewBox="0 0 24 24" fill="none">
            <circle
              class="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              stroke-width="4"
            ></circle>
            <path
              class="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            ></path>
          </svg>
          <span class="text-sm font-medium">Loading Playlists...</span>
        </div>
      </div>

      <!-- Error State -->
      <div
        v-else-if="errorMessage"
        class="bg-red-500/10 border border-red-500/30 rounded-xl p-6 text-red-400 text-center"
      >
        <p class="text-sm font-medium">{{ errorMessage }}</p>
      </div>

      <!-- Reusable MediaTrackList Component -->
      <MediaTrackList
        v-else
        v-model="tracks"
        @play="handlePlayTrack"
        @view="handleViewTrack"
        @playlist="addToPlaylistFunc"
      />
    </div>
  </div>
  <AddToPlaylistModal v-model:isOpen="addToPlaylist" :item="playlistItem" />
</template>
