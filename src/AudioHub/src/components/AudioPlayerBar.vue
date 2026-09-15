<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue';
import { AudioItem } from '../types/audio';
import { useMediaPlayerApi } from '../composables/useMediaPlayerApi';

const props = defineProps<{
  currentTrack: AudioItem | null;
  isPlaying: boolean;
}>();

const emit = defineEmits<{
  (e: 'togglePlay'): void;
  (e: 'next'): void;
  (e: 'prev'): void;
}>();

const mediaPlayer = useMediaPlayerApi();

const currentTime = ref(0);
const duration = ref(0);
const volume = ref(0.8);
const isMuted = ref(false);
let progressTimer: ReturnType<typeof setInterval> | null = null;

const formatTime = (seconds: number): string => {
  if (!seconds || isNaN(seconds)) return '0:00';
  const min = Math.floor(seconds / 60);
  const sec = Math.floor(seconds % 60);
  return `${min}:${sec < 10 ? '0' : ''}${sec}`;
};

const startTimer = () => {
  stopTimer();
  progressTimer = setInterval(() => {
    if (props.isPlaying && duration.value > 0 && currentTime.value < duration.value) {
      currentTime.value += 1;
    }
  }, 1000);
};

const stopTimer = () => {
  if (progressTimer) {
    clearInterval(progressTimer);
    progressTimer = null;
  }
};

const handleTogglePlay = async () => {
  try {
    if (props.isPlaying) {
      await mediaPlayer.pause();
    } else {
      await mediaPlayer.resume();
    }
  } catch (err) {
    console.error('Failed to toggle playback state via server API:', err);
  } finally {
    emit('togglePlay');
  }
};

const handleNext = async () => {
  try {
    await mediaPlayer.next();
  } catch (err) {
    console.error('Failed to trigger next track via server API:', err);
  } finally {
    emit('next');
  }
};

const handlePrev = async () => {
  try {
    await mediaPlayer.previous();
  } catch (err) {
    console.error('Failed to trigger previous track via server API:', err);
  } finally {
    emit('prev');
  }
};

const seek = async (event: MouseEvent) => {
  const target = event.currentTarget as HTMLElement;
  if (!target || !duration.value || isNaN(duration.value)) return;
  const rect = target.getBoundingClientRect();
  const clickX = event.clientX - rect.left;
  const percentage = clickX / rect.width;
  const newTime = percentage * duration.value;

  currentTime.value = newTime;

  try {
    await mediaPlayer.seek({ positionMs: Math.floor(newTime * 1000) });
  } catch (err) {
    console.error('Failed to execute seek via server API:', err);
  }
};

const setVolume = async (event: Event) => {
  const target = event.target as HTMLInputElement;
  volume.value = parseFloat(target.value);
  isMuted.value = volume.value === 0;

  try {
    await mediaPlayer.setVolume({ volume: volume.value, muted: isMuted.value });
  } catch (err) {
    console.error('Failed to update volume via server API:', err);
  }
};

const toggleMute = async () => {
  if (isMuted.value) {
    volume.value = 0.8;
    isMuted.value = false;
  } else {
    volume.value = 0;
    isMuted.value = true;
  }

  try {
    await mediaPlayer.setVolume({ volume: volume.value, muted: isMuted.value });
  } catch (err) {
    console.error('Failed to toggle mute state via server API:', err);
  }
};

watch(
  () => props.currentTrack,
  async (newTrack) => {
    currentTime.value = 0;
    duration.value = 0;
    if (newTrack) {
      try {
        await mediaPlayer.play({ uri: newTrack.url });
      } catch (err) {
        console.error('Failed to start server playback for track:', err);
      }
    }
  },
  { immediate: true }
);

watch(
  () => props.isPlaying,
  (playing) => {
    if (playing) {
      startTimer();
    } else {
      stopTimer();
    }
  },
  { immediate: true }
);

