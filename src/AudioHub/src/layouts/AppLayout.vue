<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import AppHeader from '@/components/AppHeader.vue'
import AppSidebar from '@/components/AppSidebar.vue'
import PlayerFooter from '@/components/PlayerFooter.vue'

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
</script>

<template>
  <div
    class="fixed inset-0 bg-slate-950 text-slate-100 h-dvh w-full flex flex-col font-sans antialiased overflow-hidden overscroll-none selection:bg-indigo-500 selection:text-white"
  >
    <!-- Main Application Header with Mobile Toggle Emit -->
    <AppHeader @toggle-sidebar="toggleSidebar" />

    <!-- Core Shell Layout Body -->
    <div class="flex-1 flex overflow-hidden relative">
      <!-- Sidebar Container -->
      <aside class="flex shrink-0 z-40">
        <AppSidebar :is-mobile-open="isSidebarOpen" @close-sidebar="closeSidebar" />
      </aside>

      <!-- Main Scrollable Content Viewport -->
      <main
        id="main-viewport"
        class="flex-1 overflow-y-auto overscroll-y-contain p-3 sm:p-4 md:p-6 lg:p-8 bg-slate-950 min-w-0 focus:outline-none"
        tabindex="-1"
      >
        <div class="max-w-7xl mx-auto w-full h-full">
          <!-- Router View Wrapper -->
          <router-view v-slot="{ Component }">
            <component :is="Component" :key="$route.fullPath" />
          </router-view>
        </div>
      </main>
    </div>

    <!-- Sticky Persistent Player Footer -->
    <PlayerFooter class="shrink-0 z-30" />
  </div>
</template>
