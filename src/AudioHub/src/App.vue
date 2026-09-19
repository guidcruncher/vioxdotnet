<script setup lang="ts">
import { onMounted } from 'vue'
import AppHeader from '@/components/AppHeader.vue'
import AppSidebar from '@/components/AppSidebar.vue'
import PlayerFooter from '@/components/PlayerFooter.vue'
import { usePlaybackStore } from '@/stores/playbackStore'
import { useApiClient } from '@/composables/useApiClient'
import type { SpotifyResponse, SpotifyUserProfile } from './types/api'

const api = useApiClient()
const store = usePlaybackStore()

onMounted(async () => {
  const profile: SpotifyResponse<SpotifyUserProfile> = await api.spotify.getProfile()

  if (!profile || !profile.isSuccess) {
    window.location.href = '/api/v1/auth/login'
    return
  }

  store.startPolling()
})
</script>

<template>
  <div class="bg-slate-950 text-slate-100 min-h-screen flex flex-col font-sans antialiased">
    <AppHeader />
    <div class="flex-1 flex overflow-hidden">
      <AppSidebar />
      <main class="flex-1 overflow-y-auto p-6 bg-slate-950">
        <router-view />
      </main>
    </div>
    <PlayerFooter />
  </div>
</template>
