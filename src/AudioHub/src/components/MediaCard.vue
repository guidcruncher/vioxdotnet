<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import type { MediaMetaData } from '@/types/api'

interface Props {
  defaultimage?: string
}

// Assign to `props` so references like props.defaultimage resolve cleanly
const props = withDefaults(defineProps<Props>(), {
  defaultimage: '/tuneinregion.png',
})

// Vue 3.4+ model macro for two-way binding with parent
const item = defineModel<MediaMetaData | null>('item', { default: null })

const emit = defineEmits<{
  (e: 'play', item: MediaMetaData): void
  (e: 'view', item: MediaMetaData): void
  (e: 'favourite', isFav: boolean, item: MediaMetaData): void
}>()

const api = useApiClient()

const NON_PLAYABLE_TYPES = new Set(['podcast', 'audiobook', 'show', 'link', 'album', 'playlist'])
const FAVOURITE_ALLOWED_TYPES = new Set([
  'track',
  'album',
  'playlist',
  'station',
  'podcast',
  'show',
  'media',
])

// Local reactive state specifically for UI toggle tracking
const isFavourite = ref<boolean>(false)
const isSubmitting = ref<boolean>(false)

// Sync initial status when item changes or loads
watch(
  () => [item.value?.rawUri, item.value?.favourite],
  () => {
    isFavourite.value = Boolean(item.value?.favourite)
  },
  { immediate: true }
)

const playItem = () => {
  if (item.value) emit('play', item.value)
}

const viewItem = () => {
  if (item.value) emit('view', item.value)
}

const toggleFavourite = async () => {
  if (!item.value || isSubmitting.value) return

  const targetUri = item.value.rawUri
  const previousState = isFavourite.value
  const nextState = !previousState

  // Direct reactive update to trigger instant UI refresh
  isFavourite.value = nextState
  isSubmitting.value = true

  const updatedItem: MediaMetaData = {
    ...item.value,
    favourite: nextState,
  }

  try {
    if (nextState) {
      await api.favourites.add(updatedItem)
    } else {
      await api.favourites.remove(targetUri)
    }

    // Propagate changes upstream via v-model binding
    item.value = updatedItem

    // Emit explicit event payload for listeners
    emit('favourite', nextState, updatedItem)
  } catch (error) {
    // Revert local state if API request fails
    isFavourite.value = previousState
    console.error('Failed to sync favourite state:', error)
  } finally {
    isSubmitting.value = false
  }
}

const handleImageError = (event: Event) => {
  const target = event.target as HTMLImageElement
  if (target && target.src !== props.defaultimage) {
    target.src = props.defaultimage
  }
}

const imageUrl = computed(() => item.value?.imageUrl || props.defaultimage)

const isPlayable = computed(() => {
  const type = item.value?.uri?.type
  return Boolean(type && !NON_PLAYABLE_TYPES.has(type))
})

const isFavouriteSupported = computed(() => {
  const type = item.value?.uri?.type
  return Boolean(type && FAVOURITE_ALLOWED_TYPES.has(type))
})
</script>

<template>
  <div
    v-if="item"
    class="bg-slate-900/90 border border-slate-800/80 hover:border-slate-700/80 p-3 sm:p-3.5 rounded-xl flex flex-col justify-between h-full transition-all duration-200 group shadow-md"
  >
    <!-- Artwork & Track Details Container -->
    <div class="flex flex-col w-full">
      <!-- Fixed Square Artwork Box -->
      <div
        class="w-full aspect-square bg-slate-800 rounded-lg overflow-hidden relative shadow-inner mb-2.5"
      >
        <img
          :src="imageUrl"
          :alt="item.title || 'Media Artwork'"
          @error="handleImageError"
          class="w-full h-full object-cover rounded-lg group-hover:scale-105 transition-transform duration-300"
        />
      </div>

      <!-- Text Details -->
      <div class="w-full min-w-0">
        <p class="font-bold text-xs sm:text-sm text-slate-100 truncate w-full" :title="item.title">
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

    <!-- Responsive Action Bar -->
    <div class="mt-3 flex items-center gap-1.5 w-full shrink-0">
      <!-- Play Action -->
      <button
        v-if="isPlayable"
        @click="playItem"
        class="flex-1 bg-indigo-600 hover:bg-indigo-500 active:scale-95 text-xs text-white py-2 sm:py-1.5 px-3 rounded-lg font-medium transition shadow-sm shadow-indigo-600/30 flex items-center justify-center space-x-1.5 focus:outline-none min-w-0"
      >
        <svg class="w-3.5 h-3.5 fill-current shrink-0" viewBox="0 0 24 24">
          <path d="M8 5v14l11-7z" />
        </svg>
        <span class="truncate">Play</span>
      </button>

      <!-- View Action -->
      <button
        v-else
        @click="viewItem"
        class="flex-1 bg-slate-800 hover:bg-slate-700 active:scale-95 text-xs text-slate-200 py-2 sm:py-1.5 px-3 rounded-lg font-medium border border-slate-700 transition flex items-center justify-center space-x-1.5 focus:outline-none min-w-0"
      >
        <span class="truncate">View</span>
      </button>

      <button
        v-if="isFavouriteSupported"
        @click="toggleFavourite"
        :disabled="isSubmitting"
        type="button"
        :aria-label="isFavourite ? 'Remove from favourites' : 'Add to favourites'"
        :class="[
          'py-2 sm:py-1.5 px-2.5 rounded-lg border transition flex items-center justify-center shrink-0 focus:outline-none active:scale-95',
          isFavourite
            ? 'bg-rose-500/10 border-rose-500/30 text-rose-500 hover:bg-rose-500/20'
            : 'bg-slate-800 border-slate-700 text-slate-400 hover:text-slate-200 hover:bg-slate-700',
        ]"
      >
        <!-- Heart Solid (Is Favourite) -->
        <svg
          v-if="isFavourite"
          key="fav-active"
          class="w-3.5 h-3.5 fill-current"
          viewBox="0 0 24 24"
        >
          <path
            d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"
          />
        </svg>
        <!-- Heart Outline (Is Not Favourite) -->
        <svg
          v-else
          key="fav-inactive"
          class="w-3.5 h-3.5 fill-none stroke-current stroke-2"
          viewBox="0 0 24 24"
        >
          <path
            d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"
          />
        </svg>
      </button>
    </div>
  </div>
</template>
