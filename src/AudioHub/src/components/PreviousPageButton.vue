<script setup lang="ts">
import { useRouter } from 'vue-router'
import {ChevronLeft } from '@lucide/vue'
  
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
    <Chevron?eft class="w-5 h-5 fill-current" />
  </button>
</template>
