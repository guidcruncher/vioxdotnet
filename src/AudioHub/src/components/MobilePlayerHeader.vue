<script setup lang="ts">
import { withDefaults } from 'vue'

const props = withDefaults(
  defineProps<{
    playlistTitle?: string
    playlistCount?: number
    isMinimized?: boolean
  }>(),
  {
    playlistTitle: '',
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
  } else {
    emit('minimize')
    emit('update:isMinimized', true)
  }
}
</script>

<template>
  <header class="px-6 py-4 flex items-center justify-between z-20 text-slate-300">
    <!-- Minimize / Maximize Button -->
    <button
      @click="handleToggleMinimize"
      class="w-10 h-10 rounded-full bg-slate-800/80 hover:bg-slate-700/80 flex items-center justify-center transition-colors"
      :aria-label="isMinimized ? 'Maximize player' : 'Minimize player'"
      :title="isMinimized ? 'Maximize' : 'Minimize'"
    >
      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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
      <p class="text-[10px] uppercase tracking-widest text-slate-400 font-semibold">
        Playing from Playlist
      </p>
      <h2 class="text-xs font-bold text-slate-200 truncate max-w-[160px]">{{ playlistTitle }}</h2>
    </div>

    <!-- Toggle Queue Button with Pulse Badge -->
    <button
      @click="$emit('toggle-queue')"
      class="w-10 h-10 rounded-full bg-slate-800/80 hover:bg-slate-700/80 flex items-center justify-center transition-colors relative"
      aria-label="Toggle queue"
      title="Queue"
    >
      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M4 6h16M4 12h16M4 18h7"
        />
      </svg>
      <span
        v-if="playlistCount > 0"
        class="absolute top-2 right-2 w-2 h-2 bg-purple-500 rounded-full animate-pulse"
      ></span>
    </button>
  </header>
</template>
