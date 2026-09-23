<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '../types/api'

const route = useRoute()
const api = useApiClient()
const playbackStore = usePlaybackStore()

const album = ref<MediaMetaData | null>(null)
const tracks = ref<MediaMetaData[]>([])
const isLoading = ref<boolean>(true)
const errorMessage = ref<string | null>(null)

async function loadData(id: string) {
  isLoading.value = true
  errorMessage.value = null

  try {
    const [albumRes, tracksRes] = await Promise.all([
      api.spotify.getAlbum(id),
      api.spotify.getAlbumTracks(id),
    ])
    album.value = albumRes
    tracks.value = tracksRes
  } catch (error) {
    console.error('Failed to load Spotify data:', error)
    errorMessage.value = 'Failed to load album information. Please try again.'
  } finally {
    isLoading.value = false
  }
}

// Improved duration formatter: returns MM:SS or HH:MM:SS conditionally
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

async function playTrack(track: MediaMetaData) {
  if (track.rawUri) {
    await playbackStore.playUri(track.rawUri)
  }
}

async function playAll() {
  const albumId = route.params.id as string
  if (albumId) {
    await playbackStore.playUri(`spotify:album:${albumId}`)
  }
}

async function refresh() {
  const albumId = route.params.id as string
  if (albumId) {
    loadData(albumId)
  }
}

onMounted(() => {
  const albumId = route.params.id as string
  if (albumId) {
    loadData(albumId)
  }
})

watch(
  () => route.params.id,
  (newId) => {
    if (newId && typeof newId === 'string') {
      loadData(newId)
    }
  }
)
</script>

<template>
  <div class="min-h-screen text-slate-100">
    <div class="p-4 sm:p-6 lg:p-8 max-w-7xl mx-auto space-y-6 sm:space-y-8">
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
          <span class="text-sm font-medium">Loading Album...</span>
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
        <!-- Album Header Section -->
        <section
          v-if="album"
          class="flex flex-col sm:flex-row items-center sm:items-end gap-6 text-center sm:text-left"
        >
          <img
            :src="album.imageUrl || '/vinyl.png'"
            :alt="album.title || 'Album Cover'"
            class="w-40 h-40 sm:w-48 sm:h-48 md:w-56 md:h-56 rounded-2xl bg-slate-800 object-cover shadow-2xl shrink-0"
          />
          <div class="space-y-3 flex-1 min-w-0">
            <span
              class="inline-block px-3 py-1 text-xs rounded-full bg-indigo-500/10 text-indigo-400 border border-indigo-500/20 font-semibold uppercase tracking-wider"
            >
              Spotify Album
            </span>
            <h1
              class="text-2xl sm:text-3xl md:text-4xl font-extrabold tracking-tight text-white leading-tight break-words"
            >
              {{ album.title || 'Untitled Album' }}
            </h1>
            <p class="text-slate-400 text-sm sm:text-base font-medium truncate">
              {{ album.artist || 'Unknown Artist' }}
            </p>
            <div class="pt-2 flex justify-center sm:justify-start">
              <button
                @click="playAll()"
                class="inline-flex items-center gap-2 px-5 py-2.5 rounded-full bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-sm shadow-lg hover:scale-105 active:scale-95 transition shrink-0"
                title="Play All Tracks"
              >
                <svg class="w-5 h-5 fill-current" viewBox="0 0 24 24">
                  <path d="M8 5v14l11-7z" />
                </svg>
                <span>Play Album</span>
              </button>
            </div>
          </div>
        </section>

        <!-- Track List Section -->
        <section class="space-y-4">
          <div class="flex items-center justify-between border-b border-slate-800 pb-3">
            <h2 class="text-lg sm:text-xl font-semibold text-slate-200">Tracks</h2>
            <span class="text-xs sm:text-sm text-slate-400 font-medium">
              {{ tracks.length }} {{ tracks.length === 1 ? 'song' : 'songs' }}
            </span>
          </div>

          <div v-if="tracks.length === 0" class="text-slate-500 py-12 text-center text-sm">
            No tracks found for this album.
          </div>

          <div v-else class="space-y-2">
            <div
              v-for="(track, index) in tracks"
              :key="track.rawUri || index"
              class="flex items-center justify-between p-3 sm:p-4 rounded-xl bg-slate-800/40 hover:bg-slate-800/80 border border-slate-800/60 transition group gap-4"
            >
              <!-- Play Button & Track Metadata -->
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
