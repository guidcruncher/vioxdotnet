<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useClientConfig } from '@/composables/useClientConfig'
import { useApiClient } from '@/composables/useApiClient'
import type { Sources } from '@/types'
import { Clock, Home, Settings, AudioLines, ListMusic, ArrowLeftFromLine } from '@lucide/vue'

const { fetchConfig } = useClientConfig()
const api = useApiClient()

const defaultCountry = ref('')
const tuneInRegion = ref('')
const menuItems = ref<Sources | null>(null)

const STORAGE_KEY = 'sidebar_collapsed'
const isCollapsed = ref(false)
const isMobile = ref(false)

function checkMobile() {
  isMobile.value = window.innerWidth < 768
}

onMounted(async () => {
  checkMobile()
  window.addEventListener('resize', checkMobile)

  const cfg = await fetchConfig()
  menuItems.value = await api.library.getSourceProps()

  if (cfg) {
    defaultCountry.value = cfg.defaultCountry
    tuneInRegion.value = cfg.tuneInRegion
  }

  const saved = localStorage.getItem(STORAGE_KEY)
  if (saved !== null) {
    isCollapsed.value = JSON.parse(saved)
  } else {
    // Mobile starts collapsed
    isCollapsed.value = isMobile.value
  }
})

onUnmounted(() => {
  window.removeEventListener('resize', checkMobile)
})

function toggleSidebar() {
  isCollapsed.value = !isCollapsed.value
  localStorage.setItem(STORAGE_KEY, JSON.stringify(isCollapsed.value))
}

function getUrl(source: string, item: Record<string, string>) {
  const base = item.url || `/library/${source}`
  return base
    .replace(':defaultCountry', defaultCountry.value)
    .replace(':tuneInRegion', tuneInRegion.value)
}

function getIcon(source: string, item: Record<string, string>) {
  return item.icon || `/${source}.png`
}
</script>

<template>
  <aside
    class="bg-slate-900/90 backdrop-blur-md border-r border-slate-800
           flex flex-col overflow-hidden transition-all duration-200 ease-in-out"
    :class="isCollapsed ? 'w-16' : 'w-64'"
  >
    <!-- Header -->
    <div class="flex items-center justify-between p-4">
      <span
        v-if="!isCollapsed"
        class="text-xs font-semibold text-slate-400 uppercase tracking-wider"
      >
        Navigation
      </span>

      <button
        @click="toggleSidebar"
        class="p-2 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition"
      >
        <ArrowLeftFromLine
          class="w-5 h-5 transition-transform duration-300"
          :class="{ 'rotate-180': isCollapsed }"
        />
      </button>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 px-2 space-y-1 overflow-y-auto">
      <!-- Home -->
      <router-link
        to="/"
        class="flex items-center px-3 py-2 rounded-lg hover:bg-slate-800/60 transition text-slate-300"
        :class="{ 'justify-center': isCollapsed }"
      >
        <div class="w-6 flex justify-center">
          <Home class="w-4 h-4 text-emerald-400" />
        </div>

        <span v-if="!isCollapsed" class="ml-3 truncate">Now Playing</span>
      </router-link>

      <!-- Dynamic items -->
      <router-link
        v-for="(item, key) in menuItems"
        :key="key"
        :to="getUrl(key, item)"
        class="flex items-center px-3 py-2 rounded-lg hover:bg-slate-800/60 transition text-slate-300"
        :class="{ 'justify-center': isCollapsed }"
      >
        <div class="w-6 flex justify-center">
          <img
            :src="getIcon(key, item)"
            class="w-4 h-4 object-contain flex-shrink-0"
          />
        </div>

        <span v-if="!isCollapsed" class="ml-3 truncate">{{ item.title }}</span>
      </router-link>

      <!-- Static items -->
      <router-link
        to="/playlists"
        class="flex items-center px-3 py-2 rounded-lg hover:bg-slate-800/60 transition text-slate-300"
        :class="{ 'justify-center': isCollapsed }"
      >
        <div class="w-6 flex justify-center">
          <ListMusic class="w-4 h-4 text-emerald-400" />
        </div>
        <span v-if="!isCollapsed" class="ml-3 truncate">Playlists</span>
      </router-link>

      <router-link
        to="/equalizer"
        class="flex items-center px-3 py-2 rounded-lg hover:bg-slate-800/60 transition text-slate-300"
        :class="{ 'justify-center': isCollapsed }"
      >
        <div class="w-6 flex justify-center">
          <AudioLines class="w-4 h-4 text-emerald-400" />
        </div>
        <span v-if="!isCollapsed" class="ml-3 truncate">Equalizer</span>
      </router-link>

      <router-link
        to="/clock"
        class="flex items-center px-3 py-2 rounded-lg hover:bg-slate-800/60 transition text-slate-300"
        :class="{ 'justify-center': isCollapsed }"
      >
        <div class="w-6 flex justify-center">
          <Clock class="w-4 h-4 text-emerald-400" />
        </div>
        <span v-if="!isCollapsed" class="ml-3 truncate">Clock</span>
      </router-link>

      <router-link
        to="/config"
        class="flex items-center px-3 py-2 rounded-lg hover:bg-slate-800/60 transition text-slate-300"
        :class="{ 'justify-center': isCollapsed }"
      >
        <div class="w-6 flex justify-center">
          <Settings class="w-4 h-4 text-emerald-400" />
        </div>
        <span v-if="!isCollapsed" class="ml-3 truncate">Configuration</span>
      </router-link>
    </nav>
  </aside>
</template>
