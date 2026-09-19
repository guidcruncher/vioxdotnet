<script setup lang="ts">
import { ref, onMounted } from 'vue'

const STORAGE_KEY = 'sidebar_collapsed'
const isCollapsed = ref(false)

onMounted(() => {
  try {
    const savedState = localStorage.getItem(STORAGE_KEY)
    if (savedState !== null) {
      isCollapsed.value = JSON.parse(savedState)
    }
  } catch (error) {
    console.error('Failed to read sidebar state from localStorage:', error)
  }
})

const toggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(isCollapsed.value))
  } catch (error) {
    console.error('Failed to save sidebar state to localStorage:', error)
  }
}
</script>

<template>
  <aside
    :class="[
      'border-r border-slate-800 bg-slate-900/30 p-4 space-y-6 flex-shrink-0 flex flex-col transition-all duration-300 ease-in-out',
      isCollapsed ? 'w-16' : 'w-64',
    ]"
  >
    <div class="flex items-center justify-between">
      <span
        v-if="!isCollapsed"
        class="text-xs font-semibold text-slate-400 uppercase tracking-wider truncate"
      >
        Navigation
      </span>
      <button
        @click="toggleSidebar"
        type="button"
        class="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition focus:outline-none"
        :class="{ 'mx-auto': isCollapsed }"
        :title="isCollapsed ? 'Expand Sidebar' : 'Collapse Sidebar'"
      >
        <svg
          class="w-5 h-5 transition-transform duration-300"
          :class="{ 'rotate-180': isCollapsed }"
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

    <div>
      <p
        v-if="!isCollapsed"
        class="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2"
      >
        Sources & Media
      </p>
      <nav class="space-y-1">
        <router-link
          to="/spotify"
          active-class="bg-slate-800 text-white"
          class="w-full text-left px-3 py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-2.5 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': isCollapsed }"
          :title="isCollapsed ? 'Spotify Library' : ''"
        >
          <svg
            class="w-4 h-4 text-emerald-400 flex-shrink-0"
            fill="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              d="M12 0C5.376 0 0 5.376 0 12s5.376 12 12 12 12-5.376 12-12S18.624 0 12 0zm5.521 17.341c-.218.359-.696.475-1.055.257-2.887-1.764-6.521-2.164-10.803-1.185-.413.094-.813-.166-.907-.579-.094-.413.166-.813.579-.907 4.686-1.07 8.696-.619 11.929 1.359.359.218.475.696.257 1.055zm1.474-3.278c-.274.446-.858.59-1.304.316-3.302-2.029-8.336-2.617-12.244-1.43-.501.152-1.03-.132-1.182-.633-.152-.501.132-1.03.633-1.182 4.464-1.354 10.012-.7 13.781 1.62.446.274.59.858.316 1.304zm.156-3.415C15.222 8.36 8.783 8.147 5.093 9.267c-.612.186-1.258-.168-1.444-.78-.186-.612.168-1.258.78-1.444 4.248-1.289 11.352-1.042 15.939 1.683.551.328.736 1.037.408 1.588-.328.55-1.037.736-1.588.408z"
            />
          </svg>
          <span v-if="!isCollapsed" class="truncate">Spotify Library</span>
        </router-link>
        <router-link
          to="/radio"
          active-class="bg-slate-800 text-white"
          class="w-full text-left px-3 py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-2.5 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': isCollapsed }"
          :title="isCollapsed ? 'Radio Browser' : ''"
        >
          <svg
            class="w-4 h-4 text-sky-400 flex-shrink-0"
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
          <span v-if="!isCollapsed" class="truncate">Radio Browser</span>
        </router-link>
        <router-link
          to="/podverse"
          active-class="bg-slate-800 text-white"
          class="w-full text-left px-3 py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-2.5 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': isCollapsed }"
          :title="isCollapsed ? 'Podverse' : ''"
        >
          <svg
            class="w-4 h-4 text-purple-400 flex-shrink-0"
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
          <span v-if="!isCollapsed" class="truncate">Podverse</span>
        </router-link>
        <router-link
          to="/tunein"
          active-class="bg-slate-800 text-white"
          class="w-full text-left px-3 py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-2.5 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': isCollapsed }"
          :title="isCollapsed ? 'TuneIn' : ''"
        >
          <svg
            class="w-4 h-4 text-amber-400 flex-shrink-0"
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
          <span v-if="!isCollapsed" class="truncate">TuneIn</span>
        </router-link>
      </nav>
    </div>

    <div>
      <p
        v-if="!isCollapsed"
        class="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2"
      >
        Engines & Protocols
      </p>
      <nav class="space-y-1">
        <router-link
          to="/snapcast"
          active-class="bg-slate-800 text-white"
          class="w-full text-left px-3 py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-2.5 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': isCollapsed }"
          :title="isCollapsed ? 'Multi-Room (Snapcast)' : ''"
        >
          <svg
            class="w-4 h-4 text-indigo-400 flex-shrink-0"
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
          <span v-if="!isCollapsed" class="truncate">Multi-Room (Snapcast)</span>
        </router-link>
        <router-link
          to="/mpd"
          active-class="bg-slate-800 text-white"
          class="w-full text-left px-3 py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-2.5 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': isCollapsed }"
          :title="isCollapsed ? 'MPD Backend' : ''"
        >
          <svg
            class="w-4 h-4 text-rose-400 flex-shrink-0"
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
          <span v-if="!isCollapsed" class="truncate">MPD Backend</span>
        </router-link>
        <router-link
          to="/librespot"
          active-class="bg-slate-800 text-white"
          class="w-full text-left px-3 py-2 rounded-lg hover:bg-slate-800/60 transition flex items-center space-x-2.5 text-sm text-slate-300"
          :class="{ 'justify-center space-x-0 px-0': isCollapsed }"
          :title="isCollapsed ? 'Librespot Controls' : ''"
        >
          <svg
            class="w-4 h-4 text-emerald-400 flex-shrink-0"
            fill="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              d="M12 0C5.376 0 0 5.376 0 12s5.376 12 12 12 12-5.376 12-12S18.624 0 12 0zm5.521 17.341c-.218.359-.696.475-1.055.257-2.887-1.764-6.521-2.164-10.803-1.185-.413.094-.813-.166-.907-.579-.094-.413.166-.813.579-.907 4.686-1.07 8.696-.619 11.929 1.359.359.218.475.696.257 1.055zm1.474-3.278c-.274.446-.858.59-1.304.316-3.302-2.029-8.336-2.617-12.244-1.43-.501.152-1.03-.132-1.182-.633-.152-.501.132-1.03.633-1.182 4.464-1.354 10.012-.7 13.781 1.62.446.274.59.858.316 1.304zm.156-3.415C15.222 8.36 8.783 8.147 5.093 9.267c-.612.186-1.258-.168-1.444-.78-.186-.612.168-1.258.78-1.444 4.248-1.289 11.352-1.042 15.939 1.683.551.328.736 1.037.408 1.588-.328.55-1.037.736-1.588.408z"
            />
          </svg>
          <span v-if="!isCollapsed" class="truncate">Librespot Controls</span>
        </router-link>
      </nav>
    </div>
  </aside>
</template>
