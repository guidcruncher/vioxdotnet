<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useClientConfig } from '@/composables/useClientConfig'
import { useApiClient } from '@/composables/useApiClient'
import type { Sources } from '@/types/api'

const { config, isLoading, error, fetchConfig } = useClientConfig()
const api = useApiClient()

const defaultCountry = ref<string>('')
const tuneInRegion = ref<string>('')

const menuItems = ref<Sources | null>(null)

const props = defineProps<{
  isMobileOpen?: boolean
}>()

const emit = defineEmits<{
  (e: 'close-sidebar'): void
}>()

const STORAGE_KEY = 'sidebar_collapsed'
const isCollapsed = ref(false)
const isMobile = ref(false)

function checkMobile() {
  isMobile.value = window.innerWidth < 768
}

onMounted(async () => {
  checkMobile()
  const config = await fetchConfig()
  menuItems.value = await api.library.getSourceProps()

  if (config) {
    defaultCountry.value = config.defaultCountry
    tuneInRegion.value = config.tuneInRegion
  }

  window.addEventListener('resize', checkMobile)
  try {
    const savedState = localStorage.getItem(STORAGE_KEY)
    if (savedState !== null) {
      isCollapsed.value = JSON.parse(savedState)
    }
  } catch (error) {
    console.error('Failed to read sidebar state from localStorage:', error)
  }
})

onUnmounted(() => {
  window.removeEventListener('resize', checkMobile)
})

const toggleSidebar = () => {
  if (isMobile.value) {
    emit('close-sidebar')
  } else {
    isCollapsed.value = !isCollapsed.value
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(isCollapsed.value))
    } catch (error) {
      console.error('Failed to save sidebar state to localStorage:', error)
    }
  }
}

const getUrl = (source: string, item: Record<string, string>) => {
  let url = ''

  if (item['Url'] && item['Url'] != '') {
    url = item['Url']
  } else {
    url = `/library/${source}`
  }

  url = url.replace(':defaultCountry', defaultCountry.value)
  url = url.replace(':tuneInRegion', tuneInRegion.value)
  return url
}

const getIcon = (source: string, item: Record<string, string>) => {
  let icon = ''

  if (item['Icon'] && item['Icon'] != '') {
    icon = item['icon']
  } else {
    icon = `/${source}.png`
  }

  return icon
}

const closeMobileMenu = () => {
  emit('close-sidebar')
}
</script>

