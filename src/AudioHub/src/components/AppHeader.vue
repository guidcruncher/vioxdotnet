<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  searchQuery: string;
  activeSource: string;
}>();

const emit = defineEmits<{
  (e: 'update:searchQuery', value: string): void;
  (e: 'toggleMenu'): void;
}>();

const activeBadgeText = computed(() => {
  return `Source: ${props.activeSource === 'all' ? 'All' : props.activeSource}`;
});

const onInput = (event: Event) => {
  const target = event.target as HTMLInputElement;
  emit('update:searchQuery', target.value);
};
</script>

<template>
  <header class="h-16 border-b border-zinc-800 bg-zinc-900/80 backdrop-blur px-4 sm:px-6 flex items-center justify-between z-10 shrink-0">
    <div class="flex items-center space-x-4 flex-1 max-w-xl">
      <button @click="emit('toggleMenu')" class="md:hidden text-zinc-400 hover:text-white focus:outline-none">
        <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
        </svg>
      </button>

      <div class="relative w-full">
        <span class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none text-zinc-400">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </span>
        <input
          type="text"
          :value="searchQuery"
          @input="onInput"
          placeholder="Search podcasts, music, streams across services..."
          class="w-full bg-zinc-800 text-sm text-zinc-100 placeholder-zinc-400 rounded-full pl-9 pr-4 py-2 border border-zinc-700/50 focus:outline-none focus:border-indigo-500 transition"
        />
      </div>
    </div>

    <div class="flex items-center space-x-3 ml-4">
      <span class="hidden sm:inline-block text-xs font-semibold px-3 py-1 rounded-full bg-zinc-800 text-zinc-300 border border-zinc-700">
        {{ activeBadgeText }}
      </span>
    </div>
  </header>
</template>
