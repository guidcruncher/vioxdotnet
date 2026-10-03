<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import type { MediaMetaData } from '@/types'

interface Props {
  defaultimage?: string
}

const props = withDefaults(defineProps<Props>(), {
  defaultimage: '/tuneinregion.png',
})

const item = defineModel<MediaMetaData | null>('item', { default: null })

const emit = defineEmits<{
  (e: 'play', item: MediaMetaData): void
  (e: 'view', item: MediaMetaData): void
  (e: 'favourite', isFav: boolean, item: MediaMetaData): void
}>()

const api = useApiClient()

const NON_PLAYABLE_TYPES = new Set([
  'podcast',
  'audiobook',
  'show',
  'link',
  'album',
  'playlist',
  'file',
])
const FAVOURITE_ALLOWED_TYPES = new Set([
  'track',
  'album',
  'playlist',
  'station',
  'podcast',
  'show',
  'media',
])

const isFavourite = ref(false)
const isSubmitting = ref(false)

watch(
  () => [item.value?.rawUri, item.value?.favourite],
  () => {
    isFavourite.value = Boolean(item.value?.favourite)
  },
  { immediate: true }
)

const playItem = () => item.value && emit('play', item.value)
const viewItem = () => item.value && emit('view', item.value)

/* --- CLICK FLASH OVERLAY FLAG --- */
const showClickOverlay = ref(false)

const handleImageClick = () => {
  if (isPlayable.value) playItem()
  else viewItem()

  // Flash overlay for 600ms
  showClickOverlay.value = true
  setTimeout(() => {
    showClickOverlay.value = false
  }, 600)
}

const toggleFavourite = async () => {
  if (!item.value || isSubmitting.value) return

  const previous = isFavourite.value
  const next = !previous

  isFavourite.value = next
  isSubmitting.value = true

  const updated: MediaMetaData = { ...item.value, favourite: next }

  try {
    if (next) await api.favourites.add(updated)
    else await api.favourites.remove(item.value.rawUri)

    item.value = updated
    emit('favourite', next, updated)
  } catch (err) {
    isFavourite.value = previous
    console.error(err)
  } finally {
    isSubmitting.value = false
  }
}

const handleImageError = (event: Event) => {
  const img = event.target as HTMLImageElement
  if (img.src !== props.defaultimage) img.src = props.defaultimage
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
    class="flex flex-col w-full bg-slate-900/90 hover:bg-slate-800/80 border border-slate-800/80 hover:border-indigo-500/50 p-2 rounded-xl cursor-pointer transition-all duration-200 group shadow-md hover:shadow-indigo-500/10 snap-start"
  >
    <!-- Artwork + Text -->
    <div class="flex flex-col w-full">
      <!-- Artwork Container with hover + click overlay -->
      <div
        class="w-full aspect-square bg-slate-800 rounded-lg overflow-hidden shadow-inner mb-2.5 flex items-center justify-center relative cursor-pointer"
        @click="handleImageClick"
      >
        <img
          :src="imageUrl"
          :alt="item.title || 'Media Artwork'"
          @error="handleImageError"
          class="max-w-full max-h-full object-contain rounded-lg transition-transform duration-300 group-hover:scale-105"
        />

        <!-- Hover + Click overlay -->
        <div
          class="absolute inset-0 bg-slate-950/40 transition-opacity duration-200 flex items-center justify-center"
          :class="{
            'opacity-100': showClickOverlay,
            'opacity-0 group-hover:opacity-100': !showClickOverlay,
          }"
        >
          <div
            class="w-6 h-6 sm:w-7 sm:h-7 rounded-full bg-indigo-600 text-white flex items-center justify-center shadow-lg transform group-hover:scale-100 scale-90 transition-transform duration-200 ring-2 ring-indigo-400/70"
          >
            <svg class="w-3 h-3 sm:w-3.5 sm:h-3.5 fill-current ml-0.5" viewBox="0 0 24 24">
              <path d="M8 5v14l11-7z" />
            </svg>
          </div>
        </div>
      </div>

      <!-- Title + Artist -->
      <div class="w-full min-w-0">
        <p class="font-bold text-xs sm:text-sm text-slate-100 truncate" :title="item.title">
          {{ item.title || 'Untitled' }}
        </p>
        <p
          class="text-[11px] sm:text-xs text-slate-400 truncate mt-0.5"
          :title="item.artist || item.album"
        >
          {{ item.artist || item.album || 'Unknown Artist' }}
        </p>
      </div>
    </div>

    <!-- Favourite Button -->
    <div class="mt-3 flex items-center gap-1.5 w-full">
      <button
        v-if="isFavouriteSupported"
        @click="toggleFavourite"
        :disabled="isSubmitting"
        type="button"
        :class="[
          'py-2 sm:py-1.5 px-2.5 rounded-lg border transition flex items-center justify-center shrink-0 active:scale-95',
          isFavourite
            ? 'bg-rose-500/10 border-rose-500/30 text-rose-500 hover:bg-rose-500/20'
            : 'bg-slate-800 border-slate-700 text-slate-400 hover:text-slate-200 hover:bg-slate-700',
        ]"
      >
        <svg v-if="isFavourite" class="w-3.5 h-3.5 fill-current" viewBox="0 0 24 24">
          <path
            d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"
          />
        </svg>

        <svg v-else class="w-3.5 h-3.5 fill-none stroke-current stroke-2" viewBox="0 0 24 24">
          <path
            d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"
          />
        </svg>
      </button>
    </div>
  </div>
</template>
