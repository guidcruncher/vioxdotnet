<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import type { MediaMetaData } from '@/types'
import { useRoute, useRouter } from 'vue-router'
import { Heart, ArrowLeft, ArrowRight } from '@lucide/vue'

const emit = defineEmits<{
  (e: 'play', item: MediaMetaData): void
}>()

const router = useRouter()
const route = useRoute()
const api = useApiClient()

const favourites = ref<MediaMetaData[]>([])
const isLoading = ref<boolean>(true)
const error = ref<string | null>(null)
const scrollContainer = ref<HTMLDivElement | null>(null)

const defaultImage = '/tuneinregion.png'

const fetchFavourites = async () => {
  isLoading.value = true
  error.value = null
  try {
    const response: MediaMetaData[] = await api.favourites.getAll()
    favourites.value = response ?? []
  } catch (err) {
    console.error('Failed to load favourites:', err)
    error.value = 'Failed to load your favourites. Please try again.'
  } finally {
    isLoading.value = false
  }
}

const handlePlay = (item: MediaMetaData) => {
  if (item.uri) {
    if (item.uri.source === 'podverse' && item.uri.type === 'podcast') {
      router.push(`/podverse/podcast/${encodeURIComponent(item.rawUri)}`)
      return
    }

    if (item.uri.source === 'spotify') {
      switch (item.uri.type) {
        case 'show':
          router.push(`/spotify/show/${encodeURIComponent(item.uri.id)}`)
          return
        case 'album':
          router.push(`/spotify/album/${encodeURIComponent(item.uri.id)}`)
          return
        case 'playlist':
          router.push(`/spotify/playlist/${encodeURIComponent(item.uri.id)}`)
          return
      }
    }
  }

  emit('play', item)
}

const scroll = (direction: 'left' | 'right') => {
  if (!scrollContainer.value) return
  const scrollAmount = scrollContainer.value.clientWidth * 0.75
  scrollContainer.value.scrollBy({
    left: direction === 'left' ? -scrollAmount : scrollAmount,
    behavior: 'smooth',
  })
}

const handleImageError = (event: Event) => {
  const target = event.target as HTMLImageElement
  if (target && target.src !== defaultImage) {
    target.src = defaultImage
  }
}

onMounted(() => {
  fetchFavourites()
})
</script>

