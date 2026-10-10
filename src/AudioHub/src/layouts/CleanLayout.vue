<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()
</script>

<template>
  <div
    class="fixed inset-0 bg-slate-950 text-slate-100 h-dvh w-full flex flex-col font-sans antialiased overflow-hidden overscroll-none selection:bg-indigo-500 selection:text-white"
  >
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
</template>
