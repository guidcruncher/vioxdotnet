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
  <div class="px-8 py-2 flex items-center justify-between">
    <div class="space-y-1 overflow-hidden pr-4">
      <h1 class="text-xl font-bold text-zinc-100 truncate">{{ track.title }}</h1>
      <p class="text-sm font-medium text-zinc-400 truncate">{{ track.artist }}</p>
    </div>
    <button
      @click="handleToggleLike"
      class="p-2 rounded-full hover:bg-zinc-800/80 transition-colors text-zinc-400"
      :class="{ 'text-rose-500': isLiked }"
      aria-label="Like track"
    >
      <svg
        class="w-6 h-6"
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
