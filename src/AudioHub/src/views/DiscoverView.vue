<script setup lang="ts">
import { computed } from 'vue';
import { AudioItem } from '../types/audio';
import MediaCard from '../components/MediaCard.vue';

const props = defineProps<{
  audioList: AudioItem[];
  searchQuery: string;
  activeFilter: string;
  currentTrack: AudioItem | null;
  setFilter: (source: string) => void;
  playTrack: (item: AudioItem) => void;
}>();

const filteredItems = computed(() => {
  return props.audioList.filter(item => {
    const itemSource = item.uri?.source || '';
    const matchesSource =
      props.activeFilter === 'all' ||
      itemSource.toLowerCase() === props.activeFilter.toLowerCase();

    const query = props.searchQuery.toLowerCase();
    const matchesQuery =
      item.title.toLowerCase().includes(query) ||
      item.artist.toLowerCase().includes(query) ||
      item.album.toLowerCase().includes(query);

    return matchesSource && matchesQuery;
  });
});

const isTrackActive = (item: AudioItem): boolean => {
  if (!props.currentTrack) return false;
  if (props.currentTrack.uri?.id && item.uri?.id) {
    return props.currentTrack.uri.id === item.uri.id;
  }
  return props.currentTrack.url === item.url;
};
</script>

<template>
  <div>
    <!-- Service Filter Pill Tabs -->
    <div class="flex items-center space-x-2 overflow-x-auto pb-4 mb-6 border-b border-zinc-800/60 no-scrollbar">
      <button
        @click="setFilter('all')"
        :class="[
          'px-4 py-1.5 rounded-full text-xs font-semibold transition whitespace-nowrap',
          activeFilter === 'all' ? 'bg-indigo-600 text-white' : 'bg-zinc-800 text-zinc-300 hover:bg-zinc-700'
        ]"
      >
        All Sources
      </button>
      <button
        @click="setFilter('Podverse')"
        :class="[
          'px-4 py-1.5 rounded-full text-xs font-semibold transition whitespace-nowrap',
          activeFilter === 'Podverse' ? 'bg-indigo-600 text-white' : 'bg-zinc-800 text-zinc-300 hover:bg-zinc-700'
        ]"
      >
        Podverse
      </button>
      <button
        @click="setFilter('Spotify')"
        :class="[
          'px-4 py-1.5 rounded-full text-xs font-semibold transition whitespace-nowrap',
          activeFilter === 'Spotify' ? 'bg-indigo-600 text-white' : 'bg-zinc-800 text-zinc-300 hover:bg-zinc-700'
        ]"
      >
        Spotify
      </button>
      <button
        @click="setFilter('RadioBrowser')"
        :class="[
          'px-4 py-1.5 rounded-full text-xs font-semibold transition whitespace-nowrap',
          activeFilter === 'RadioBrowser' ? 'bg-indigo-600 text-white' : 'bg-zinc-800 text-zinc-300 hover:bg-zinc-700'
        ]"
      >
        RadioBrowser
      </button>
      <button
        @click="setFilter('TuneIn')"
        :class="[
          'px-4 py-1.5 rounded-full text-xs font-semibold transition whitespace-nowrap',
          activeFilter === 'TuneIn' ? 'bg-indigo-600 text-white' : 'bg-zinc-800 text-zinc-300 hover:bg-zinc-700'
        ]"
      >
        TuneIn
      </button>
    </div>

    <!-- Hero Section -->
    <div class="relative rounded-2xl bg-gradient-to-r from-indigo-900/60 via-purple-900/40 to-zinc-900 border border-indigo-500/20 p-6 mb-8 overflow-hidden">
      <div class="relative z-10 max-w-2xl">
        <span class="text-xs font-bold uppercase tracking-wider text-indigo-400 mb-1 block">Vue 3 Single Page Application</span>
        <h1 class="text-2xl sm:text-3xl font-extrabold text-white tracking-tight mb-2">Listen across all services in one place</h1>
        <p class="text-sm text-zinc-300 mb-4">Stream live podcasts from Podverse, music tracks from Spotify, global stations from RadioBrowser, and broadcasts from TuneIn instantly.</p>
        <button
          v-if="audioList.length > 0"
          @click="playTrack(audioList[0])"
          class="inline-flex items-center space-x-2 bg-indigo-600 hover:bg-indigo-500 text-white font-semibold text-sm px-4 py-2 rounded-lg shadow-lg transition"
        >
          <svg class="w-4 h-4 fill-current" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
          <span>Play Featured Audio</span>
        </button>
      </div>
    </div>

    <!-- Cards Grid Area -->
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-lg font-bold text-white tracking-wide">
        {{ activeFilter === 'all' ? 'Featured Tracks & Streams' : `${activeFilter} Streams` }}
      </h2>
      <span class="text-xs text-zinc-400 font-medium">{{ filteredItems.length }} items</span>
    </div>

    <div v-if="filteredItems.length > 0" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
      <MediaCard
        v-for="item in filteredItems"
        :key="item.uri?.id || item.url"
        :item="item"
        :isActive="isTrackActive(item)"
        @play="playTrack"
      />
    </div>
    <div v-else class="py-16 text-center text-zinc-500">
      <p class="text-base">No audio streams found matching your selection.</p>
    </div>
  </div>
</template>
