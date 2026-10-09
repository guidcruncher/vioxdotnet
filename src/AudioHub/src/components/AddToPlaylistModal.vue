<template>
  <Teleport to="body">
    <transition
      enter-active-class="transition duration-300 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition duration-200 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <!-- Backdrop with blur and fade -->
        <div
          class="fixed inset-0 bg-gray-900/60 backdrop-blur-sm transition-opacity"
          @click="handleClose"
        ></div>

        <!-- Dialog Box -->
        <transition
          enter-active-class="transition duration-300 ease-out"
          enter-from-class="opacity-0 scale-95 translate-y-4"
          enter-to-class="opacity-100 scale-100 translate-y-0"
          leave-active-class="transition duration-200 ease-in"
          leave-from-class="opacity-100 scale-100 translate-y-0"
          leave-to-class="opacity-0 scale-95 translate-y-4"
        >
          <div
            v-if="isOpen"
            class="relative w-full max-w-lg rounded-2xl bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 shadow-2xl border border-gray-200 dark:border-gray-700 overflow-hidden z-10 flex flex-col max-h-[90vh]"
          >
            <!-- Header -->
            <div
              class="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700 relative"
            >
              <h3
                class="absolute left-1/2 -translate-x-1/2 text-lg font-semibold tracking-tight truncate max-w-[65%]"
              >
                Add to Playlist
              </h3>
              <div class="ml-auto">
                <button
                  type="button"
                  class="rounded-lg p-1.5 text-gray-400 hover:text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-700 dark:hover:text-gray-300 transition-colors focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  @click="handleClose"
                >
                  <span class="sr-only">Close dialog</span>
                  <X class="w-5 h-5" />
                </button>
              </div>
            </div>

            <!-- Body -->
            <div class="px-6 py-4 overflow-y-auto flex-1 space-y-4">
              <!-- Media Item Info Preview -->
              <div
                v-if="item"
                class="flex items-center gap-3 p-3 bg-gray-50 dark:bg-gray-900/40 rounded-xl border border-gray-100 dark:border-gray-700/50"
              >
                <img
                  v-if="item.artUri || item.thumbnail"
                  :src="item.artUri || item.thumbnail"
                  alt="Track artwork"
                  class="w-12 h-12 rounded-lg object-cover bg-gray-200 dark:bg-gray-700 flex-shrink-0"
                />
                <div
                  v-else
                  class="w-12 h-12 rounded-lg bg-indigo-50 dark:bg-indigo-950/50 text-indigo-500 flex items-center justify-center flex-shrink-0"
                >
                  <svg
                    class="w-6 h-6"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke-width="2"
                    stroke="currentColor"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      d="M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3"
                    />
                  </svg>
                </div>
                <div class="min-w-0 flex-1">
                  <h4 class="text-sm font-medium text-gray-900 dark:text-gray-100 truncate">
                    {{ item.title || 'Untitled Track' }}
                  </h4>
                  <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
                    {{ item.artist || item.subTitle || 'Unknown Artist' }}
                  </p>
                </div>
              </div>

              <!-- Mode Toggle Tabs -->
              <div class="flex rounded-lg bg-gray-100 dark:bg-gray-900 p-1">
                <button
                  type="button"
                  class="flex-1 py-1.5 text-xs font-medium rounded-md transition-all"
                  :class="
                    mode === 'existing'
                      ? 'bg-white dark:bg-gray-800 text-gray-900 dark:text-white shadow-sm'
                      : 'text-gray-500 hover:text-gray-900 dark:hover:text-white'
                  "
                  @click="mode = 'existing'"
                >
                  Existing Playlist
                </button>
                <button
                  type="button"
                  class="flex-1 py-1.5 text-xs font-medium rounded-md transition-all"
                  :class="
                    mode === 'new'
                      ? 'bg-white dark:bg-gray-800 text-gray-900 dark:text-white shadow-sm'
                      : 'text-gray-500 hover:text-gray-900 dark:hover:text-white'
                  "
                  @click="mode = 'new'"
                >
                  Create New Playlist
                </button>
              </div>

              <!-- Loading Indicator -->
              <div v-if="loading" class="py-8 flex justify-center items-center">
                <svg class="animate-spin h-6 w-6 text-indigo-500" fill="none" viewBox="0 0 24 24">
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
              </div>

              <!-- Existing Playlists Selection List -->
              <div v-else-if="mode === 'existing'" class="space-y-2">
                <label
                  class="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider"
                >
                  Select Playlist
                </label>
                <div
                  v-if="Object.keys(playlists).length === 0"
                  class="text-sm text-gray-500 dark:text-gray-400 py-6 text-center"
                >
                  No playlists found. Create your first one!
                </div>
                <div v-else class="max-h-48 overflow-y-auto space-y-1 pr-1">
                  <div
                    v-for="(name, id) in playlists"
                    :key="id"
                    class="flex items-center justify-between p-3 rounded-xl border transition-all cursor-pointer"
                    :class="
                      selectedPlaylistId === id
                        ? 'border-indigo-500 bg-indigo-50/50 dark:bg-indigo-950/20 text-indigo-900 dark:text-indigo-200'
                        : 'border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700/50'
                    "
                    @click="selectedPlaylistId = id"
                  >
                    <span class="text-sm font-medium truncate">{{ name }}</span>
                    <div
                      class="w-4 h-4 rounded-full border flex items-center justify-center flex-shrink-0"
                      :class="
                        selectedPlaylistId === id
                          ? 'border-indigo-600 bg-indigo-600 text-white'
                          : 'border-gray-300 dark:border-gray-600'
                      "
                    >
                      <div
                        v-if="selectedPlaylistId === id"
                        class="w-1.5 h-1.5 rounded-full bg-white"
                      ></div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- New Playlist Input Field -->
              <div v-else class="space-y-2">
                <label
                  class="block text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider"
                  for="new-playlist-title"
                >
                  Playlist Title
                </label>
                <input
                  id="new-playlist-title"
                  v-model="newPlaylistTitle"
                  type="text"
                  placeholder="e.g., Chill Vibes, Workout Mix..."
                  class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-transparent text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  @keyup.enter="handleSubmit"
                />
              </div>

              <!-- Error Banner -->
              <div
                v-if="errorMessage"
                class="p-3 bg-red-50 dark:bg-red-950/50 border border-red-200 dark:border-red-800 text-red-600 dark:text-red-400 text-xs rounded-xl"
              >
                {{ errorMessage }}
              </div>
            </div>

            <!-- Footer Actions -->
            <div
              class="px-6 py-4 bg-gray-50 dark:bg-gray-900/50 border-t border-gray-200 dark:border-gray-700 flex items-center justify-end gap-3"
            >
              <button
                type="button"
                class="px-4 py-2 rounded-xl text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                @click="handleClose"
              >
                Cancel
              </button>
              <button
                type="button"
                class="px-4 py-2 rounded-xl text-sm font-medium bg-indigo-600 hover:bg-indigo-700 text-white shadow-sm transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                :disabled="
                  isSubmitting ||
                  (mode === 'existing' && !selectedPlaylistId) ||
                  (mode === 'new' && !newPlaylistTitle.trim())
                "
                @click="handleSubmit"
              >
                <svg
                  v-if="isSubmitting"
                  class="animate-spin h-4 w-4 text-white"
                  fill="none"
                  viewBox="0 0 24 24"
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
                <span>Add Item</span>
              </button>
            </div>
          </div>
        </transition>
      </div>
    </transition>
  </Teleport>
