<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '../types/api'

const route = useRoute()
const api = useApiClient()
const playbackStore = usePlaybackStore()

const podcast = ref<MediaMetaData | null>(null)
const episodes = ref<MediaMetaData[]>([])
const isLoading = ref<boolean>(true)
const errorMessage = ref<string | null>(null)

async function loadPodcastData(id: string) {
  isLoading.value = true
  errorMessage.value = null

  try {
    const [podcastRes, episodesRes] = await Promise.all([
      api.podverse.getPodcast(id),
      api.podverse.getEpisodes(id),
    ])
    podcast.value = podcastRes
    episodes.value = episodesRes
  } catch (error) {
    console.error('Failed to load Podverse data:', error)
    errorMessage.value = 'Failed to load podcast information. Please try again.'
  } finally {
    isLoading.value = false
  }
}

// Adaptive duration formatter: MM:SS or HH:MM:SS
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

async function playEpisode(episode: MediaMetaData) {
  if (episode.rawUri) {
    await playbackStore.playUri(episode.rawUri)
  }
}

async function refresh() {
  const podcastId = route.params.id as string
  if (podcastId) {
    loadPodcastData(podcastId)
  }
}

onMounted(() => {
  const podcastId = route.params.id as string
  if (podcastId) {
    loadPodcastData(podcastId)
  }
})

watch(
  () => route.params.id,
  (newId) => {
    if (newId && typeof newId === 'string') {
      loadPodcastData(newId)
    }
  }
)
</script>

<template>
  <div class="min-h-screen text-slate-100">
    <!-- Fluid outer container padding -->
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
          <span class="text-sm font-medium">Loading Podcast...</span>
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
        <!-- Podcast Header Info -->
        <section
          v-if="podcast"
          class="flex flex-col sm:flex-row items-center sm:items-start gap-6 text-center sm:text-left"
        >
          <img
            :src="podcast.imageUrl || '/music.png'"
            :alt="podcast.title || 'Podcast Cover'"
            class="w-40 h-40 sm:w-48 sm:h-48 md:w-56 md:h-56 rounded-2xl bg-slate-800 object-cover shadow-2xl shrink-0"
          />
          <div class="space-y-3 flex-1 min-w-0">
            <span
              class="inline-block px-3 py-1 text-xs rounded-full bg-indigo-500/10 text-indigo-400 border border-indigo-500/20 font-semibold uppercase tracking-wider"
            >
              Podverse Podcast
            </span>
            <h1
              class="text-2xl sm:text-3xl md:text-4xl font-extrabold tracking-tight text-white leading-tight break-words"
            >
              {{ podcast.title || 'Untitled Podcast' }}
            </h1>
            <p class="text-slate-400 text-sm sm:text-base font-medium truncate">
              {{ podcast.artist || 'Unknown Author' }}
            </p>
            <p
              v-if="podcast.album"
              class="text-slate-300 text-xs sm:text-sm line-clamp-3 sm:line-clamp-5 leading-relaxed pt-1"
            >
              {{ podcast.album }}
            </p>
          </div>
        </section>

        <!-- Episode List Section -->
        <section class="space-y-4">
          <div class="flex items-center justify-between border-b border-slate-800 pb-3">
            <h2 class="text-lg sm:text-xl font-semibold text-slate-200">Episodes</h2>
            <span class="text-xs sm:text-sm text-slate-400 font-medium">
              {{ episodes.length }} {{ episodes.length === 1 ? 'episode' : 'episodes' }}
            </span>
          </div>

          <div v-if="episodes.length === 0" class="text-slate-500 py-12 text-center text-sm">
            No episodes found for this podcast.
          </div>

          <div v-else class="space-y-2">
            <div
              v-for="(episode, index) in episodes"
              :key="episode.rawUri || index"
              class="flex items-center justify-between p-3 sm:p-4 rounded-xl bg-slate-800/40 hover:bg-slate-800/80 border border-slate-800/60 transition group gap-4"
            >
              <!-- Play Button & Episode Title/Description -->
              <div class="flex items-center space-x-3 sm:space-x-4 min-w-0 flex-1">
                <button
                  @click="playEpisode(episode)"
                  class="w-9 h-9 sm:w-10 sm:h-10 rounded-full bg-indigo-600/90 hover:bg-indigo-500 text-white flex items-center justify-center shadow hover:scale-105 transition shrink-0"
                  title="Play Episode"
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
                    {{ episode.title || 'Untitled Episode' }}
                  </h3>
                  <p
                    v-if="episode.album"
                    class="text-xs text-slate-400 line-clamp-2 sm:line-clamp-1"
                  >
                    {{ episode.album }}
                  </p>
                </div>
              </div>

              <!-- Episode Duration -->
              <div class="shrink-0 text-xs font-mono text-slate-400 pl-2">
                <span>{{ convertSeconds(episode.duration) }}</span>
              </div>
            </div>
          </div>
        </section>
      </template>
    </div>
  </div>
</template>
