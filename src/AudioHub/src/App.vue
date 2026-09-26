<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import AppHeader from '@/components/AppHeader.vue'
import AppSidebar from '@/components/AppSidebar.vue'
import PlayerFooter from '@/components/PlayerFooter.vue'
import { usePlaybackStore } from '@/stores/playbackStore'
import { useApiClient } from '@/composables/useApiClient'
import type { SpotifyResponse, SpotifyUserProfile } from './types/api'

const api = useApiClient()
const store = usePlaybackStore()
const route = useRoute()

// Responsive mobile drawer state
const isSidebarOpen = ref(false)

const toggleSidebar = () => {
  isSidebarOpen.value = !isSidebarOpen.value
}

const closeSidebar = () => {
  if (isSidebarOpen.value) {
    isSidebarOpen.value = false
  }
}

// Auto-close mobile sidebar on route navigation
watch(
  () => route.path,
  () => {
    if (isSidebarOpen.value) {
      closeSidebar()
    }
  }
)

onMounted(async () => {
  const isDevelopment = import.meta.env.VITE_DEVELOPMENT ?? 'false'

  if (isDevelopment === 'false') {
    try {
      const profile: SpotifyResponse<SpotifyUserProfile> = await api.spotify.getProfile()

      if (!profile || !profile.isSuccess) {
        window.location.href = '/api/v1/auth/login'
        return
      }
    } catch (error) {
      console.error('Authentication check failed:', error)
      window.location.href = '/api/v1/auth/login'
      return
    }
  }

  store.startPolling()
})
</script>

<template>
  <div
    class="fixed inset-0 bg-slate-950 text-slate-100 h-dvh w-full flex flex-col font-sans antialiased overflow-hidden overscroll-none selection:bg-indigo-500 selection:text-white"
  >
    <!-- Main Application Header with Mobile Toggle Emit -->
    <AppHeader @toggle-sidebar="toggleSidebar" />

    <!-- Core Shell Layout Body -->
    <div class="flex-1 flex overflow-hidden relative">
      <!-- Sidebar Container (AppSidebar manages its own internal backdrop & expanded/collapsed widths) -->
      <aside class="flex shrink-0 z-40">
        <AppSidebar :is-mobile-open="isSidebarOpen" @close-sidebar="closeSidebar" />
      </aside>

      <!-- Main Scrollable Content Viewport -->
      <main
        class="flex-1 overflow-y-auto overscroll-y-contain p-3 sm:p-4 md:p-6 lg:p-8 bg-slate-950 min-w-0 focus:outline-none"
        tabindex="-1"
      >
        <div class="max-w-7xl mx-auto w-full h-full">
          <router-view v-slot="{ Component }">
            <transition name="fade" mode="out-in">
              <component :is="Component" />
            </transition>
          </router-view>
        </div>
      </main>
    </div>

    <!-- Sticky Persistent Player Footer -->
    <PlayerFooter class="shrink-0 z-30" />
  </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.15s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
