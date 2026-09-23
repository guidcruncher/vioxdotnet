<script setup lang="ts">
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '@/types/api'

const store = usePlaybackStore()

async function playItem(item: MediaMetaData) {
  await store.playUri(item.rawUri)
}
</script>

<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
    <div>
      <FavouritesList @play="playItem" />
    </div>
  </div>
</template>
