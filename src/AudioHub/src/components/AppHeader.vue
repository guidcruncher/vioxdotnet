<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const searchQuery = ref('')
const router = useRouter()

function onSearch() {
  if (searchQuery.value.trim()) {
    router.push({ path: '/search', query: { q: searchQuery.value } })
  }
}

function authSpotify() {
  window.location.href = '/api/spotify/me'
}
</script>

<template>
  <header
    class="border-b border-slate-800 bg-slate-900/50 backdrop-blur sticky top-0 z-40 px-6 py-4 flex items-center justify-between"
  >
    <div class="flex items-center space-x-3">
      <div
        class="h-8 w-8 rounded-lg bg-indigo-600 flex items-center justify-center font-black text-xl text-white shadow-lg shadow-indigo-500/30"
      >
        V
      </div>
      <h1
        class="text-xl font-bold tracking-tight bg-gradient-to-r from-white to-slate-400 bg-clip-text text-transparent"
      >
        Viox Control Center
      </h1>
    </div>

    <div class="flex-1 max-w-md mx-8">
      <form @submit.prevent="onSearch" class="relative">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search all media (Spotify, Radio, Podverse)..."
          class="w-full bg-slate-800/80 border border-slate-700/60 rounded-full py-2 pl-10 pr-4 text-sm focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition"
        />
        <svg
          class="w-4 h-4 text-slate-400 absolute left-3.5 top-3"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
          />
        </svg>
      </form>
    </div>

    <button
      @click="authSpotify"
      class="hover:text-indigo-400 transition flex items-center gap-2 bg-slate-800 px-3 py-1.5 rounded-full border border-slate-700 text-sm font-medium"
    >
      <span class="w-2 h-2 rounded-full bg-emerald-500"></span>
      Spotify Auth
    </button>
  </header>
</template>
