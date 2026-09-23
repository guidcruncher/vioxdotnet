<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  // A single line/entry from Object.entries(Record<string, string>) is a [key, value] tuple
  item: [string, string] | null
  defaultimage?: string
}

const props = withDefaults(defineProps<Props>(), {
  item: null,
  defaultimage: '/tuneinregion.png',
})

const emit = defineEmits<{
  (e: 'view', itemKey: string): void
}>()

// Extract key and value safely from the tuple
const itemKey = computed(() => (props.item ? props.item[0] : ''))
const itemValue = computed(() => (props.item ? props.item[1] : ''))

const viewItem = () => {
  if (props.item) {
    emit('view', itemKey.value)
  }
}

const handleImageError = (event: Event) => {
  const target = event.target as HTMLImageElement
  if (target && target.src !== props.defaultimage) {
    target.src = props.defaultimage
  }
}

const imageUrl = computed(() => {
  return props.defaultimage
})
</script>

<template>
  <div
    v-if="item"
    class="bg-slate-900/90 border border-slate-800/80 hover:border-slate-700/80 p-3 sm:p-3.5 rounded-xl flex flex-col justify-between h-full transition-all duration-200 group shadow-md"
  >
    <!-- Artwork & Track Details Container -->
    <div class="flex flex-col w-full">
      <!-- Fixed Square Artwork Box -->
      <div
        class="w-full aspect-square bg-slate-800 rounded-lg overflow-hidden relative shadow-inner mb-2.5"
      >
        <img
          :src="imageUrl"
          :alt="itemValue || 'Media Artwork'"
          @error="handleImageError"
          class="w-full h-full object-cover rounded-lg group-hover:scale-105 transition-transform duration-300"
        />
      </div>
      <!-- Text Details -->
      <div class="w-full min-w-0">
        <p class="font-bold text-xs sm:text-sm text-slate-100 truncate w-full" :title="itemValue">
          {{ itemValue }}
        </p>
        <p class="text-[11px] sm:text-xs text-slate-400 truncate w-full mt-0.5">
          {{ itemKey }}
        </p>
      </div>
    </div>
    <!-- Responsive Action Button -->
    <button
      @click="viewItem"
      class="mt-3 w-full bg-slate-800 hover:bg-slate-700 active:scale-95 text-xs text-slate-200 py-2 sm:py-1.5 px-3 rounded-lg font-medium border border-slate-700 transition flex items-center justify-center space-x-1.5 shrink-0 focus:outline-none"
    >
      <span>View</span>
    </button>
  </div>
</template>
