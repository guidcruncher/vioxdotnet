<script setup lang="ts">
import { computed } from 'vue';
import { AudioItem } from '../types/audio';
import MediaCard from '../components/MediaCard.vue';

const props = defineProps<{
  name: string;
  audioList: AudioItem[];
  searchQuery: string;
  currentTrack: AudioItem | null;
  playTrack: (item: AudioItem) => void;
}>();

const platformItems = computed(() => {
  return props.audioList.filter(item => {
    const itemSource = item.uri?.source || '';
    const matchesSource = itemSource.toLowerCase() === props.name.toLowerCase();

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
    <div class="mb-6 border-b border-zinc-800/60 pb-4">
      <h1 class="text-2xl font-bold text-white tracking-wide">{{ name }} View</h1>
      <p class="text-xs text-zinc-400">Filtering tracks specifically for platform: {{ name }}</p>
    </div>

    <div v-if="platformItems.length > 0" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
      <MediaCard
        v-for="item in platformItems"
        :key="item.uri?.id || item.url"
        :item="item"
        :isActive="isTrackActive(item)"
        @play="playTrack"
      />
    </div>
    <div v-else class="py-16 text-center text-zinc-500">
      <p class="text-base">No content found for {{ name }}.</p>
    </div>
  </div>
</template>
