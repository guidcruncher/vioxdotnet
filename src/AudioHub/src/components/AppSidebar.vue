<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useClientConfig } from '@/composables/useClientConfig'
import { useApiClient } from '@/composables/useApiClient'
import type { Sources } from '@/types'
import { Home, Settings, AudioLines, ListMusic, ArrowLeftFromLine } from '@lucide/vue'

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

  if (item['url'] && item['url'] != '') {
    url = item['url']
  } else {
    url = `/library/${source}`
  }

  url = url.replace(':defaultCountry', defaultCountry.value)
  url = url.replace(':tuneInRegion', tuneInRegion.value)
  return url
}

const getIcon = (source: string, item: Record<string, string>) => {
  let icon = ''

  if (item['icon'] && item['icon'] != '') {
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
        <ArrowLeftFromLine
          class="w-5 h-5 transition-transform duration-300"
          :class="{ 'rotate-180': (isCollapsed && !isMobile) || (!isMobileOpen && isMobile) }"
        />
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
          <Home class="w-4 h-4 text-emerald-400 shrink-0" />
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
            :title="!isMobileOpen && (isCollapsed || isMobile) ? item.title : ''"
          >
            <img :src="getIcon(key, item)" class="w-4 h-4" />
            <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate">
              {{ item.title }}
            </span>
          </router-link>
        </template>
      </nav>
    </div>

    <!-- Group 2: Engines & Protocols -->
    <div class="space-y-2">
      <nav class="space-y-1">
        <router-link
          to="/playlists"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'Playlists' : ''"
        >
          <ListMusic class="w-4 h-4 text-emerald-400 shrink-0" />
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate">Playlists</span>
        </router-link>

        <router-link
          to="/equalizer"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'Librespot Controls' : ''"
        >
          <AudioLines class="w-4 h-4 text-emerald-400 shrink-0" />
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate">Equalizer</span>
        </router-link>

        <router-link
          to="/config"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'Librespot Controls' : ''"
        >
          <Settings class="w-4 h-4 text-emerald-400 shrink-0" />
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate"
            >Configuration</span
          >
        </router-link>
      </nav>
    </div>
  </aside>
</template>
