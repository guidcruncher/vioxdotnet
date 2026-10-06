<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import { PlaybackRepeatMode } from '@/types'
import type { EventPayload, PlayRequest, MediaMetaData, QueueStatusResponse } from '@/types'
import { useServerEvents } from '@/composables/useServerEvents'

const sse = useServerEvents({
  autoReconnect: true,
  immediate: true,
})

const apiClient = useApiClient()
const queueApi = apiClient.queue

const queue = ref<MediaMetaData[]>([])
const currentIndex = ref<number>(-1)
const currentItem = ref<MediaMetaData | null>(null)
const isShuffleEnabled = ref<boolean>(false)
const repeatMode = ref<PlaybackRepeatMode>(PlaybackRepeatMode.Off)

const isLoading = ref<boolean>(false)
const isActionPending = ref<boolean>(false)
const errorMessage = ref<string | null>(null)

const trackEventTypes = ['play', 'pause', 'previous', 'next', 'trackchanged']
trackEventTypes.forEach((eventType) => {
  sse.on(eventType, async (payload: EventPayload) => {
    try {
      await fetchStatus()
    } catch (err) {
      console.error(`Failed to parse ${eventType} event message:`, err)
    }
  })
})

const totalDuration = computed<number>(() => {
  return queue.value.reduce((acc, item) => acc + (item.duration || 0), 0)
})

function formatDuration(seconds?: number): string {
  if (seconds === undefined || seconds === null || isNaN(seconds)) {
    return '0:00'
  }
  const mins = Math.floor(seconds / 60)
  const secs = Math.floor(seconds % 60)
  return `${mins}:${secs < 10 ? '0' : ''}${secs}`
}

async function fetchStatus(): Promise<void> {
  isLoading.value = true
  errorMessage.value = null
  try {
    const status: QueueStatusResponse = await queueApi.getStatus()
    queue.value = status.items || []
    currentIndex.value = status.currentIndex ?? -1
    currentItem.value = status.currentItem || null
    isShuffleEnabled.value = status.isShuffleEnabled ?? false
    repeatMode.value = status.repeatMode ?? PlaybackRepeatMode.Off
  } catch (err: unknown) {
    const error = err as Error
    errorMessage.value = error.message || 'Failed to fetch queue status.'
  } finally {
    isLoading.value = false
  }
}

async function handleNext(): Promise<void> {
  if (isActionPending.value) return
  isActionPending.value = true
  try {
    await queueApi.next()
    await fetchStatus()
  } catch (err: unknown) {
    const error = err as Error
    errorMessage.value = error.message || 'Failed to skip to next track.'
  } finally {
    isActionPending.value = false
  }
}

async function handlePrevious(): Promise<void> {
  if (isActionPending.value) return
  isActionPending.value = true
  try {
    await queueApi.previous()
    await fetchStatus()
  } catch (err: unknown) {
    const error = err as Error
    errorMessage.value = error.message || 'Failed to skip to previous track.'
  } finally {
    isActionPending.value = false
  }
}

async function handleSkipTo(index: number): Promise<void> {
  if (isActionPending.value || index === currentIndex.value) return
  isActionPending.value = true
  try {
    await queueApi.skipTo(index)
    await fetchStatus()
  } catch (err: unknown) {
    const error = err as Error
    errorMessage.value = error.message || 'Failed to skip to selected track.'
  } finally {
    isActionPending.value = false
  }
}

async function handleRemoveItem(index: number): Promise<void> {
  if (isActionPending.value) return
  isActionPending.value = true
  try {
    await queueApi.removeItem(index)
    await fetchStatus()
  } catch (err: unknown) {
    const error = err as Error
    errorMessage.value = error.message || 'Failed to remove item.'
  } finally {
    isActionPending.value = false
  }
}

async function handleClear(): Promise<void> {
  if (isActionPending.value || queue.value.length === 0) return
  if (!confirm('Are you sure you want to clear the playback queue?')) return

  isActionPending.value = true
  try {
    await queueApi.clear()
    await fetchStatus()
  } catch (err: unknown) {
    const error = err as Error
    errorMessage.value = error.message || 'Failed to clear queue.'
  } finally {
    isActionPending.value = false
  }
}

