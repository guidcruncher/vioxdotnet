<script setup lang="ts">
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '@/types/api'

const store = usePlaybackStore()

async function playItem(item: MediaMetaData) {
  await store.playUri(item.rawUri)
}
</script>

<template>
  <div
    class="relative min-h-full w-full max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6 bg-cover bg-center bg-no-repeat flex flex-col before:absolute before:inset-0 before:bg-black/80 before:rounded-lg overflow-hidden"
    style="background-image: url('/icon-lg.svg')"
  >
    <!-- Foreground content sitting above the darkened background layer -->
    <div class="relative z-10 flex-1">
      <FavouritesList @play="playItem" />
    </div>
  </div>
</template>
