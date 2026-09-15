<script setup lang="ts">
import { ref } from 'vue';
import AppSidebar from '../components/AppSidebar.vue';
import AppHeader from '../components/AppHeader.vue';
import AudioPlayerBar from '../components/AudioPlayerBar.vue';
import { AudioItem } from '../types/audio';
import { useRadioBrowserApi } from '../composables/useRadioBrowserApi';

const isMobileMenuOpen = ref(false);
const searchQuery = ref('');
const activeSourceFilter = ref('all');
const audioList = ref<AudioItem[]>([]);
const currentTrack = ref<AudioItem | null>(null);
const isPlaying = ref(false);

const radioBrowserApi = useRadioBrowserApi();

const toggleMobileMenu = () => {
  isMobileMenuOpen.value = !isMobileMenuOpen.value;
};

const setFilter = (source: string) => {
  activeSourceFilter.value = source;
};

const playTrack = (track: AudioItem) => {
  currentTrack.value = track;
  isPlaying.value = true;
};

const togglePlay = () => {
  if (!currentTrack.value && audioList.value.length > 0) {
    playTrack(audioList.value[0]);
    return;
  }
  isPlaying.value = !isPlaying.value;
};

const getTrackIndex = (track: AudioItem | null): number => {
  if (!track) return -1;
  return audioList.value.findIndex(item => {
    if (track.uri?.id && item.uri?.id) {
      return track.uri.id === item.uri.id;
    }
    return track.url === item.url;
  });
};

const nextTrack = () => {
  if (audioList.value.length === 0) return;
  if (!currentTrack.value) {
    playTrack(audioList.value[0]);
    return;
  }
  const index = getTrackIndex(currentTrack.value);
  const nextIdx = index < 0 ? 0 : (index + 1) % audioList.value.length;
  playTrack(audioList.value[nextIdx]);
};

const prevTrack = () => {
  if (audioList.value.length === 0) return;
  if (!currentTrack.value) {
    playTrack(audioList.value[0]);
    return;
  }
  const index = getTrackIndex(currentTrack.value);
  const prevIdx = index < 0 ? 0 : (index - 1 + audioList.value.length) % audioList.value.length;
  playTrack(audioList.value[prevIdx]);
};

const handleFetchRadio = async () => {
  try {
    const stations = await radioBrowserApi.getTopClickedStations({Limit: 12});
    const newStations: AudioItem[] = stations.map(station => ({
      uri: {
        source: 'RadioBrowser',
        type: 'station',
        id: station.stationuuid || station.changeid || station.url || '',
      },
      title: station.name || 'Unknown Station',
      artist: station.country || station.language || 'Radio Broadcast',
      album: station.tags || 'Live Stream',
      url: station.url_resolved || station.url || '',
      imageUrl: station.favicon || 'https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?w=150&h=150&fit=crop',
    }));

    audioList.value = [...newStations, ...audioList.value];
    activeSourceFilter.value = 'RadioBrowser';
  } catch (error) {
    console.error('Failed to load radio stations from API:', error);
  }
};
</script>

<template>
  <div class="flex h-full overflow-hidden relative">
    <AppSidebar
      :isMobileOpen="isMobileMenuOpen"
      @closeMenu="isMobileMenuOpen = false"
      @fetchRadio="handleFetchRadio"
    />

    <main class="flex-1 flex flex-col min-w-0 bg-zinc-950 overflow-hidden">
      <AppHeader
        v-model:searchQuery="searchQuery"
        :activeSource="activeSourceFilter"
        @toggleMenu="toggleMobileMenu"
      />

      <div class="flex-1 overflow-y-auto p-4 sm:p-6 pb-32">
        <slot
          :audioList="audioList"
          :searchQuery="searchQuery"
          :activeFilter="activeSourceFilter"
          :currentTrack="currentTrack"
          :setFilter="setFilter"
          :playTrack="playTrack"
        ></slot>
      </div>
    </main>

    <AudioPlayerBar
      :currentTrack="currentTrack"
      :isPlaying="isPlaying"
      @togglePlay="togglePlay"
      @next="nextTrack"
      @prev="prevTrack"
    />
  </div>
</template>
