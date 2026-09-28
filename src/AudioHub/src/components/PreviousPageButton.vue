<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

function goBack() {
  router.back()
  // Alternatively: router.go(-1)
}
</script>

<template>
  <button
    @click="goBack()"
    class="inline-flex items-center gap-2 px-5 py-2.5 rounded-full bg-indigo-600 hover:bg-indigo-500 text-white font-medium text-sm shadow-lg hover:scale-105 active:scale-95 transition shrink-0"
    title="Play All Tracks"
  >
    <svg class="w-5 h-5 fill-current" viewBox="0 0 24 24">
      <path d="M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z" />
    </svg>
    <span>Go Back</span>
  </button>
</template>
