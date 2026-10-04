<script setup lang="ts">
import { watch, onMounted, computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApiClient } from '@/composables/useApiClient'
import { usePlaybackStore } from '@/stores/playbackStore'
import type { MediaMetaData } from '@/types'
import MediaCardGrid from '@/components/MediaCardGrid.vue'
import MediaCard from '@/components/MediaCard.vue'
import MediaTrackList from '@/components/MediaTrackList.vue'

const STORAGE_KEY = 'library_view_mode'

const router = useRouter()
const route = useRoute()
const api = useApiClient()
const store = usePlaybackStore()

const addToPlaylist = ref<boolean>(false)
const playlistItem = ref<MediaMetaData>()
const sources = ref<Record<string, string>>({})
const sourceProps = ref<Record<string, Record<string, string>>>({})

const viewMode = ref<'grid' | 'list'>('grid')

const { data: items, loading, execute } = api.createApiState<MediaMetaData[]>()

let currentRequestId = 0

const hasResults = computed(() => {
  if (!items.value) return false
  return items.value.length > 0
})

const itemList = computed<MediaMetaData[]>({
  get: () => items.value || [],
  set: (val) => {
    items.value = val
  },
})

const pageTitle = computed<string>(() => {
  const sourceParam = route.params.source
  const sourceKey = Array.isArray(sourceParam) ? sourceParam[0] : sourceParam

  if (sources.value && sourceKey) {
    return sources.value[sourceKey] || ''
  }

  return ''
})

async function addToPlaylistFunc(state: boolean, item: MediaMetaData) {
  playlistItem.value = item
  addToPlaylist.value = true
}

function getCleanQueryParams(query: typeof route.query): Record<string, string> {
  const cleanParams: Record<string, string> = {}
  for (const [key, value] of Object.entries(query)) {
    if (value === null || value === undefined) continue
    cleanParams[key] = Array.isArray(value) ? value[0] || '' : value
  }
  return cleanParams
}

function toggleViewMode(mode: 'grid' | 'list') {
  viewMode.value = mode
  localStorage.setItem(STORAGE_KEY, mode)
}

async function viewItem(item: MediaMetaData) {
  if (!item.uri) return

  switch (item.uri.source) {
    case 'playlist':
    case 'file':
      break
    case 'spotify':
      switch (item.uri.type) {
        case 'show':
          router.push(`/spotify/show/${encodeURIComponent(item.uri.id)}`)
          break
        case 'album':
          router.push(`/spotify/album/${encodeURIComponent(item.uri.id)}`)
          break
        case 'playlist':
          router.push(`/spotify/playlist/${encodeURIComponent(item.uri.id)}`)
          break
      }
      return
    case 'podverse':
      router.push(`/podverse/podcast/${encodeURIComponent(item.rawUri)}`)
      return
    case 'radiobrowser':
      if (item.uri.type !== 'station') {
        router.push(`/library/${item.uri.source}?id=${encodeURIComponent(item.uri.id)}`)
      }
      return
      break
    case 'tunein':
      if (item.uri.type !== 'station') {
        router.push(`/library/${item.uri.source}?id=${encodeURIComponent(item.uri.id)}`)
      }
      return
  }

  if (item.uri.secondaryId && item.uri.secondaryId !== '') {
    router.push(
      `/library/${item.uri.source}?type=${encodeURIComponent(item.uri.type)}&id=${encodeURIComponent(item.uri.id)}&secondaryid=${encodeURIComponent(item.uri.secondaryId)}`
    )
    return
  }

  router.push(
    `/library/${item.uri.source}?type=${encodeURIComponent(item.uri.type)}&id=${encodeURIComponent(item.uri.id)}`
  )
}

async function playItem(item: MediaMetaData) {
  if (item.rawUri) {
    await store.playUri(item.rawUri)
  }
}

async function loadLibrary() {
  const sourceParam = route.params.source
  const source = Array.isArray(sourceParam) ? sourceParam[0] : sourceParam

  if (!source) return

  const requestId = ++currentRequestId

  await execute(async () => {
    const queryParams = getCleanQueryParams(route.query)
    const response = await api.library.getLibrary(source, queryParams)
    if (requestId !== currentRequestId) {
      return items.value || []
    }
    return response
  })
}

async function refresh() {
  await loadLibrary()
}

watch(
  () => route.fullPath,
  async () => {
    await loadLibrary()
  },
  { immediate: true }
)

onMounted(async () => {
  const savedMode = localStorage.getItem(STORAGE_KEY)
  if (savedMode === 'grid' || savedMode === 'list') {
    viewMode.value = savedMode
  }

  try {
    const [installedSources, props] = await Promise.all([
      api.library.getInstalledSources(),
      api.library.getSourceProps(),
    ])
    sources.value = installedSources || {}
    sourceProps.value = props || {}
  } catch (err) {
    console.error('Failed to load source metadata:', err)
  }
})
</script>

<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
    <!-- Section Header with View Mode Controls -->
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-xl sm:text-2xl font-bold text-slate-100 tracking-tight">
        {{ pageTitle }} Library
      </h2>

      <!-- View Switcher Button Group -->
      <div
        v-if="!loading && hasResults"
        class="flex items-center bg-slate-800/80 p-1 rounded-lg border border-slate-700/60"
      >
        <button
          @click="toggleViewMode('grid')"
          :class="[
            'p-1.5 rounded-md transition text-slate-400 hover:text-slate-100',
            viewMode === 'grid' ? 'bg-slate-700 text-indigo-400 font-semibold shadow-sm' : '',
          ]"
          title="Grid View"
          aria-label="Switch to grid view"
        >
          <svg class="w-4 h-4 fill-current" viewBox="0 0 24 24">
            <path
              d="M4 4h4v4H4V4zm6 0h4v4h-4V4zm6 0h4v4h-4V4zM4 10h4v4H4v-4zm6 0h4v4h-4v-4zm6 0h4v4h-4v-4zM4 16h4v4H4v-4zm6 0h4v4h-4v-4zm6 0h4v4h-4v-4z"
            />
          </svg>
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
          <svg class="w-4 h-4 fill-current" viewBox="0 0 24 24">
            <path d="M4 6h16v2H4V6zm0 5h16v2H4v-2zm0 5h16v2H4v-2z" />
          </svg>
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="text-slate-400 animate-pulse py-8 text-center sm:text-left">
      Searching media providers...
    </div>

    <!-- Content Views -->
    <div v-else-if="hasResults" class="space-y-8">
      <div class="space-y-3">
        <!-- Icon Grid View -->
        <MediaCardGrid v-if="viewMode === 'grid'">
          <template v-if="items" v-for="(item, index) in items" :key="item.rawUri || index">
            <MediaCard
              v-model:item="items[index]"
              @view="viewItem"
              @play="playItem"
              @playlist="addToPlaylistFunc"
            />
          </template>
        </MediaCardGrid>

        <!-- List View -->
        <MediaTrackList
          v-else-if="viewMode === 'list'"
          v-model="itemList"
          @play="playItem"
          @view="viewItem"
        />
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-slate-400 py-8 text-center sm:text-left">
      No media found matching query.
    </div>
  </div>
  <AddToPlaylistModal v-model:isOpen="addToPlaylist" :item="playlistItem" />
</template>
