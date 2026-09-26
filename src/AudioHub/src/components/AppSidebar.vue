<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useClientConfig } from '@/composables/useClientConfig'
const { config, isLoading, error, fetchConfig } = useClientConfig()

const defaultCountry = ref<string>('')

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

  if (config) {
    defaultCountry.value = config.defaultCountry
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

        <router-link
          to="/spotify"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'Spotify Library' : ''"
        >
          <svg class="w-4 h-4 text-emerald-400 shrink-0" fill="currentColor" viewBox="0 0 24 24">
            <path
              d="M12 0C5.376 0 0 5.376 0 12s5.376 12 12 12 12-5.376 12-12S18.624 0 12 0zm5.521 17.341c-.218.359-.696.475-1.055.257-2.887-1.764-6.521-2.164-10.803-1.185-.413.094-.813-.166-.907-.579-.094-.413.166-.813.579-.907 4.686-1.07 8.696-.619 11.929 1.359.359.218.475.696.257 1.055zm1.474-3.278c-.274.446-.858.59-1.304.316-3.302-2.029-8.336-2.617-12.244-1.43-.501.152-1.03-.132-1.182-.633-.152-.501.132-1.03.633-1.182 4.464-1.354 10.012-.7 13.781 1.62.446.274.59.858.316 1.304zm.156-3.415C15.222 8.36 8.783 8.147 5.093 9.267c-.612.186-1.258-.168-1.444-.78-.186-.612.168-1.258.78-1.444 4.248-1.289 11.352-1.042 15.939 1.683.551.328.736 1.037.408 1.588-.328.55-1.037.736-1.588.408z"
            />
          </svg>
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate"
            >Spotify Library</span
          >
        </router-link>

        <router-link
          :to="`/radio${defaultCountry != '' ? `?id=${defaultCountry}` : ''}`"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'Radio Browser' : ''"
        >
          <svg
            class="w-4 h-4 text-sky-400 shrink-0"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M19 11a7 7 0 01-7 7m0 0a7 7 0 01-7-7m7 7v4m0 0H8m4 0h4m-4-8a3 3 0 100-6 3 3 0 000 6z"
            />
          </svg>
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate"
            >Radio Browser</span
          >
        </router-link>

        <router-link
          to="/podverse"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'Podverse' : ''"
        >
          <svg
            class="w-4 h-4 text-purple-400 shrink-0"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M19 20H5a2 2 0 01-2-2V6a2 2 0 012-2h10a2 2 0 012 2v1m2 13a2 2 0 01-2-2V7m2 13a2 2 0 002-2V9a2 2 0 00-2-2h-2m-4-3H9M7 16h6M7 8h6v4H7V8z"
            />
          </svg>
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate">Podverse</span>
        </router-link>

        <router-link
          :to="`/tunein?id=r101309`"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'TuneIn' : ''"
        >
<svg
  class="w-4 h-4 shrink-0"
  fill="none"
  stroke="#132E63"
  viewBox="0 0 24 24"
  xmlns="http://www.w3.org/2000/svg"
>
  <path
    stroke-linecap="round"
    stroke-linejoin="round"
    stroke-width="2"
    d="M19.467 4.102a.8.8 0 01.996.762v14.272a.8.8 0 01-.8.8 8.411 8.411 0 11-12.924.015.8.8 0 01-.8-.8V4.864a.8.8 0 01.996-.762l1.62.433a.8.8 0 01.603.762v10.455a5.166 5.166 0 106.848.026V5.297a.8.8 0 01.603-.762l1.858-.433z"
  />
</svg>
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate">TuneIn</span>
        </router-link>

        <router-link
          :to="`/playlists`"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'Playlists' : ''"
        >
          <svg
            class="w-4 h-4 text-amber-400 shrink-0"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12 0c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3"
            />
          </svg>
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate">Playlists</span>
        </router-link>
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
          to="/mpd"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'MPD Backend' : ''"
        >
          <svg
            class="w-4 h-4 text-rose-400 shrink-0"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M5 12h14M12 5l7 7-7 7"
            />
          </svg>
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate"
            >MPD Backend</span
          >
        </router-link>

        <router-link
          to="/librespot"
          @click="closeMobileMenu"
          active-class="bg-slate-800 text-white font-medium"
          class="w-full text-left px-3 py-2.5 sm:py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-3 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': !isMobileOpen && (isCollapsed || isMobile) }"
          :title="!isMobileOpen && (isCollapsed || isMobile) ? 'Librespot Controls' : ''"
        >
          <svg class="w-4 h-4 text-emerald-400 shrink-0" fill="currentColor" viewBox="0 0 24 24">
            <path
              d="M12 0C5.376 0 0 5.376 0 12s5.376 12 12 12 12-5.376 12-12S18.624 0 12 0zm5.521 17.341c-.218.359-.696.475-1.055.257-2.887-1.764-6.521-2.164-10.803-1.185-.413.094-.813-.166-.907-.579-.094-.413.166-.813.579-.907 4.686-1.07 8.696-.619 11.929 1.359.359.218.475.696.257 1.055zm1.474-3.278c-.274.446-.858.59-1.304.316-3.302-2.029-8.336-2.617-12.244-1.43-.501.152-1.03-.132-1.182-.633-.152-.501.132-1.03.633-1.182 4.464-1.354 10.012-.7 13.781 1.62.446.274.59.858.316 1.304zm.156-3.415C15.222 8.36 8.783 8.147 5.093 9.267c-.612.186-1.258-.168-1.444-.78-.186-.612.168-1.258.78-1.444 4.248-1.289 11.352-1.042 15.939 1.683.551.328.736 1.037.408 1.588-.328.55-1.037.736-1.588.408z"
            />
          </svg>
          <span v-if="isMobileOpen || (!isCollapsed && !isMobile)" class="truncate"
            >Librespot Controls</span
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
