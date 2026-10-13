<script setup lang="ts">
import { withDefaults } from 'vue'

const props = withDefaults(
  defineProps<{
    playlistCount?: number
    isMinimized?: boolean
  }>(),
  {
    playlistCount: 0,
    isMinimized: false,
  }
)

const emit = defineEmits<{
  (e: 'toggle-queue'): void
  (e: 'minimize'): void
  (e: 'maximize'): void
  (e: 'update:isMinimized', value: boolean): void
}>()

function handleToggleMinimize() {
  if (props.isMinimized) {
    emit('maximize')
    emit('update:isMinimized', false)
    console.log('Maximize action triggered')
  } else {
    emit('minimize')
    emit('update:isMinimized', true)
    console.log('Minimize action triggered')
  }
}
</script>

<template>
  <header class="flex items-center justify-between px-6 pt-6 pb-2 text-zinc-400">
    <!-- Minimize / Maximize Button -->
    <button
      @click="handleToggleMinimize"
      class="p-2 rounded-full hover:bg-zinc-800 hover:text-zinc-100 transition-colors"
      :aria-label="isMinimized ? 'Maximize player' : 'Minimize player'"
    >
      <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <!-- Chevron Up (Maximize) when minimized -->
        <path
          v-if="isMinimized"
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M5 15l7-7 7 7"
        />
        <!-- Chevron Down (Minimize) when expanded -->
        <path
          v-else
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M19 9l-7 7-7-7"
        />
      </svg>
    </button>

    <!-- Playlist Info Center -->
    <div class="text-center">
      <span class="text-xs uppercase tracking-widest font-semibold text-zinc-500">
        Playing from Playlist
      </span>
    </div>

    <!-- Toggle Queue Button with Badge -->
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
      <span
        v-if="playlistCount > 0"
        class="absolute top-1 right-1 w-2 h-2 bg-indigo-500 rounded-full"
      ></span>
    </button>
  </header>
</template>
