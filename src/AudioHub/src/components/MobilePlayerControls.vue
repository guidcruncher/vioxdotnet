<script setup lang="ts">
import { usePlaybackStore } from '@/stores/playbackStore'
import { SkipBack, Play, Pause, SkipForward, Shuffle, Repeat } from '@lucide/vue'

defineProps<{
  isShuffle: boolean
}>()

defineEmits<{
  (e: 'toggle-shuffle'): void
}>()

const store = usePlaybackStore()
</script>

<template>
  <div class="px-8 py-2 flex items-center justify-between text-zinc-400">
    <!-- Shuffle -->
    <button
      @click="$emit('toggle-shuffle')"
      class="p-2 rounded-full hover:bg-zinc-800 transition-colors focus:outline-none"
      :class="{ 'text-indigo-400': isShuffle }"
      aria-label="Toggle shuffle"
    >
      <Shuffle class="w-5 h-5" />
    </button>

    <!-- Previous -->
    <button
      @click="store.previous"
      class="p-3 rounded-full hover:bg-zinc-800 text-zinc-200 hover:text-white transition-colors focus:outline-none"
      aria-label="Previous track"
    >
      <SkipBack class="w-6 h-6" />
    </button>

    <!-- Play / Pause -->
    <button
      @click="store.togglePlayPause"
      class="w-16 h-16 rounded-full bg-indigo-600 hover:bg-indigo-500 text-white flex items-center justify-center shadow-lg shadow-indigo-600/40 transition-transform active:scale-95 focus:outline-none"
      :aria-label="store.state.playing ? 'Pause' : 'Play'"
    >
      <Play v-if="!store.state.playing" class="w-7 h-7 translate-x-0.5 fill-current" />
      <Pause v-else class="w-7 h-7 fill-current" />
    </button>

    <!-- Next -->
    <button
      @click="store.next"
      class="p-3 rounded-full hover:bg-zinc-800 text-zinc-200 hover:text-white transition-colors focus:outline-none"
      aria-label="Next track"
    >
      <SkipForward class="w-6 h-6" />
    </button>
  </div>
</template>
