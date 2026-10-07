<script setup lang="ts">
import { ref } from 'vue'
import type { MediaMetaData } from '@/types'
import { useApiClient } from '@/composables/useApiClient'
import { Play, Eye, Plus, Minus } from '@lucide/vue'

interface Props {
  defaultImage?: string
  showFavourite?: boolean
  showPlaylist?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  defaultImage: '/casette.png',
  showFavourite: true,
  showPlaylist: true,
})

// v-model for MediaMetaData[]
const tracks = defineModel<MediaMetaData[]>({ default: () => [] })

const emit = defineEmits<{
  (e: 'play', track: MediaMetaData): void
  (e: 'view', track: MediaMetaData): void
}>()

/* -----------------------------
   PLAYABILITY LOGIC
----------------------------- */
const NON_PLAYABLE_TYPES = new Set([
  'podcast',
  'audiobook',
  'show',
  'link',
  'album',
  'playlist',
  'file',
])

function isPlayable(track: MediaMetaData): boolean {
  const type = track.uri?.type
  return Boolean(type && !NON_PLAYABLE_TYPES.has(type))
}

function handleAction(track: MediaMetaData) {
  if (isPlayable(track)) emit('play', track)
  else emit('view', track)
}

/* -----------------------------
   IMAGE FALLBACK
----------------------------- */
function handleImageError(event: Event) {
  const target = event.target as HTMLImageElement
  if (target && target.src !== props.defaultImage) {
    target.src = props.defaultImage
  }
}

/* -----------------------------
   DURATION & DATE FORMATTERS
----------------------------- */
function convertSeconds(totalSeconds?: number): string {
  if (!totalSeconds || totalSeconds <= 0) return '0:00'
  const hours = Math.floor(totalSeconds / 3600)
  const minutes = Math.floor((totalSeconds % 3600) / 60)
  const seconds = Math.floor(totalSeconds % 60)
  const pad = (n: number) => n.toString().padStart(2, '0')
  return hours > 0 ? `${hours}:${pad(minutes)}:${pad(seconds)}` : `${minutes}:${pad(seconds)}`
}

function formatDate(dateValue?: string | Date | number): string | null {
  if (!dateValue) return null
  const date = new Date(dateValue)
  if (isNaN(date.getTime())) return String(dateValue)
  return date.toLocaleDateString(undefined, {
    timeZone: 'UTC',
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  })
}

/* -----------------------------
   FAVOURITE + PLAYLIST LOGIC
----------------------------- */
const api = useApiClient()

const FAVOURITE_ALLOWED_TYPES = new Set([
  'track',
  'album',
  'playlist',
  'station',
  'podcast',
  'show',
  'media',
])

const PLAYLIST_ALLOWED_TYPES = new Set(['track', 'album', 'station', 'episode', 'show', 'media'])

function isFavouriteSupported(track: MediaMetaData) {
  const type = track.uri?.type
  return Boolean(type && FAVOURITE_ALLOWED_TYPES.has(type))
}

function isPlaylistSupported(track: MediaMetaData) {
  const type = track.uri?.type
  return Boolean(type && PLAYLIST_ALLOWED_TYPES.has(type))
}

// Prevent double-clicking on the same item
const submitting = ref<string | null>(null)

/* Toggle Favourite */
async function toggleFavourite(track: MediaMetaData) {
  if (submitting.value) return
  if (!isFavouriteSupported(track)) return

  submitting.value = track.rawUri

  const previous = !!track.favourite
  const next = !previous

  track.favourite = next // optimistic update

  try {
    if (next) await api.favourites.add(track)
    else await api.favourites.remove(track.rawUri)
  } catch (err) {
    track.favourite = previous
    console.error(err)
  } finally {
    submitting.value = null
  }
}

/* Toggle Playlist */
async function togglePlaylist(track: MediaMetaData) {
  if (submitting.value) return
  if (!isPlaylistSupported(track)) return

  submitting.value = track.rawUri

  const previous = !!track.inPlaylist
  const next = !previous

  track.inPlaylist = next // optimistic update

  try {
    // Add API logic later if needed
  } catch (err) {
    track.inPlaylist = previous
    console.error(err)
  } finally {
    submitting.value = null
  }
}
</script>

