<!-- ClientConfigView.vue -->
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import type { ClientConfig } from '@/types/api'

const api = useApiClient()

// State
const isLoading = ref<boolean>(true)
const isSaving = ref<boolean>(false)
const errorMessage = ref<string | null>(null)
const successMessage = ref<string | null>(null)

// Form configuration data model
const countries = ref<Record<string, string>>({})
const config = ref<ClientConfig | null>(null)

async function loadConfig() {
  isLoading.value = true
  errorMessage.value = null

  try {
    config.value = await api.config.get()
  } catch (error) {
    console.error('Failed to load client configuration:', error)
    errorMessage.value = 'Failed to load configuration. Please try again.'
  } finally {
    isLoading.value = false
  }
}

async function saveConfig() {
  if (!config.value) {
    errorMessage.value = 'Failed to save. Config is empty.'
    return
  }

  isSaving.value = true
  errorMessage.value = null
  successMessage.value = null

  try {
    await api.config.update(config.value)
    successMessage.value = 'Client configuration updated successfully!'
  } catch (error) {
    console.error('Failed to save client configuration:', error)
    errorMessage.value = 'Failed to save configuration settings. Please check your inputs.'
  } finally {
    isSaving.value = false
  }
}

onMounted(async () => {
  try {
    countries.value = await api.core.getCountries()
  } catch (error) {
    console.error('Failed to load countries:', error)
  }
  await loadConfig()
})
</script>

<template>
  <div class="min-h-screen text-slate-100">
    <!-- Responsive outer padding wrapper -->
    <div class="p-4 sm:p-6 lg:p-8 max-w-4xl mx-auto space-y-6 sm:space-y-8">
      <!-- Loading State -->
      <div v-if="isLoading" class="flex items-center justify-center py-20">
        <div class="flex items-center space-x-3 text-slate-400">
          <svg class="animate-spin h-6 w-6 text-indigo-500" viewBox="0 0 24 24" fill="none">
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
          <span class="text-sm font-medium">Loading Configuration...</span>
        </div>
      </div>

      <!-- Error State -->
      <div
        v-else-if="errorMessage"
        class="bg-red-500/10 border border-red-500/30 rounded-xl p-6 text-red-400 text-center"
      >
        <p class="text-sm font-medium">{{ errorMessage }}</p>
        <button
          @click="loadConfig()"
          class="mt-4 px-4 py-2 text-xs rounded-lg bg-red-500/20 hover:bg-red-500/30 text-red-300 transition"
        >
          Try Again
        </button>
      </div>

      <template v-else-if="config">
        <!-- Header -->
        <header
          class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-800 pb-6"
        >
          <div>
            <span
              class="inline-block px-3 py-1 text-xs rounded-full bg-indigo-500/10 text-indigo-400 border border-indigo-500/20 font-semibold uppercase tracking-wider mb-2"
            >
              System Settings
            </span>
            <h1 class="text-2xl sm:text-3xl font-extrabold tracking-tight text-white">
              Client Configuration
            </h1>
          </div>
        </header>

        <!-- Feedback Alerts -->
        <div
          v-if="successMessage"
          class="bg-emerald-500/10 border border-emerald-500/30 rounded-xl p-4 text-emerald-400 text-sm font-medium flex items-center justify-between"
        >
          <span>{{ successMessage }}</span>
          <button @click="successMessage = null" class="text-emerald-400 hover:text-emerald-200">
            <svg
              class="w-4 h-4"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
            >
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Configuration Form -->
        <form @submit.prevent="saveConfig" class="space-y-6">
          <!-- Section 1: General Connection Details -->
          <div class="p-5 sm:p-6 rounded-2xl bg-slate-800/40 border border-slate-800/60 space-y-4">
            <h2 class="text-lg font-semibold text-slate-200 border-b border-slate-700/50 pb-2">
              Region Configuration
            </h2>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-medium text-slate-400 mb-1">Default Country</label>
                <select
                  v-model="config.defaultCountry"
                  class="w-full px-3 py-2 rounded-lg bg-slate-900 border border-slate-700 text-slate-100 text-sm focus:outline-none focus:border-indigo-500 transition"
                >
                  <option value="" disabled>Select a country</option>
                  <option v-for="(value, key) in countries" :key="key" :value="key">
                    {{ value }}
                  </option>
                </select>
              </div>
            </div>
          </div>

          <!-- Action Buttons -->
          <div class="flex justify-end pt-4">
            <button
              type="submit"
              :disabled="isSaving"
              class="inline-flex items-center gap-2 px-6 py-2.5 rounded-full bg-indigo-600 hover:bg-indigo-500 disabled:bg-indigo-800/50 disabled:cursor-not-allowed text-white font-medium text-sm shadow-lg hover:scale-105 active:scale-95 transition"
            >
              <svg
                v-if="isSaving"
                class="animate-spin h-4 w-4 text-white"
                viewBox="0 0 24 24"
                fill="none"
              >
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
              <span>{{ isSaving ? 'Saving Changes...' : 'Save Configuration' }}</span>
            </button>
          </div>
        </form>
      </template>
    </div>
  </div>
</template>
