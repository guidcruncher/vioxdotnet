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
  <div>
    <h2 class="text-xl font-bold mb-4">MPD Engine Control</h2>
    <div class="bg-slate-900 border border-slate-800 rounded-xl p-6 max-w-lg">
      <label class="block text-sm font-medium mb-2">Direct Audio Stream / File Path</label>
      <input
        v-model="streamUrl"
        type="text"
        placeholder="http://stream.example.com/live.mp3"
        class="w-full bg-slate-800 border border-slate-700 rounded-lg p-2.5 text-sm mb-4 focus:outline-none focus:border-indigo-500"
      />
      <button
        @click="playMpdStream"
        class="w-full bg-indigo-600 hover:bg-indigo-500 text-sm font-semibold py-2.5 rounded-lg transition"
      >
        Send to MPD Engine
      </button>
    </div>
  </div>
</template>
