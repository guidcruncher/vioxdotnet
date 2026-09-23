<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const emit = defineEmits<{
  (e: 'toggle-sidebar'): void
}>()

const searchQuery = ref('')
const router = useRouter()

function onSearch() {
  if (searchQuery.value.trim()) {
    router.push({ path: '/search', query: { q: searchQuery.value } })
  }
}

function authSpotify() {
  window.location.href = '/api/v1/auth/login'
}

function handleToggleSidebar() {
  emit('toggle-sidebar')
}
</script>

<template>
  <header
    class="border-b border-slate-800 bg-slate-900/80 backdrop-blur-md sticky top-0 z-40 px-3.5 sm:px-6 py-3 sm:py-4 flex items-center justify-between gap-2 sm:gap-4"
  >
    <!-- Brand / Logo Section with Mobile Menu Toggle -->
    <div class="flex items-center space-x-2.5 sm:space-x-3 shrink-0">
      <!-- Mobile Sidebar Toggle Button -->
      <button
        @click="handleToggleSidebar"
        type="button"
        class="md:hidden p-1.5 text-slate-400 hover:text-white hover:bg-slate-800 rounded-lg transition focus:outline-none focus:ring-2 focus:ring-indigo-500/50"
        aria-label="Toggle navigation drawer"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M4 6h16M4 12h16M4 18h16"
          />
        </svg>
      </button>

      <div class="h-8 w-8 sm:h-9 sm:w-9 rounded-xl bg-indigo-600 flex items-center justify-center">
        <img
          src="/icon-sm.svg"
          alt="Music Centre"
          class="h-5 w-5 sm:h-[1.35rem] sm:w-[1.35rem] object-contain"
        />
      </div>
      <h1
        class="hidden md:inline text-lg sm:text-xl font-bold tracking-tight bg-gradient-to-r from-white to-slate-400 bg-clip-text text-transparent"
      >
        Viox Control Center
      </h1>
    </div>

    <!-- Search Input Bar -->
    <div class="flex-1 max-w-xs sm:max-w-md mx-1 sm:mx-4 md:mx-8">
      <form @submit.prevent="onSearch" class="relative w-full">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search media..."
          class="w-full bg-slate-800/90 border border-slate-700/60 rounded-full py-1.5 sm:py-2 pl-9 sm:pl-10 pr-4 text-xs sm:text-sm text-slate-100 placeholder-slate-400 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition"
        />
        <svg
          class="w-4 h-4 text-slate-400 absolute left-3 top-2.5 sm:top-3"
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

    <!-- Actions / Auth Button -->
    <div class="flex items-center shrink-0">
      <button
        @click="authSpotify"
        class="px-3 py-1.5 text-xs font-semibold rounded-full bg-emerald-600/20 hover:bg-emerald-600/30 text-emerald-400 border border-emerald-500/30 transition flex items-center space-x-1.5 active:scale-95"
        title="Connect Spotify"
      >
        <svg class="w-3.5 h-3.5 fill-current" viewBox="0 0 24 24">
          <path
            d="M12 0C5.376 0 0 5.376 0 12s5.376 12 12 12 12-5.376 12-12S18.624 0 12 0zm5.521 17.34c-.24.359-.66.48-1.021.24-2.82-1.74-6.36-2.101-10.561-1.141-.418.122-.779-.179-.899-.539-.12-.421.18-.78.54-.899 4.56-1.021 8.52-.6 11.64 1.32.42.18.48.66.301 1.019zm1.44-3.3c-.301.42-.841.6-1.262.3-3.239-1.98-8.159-2.58-11.939-1.38-.479.12-1.02-.12-1.14-.6-.12-.48.12-1.021.6-1.141 C9.6 9.9 15 10.561 18.72 12.84c.361.181.54.78.241 1.2zm.12-3.36C15.24 8.4 8.82 8.16 5.16 9.301c-.6.18-1.2-.18-1.38-.72-.18-.6.18-1.2.72-1.38 4.26-1.26 11.28-1.02 15.721 1.621.539.3.719 1.02.419 1.56-.299.421-1.02.599-1.559.3z"
          />
        </svg>
        <span class="hidden sm:inline">Spotify</span>
      </button>
    </div>
  </header>
</template>
