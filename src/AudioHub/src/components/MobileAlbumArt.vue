<script setup lang="ts">
import type { MediaMetaData } from '@/types'

const props = defineProps<{
  defaultimage?: string
  track: MediaMetaData
  isPlaying: boolean
}>()

const handleImageError = (event: Event) => {
  const img = event.target as HTMLImageElement
  if (props.defaultimage) {
    if (img.src !== props.defaultimage) img.src = props.defaultimage
  } else {
    if (img.src !== '/audiosources.png') img.src = '/audiosources.png'
  }
}
</script>

<template>
  <div class="flex items-center justify-center px-8 py-6">
    <div
      class="relative w-72 h-72 rounded-3xl overflow-hidden shadow-2xl shadow-indigo-950/50 bg-zinc-800 border border-zinc-700/50 transition-transform duration-500"
      :class="{ 'scale-95': !isPlaying, 'scale-100': isPlaying }"
    >
      <img
        :src="track.imageUrl"
        @error="handleImageError"
        :alt="track.title"
        class="w-full h-full object-cover transition-transform duration-700"
        :class="{ 'scale-105': isPlaying }"
      />
      <div
        class="absolute inset-0 bg-gradient-to-t from-zinc-950/40 via-transparent to-transparent pointer-events-none"
      ></div>

      <!-- Visualizer animation overlay when playing -->
      <div v-if="isPlaying" class="absolute bottom-4 left-4 flex items-end gap-1 h-6">
        <span
          class="w-1 bg-indigo-400 rounded-full animate-bounce"
          style="animation-duration: 0.6s"
        ></span>
        <span
          class="w-1 bg-indigo-400 rounded-full animate-bounce"
          style="animation-duration: 0.4s"
        ></span>
        <span
          class="w-1 bg-indigo-400 rounded-full animate-bounce"
          style="animation-duration: 0.7s"
        ></span>
        <span
          class="w-1 bg-indigo-400 rounded-full animate-bounce"
          style="animation-duration: 0.5s"
        ></span>
      </div>
    </div>
  </div>
</template>