<template>
  <section class="space-y-4">
    <div class="flex items-center justify-between border-b border-slate-800 pb-3">
      <span class="text-xs sm:text-sm text-slate-400 font-medium">
        {{ tracks.length }} {{ tracks.length === 1 ? 'item' : 'items' }}
      </span>
    </div>

    <div v-if="tracks.length === 0" class="text-slate-500 py-12 text-center text-sm">
      No items found.
    </div>

    <div v-else class="space-y-2">
      <div
        v-for="(track, index) in tracks"
        :key="track.rawUri || index"
        @click="handleAction(track)"
        class="flex items-center justify-between p-3 sm:p-4 rounded-xl bg-slate-800/40 hover:bg-slate-800/80 border border-slate-800/60 transition cursor-pointer group gap-4"
      >
        <!-- LEFT SIDE -->
        <div class="flex items-center space-x-3 sm:space-x-4 min-w-0 flex-1">
          <span
            class="text-xs font-mono text-slate-500 w-5 text-right hidden sm:inline-block shrink-0"
          >
            {{ index + 1 }}
          </span>

          <!-- Play / View Button -->
          <button
            v-if="isPlayable(track)"
            @click.stop="emit('play', track)"
            class="w-9 h-9 sm:w-10 sm:h-10 rounded-full bg-indigo-600/90 hover:bg-indigo-500 text-white flex items-center justify-center shadow hover:scale-105 transition shrink-0 active:scale-95"
            title="Play Track"
          >
            <Play class="w-4 h-4 sm:w-5 sm:h-5 fill-current translate-x-0.5" />
          </button>

          <button
            v-else
            @click.stop="emit('view', track)"
            class="w-9 h-9 sm:w-10 sm:h-10 rounded-full bg-slate-700/80 hover:bg-slate-600 text-slate-200 flex items-center justify-center shadow hover:scale-105 transition shrink-0 border border-slate-600/50 active:scale-95"
            title="View Details"
          >
            <Eye class="w-4 h-4 sm:w-5 sm:h-5 fill-current" />
          </button>

          <!-- Thumbnail -->
          <div
            class="relative w-10 h-10 sm:w-12 sm:h-12 rounded-lg overflow-hidden bg-slate-900 border border-slate-700/50 shrink-0"
          >
            <img
              :src="track.imageUrl || props.defaultImage"
              :alt="track.title || 'Track thumbnail'"
              @error="handleImageError"
              class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
            />
          </div>

          <!-- Title + Artist + Release Date -->
          <div class="min-w-0 flex-1 space-y-0.5">
            <h3
              class="font-medium text-xs sm:text-sm text-slate-100 truncate group-hover:text-indigo-300 transition"
              :title="track.title"
            >
              {{ track.title || 'Untitled Track' }}
            </h3>
            <div
              v-if="track.artist || track.album || track.releaseDate"
              class="text-xs text-slate-400 flex items-center gap-1.5 min-w-0"
              :title="
                [track.artist || track.album, formatDate(track.releaseDate)]
                  .filter(Boolean)
                  .join(' • ')
              "
            >
              <span v-if="track.artist || track.album" class="truncate min-w-0">
                {{ track.artist || track.album }}
              </span>
              <span
                v-if="(track.artist || track.album) && track.releaseDate"
                class="text-slate-600 shrink-0"
                >&bull;</span
              >
              <span v-if="track.releaseDate" class="text-slate-400 shrink-0 whitespace-nowrap">
                {{ formatDate(track.releaseDate) }}
              </span>
            </div>
          </div>
        </div>

        <!-- RIGHT SIDE -->
        <div class="flex items-center gap-3 shrink-0">
          <!-- Playlist Button -->
          <button
            @click.stop="togglePlaylist(track)"
            :disabled="!isPlaylistSupported(track) || submitting === track.rawUri"
            class="w-8 h-8 rounded-lg border flex items-center justify-center transition bg-slate-800 border-slate-700 text-slate-400 hover:bg-slate-700 hover:text-slate-200 active:scale-95"
            :class="
              track.inPlaylist
                ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-400 hover:bg-emerald-500/20 scale-110'
                : ''
            "
            title="Toggle Playlist"
          >
            <Minus v-if="track.inPlaylist" class="w-3.5 h-3.5 stroke-current stroke-2" />
            <Plus v-else class="w-3.5 h-3.5 stroke-current stroke-2" />
          </button>

          <!-- Favourite Button -->
          <button
            v-if="showFavourite"
            @click.stop="toggleFavourite(track)"
            :disabled="!isFavouriteSupported(track) || submitting === track.rawUri"
            class="w-8 h-8 rounded-lg border flex items-center justify-center transition bg-slate-800 border-slate-700 text-slate-400 hover:bg-slate-700 hover:text-slate-200 active:scale-95"
            :class="
              track.favourite
                ? 'bg-rose-500/10 border-rose-500/30 text-rose-500 hover:bg-rose-500/20 scale-110'
                : ''
            "
            title="Toggle Favourite"
          >
            <svg v-if="track.favourite" class="w-3.5 h-3.5 fill-current" viewBox="0 0 24 24">
              <path
                d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"
              />
            </svg>
            <svg v-else class="w-3.5 h-3.5 stroke-current stroke-2" viewBox="0 0 24 24">
              <path
                d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"
              />
            </svg>
          </button>

          <!-- Duration -->
          <span class="text-xs font-mono text-slate-400 pl-2">
            {{ convertSeconds(track.duration) }}
          </span>
        </div>
      </div>
    </div>
  </section>
</template>