onMounted(async () => {
  try {
    const remoteVolume = await mediaPlayer.getVolume();
    if (typeof remoteVolume === 'number') {
      volume.value = remoteVolume;
    }
  } catch (err) {
    console.error('Failed to sync initial volume from server API:', err);
  }
});

onUnmounted(() => {
  stopTimer();
});
</script>

<template>
  <footer class="fixed bottom-0 left-0 right-0 h-24 bg-zinc-900/95 backdrop-blur border-t border-zinc-800 px-4 sm:px-6 flex items-center justify-between z-50">
    <!-- Track Info (Left) -->
    <div class="flex items-center space-x-3 w-1/4 min-w-[160px]">
      <img
        :src="currentTrack?.imageUrl || 'https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?w=150&h=150&fit=crop'"
        class="w-12 h-12 rounded-lg object-cover bg-zinc-800 border border-zinc-700/50 shrink-0"
        alt="Cover"
      />
      <div class="min-w-0">
        <h4 class="text-sm font-semibold text-white truncate">{{ currentTrack?.title || 'Select a track' }}</h4>
        <p class="text-xs text-zinc-400 truncate">{{ currentTrack?.artist || 'No playback active' }}</p>
        <span
          class="inline-block mt-0.5 text-[10px] font-bold px-1.5 py-0.5 rounded border bg-zinc-800 text-zinc-300 border-zinc-700 uppercase"
        >
          {{ currentTrack?.uri?.source || 'Standby' }}
        </span>
      </div>
    </div>

    <!-- Playback Controls (Middle) -->
    <div class="flex flex-col items-center justify-center flex-1 max-w-xl px-2">
      <div class="flex items-center space-x-4 mb-1">
        <button @click="handlePrev" class="text-zinc-400 hover:text-white transition focus:outline-none">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 19l-7-7 7-7m8 14l-7-7 7-7" />
          </svg>
        </button>
        <button
          @click="handleTogglePlay"
          class="w-10 h-10 rounded-full bg-white text-black flex items-center justify-center hover:scale-105 transition focus:outline-none"
        >
          <svg v-if="!isPlaying" class="w-5 h-5 fill-current ml-0.5" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
          <svg v-else class="w-5 h-5 fill-current" viewBox="0 0 24 24"><path d="M6 19h4V5H6v14zm8-14v14h4V5h-4z"/></svg>
        </button>
        <button @click="handleNext" class="text-zinc-400 hover:text-white transition focus:outline-none">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 5l7 7-7 7M6 5l7 7-7 7" />
          </svg>
        </button>
      </div>
      <div class="w-full flex items-center space-x-2">
        <span class="text-[11px] font-mono text-zinc-400 w-9 text-right">
          {{ isNaN(duration) || duration === 0 ? 'LIVE' : formatTime(currentTime) }}
        </span>
        <div class="relative flex-1 group cursor-pointer" @click="seek">
          <div class="w-full h-1 bg-zinc-700 rounded-full overflow-hidden">
            <div
              class="h-full bg-indigo-500 transition-all duration-100"
              :style="{ width: isNaN(duration) || duration === 0 ? '100%' : `${(currentTime / duration) * 100}%` }"
            ></div>
          </div>
        </div>
        <span class="text-[11px] font-mono text-zinc-400 w-9">
          {{ isNaN(duration) || duration === 0 ? 'LIVE' : formatTime(duration) }}
        </span>
      </div>
    </div>

    <!-- Volume Controls (Right) -->
    <div class="flex items-center justify-end space-x-3 w-1/4 min-w-[120px]">
      <button @click="toggleMute" class="text-zinc-400 hover:text-white transition focus:outline-none">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.536 8.464a5 5 0 010 7.072M18.364 5.636a9 9 0 010 12.728M11 5L6 9H2v6h4l5 4V5z" />
        </svg>
      </button>
      <input
        type="range"
        min="0"
        max="1"
        step="0.01"
        :value="volume"
        @input="setVolume"
        class="w-16 sm:w-24 accent-indigo-500 bg-zinc-700 h-1 rounded-lg cursor-pointer"
      />
    </div>
  </footer>
</template>