</template>

<script setup>
import { X } from '@lucide/vue'
import { ref, watch } from 'vue'
import { useApiClient } from '@/composables/useApiClient'

const props = defineProps({
  isOpen: {
    type: Boolean,
    required: true,
  },
  item: {
    type: Object,
    required: true, // Expects MediaMetaData object
  },
})

const emit = defineEmits(['update:isOpen', 'close', 'success'])

const apiClient = useApiClient()

// Component State
const mode = ref('existing')
const playlists = ref({})
const selectedPlaylistId = ref('')
const newPlaylistTitle = ref('')
const loading = ref(false)
const isSubmitting = ref(false)
const errorMessage = ref('')

/**
 * Watch modal open state to refresh playlists and clear form state on open.
 */
watch(
  () => props.isOpen,
  async (isOpen) => {
    if (isOpen) {
      mode.value = 'existing'
      selectedPlaylistId.value = ''
      newPlaylistTitle.value = ''
      errorMessage.value = ''

      await refreshPlaylists()
    }
  }
)

const refreshPlaylists = async () => {
  loading.value = true
  errorMessage.value = ''
  try {
    playlists.value = await apiClient.playlists.getAll()
  } catch (err) {
    console.error('Failed to load playlists:', err)
    errorMessage.value = 'Could not refresh playlists. Please check your connection.'
  } finally {
    loading.value = false
  }
}

const handleSubmit = async () => {
  if (!props.item) return
  isSubmitting.value = true
  errorMessage.value = ''

  try {
    if (mode.value === 'existing') {
      if (!selectedPlaylistId.value) return
      await apiClient.playlists.addItem(selectedPlaylistId.value, props.item)
    } else {
      const title = newPlaylistTitle.value.trim()
      if (!title) return
      await apiClient.playlists.createPlaylist(title, props.item)
    }

    emit('success')
    handleClose()
  } catch (err) {
    console.error('Failed to save media item to playlist:', err)
    errorMessage.value = 'Failed to process request. Please try again.'
  } finally {
    isSubmitting.value = false
  }
}

const handleClose = () => {
  emit('update:isOpen', false)
  emit('close')
}
</script>
