<script setup lang="ts">
import { onMounted } from 'vue'
import { usePlaybackStore } from '@/stores/playbackStore'
import { useApiClient } from '@/composables/useApiClient'
import type { SpotifyResponse, SpotifyUserProfile } from '@/types'

const api = useApiClient()
const store = usePlaybackStore()

onMounted(async () => {
  const isDevelopment = import.meta.env.VITE_DEVELOPMENT ?? 'false'

  if (isDevelopment === 'false') {
    try {
      const profile: SpotifyResponse<SpotifyUserProfile> = await api.auth.me()

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
})
</script>

<template>
  <router-view />
</template>
