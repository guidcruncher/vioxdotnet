<script setup lang="ts">
import { watch, ref } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '../types/api'
import { useRoute, useRouter } from 'vue-router'
import MediaCard from '@/components/MediaCard.vue'
import CountryCard from '@/components/CountryCard.vue'

const router = useRouter()
const route = useRoute()
const api = useApiClient()
const store = usePlaybackStore()

const countries = ref<Record<string, string> | null>(null)
const stations = ref<MediaMetaData[]>([])
const { loading, execute } = api.createApiState<MediaMetaData[]>()

async function ensureCountriesLoaded() {
  stations.value = [] // Clear previous station results when returning to country view
  if (!countries.value) {
    countries.value = await api.core.getCountries()
  }
}

async function loadStationsForCountry(countryCode: string) {
  const result = await execute(() => api.radioBrowser.getByCountry(countryCode))
  if (result) {
    stations.value = result
  }
}

async function playItem(station: MediaMetaData) {
  if (station.rawUri) {
    await store.playUri(station.rawUri)
  }
}

function viewItem(countryCode: string) {
  if (!countryCode) {
    router.push('/radio')
    return
  }
  router.push({ path: '/radio', query: { id: countryCode } })
}

async function refresh() {
  const id = route.query.id
  if (typeof id === 'string' && id.trim() !== '') {
    await loadStationsForCountry(id)
  } else {
    await ensureCountriesLoaded()
  }
}

// React to route query changes
watch(
  () => route.query.id,
  async (newId) => {
    if (typeof newId === 'string' && newId.trim() !== '') {
      await loadStationsForCountry(newId)
    } else {
      await ensureCountriesLoaded()
    }
  },
  { immediate: true }
)
</script>

<template>
  <div class="w-full min-h-full flex flex-col justify-start overscroll-y-contain touch-pan-y">
    <div class="max-w-7xl mx-auto w-full px-4 sm:px-6 lg:px-8 py-6">
      <h2 class="text-xl sm:text-2xl font-bold mb-4 text-slate-100 tracking-tight">
        Radio Stations
      </h2>

      <!-- Loading State -->
      <div v-if="loading" class="text-slate-400 animate-pulse py-8 text-center sm:text-left">
        Searching media providers...
      </div>

      <!-- Content Grid -->
      <div v-else class="space-y-8">
        <div class="space-y-3">
          <MediaCardGrid>
            <!-- Show Stations if 'id' query parameter is present -->
            <template v-if="route.query.id" v-for="(s, index) in stations" :key="s.rawUri">
              <MediaCard v-model:item="stations[index]" @play="playItem" />
            </template>

            <!-- Show Countries if no 'id' query parameter -->
            <template v-else-if="countries">
              <CountryCard
                v-for="[key, value] in Object.entries(countries)"
                :key="key"
                :item="[key, value]"
                @view="viewItem"
              />
            </template>
          </MediaCardGrid>
        </div>
      </div>
    </div>
  </div>
</template>