async function handleToggleShuffle(): Promise<void> {
  if (isActionPending.value) return
  isActionPending.value = true
  const nextState = !isShuffleEnabled.value
  try {
    await queueApi.setShuffle(nextState)
    isShuffleEnabled.value = nextState
    await fetchStatus()
  } catch (err: unknown) {
    const error = err as Error
    errorMessage.value = error.message || 'Failed to toggle shuffle.'
  } finally {
    isActionPending.value = false
  }
}

async function handleCycleRepeat(): Promise<void> {
  if (isActionPending.value) return
  isActionPending.value = true

  let nextMode: PlaybackRepeatMode
  if (repeatMode.value === PlaybackRepeatMode.Off) {
    nextMode = PlaybackRepeatMode.Queue
  } else if (repeatMode.value === PlaybackRepeatMode.Queue) {
    nextMode = PlaybackRepeatMode.Track
  } else {
    nextMode = PlaybackRepeatMode.Off
  }

  try {
    await queueApi.setRepeatMode(nextMode)
    repeatMode.value = nextMode
    await fetchStatus()
  } catch (err: unknown) {
    const error = err as Error
    errorMessage.value = error.message || 'Failed to update repeat mode.'
  } finally {
    isActionPending.value = false
  }
}

function getRepeatModeLabel(): string {
  switch (repeatMode.value) {
    case PlaybackRepeatMode.Track:
      return 'Repeat Track'
    case PlaybackRepeatMode.Queue:
      return 'Repeat Queue'
    default:
      return 'Repeat Off'
  }
}

onMounted(() => {
  fetchStatus()
})
</script>

