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
                {{ title }}
              </h3>
              <div class="ml-auto">
                <button
                  type="button"
                  class="rounded-lg p-1.5 text-gray-400 hover:text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-700 dark:hover:text-gray-300 transition-colors focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  @click="handleClose"
                >
                  <span class="sr-only">Close dialog</span>
                  <X
                    class="w-5 h-5" />
                </button>
              </div>
            </div>

            <!-- Body Slot -->
            <div class="px-6 py-4 overflow-y-auto flex-1">
              <slot></slot>
            </div>

            <!-- Footer Slot -->
            <div
              v-if="$slots.footer"
              class="px-6 py-4 bg-gray-50 dark:bg-gray-900/50 border-t border-gray-200 dark:border-gray-700 flex items-center justify-end gap-3"
            >
              <slot name="footer"></slot>
            </div>
          </div>
        </transition>
      </div>
    </transition>
  </Teleport>
</template>

<script setup>
import { watch } from 'vue'
import { X } from '@lucide/vue'
const props = defineProps({
  isOpen: {
    type: Boolean,
    required: true,
  },
  title: {
    type: String,
    default: '',
  },
})

const emit = defineEmits(['update:isOpen', 'close'])

const handleClose = () => {
  emit('update:isOpen', false)
  emit('close')
}

// Optional: Prevent background scrolling when dialog is open
watch(
  () => props.isOpen,
  (newValue) => {
    if (typeof document !== 'undefined') {
      if (newValue) {
        document.body.style.overflow = 'hidden'
      } else {
        document.body.style.overflow = ''
      }
    }
  }
)
</script>
