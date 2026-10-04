<script setup lang="ts">
import type { MediaMetaData } from '@/types'

interface Props {
  defaultImage?: string
}

const props = withDefaults(defineProps<Props>(), {
  defaultImage: '/casette.png',
})

// Defines v-model for MediaMetaData[]
const tracks = defineModel<MediaMetaData[]>({ default: () => [] })

const emit = defineEmits<{
  (e: 'play', track: MediaMetaData): void
  (e: 'view', track: MediaMetaData): void
}>()

// Types that are informational/containers and should trigger 'view' rather than direct playback
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
  if (isPlayable(track)) {
    emit('play', track)
  } else {
    emit('view', track)
  }
}

function handleImageError(event: Event) {
  const target = event.target as HTMLImageElement
  if (target && target.src !== props.defaultImage) {
    target.src = props.defaultImage
  }
}

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
        <!-- Track Index, Action Button (Play / View), Artwork & Metadata -->
        <div class="flex items-center space-x-3 sm:space-x-4 min-w-0 flex-1">
          <span
            class="text-xs font-mono text-slate-500 w-5 text-right hidden sm:inline-block shrink-0"
          >
            {{ index + 1 }}
          </span>

          <!-- Play Action Button -->
          <button
            v-if="isPlayable(track)"
            @click.stop="emit('play', track)"
            class="w-9 h-9 sm:w-10 sm:h-10 rounded-full bg-indigo-600/90 hover:bg-indigo-500 text-white flex items-center justify-center shadow hover:scale-105 transition shrink-0 active:scale-95"
            title="Play Track"
          >
            <svg class="w-4 h-4 sm:w-5 sm:h-5 fill-current translate-x-0.5" viewBox="0 0 24 24">
              <path d="M8 5v14l11-7z" />
            </svg>
          </button>

          <!-- View Action Button -->
          <button
            v-else
            @click.stop="emit('view', track)"
            class="w-9 h-9 sm:w-10 sm:h-10 rounded-full bg-slate-700/80 hover:bg-slate-600 text-slate-200 flex items-center justify-center shadow hover:scale-105 transition shrink-0 border border-slate-600/50 active:scale-95"
            title="View Details"
          >
            <svg class="w-4 h-4 sm:w-5 sm:h-5 fill-current" viewBox="0 0 24 24">
              <path
                d="M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z"
              />
            </svg>
          </button>

          <!-- Track Image Thumbnail -->
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

          <div class="min-w-0 flex-1 space-y-0.5">
            <h3
              class="font-medium text-xs sm:text-sm text-slate-100 truncate group-hover:text-indigo-300 transition"
              :title="track.title"
            >
              {{ track.title || 'Untitled Track' }}
            </h3>
            <p
              v-if="track.artist || track.album"
              class="text-xs text-slate-400 truncate"
              :title="track.artist || track.album"
            >
              {{ track.artist || track.album }}
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
