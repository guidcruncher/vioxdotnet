<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '../types/api'

const api = useApiClient()
const playbackStore = usePlaybackStore()

const playlistsMap = ref<Record<string, MediaMetaData[]>>({})
const selectedPlaylistName = ref<string>('')
const isLoading = ref<boolean>(true)
const errorMessage = ref<string | null>(null)

// Computed list of available playlist names for the dropdown
const playlistNames = computed<string[]>(() => Object.keys(playlistsMap.value))

// Computed track list for the currently selected playlist
const tracks = computed<MediaMetaData[]>(() => {
  if (!selectedPlaylistName.value || !playlistsMap.value[selectedPlaylistName.value]) {
    return []
  }
  return playlistsMap.value[selectedPlaylistName.value]
})

// Active playlist metadata derived from the first track in the selected playlist
const playlistInfo = computed<Partial<MediaMetaData> | null>(() => {
  if (!selectedPlaylistName.value) return null
  const firstTrack = tracks.value[0]
  return {
    title: selectedPlaylistName.value,
    artist: firstTrack?.artist || 'Various Artists',
    imageUrl: firstTrack?.imageUrl || '/hifi.png',
  }
})

async function fetchPlaylists() {
  isLoading.value = true
  errorMessage.value = null
  try {
    const data = await api.playlist.getPlaylists()
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

// Formats duration dynamically (MM:SS or HH:MM:SS)
function convertSeconds(totalSeconds?: number): string {
  if (!totalSeconds || totalSeconds <= 0) {
    return '0:00'
  }
  const hours = Math.floor(totalSeconds / 3600)
  const minutes = Math.floor((totalSeconds % 3600) / 60)
  const seconds = Math.floor(totalSeconds % 60)
  const pad = (num: number): string => num.toString().padStart(2, '0')

  if (hours > 0) {
    return `${hours}:${pad(minutes)}:${pad(seconds)}`
  }
  return `${minutes}:${pad(seconds)}`
}

async function playAll() {
  const currentTrackList = tracks.value
  if (currentTrackList.length > 0 && currentTrackList[0].rawUri) {
    await playbackStore.playUri(currentTrackList[0].rawUri)
  }
}

async function playTrack(track: MediaMetaData) {
  if (track.rawUri) {
    await playbackStore.playUri(track.rawUri)
  }
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

      <template v-else>
        <!-- Track List -->
        <section class="space-y-4">
          <div class="flex items-center justify-between border-b border-slate-800 pb-3">
            <h2 class="text-lg sm:text-xl font-semibold text-slate-200">Tracks</h2>
            <span class="text-xs sm:text-sm text-slate-400 font-medium">
              {{ tracks.length }} {{ tracks.length === 1 ? 'item' : 'items' }}
            </span>
          </div>

          <div v-if="tracks.length === 0" class="text-slate-500 py-12 text-center text-sm">
            No tracks found for this playlist.
          </div>

          <div v-else class="space-y-2">
            <div
              v-for="(track, index) in tracks"
              :key="track.rawUri || index"
              class="flex items-center justify-between p-3 sm:p-4 rounded-xl bg-slate-800/40 hover:bg-slate-800/80 border border-slate-800/60 transition group gap-4"
            >
              <!-- Track Index, Play Button, Artwork & Metadata -->
              <div class="flex items-center space-x-3 sm:space-x-4 min-w-0 flex-1">
                <span
                  class="text-xs font-mono text-slate-500 w-5 text-right hidden sm:inline-block shrink-0"
                >
                  {{ index + 1 }}
                </span>

                <button
                  @click="playTrack(track)"
                  class="w-9 h-9 sm:w-10 sm:h-10 rounded-full bg-indigo-600/90 hover:bg-indigo-500 text-white flex items-center justify-center shadow hover:scale-105 transition shrink-0"
                  title="Play Track"
                >
                  <svg
                    class="w-4 h-4 sm:w-5 sm:h-5 fill-current translate-x-0.5"
                    viewBox="0 0 24 24"
                  >
                    <path d="M8 5v14l11-7z" />
                  </svg>
                </button>

                <!-- Track Image Thumbnail -->
                <div
                  class="relative w-10 h-10 sm:w-12 sm:h-12 rounded-lg overflow-hidden bg-slate-900 border border-slate-700/50 shrink-0"
                >
                  <img
                    v-if="track.imageUrl"
                    :src="track.imageUrl"
                    onerror="
                      this.onerror = null
                      this.src = '/casette.png'
                    "
                    :alt="track.title || 'Track thumbnail'"
                    class="w-full h-full object-cover"
                  />
                  <div v-else class="w-full h-full flex items-center justify-center text-slate-600">
                    <svg class="w-5 h-5 fill-current" viewBox="0 0 24 24">
                      <path
                        d="M12 3v10.55c-.59-.34-1.27-.55-2-.55-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4V7h4V3h-6z"
                      />
                    </svg>
                  </div>
                </div>

                <div class="min-w-0 flex-1 space-y-0.5">
                  <h3
                    class="font-medium text-xs sm:text-sm text-slate-100 truncate group-hover:text-indigo-300 transition"
                  >
                    {{ track.title || 'Untitled Track' }}
                  </h3>
                  <p v-if="track.artist" class="text-xs text-slate-400 truncate">
                    {{ track.artist }}
                  </p>
                </div>
              </div>

              <!-- Track Duration -->
              <div class="shrink-0 text-xs font-mono text-slate-400 pl-2">
                <span>{{ convertSeconds(track.duration) }}</span>
              </div>
            </div>
          </div>
        </section>
      </template>
    </div>
  </div>
</template>
