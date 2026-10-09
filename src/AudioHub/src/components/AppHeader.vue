<script setup lang="ts">
import { TextAlignJustify, Search } from '@lucide/vue'
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
        <TextAlignJustify class="w-5 h-5" />
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
        AudioHub
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
        <Search class="w-4 h-4 text-slate-400 absolute left-3 top-2.5 sm:top-3" />
      </form>
    </div>

    <!-- Actions / Auth Button -->
    <div class="flex items-center shrink-0"></div>
  </header>
</template>
