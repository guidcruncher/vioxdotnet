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
  <div>
    <h2 class="text-xl font-bold mb-4">Snapcast Multi-Room Management</h2>
    <div v-if="loading" class="text-slate-400 animate-pulse">Loading Snapcast server status...</div>
    <div v-else-if="groups.length" class="space-y-4">
      <div
        v-for="g in groups"
        :key="g.id"
        class="bg-slate-900 border border-slate-800 rounded-xl p-4"
      >
        <div class="flex items-center justify-between mb-3">
          <span class="font-bold text-slate-200">Group: {{ g.id }}</span>
          <button
            @click="toggleMute(g.id, !g.muted)"
            :class="
              g.muted
                ? 'bg-rose-900/40 text-rose-400 border border-rose-800'
                : 'bg-slate-800 text-slate-300'
            "
            class="px-3 py-1 text-xs rounded border"
          >
            {{ g.muted ? 'Muted' : 'Mute Group' }}
          </button>
        </div>
        <div class="space-y-2">
          <div
            v-for="c in g.clients"
            :key="c.id"
            class="flex items-center justify-between bg-slate-950 p-3 rounded-lg border border-slate-800/80"
          >
            <div>
              <p class="font-medium text-sm">{{ c.config?.name || c.host?.name }}</p>
              <p class="text-[10px] text-slate-500">
                {{ c.host?.ip }} • Latency: {{ c.config?.latency }}ms
              </p>
            </div>
            <input
              type="range"
              min="0"
              max="100"
              :value="c.config?.volume?.percent || 50"
              @change="(e) => updateVolume(c.id, Number((e.target as HTMLInputElement).value))"
              class="w-32 accent-indigo-500 bg-slate-800 h-1.5 rounded cursor-pointer"
            />
          </div>
        </div>
      </div>
    </div>
    <div v-else class="text-slate-400">No Snapcast groups or clients detected.</div>
  </div>
</template>
