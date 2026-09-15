<script setup lang="ts">
import { RouterLink } from 'vue-router';

defineProps<{
  isMobileOpen: boolean;
}>();

const emit = defineEmits<{
  (e: 'closeMenu'): void;
  (e: 'fetchRadio'): void;
}>();
</script>

<template>
  <div>
    <!-- Mobile Drawer Overlay -->
    <div
      v-if="isMobileOpen"
      @click="emit('closeMenu')"
      class="fixed inset-0 bg-black/70 z-40 md:hidden transition-opacity"
    ></div>

    <!-- Sidebar Container -->
    <aside
      :class="[
        'fixed inset-y-0 left-0 z-50 w-64 bg-zinc-900 border-r border-zinc-800 flex flex-col transition-transform duration-300 ease-in-out md:static md:translate-x-0',
        isMobileOpen ? 'translate-x-0' : '-translate-x-full'
      ]"
    >
      <!-- Brand Logo -->
      <div class="p-6 border-b border-zinc-800 flex items-center justify-between">
        <div class="flex items-center space-x-3">
          <div class="w-8 h-8 rounded-lg bg-indigo-600 flex items-center justify-center font-bold text-white text-lg">
            A
          </div>
          <span class="font-bold text-lg tracking-wide text-white">AudioHub</span>
        </div>
        <button @click="emit('closeMenu')" class="md:hidden text-zinc-400 hover:text-white">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <!-- Navigation Links -->
      <nav class="flex-1 overflow-y-auto px-4 py-6 space-y-6">
        <div>
          <h3 class="px-3 text-xs font-semibold text-zinc-400 uppercase tracking-wider mb-2">Menu</h3>
          <div class="space-y-1">
            <RouterLink
              to="/"
              @click="emit('closeMenu')"
              class="flex items-center space-x-3 px-3 py-2 rounded-lg text-sm font-medium hover:bg-zinc-800 transition text-white"
              active-class="bg-zinc-800 text-indigo-400"
            >
              <svg class="w-5 h-5 text-indigo-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
              </svg>
              <span>Discover All</span>
            </RouterLink>
          </div>
        </div>

        <!-- Platform Routes -->
        <div>
          <h3 class="px-3 text-xs font-semibold text-zinc-400 uppercase tracking-wider mb-2">Connected Platforms</h3>
          <div class="space-y-1">
            <RouterLink
              to="/platform/Podverse"
              @click="emit('closeMenu')"
              class="flex items-center justify-between px-3 py-2 rounded-lg text-sm font-medium text-zinc-400 hover:text-white hover:bg-zinc-800/60 transition"
              active-class="bg-zinc-800 text-white"
            >
              <div class="flex items-center space-x-3">
                <span class="w-2 h-2 rounded-full bg-purple-500"></span>
                <span>Podverse</span>
              </div>
              <span class="text-xs bg-purple-900/50 text-purple-300 px-2 py-0.5 rounded border border-purple-700/50">Podcasts</span>
            </RouterLink>

            <RouterLink
              to="/platform/Spotify"
              @click="emit('closeMenu')"
              class="flex items-center justify-between px-3 py-2 rounded-lg text-sm font-medium text-zinc-400 hover:text-white hover:bg-zinc-800/60 transition"
              active-class="bg-zinc-800 text-white"
            >
              <div class="flex items-center space-x-3">
                <span class="w-2 h-2 rounded-full bg-emerald-500"></span>
                <span>Spotify</span>
              </div>
              <span class="text-xs bg-emerald-900/50 text-emerald-300 px-2 py-0.5 rounded border border-emerald-700/50">Music</span>
            </RouterLink>

            <RouterLink
              to="/platform/RadioBrowser"
              @click="emit('closeMenu')"
              class="flex items-center justify-between px-3 py-2 rounded-lg text-sm font-medium text-zinc-400 hover:text-white hover:bg-zinc-800/60 transition"
              active-class="bg-zinc-800 text-white"
            >
              <div class="flex items-center space-x-3">
                <span class="w-2 h-2 rounded-full bg-sky-500"></span>
                <span>RadioBrowser</span>
              </div>
              <span class="text-xs bg-sky-900/50 text-sky-300 px-2 py-0.5 rounded border border-sky-700/50">Web Radio</span>
            </RouterLink>

            <RouterLink
              to="/platform/TuneIn"
              @click="emit('closeMenu')"
              class="flex items-center justify-between px-3 py-2 rounded-lg text-sm font-medium text-zinc-400 hover:text-white hover:bg-zinc-800/60 transition"
              active-class="bg-zinc-800 text-white"
            >
              <div class="flex items-center space-x-3">
                <span class="w-2 h-2 rounded-full bg-amber-500"></span>
                <span>TuneIn</span>
              </div>
              <span class="text-xs bg-amber-900/50 text-amber-300 px-2 py-0.5 rounded border border-amber-700/50">Live</span>
            </RouterLink>
          </div>
        </div>

        <div>
          <h3 class="px-3 text-xs font-semibold text-zinc-400 uppercase tracking-wider mb-2">Actions</h3>
          <button
            @click="emit('fetchRadio')"
            class="w-full flex items-center space-x-3 px-3 py-2 rounded-lg text-sm font-medium text-sky-400 bg-sky-950/40 border border-sky-800/40 hover:bg-sky-900/40 transition"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            <span>Fetch Radio API</span>
          </button>
        </div>
      </nav>

      <div class="p-4 border-t border-zinc-800 bg-zinc-900/50">
        <div class="flex items-center space-x-2">
          <span class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
          <span class="text-xs text-zinc-400 font-medium">Vue 3 Engine Active</span>
        </div>
      </div>
    </aside>
  </div>
</template>
