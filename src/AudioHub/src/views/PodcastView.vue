<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '@/types'
import MediaTrackList from '@/components/MediaTrackList.vue'
import { Play } from '@lucide/vue'

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
      api.library.getLibraryItem('podverse', id),
      api.library.getLibraryItemChildItems('podverse', id),
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
            &nbsp;<PreviousPageButton />
          </div>
        </section>

        <!-- Episode List Section -->
        <MediaTrackList v-model="episodes" @play="playEpisode" />
      </template>
    </div>
  </div>
</template>