<template>
  <div
    class="w-[90%] mx-auto bg-slate-950 p-2.5 sm:p-3 rounded-2xl border border-slate-800/80 shadow-lg overflow-hidden min-w-0"
  >
    <!-- Header Controls -->
    <div class="flex items-center justify-between mb-2.5 px-0.5">
      <div class="flex items-center space-x-1.5">
        <Heart class="w-3.5 h-3.5 text-rose-500 fill-current" />
        <h2 class="text-xs sm:text-sm font-bold text-slate-100 tracking-wide">Your Favourites</h2>
      </div>

      <!-- Navigation Arrows -->
      <div v-if="!isLoading && favourites.length > 0" class="flex items-center gap-1">
        <button
          @click="scroll('left')"
          type="button"
          aria-label="Scroll left"
          class="p-1 rounded-md bg-slate-900 border border-slate-800 text-slate-300 hover:bg-slate-800 hover:text-white transition active:scale-95 focus:outline-none"
        >
          <ArrowLeft class="w-3.5 h-3.5" />
        </button>

        <button
          @click="scroll('right')"
          type="button"
          aria-label="Scroll right"
          class="p-1 rounded-md bg-slate-900 border border-slate-800 text-slate-300 hover:bg-slate-800 hover:text-white transition active:scale-95 focus:outline-none"
        >
          <ArrowRight class="w-3.5 h-3.5" />
        </button>
      </div>
    </div>

    <!-- Loading Skeleton State -->
    <div v-if="isLoading" class="flex space-x-2 overflow-hidden scrollbar-none py-0.5 w-full">
      <div
        v-for="n in 6"
        :key="n"
        class="flex-none w-32 sm:w-36 bg-slate-900 border border-slate-800/80 p-2 rounded-xl animate-pulse"
      >
        <div class="w-full aspect-square bg-slate-800 rounded-lg mb-2"></div>
        <div class="h-2.5 bg-slate-800 rounded w-3/4 mb-1"></div>
        <div class="h-2 bg-slate-800 rounded w-1/2"></div>
      </div>
    </div>

    <!-- Error State -->
    <div
      v-else-if="error"
      class="p-3 rounded-xl bg-rose-500/10 border border-rose-500/20 text-rose-400 text-xs text-center flex justify-between items-center"
    >
      <span>{{ error }}</span>
      <button
        @click="fetchFavourites"
        class="px-2 py-1 bg-rose-500/20 hover:bg-rose-500/30 font-medium rounded-md transition text-rose-300"
      >
        Retry
      </button>
    </div>

    <!-- Empty State -->
    <div
      v-else-if="favourites.length === 0"
      class="py-6 text-center text-xs text-slate-400 bg-slate-900/50 rounded-xl border border-slate-800/50"
    >
      No favourites added yet.
    </div>

    <!-- Horizontal Scroll List -->
    <div
      v-else
      ref="scrollContainer"
      class="flex space-x-2 overflow-x-auto scrollbar-none scroll-smooth py-0.5 w-full snap-x snap-mandatory min-w-0"
    >
      <div
        v-for="item in favourites"
        :key="item.rawUri"
        @click="handlePlay(item)"
        class="flex-none w-32 sm:w-36 bg-slate-900/90 hover:bg-slate-800/80 border border-slate-800/80 hover:border-indigo-500/50 p-2 rounded-xl flex flex-col justify-between cursor-pointer transition-all duration-200 group shadow-md hover:shadow-indigo-500/10 snap-start focus:outline-none"
        tabindex="0"
        role="button"
        @keydown.enter="handlePlay(item)"
        @keydown.space.prevent="handlePlay(item)"
      >
        <!-- Artwork Container (MediaCard-like sizing & behaviour) -->
        <div
          class="w-full aspect-square bg-slate-800 rounded-lg overflow-hidden shadow-inner mb-2.5 flex items-center justify-center relative"
        >
          <img
            :src="item.imageUrl || defaultImage"
            :alt="item.title || 'Favourite Artwork'"
            @error="handleImageError"
            class="max-w-full max-h-full object-contain rounded-lg transition-transform duration-300 group-hover:scale-105"
          />

          <!-- Hover Overlay Play Icon (existing favourite UI kept) -->
          <div
            class="absolute inset-0 bg-slate-950/40 opacity-0 group-hover:opacity-100 transition-opacity duration-200 flex items-center justify-center"
          >
            <div
              class="w-6 h-6 sm:w-7 sm:h-7 rounded-full bg-indigo-600 text-white flex items-center justify-center shadow-lg transform group-hover:scale-100 scale-90 transition-transform duration-200"
            >
              <svg class="w-3 h-3 sm:w-3.5 sm:h-3.5 fill-current ml-0.5" viewBox="0 0 24 24">
                <path d="M8 5v14l11-7z" />
              </svg>
            </div>
          </div>
        </div>

        <!-- Track / Station Details (match MediaCard text scale) -->
        <div class="w-full min-w-0">
          <p
            class="font-bold text-xs sm:text-sm text-slate-100 group-hover:text-indigo-400 transition-colors truncate w-full"
            :title="item.title"
          >
            {{ item.title || 'Untitled' }}
          </p>
          <p
            class="text-[11px] sm:text-xs text-slate-400 truncate w-full mt-0.5"
            :title="item.artist || item.album"
          >
            {{ item.artist || item.album || 'Unknown Artist' }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.scrollbar-none {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
.scrollbar-none::-webkit-scrollbar {
  display: none;
}
</style>
