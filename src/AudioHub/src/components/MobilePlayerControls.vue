<script setup lang="ts">
import { usePlaybackStore } from '@/stores/playbackStore'
import { SkipBack, Play, Pause, SkipForward, Shuffle, Repeat } from '@lucide/vue'

defineProps<{
  isShuffle: boolean
  isRepeat: boolean
}>()

defineEmits<{
  (e: 'toggle-shuffle'): void
  (e: 'toggle-repeat'): void
}>()

const store = usePlaybackStore()
</script>

<template>
  <div class="w-full flex items-center justify-between mt-4 px-2 text-slate-400">
    <!-- Shuffle -->
    <button
      @click="$emit('toggle-shuffle')"
      class="w-10 h-10 rounded-full flex items-center justify-center hover:text-white transition-colors"
      :class="{ 'text-purple-400': isShuffle }"
      title="Shuffle"
    >
      <Shuffle class="w-5 h-5" />
    </button>

    <!-- Previous Track -->
    <button
      @click="store.previous"
      class="w-12 h-12 rounded-full bg-slate-800/80 hover:bg-slate-700/80 flex items-center justify-center text-white transition-all active:scale-95 shadow-lg"
      title="Previous"
    >
      <SkipBack class="w-5 h-5" />
    </button>

    <!-- Play / Pause Button -->
    <button
      @click="store.togglePlayPause"
      class="w-16 h-16 rounded-full bg-gradient-to-tr from-purple-600 to-indigo-500 hover:from-purple-500 hover:to-indigo-400 flex items-center justify-center text-white glow-shadow-sm transition-all transform hover:scale-105 active:scale-95"
      title="Play / Pause"
      :aria-label="store.state.playing ? 'Pause' : 'Play'"
    >
      <Play v-if="!store.state.playing" class="w-7 h-7 translate-x-0.5 fill-current" />
      <Pause v-else class="w-7 h-7 fill-current" />
    </button>

    <!-- Next Track -->
    <button
      @click="store.next"
      class="w-12 h-12 rounded-full bg-slate-800/80 hover:bg-slate-700/80 flex items-center justify-center text-white transition-all active:scale-95 shadow-lg"
      title="Next"
    >
      <SkipForward class="w-5 h-5" />
    </button>

    <!-- Repeat -->
    <button
      @click="$emit('toggle-repeat')"
      class="w-10 h-10 rounded-full flex items-center justify-center hover:text-white transition-colors"
      :class="{ 'text-purple-400': isRepeat }"
      title="Repeat"
    >
      <Repeat class="w-5 h-5" />
    </button>
  </div>
</template>
