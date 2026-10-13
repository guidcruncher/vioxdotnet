<script setup lang="ts">
import { watch, onMounted } from 'vue'
import { usePlaybackStore } from '@/stores/playbackStore'
import { useApiClient } from '@/composables/useApiClient'
import { useRouter, useRoute } from 'vue-router'
import { useBreakpoint } from '@/composables/useBreakpoint'
import type { SpotifyResponse, SpotifyUserProfile } from '@/types'

const router = useRouter()
const route = useRoute()

const { isSmallerThan } = useBreakpoint()
const api = useApiClient()
const store = usePlaybackStore()

// watch(
//   () => isSmallerThan('xs'),
//   (isMobileSize) => {
//     const isMobileRoute = route.path === '/remote'
//
//     if (isMobileSize && !isMobileRoute) {
//       router.push('/remote')
//     } else if (!isMobileSize && isMobileRoute) {
//      router.push('/')
//     }
//   },
//   { immediate: true }
// )

onMounted(async () => {
  const isDevelopment = import.meta.env.VITE_DEVELOPMENT ?? 'false'

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
})
</script>

<template>
  <router-view />
</template>
