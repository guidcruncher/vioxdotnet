<script setup lang="ts">
import { AudioItem } from '../types/audio';

defineProps<{
  item: AudioItem;
  isActive: boolean;
}>();

const emit = defineEmits<{
  (e: 'play', item: AudioItem): void;
}>();
</script>

<template>
  <div
    :class="[
      'group rounded-xl p-4 border transition duration-200 flex flex-col justify-between',
      isActive ? 'border-indigo-500 bg-zinc-800/80' : 'border-zinc-800/80 bg-zinc-900/60 hover:border-zinc-700'
    ]"
  >
    <div>
      <div class="relative aspect-square rounded-lg overflow-hidden mb-3 bg-zinc-800">
        <img :src="item.imageUrl" class="w-full h-full object-cover group-hover:scale-105 transition duration-300" :alt="item.title" />
        <button
          @click="emit('play', item)"
          class="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 flex items-center justify-center transition"
        >
          <div class="w-12 h-12 rounded-full bg-indigo-600 text-white flex items-center justify-center shadow-lg hover:scale-110 transition">
            <svg class="w-6 h-6 fill-current ml-0.5" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
          </div>
        </button>
      </div>
      <div class="flex items-center justify-between gap-2 mb-1">
        <h3 class="font-semibold text-sm text-white truncate flex-1">{{ item.title }}</h3>
        <span :class="['text-[10px] font-bold px-2 py-0.5 rounded border']">
          {{ item.uri?.source }}
        </span>
      </div>
      <p class="text-xs text-zinc-400 truncate mb-3">{{ item.artist }}</p>
    </div>
    <div class="flex items-center justify-between text-xs text-zinc-400 pt-2 border-t border-zinc-800/50">
      <span>{{ (item.uri?.source == "tunein" || item.uri?.source == "radiobrowser") ? 'Live Stream' : 'Audio Track' }}</span>
      <button @click="emit('play', item)" class="text-indigo-400 hover:text-indigo-300 font-medium flex items-center space-x-1">
        <span>Play Now</span>
      </button>
    </div>
  </div>
</template>
