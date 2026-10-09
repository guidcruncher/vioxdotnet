<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { Grid3x3, Rows3 } from '@lucide/vue'

const STORAGE_KEY = 'view_mode'
const viewMode = defineModel({ type: String, default: 'grid' })

const toggleViewMode = (mode: string) => {
  localStorage.setItem(STORAGE_KEY, mode)
  viewMode.value = mode
}

onMounted(() => {
  const savedMode = localStorage.getItem(STORAGE_KEY)
  if (savedMode === 'grid' || savedMode === 'list') {
    viewMode.value = savedMode
  }
})
</script>

<template>
  <div class="flex items-center bg-slate-800/80 p-1 rounded-lg border border-slate-700/60">
    <button
      @click="toggleViewMode('grid')"
      :class="[
        'p-1.5 rounded-md transition text-slate-400 hover:text-slate-100',
        viewMode === 'grid' ? 'bg-slate-700 text-indigo-400 font-semibold shadow-sm' : '',
      ]"
      title="Grid View"
      aria-label="Switch to grid view"
    >
      <Grid3x3 class="w-4 h-4" />
    </button>
    <button
      @click="toggleViewMode('list')"
      :class="[
        'p-1.5 rounded-md transition text-slate-400 hover:text-slate-100',
        viewMode === 'list' ? 'bg-slate-700 text-indigo-400 font-semibold shadow-sm' : '',
      ]"
      title="List View"
      aria-label="Switch to list view"
    >
      <Rows3 class="w-4 h-4" />
    </button>
  </div>
</template>
