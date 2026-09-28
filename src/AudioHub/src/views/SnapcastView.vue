<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import type { SnapGroup, SnapServer } from '@/types/api'

const api = useApiClient()
const { loading, execute } = api.createApiState<SnapServer>()
const groups = ref<SnapGroup[]>([])

async function loadStatus() {
  const res = await execute(() => api.snapcast.getServer())
  if (res?.groups) {
    groups.value = res.groups
  }
}

async function toggleMute(groupId: string, mute: boolean) {
  await api.snapcast.setGroupMute(groupId, { mute })
  await loadStatus()
}

async function updateVolume(clientId: string, vol: number) {
  await api.snapcast.setClientVolume(clientId, { volumePercent: vol })
}

onMounted(loadStatus)
</script>

<template>
  <div class="min-h-screen text-slate-100">
    <div class="p-4 sm:p-6 lg:p-8 max-w-7xl mx-auto space-y-6">
      <!-- Page Header -->
      <div class="flex items-center justify-between border-b border-slate-800 pb-4">
        <h2 class="text-lg sm:text-2xl font-bold tracking-tight text-white">
          Snapcast Multi-Room Management
        </h2>
        <button
          @click="loadStatus"
          class="px-3 py-1.5 text-xs font-medium rounded-lg bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-300 transition shrink-0"
        >
          Refresh
        </button>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex items-center justify-center py-16">
        <div class="flex items-center space-x-3 text-slate-400">
          <svg class="animate-spin h-5 w-5 text-indigo-500" viewBox="0 0 24 24" fill="none">
            <circle
              class="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              stroke-width="4"
            ></circle>
            <path
              class="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            ></path>
          </svg>
          <span class="text-sm font-medium">Loading Snapcast server status...</span>
        </div>
      </div>

      <!-- Group Cards Grid -->
      <div v-else-if="groups.length" class="grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6">
        <div
          v-for="g in groups"
          :key="g.id"
          class="bg-slate-900 border border-slate-800 rounded-2xl p-4 sm:p-5 flex flex-col justify-between space-y-4 shadow-xl"
        >
          <!-- Group Header -->
          <div class="flex items-center justify-between gap-3 pb-2 border-b border-slate-800/80">
            <div class="min-w-0 flex-1">
              <span class="text-xs uppercase tracking-wider text-slate-500 font-semibold block"
                >Group</span
              >
              <h3 class="font-bold text-sm sm:text-base text-slate-200 truncate">
                {{ g.name ? (g.name == '' ? g.id : g.name) : g.id }}
              </h3>
            </div>
            <button
              @click="toggleMute(g.id, !g.muted)"
              :class="
                g.muted
                  ? 'bg-rose-500/10 text-rose-400 border-rose-500/30 hover:bg-rose-500/20'
                  : 'bg-slate-800 text-slate-300 border-slate-700 hover:bg-slate-700'
              "
              class="px-3.5 py-1.5 text-xs font-semibold rounded-lg border transition shrink-0 active:scale-95"
            >
              {{ g.muted ? 'Muted' : 'Mute Group' }}
            </button>
          </div>

          <!-- Clients List -->
          <div class="space-y-2.5">
            <div
              v-for="c in g.clients"
              :key="c.id"
              class="flex flex-col sm:flex-row sm:items-center justify-between bg-slate-950 p-3.5 rounded-xl border border-slate-800/80 gap-3"
            >
              <!-- Client Information -->
              <div class="min-w-0 flex-1">
                <p class="font-medium text-xs sm:text-sm text-slate-200 truncate">
                  {{ c.config?.name || c.host?.name || 'Unnamed Client' }}
                </p>
                <p class="text-[11px] text-slate-400 mt-0.5 font-mono truncate">
                  {{ c.host?.ip || '0.0.0.0' }} • Latency: {{ c.config?.latency ?? 0 }}ms
                </p>
              </div>

              <!-- Volume Slider & Value Indicator -->
              <div
                class="flex items-center space-x-3 shrink-0 w-full sm:w-auto pt-1 sm:pt-0 border-t sm:border-t-0 border-slate-900"
              >
                <input
                  type="range"
                  min="0"
                  max="100"
                  :value="c.config?.volume?.percent || 50"
                  @change="(e) => updateVolume(c.id, Number((e.target as HTMLInputElement).value))"
                  class="flex-1 sm:w-36 accent-indigo-500 bg-slate-800 h-2 rounded-lg cursor-pointer"
                />
                <span class="text-xs font-mono text-slate-400 w-8 text-right shrink-0">
                  {{ c.config?.volume?.percent || 50 }}%
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div
        v-else
        class="bg-slate-900/50 border border-slate-800/80 rounded-2xl p-8 sm:p-12 text-center"
      >
        <p class="text-slate-400 text-sm font-medium">No Snapcast groups or clients detected.</p>
      </div>
    </div>
  </div>
</template>
