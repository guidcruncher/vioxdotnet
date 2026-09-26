<script setup lang="ts">
import { ref } from 'vue'
import { useApiClient } from '@/composables/useApiClient'

const api = useApiClient()
const streamUrl = ref('')

async function playMpdStream() {
  if (streamUrl.value.trim()) {
    await api.mpd.playFileOrUrl({ fileOrUrl: streamUrl.value.trim() })
  }
}
</script>

<template>
  <div
    class="w-full min-h-full flex flex-col items-center justify-center p-4 sm:p-6 lg:p-8 overscroll-none touch-none select-none text-slate-100"
  >
    <div class="w-full max-w-lg">
      <h2 class="text-lg sm:text-xl font-bold mb-3 sm:mb-4 text-white">MPD Engine Control</h2>

      <div class="bg-slate-900 border border-slate-800 rounded-2xl p-4 sm:p-6 space-y-4 shadow-2xl">
        <div>
          <label class="block text-xs sm:text-sm font-medium mb-2 text-slate-300">
            Direct Audio Stream / File Path
          </label>
          <input
            v-model="streamUrl"
            type="text"
            placeholder="http://stream.example.com/live.mp3"
            class="w-full bg-slate-800 border border-slate-700 rounded-lg p-2.5 text-xs sm:text-sm text-slate-100 focus:outline-none focus:border-indigo-500 transition"
          />
        </div>

        <button
          @click="playMpdStream"
          class="w-full bg-indigo-600 hover:bg-indigo-500 active:scale-95 text-xs sm:text-sm font-semibold py-2.5 rounded-lg text-white shadow-sm shadow-indigo-600/30 transition"
        >
          Send to MPD Engine
        </button>
      </div>
    </div>
  </div>
</template>