<template>
  <div class="min-h-screen bg-slate-950 text-slate-100 p-4 md:p-8 font-sans">
    <div class="max-w-7xl mx-auto space-y-6">
      <!-- Top Bar / Header -->
      <header
        class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-800 pb-5"
      >
        <div>
          <h1 class="text-2xl md:text-3xl font-bold tracking-tight text-white">Playback Queue</h1>
          <p class="text-xs md:text-sm text-slate-400 mt-1">
            {{ queue.length }} track{{ queue.length === 1 ? '' : 's' }} in queue
            <span class="mx-1">--</span>
            Total runtime: {{ formatDuration(totalDuration) }}
          </p>
        </div>

        <div class="flex items-center gap-2">
          <button
            type="button"
            @click="fetchStatus"
            :disabled="isLoading"
            class="p-2 text-slate-400 hover:text-white rounded-lg hover:bg-slate-800 transition-colors disabled:opacity-50"
            title="Refresh Queue"
          >
            <svg
              class="w-5 h-5"
              :class="{ 'animate-spin': isLoading }"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
              />
            </svg>
          </button>

          <button
            type="button"
            @click="handleClear"
            :disabled="queue.length === 0 || isActionPending"
            class="p-2 text-red-400 hover:text-red-300 rounded-lg hover:bg-red-950/40 transition-colors disabled:opacity-30 disabled:hover:bg-transparent"
            title="Clear Queue"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
              />
            </svg>
          </button>
        </div>
      </header>

      <!-- Error Notification -->
      <div
        v-if="errorMessage"
        class="p-4 rounded-xl bg-red-900/30 border border-red-700/50 text-red-200 text-sm flex items-start justify-between"
      >
        <div>
          <span class="font-semibold block mb-1">Queue Error</span>
          <span>{{ errorMessage }}</span>
        </div>
        <button type="button" @click="errorMessage = null" class="text-red-400 hover:text-red-200">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </button>
      </div>

      <!-- Main Layout -->
      <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
        <!-- Now Playing Card Sidebar -->
        <section
          class="lg:col-span-5 xl:col-span-4 bg-slate-900 border border-slate-800 rounded-2xl p-6 flex flex-col justify-between shadow-xl"
        >
          <div>
            <div class="flex items-center justify-between mb-4">
              <span class="text-xs font-semibold tracking-wider uppercase text-indigo-400"
                >Now Playing</span
              >
              <span v-if="currentItem" class="text-xs text-slate-500 font-mono"
                >Index #{{ currentIndex }}</span
              >
            </div>

            <!-- Artwork -->
            <div
              class="relative aspect-square rounded-xl overflow-hidden bg-slate-800 mb-6 shadow-inner flex items-center justify-center border border-slate-700/50"
            >
              <img
                v-if="currentItem?.imageUrl"
                :src="currentItem.imageUrl"
                :alt="currentItem.title"
                class="w-full h-full object-cover"
              />
              <div v-else class="text-slate-600 flex flex-col items-center">
                <svg class="w-16 h-16 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="1.5"
                    d="M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 .895-2 3-2 3 .895 3 2zm12 0c0 1.105-1.343 2-3 2s-3-.895-3-2 .895-2 3-2 3 .895 3 2zM9 10l12-3"
                  />
                </svg>
                <span class="text-xs">No Track Cover</span>
              </div>
            </div>

            <!-- Metadata -->
            <div class="space-y-1 text-center sm:text-left">
              <h2
                class="text-xl font-bold text-white truncate"
                :title="currentItem?.title || 'Nothing Playing'"
              >
                {{ currentItem?.title || 'Nothing Playing' }}
              </h2>
              <p
                class="text-slate-400 text-sm truncate"
                :title="currentItem?.artist || 'Unknown Artist'"
              >
                {{ currentItem?.artist || 'Select a track to start playback' }}
              </p>
              <p v-if="currentItem?.album" class="text-slate-500 text-xs truncate">
                {{ currentItem.album }}
              </p>
            </div>
          </div>

          <!-- Controls -->
          <div class="mt-8 space-y-6">
            <div class="flex items-center justify-center gap-4">
              <!-- Shuffle Button -->
              <button
                type="button"
                @click="handleToggleShuffle"
                :disabled="isActionPending"
                class="p-2.5 rounded-full transition-colors"
                :class="
                  isShuffleEnabled
                    ? 'bg-indigo-600/20 text-indigo-400 border border-indigo-500/40'
                    : 'text-slate-400 hover:text-white hover:bg-slate-800'
                "
                :title="isShuffleEnabled ? 'Disable Shuffle' : 'Enable Shuffle'"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h2m8 0h2a2 2 0 012 2v8a2 2 0 01-2 2h-2m-4-4l3 3m0 0l-3 3m3-3H3m13-8l3 3m0 0l-3 3m3-3H9"
                  />
                </svg>
              </button>

              <!-- Previous Button -->
              <button
                type="button"
                @click="handlePrevious"
                :disabled="isActionPending || queue.length === 0"
                class="p-3 text-slate-300 hover:text-white rounded-full hover:bg-slate-800 transition-colors disabled:opacity-30"
                title="Previous Track"
              >
                <svg class="w-6 h-6" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M6 6h2v12H6zm3.5 6l8.5 6V6z" />
                </svg>
              </button>

              <!-- Next Button -->
              <button
                type="button"
                @click="handleNext"
                :disabled="isActionPending || queue.length === 0"
                class="p-3 text-slate-300 hover:text-white rounded-full hover:bg-slate-800 transition-colors disabled:opacity-30"
                title="Next Track"
              >
                <svg class="w-6 h-6" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M6 18l8.5-6L6 6v12zM16 6v12h2V6h-2z" />
                </svg>
              </button>

              <!-- Repeat Mode Button -->
              <button
                type="button"
                @click="handleCycleRepeat"
                :disabled="isActionPending"
                class="p-2.5 rounded-full transition-colors relative"
                :class="
                  repeatMode !== PlaybackRepeatMode.Off
                    ? 'bg-indigo-600/20 text-indigo-400 border border-indigo-500/40'
                    : 'text-slate-400 hover:text-white hover:bg-slate-800'
                "
                :title="getRepeatModeLabel()"
              >
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
                  />
                </svg>
                <span
                  v-if="repeatMode === PlaybackRepeatMode.Track"
                  class="absolute -top-1 -right-1 bg-indigo-500 text-slate-950 font-bold text-[10px] w-4 h-4 rounded-full flex items-center justify-center"
                >
                  1
                </span>
              </button>
            </div>
          </div>
        </section>

        <!-- Queue Items List -->
        <section
          class="lg:col-span-7 xl:col-span-8 bg-slate-900 border border-slate-800 rounded-2xl p-4 md:p-6 shadow-xl flex flex-col"
        >
          <div class="flex items-center justify-between mb-4 px-2">
            <h2 class="text-lg font-semibold text-white">Queue List</h2>
            <span class="text-xs text-slate-400">Click a track to skip to it</span>
          </div>

          <div v-if="isLoading && queue.length === 0" class="py-20 text-center text-slate-500">
            <svg
              class="w-8 h-8 animate-spin mx-auto mb-2 text-indigo-500"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
              />
            </svg>
            Fetching queue status...
          </div>

          <div
            v-else-if="queue.length === 0"
            class="py-20 text-center text-slate-500 border border-dashed border-slate-800 rounded-xl"
          >
            <p class="text-base font-medium">The queue is currently empty.</p>
          </div>

          <div v-else class="space-y-2 overflow-y-auto max-h-[600px] pr-1 custom-scrollbar">
            <div
              v-for="(item, idx) in queue"
              :key="idx"
              @click="handleSkipTo(idx)"
              class="group flex items-center justify-between p-3 rounded-xl border transition-all cursor-pointer"
              :class="[
                idx === currentIndex
                  ? 'bg-indigo-950/40 border-indigo-600/50 text-white'
                  : 'bg-slate-950/60 border-slate-800/80 text-slate-300 hover:border-slate-700 hover:bg-slate-800/50',
              ]"
            >
              <!-- Track Info -->
              <div class="flex items-center gap-3.5 min-w-0 pr-3">
                <!-- Index or Active Speaker Icon -->
                <div class="w-6 text-center text-xs font-mono text-slate-500 shrink-0">
                  <span
                    v-if="idx === currentIndex"
                    class="inline-block text-indigo-400 animate-pulse"
                  >
                    <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                      <path
                        d="M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02z"
                      />
                    </svg>
                  </span>
                  <span v-else>{{ idx + 1 }}</span>
                </div>

                <!-- Thumbnail -->
                <div
                  class="w-11 h-11 rounded-lg bg-slate-800 overflow-hidden shrink-0 border border-slate-700/50"
                >
                  <img
                    v-if="item.imageUrl"
                    :src="item.imageUrl"
                    :alt="item.title"
                    class="w-full h-full object-cover"
                  />
                  <div v-else class="w-full h-full flex items-center justify-center text-slate-600">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="1.5"
                        d="M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 .895-2 3-2 3 .895 3 2zm12 0c0 1.105-1.343 2-3 2s-3-.895-3-2 .895-2 3-2 3 .895 3 2zM9 10l12-3"
                      />
                    </svg>
                  </div>
                </div>

                <!-- Text Details -->
                <div class="min-w-0">
                  <p
                    class="text-sm font-medium truncate"
                    :class="idx === currentIndex ? 'text-indigo-300' : 'text-slate-100'"
                  >
                    {{ item.title }}
                  </p>
                  <p class="text-xs text-slate-400 truncate">
                    {{ item.artist }} <span v-if="item.album">-- {{ item.album }}</span>
                  </p>
                </div>
              </div>

              <!-- Action & Duration -->
              <div class="flex items-center gap-3 shrink-0">
                <span class="text-xs text-slate-500 font-mono hidden sm:inline-block">
                  {{ formatDuration(item.duration) }}
                </span>

                <button
                  type="button"
                  @click.stop="handleRemoveItem(idx)"
                  :disabled="isActionPending"
                  class="p-1.5 text-slate-500 hover:text-red-400 hover:bg-slate-800 rounded-lg transition-colors opacity-0 group-hover:opacity-100 focus:opacity-100"
                  title="Remove from queue"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                    />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </section>
      </div>
    </div>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #334155;
  border-radius: 4px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #475569;
}
</style>
