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

function convertSeconds(totalSeconds?: number): string {
  if (!totalSeconds || totalSeconds <= 0) {
    return '00:00:00'
  }
  const hours = Math.floor(totalSeconds / 3600)
  const minutes = Math.floor((totalSeconds % 3600) / 60)
  const seconds = Math.floor(totalSeconds % 60)
  const pad = (num: number): string => num.toString().padStart(2, '0')
  return `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`
}

async function playEpisode(episode: MediaMetaData) {
  if (episode.rawUri) {
    await playbackStore.playUri(episode.rawUri)
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
  <div class="p-6 max-w-7xl mx-auto space-y-8 text-slate-100">
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
        <span>Loading Podcast...</span>
      </div>
    </div>

    <!-- Error State -->
    <div
      v-else-if="errorMessage"
      class="bg-red-500/10 border border-red-500/30 rounded-lg p-6 text-red-400 text-center"
    >
      <p>{{ errorMessage }}</p>
    </div>

    <template v-else>
      <!-- Podcast Header Info -->
      <section v-if="podcast" class="flex flex-col md:flex-row gap-6 items-start">
        <img
          :src="
            podcast.imageUrl ||
            'data:image/svg+xml,%3Csvg xmlns=\'http://www.w3.org/2000/svg\' width=\'100\' height=\'100\'%3E%3Crect width=\'100\' height=\'100\' fill=\'%231e293b\'/%3E%3C/svg%3E'
          "
          :alt="podcast.title || 'Podcast Cover'"
          class="w-48 h-48 rounded-xl bg-slate-800 object-cover shadow-xl flex-shrink-0"
        />
        <div class="space-y-3 flex-1">
          <span
            class="px-2.5 py-1 text-xs rounded-full bg-indigo-500/10 text-indigo-400 border border-indigo-500/20 font-medium"
          >
            Podverse Podcast
          </span>
          <h1 class="text-3xl font-bold tracking-tight text-white">
            {{ podcast.title || 'Untitled Podcast' }}
          </h1>
          <p class="text-slate-400 text-sm">
            {{ podcast.artist || 'Unknown Author' }}
          </p>
          <p v-if="podcast.album" class="text-slate-300 text-sm line-clamp-3 leading-relaxed">
            {{ podcast.album }}
          </p>
        </div>
      </section>

      <!-- Episode List -->
      <section class="space-y-4">
        <h2 class="text-xl font-semibold text-slate-200 border-b border-slate-800 pb-3">
          Episodes ({{ episodes.length }})
        </h2>

        <div v-if="episodes.length === 0" class="text-slate-500 py-8 text-center">
          No episodes found for this podcast.
        </div>

        <div v-else class="space-y-2">
          <div
            v-for="episode in episodes"
            :key="episode.rawUri"
            class="flex items-center justify-between p-4 rounded-lg bg-slate-800/40 hover:bg-slate-800/80 border border-slate-800/60 transition group"
          >
            <!-- Play Button & Metadata -->
            <div class="flex items-center space-x-4 min-w-0 pr-4">
              <button
                @click="playEpisode(episode)"
                class="w-10 h-10 rounded-full bg-indigo-600 hover:bg-indigo-500 text-white flex items-center justify-center shadow-md hover:scale-105 transition flex-shrink-0"
                title="Play Episode"
              >
                <svg class="w-5 h-5 fill-current translate-x-0.5" viewBox="0 0 24 24">
                  <path d="M8 5v14l11-7z" />
                </svg>
              </button>

              <div class="min-w-0 space-y-1">
                <h3
                  class="font-medium text-sm text-slate-100 truncate group-hover:text-indigo-300 transition"
                >
                  {{ episode.title || 'Untitled Episode' }}
                </h3>
                <p v-if="episode.album" class="text-xs text-slate-400 line-clamp-1">
                  {{ episode.album }}
                </p>
              </div>
            </div>

            <!-- Episode Duration / Actions -->
            <div class="flex items-center space-x-4 flex-shrink-0 text-xs font-mono text-slate-400">
              <span>{{ convertSeconds(!episode.duration ? 0 : episode.duration) }}</span>
            </div>
          </div>
        </div>
      </section>
    </template>
  </div>
</template>
