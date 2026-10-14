<script setup lang="ts">
import { ref } from 'vue'
import type { MediaMetaData } from '@/types'

defineProps<{
  track: MediaMetaData
}>()

// TODO: Replace with your like/favorite API composable or store action
// e.g., const { toggleLike, isTrackLiked } = useFavoritesStore();
const isLiked = ref(false)

function handleToggleLike() {
  isLiked.value = !isLiked.value
  // TODO: Call your backend API or store mutation here
  // toggleLike(props.track.rawUri);
}
</script>

<template>
  <div class="w-full mt-6 flex items-center justify-between">
    <div class="flex-1 pr-4 truncate">
      <h1 class="text-xl font-bold text-white truncate tracking-tight">{{ track.title }}</h1>
      <p class="text-sm font-medium text-slate-400 truncate mt-0.5">{{ track.artist }}</p>
    </div>
    <button
      @click="handleToggleLike"
      class="w-11 h-11 rounded-full bg-slate-800/60 hover:bg-slate-800 border border-slate-700/50 flex items-center justify-center text-slate-300 transition-all active:scale-95"
      :class="{ 'text-purple-400 bg-purple-950/40 border-purple-500/40': isLiked }"
      aria-label="Like track"
      title="Like"
    >
      <svg
        class="w-5 h-5"
        :fill="isLiked ? 'currentColor' : 'none'"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"
        />
      </svg>
    </button>
  </div>
</template>
