<script setup lang="ts">
import type { MediaMetaData } from '@/types'

const props = defineProps<{
  defaultimage?: string
  track: MediaMetaData
  isPlaying: boolean
  glowGradient?: string
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
  <div class="flex-1 flex flex-col items-center justify-center my-4">
    <div class="relative group w-64 h-64 sm:w-60 sm:h-60 mx-auto">
      <!-- Glowing Backlight -->
      <div
        class="absolute inset-0 bg-gradient-to-tr from-purple-600 to-indigo-500 rounded-3xl blur-2xl transition-opacity duration-500"
        :class="isPlaying ? 'opacity-80' : 'opacity-40'"
      ></div>

      <!-- Cover Art Container -->
      <div
        class="relative w-full h-full rounded-3xl overflow-hidden bg-gradient-to-br from-indigo-900 via-purple-900 to-slate-900 border border-slate-700/50 shadow-2xl flex flex-col items-center justify-center p-6 text-center"
      >
        <img
          :src="track.imageUrl"
          @error="handleImageError"
          :alt="track.title"
          class="absolute inset-0 w-full h-full object-cover opacity-80 transition-transform duration-700"
          :class="{ 'scale-105': isPlaying }"
        />
        <div
          class="absolute inset-0 bg-gradient-to-t from-slate-950/80 via-slate-950/20 to-transparent"
        ></div>

        <!-- Animated Equalizer Overlay matching the mockup -->
        <div
          class="absolute bottom-4 flex items-end space-x-1 h-8 transition-opacity duration-300"
          :class="isPlaying ? 'opacity-90' : 'opacity-0'"
        >
          <span
            class="w-1.5 bg-purple-400 rounded-full animate-bounce"
            style="height: 60%; animation-delay: 0.1s"
          ></span>
          <span
            class="w-1.5 bg-indigo-400 rounded-full animate-bounce"
            style="height: 100%; animation-delay: 0.3s"
          ></span>
          <span
            class="w-1.5 bg-purple-300 rounded-full animate-bounce"
            style="height: 40%; animation-delay: 0.2s"
          ></span>
          <span
            class="w-1.5 bg-pink-400 rounded-full animate-bounce"
            style="height: 80%; animation-delay: 0.4s"
          ></span>
          <span
            class="w-1.5 bg-indigo-300 rounded-full animate-bounce"
            style="height: 50%; animation-delay: 0.15s"
          ></span>
        </div>
      </div>
    </div>
  </div>
</template>
