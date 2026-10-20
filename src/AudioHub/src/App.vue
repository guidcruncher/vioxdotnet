<script setup lang="ts">
import { watch, onMounted } from 'vue'
import { usePlaybackStore } from '@/stores/playbackStore'
import { useApiClient } from '@/composables/useApiClient'
import { useRouter, useRoute } from 'vue-router'
import { useBreakpoint } from '@/composables/useBreakpoint'
import type { SpotifyResponse, SpotifyUserProfile } from '@/types'
import { useAlert } from '@/composables/useAlert'

const router = useRouter()
const route = useRoute()

const { isSmallerThan } = useBreakpoint()
const api = useApiClient()
const store = usePlaybackStore()
const { state, handleRetry, closeError } = useAlert()

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
  <div id="app">
    <RouterView />

    <ErrorDialog
      v-model:isOpen="state.isOpen"
      :title="state.title"
      :error-message="state.errorMessage"
      :error-details="state.errorDetails"
      :show-retry="state.showRetry"
      :retry-text="state.retryText"
      @retry="handleRetry"
      @close="closeError"
    />
  </div>
</template>
