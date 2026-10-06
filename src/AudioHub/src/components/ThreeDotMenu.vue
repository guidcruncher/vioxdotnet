<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'

export interface MenuItem {
  id: string
  label: string
}

const props = defineProps<{
  items: MenuItem[]
}>()

const emit = defineEmits<{
  (e: 'click', item: MenuItem): void
}>()

const open = ref(false)
const root = ref<HTMLElement | null>(null)

function toggle(): void {
  open.value = !open.value
}

function handleItemClick(item: MenuItem): void {
  emit('click', item)
  open.value = false
}

function handleClickOutside(e: MouseEvent): void {
  if (root.value && !root.value.contains(e.target as Node)) {
    open.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<template>
  <div ref="root" class="relative inline-block text-left">
    <!-- Trigger button -->
    <button
      type="button"
      @click="toggle"
      class="p-2 rounded-full text-gray-300 hover:text-white hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-indigo-500"
    >
      <span class="sr-only">Open menu</span>

      <!-- 3-dot icon -->
      <svg class="h-5 w-5" fill="currentColor" viewBox="0 0 20 20">
        <circle cx="4" cy="10" r="1.5" />
        <circle cx="10" cy="10" r="1.5" />
        <circle cx="16" cy="10" r="1.5" />
      </svg>
    </button>

    <!-- Dropdown -->
    <transition
      enter-active-class="transition ease-out duration-100"
      enter-from-class="transform opacity-0 scale-95"
      enter-to-class="transform opacity-100 scale-100"
      leave-active-class="transition ease-in duration-75"
      leave-from-class="transform opacity-100 scale-100"
      leave-to-class="transform opacity-0 scale-95"
    >
      <div
        v-if="open"
        class="absolute right-0 mt-2 w-40 rounded-md shadow-lg bg-gray-800 ring-1 ring-black ring-opacity-5 focus:outline-none py-1"
      >
        <button
          v-for="item in items"
          :key="item.id"
          @click="handleItemClick(item)"
          class="w-full text-left px-4 py-2 text-sm text-gray-200 hover:bg-gray-700 hover:text-white"
        >
          {{ item.label }}
        </button>
      </div>
    </transition>
  </div>
</template>

<style scoped>
button {
  cursor: pointer;
}
</style>
