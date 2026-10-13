<script setup lang="ts">
// TODO: Import your store/composable for queue management and navigation if needed
// e.g., import { usePlayerStore } from '../stores/playerStore';

defineProps<{
  playlistCount: number
}>()

defineEmits<{
  (e: 'toggle-queue'): void
  (e: 'minimize'): void
}>()

// TODO: Replace or wire up minimize handler with router or external navigation composable
function handleMinimize() {
  // TODO: Call your router or window control interface here
  console.log('Minimize action triggered')
}
</script>

<template>
  <header class="flex items-center justify-between px-6 pt-6 pb-2 text-zinc-400">
    <button
      @click="
        $emit('minimize')
        handleMinimize()
      "
      class="p-2 rounded-full hover:bg-zinc-800 hover:text-zinc-100 transition-colors"
      aria-label="Minimize player"
    >
      <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
      </svg>
    </button>
    <div class="text-center">
      <span class="text-xs uppercase tracking-widest font-semibold text-zinc-500"
        >Playing from Playlist</span
      >
      <h2 class="text-sm font-medium text-zinc-200">Synthwave Essentials</h2>
    </div>
    <button
      @click="$emit('toggle-queue')"
      class="p-2 rounded-full hover:bg-zinc-800 hover:text-zinc-100 transition-colors relative"
      aria-label="Toggle queue"
    >
      <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M4 6h16M4 12h16M4 18h7"
        />
      </svg>
      <span class="absolute top-1 right-1 w-2 h-2 bg-indigo-500 rounded-full"></span>
    </button>
  </header>
</template>