<template>
  <!-- Mobile Backdrop Overlay -->
  <div
    v-if="isMobileOpen"
    @click="closeMobileMenu"
    class="md:hidden fixed inset-0 bg-slate-950/80 backdrop-blur-sm z-40 transition-opacity"
  ></div>

  <!-- Responsive Sidebar Drawer / Rail -->
  <aside
    :class="[
      'border-r border-slate-800 bg-slate-900/90 backdrop-blur-md p-4 space-y-6 flex flex-col transition-all duration-300 ease-in-out z-40 shrink-0',
      /* Enable Vertical Scrollbar When Overflowing */
      'overflow-y-auto max-h-full overscroll-contain scrollbar-thin scrollbar-thumb-slate-700',
      /* Mobile layout mechanics */
      isMobileOpen ? 'fixed inset-y-0 left-0 w-64 z-50' : 'w-16 md:w-auto',
      /* Desktop static layout mechanics */
      !isMobileOpen && isCollapsed ? 'md:w-16' : '',
      !isMobileOpen && !isCollapsed ? 'md:w-64' : '',
    ]"
  >
    <!-- Sidebar Header -->
    <div class="flex items-center justify-between min-h-[32px] shrink-0">
      <span
        v-if="isMobileOpen || (!isCollapsed && !isMobile)"
        class="text-xs font-semibold text-slate-400 uppercase tracking-wider truncate"
      >
        Navigation
      </span>
      <button
        @click="toggleSidebar"
        type="button"
        class="p-2 sm:p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition focus:outline-none"
        :class="{ 'mx-auto': (isCollapsed && !isMobile) || (!isMobileOpen && isMobile) }"
        :title="isCollapsed ? 'Expand Sidebar' : 'Collapse Sidebar'"
      >
        <svg
          class="w-5 h-5 transition-transform duration-300"
          :class="{ 'rotate-180': (isCollapsed && !isMobile) || (!isMobileOpen && isMobile) }"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M11 19l-7-7 7-7m8 14l-7-7 7-7"
          />
        </svg>
      </button>
    </div>

    <!-- Group 1: Sources & Media -->
    <div class="space-y-2">
      <nav class="space-y-1">
        <router-link
          to="/"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'Now Playing' : ''"
        >
          <svg class="w-4 h-4 text-emerald-400 shrink-0" fill="currentColor" viewBox="0 0 24 24">
            <path d="M8 5v14l11-7z" />
          </svg>
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate"
            >Now Playing</span
          >
        </router-link>

        <template v-if="menuItems">
          <router-link
            v-for="(item, key) in menuItems"
            :key="key"
            :to="getUrl(key, item)"
            @click="closeMobileMenu"
            active-class="bg-slate-800 text-white font-medium"
            class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
            :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
            :title="!isMobileOpen && (isCollapsed || isMobile) ? 'Spotify Library' : ''"
          >
            <img :src="getIcon(key, item)" class="w-4 h-4" />
            <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate">
              {{ item.Title }}
            </span>
          </router-link>
        </template>
      </nav>
    </div>

    <!-- Group 2: Engines & Protocols -->
    <div class="space-y-2">
      <nav class="space-y-1">
        <router-link
          to="/equalizer"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'Librespot Controls' : ''"
        >
          <svg class="w-4 h-4 text-emerald-400 shrink-0" fill="currentColor" viewBox="0 0 24 24">
            <path
              d="M4 19h2v-5H4v5zm4 0h2V5H8v14zm4 0h2v-8h-2v8zm4 0h2V9h-2v10zm4 0h2v-11h-2v11z"
            />
          </svg>
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate">Equalizer</span>
        </router-link>

        <router-link
          to="/snapcast"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'Multi-Room (Snapcast)' : ''"
        >
          <svg
            class="w-4 h-4 text-indigo-400 shrink-0"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
            />
          </svg>
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate"
            >Multi-Room (Snapcast)</span
          >
        </router-link>

        <router-link
          to="/config"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'Librespot Controls' : ''"
        >
          <svg class="w-4 h-4 text-emerald-400 shrink-0" fill="currentColor" viewBox="0 0 24 24">
            <path
              d="M12 15.5a3.5 3.5 0 100-7 3.5 3.5 0 000 7zm7.43-2.53l1.86-1.08a.5.5 0 00.18-.68l-1.86-3.23a.5.5 0 00-.63-.22l-2.07.83a7.35 7.35 0 00-1.8-1.04l-.32-2.2A.5.5 0 0014.3 5h-3.72a.5.5 0 00-.49.43l-.32 2.2a7.35 7.35 0 00-1.8 1.04l-2.07-.83a.5.5 0 00-.63.22L3.41 11.2a.5.5 0 00.18.68l1.86 1.08c-.06.35-.1.7-.1 1.04s.04.69.1 1.04l-1.86 1.08a.5.5 0 00-.18.68l1.86 3.23a.5.5 0 00.63.22l2.07-.83c.56.42 1.17.77 1.8 1.04l.32 2.2a.5.5 0 00.49.43h3.72a.5.5 0 00.49-.43l.32-2.2c.63-.27 1.24-.62 1.8-1.04l2.07.83a.5.5 0 00.63-.22l1.86-3.23a.5.5 0 00-.18-.68l-1.86-1.08c.06-.35.1-.7.1-1.04s-.04-.69-.1-1.04z"
            />
          </svg>
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate"
            >Configuration</span
          >
        </router-link>
      </nav>
    </div>
  </aside>
</template>
