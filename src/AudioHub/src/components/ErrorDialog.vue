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
      <div
        v-if="isOpen"
        class="fixed inset-0 z-50 flex items-center justify-center p-4"
        role="alertdialog"
        aria-modal="true"
        :aria-labelledby="title ? 'error-dialog-title' : undefined"
        :aria-describedby="errorMessage ? 'error-dialog-description' : undefined"
      >
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
            class="relative w-full max-w-lg rounded-2xl bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 shadow-2xl border border-red-200 dark:border-red-900/50 overflow-hidden z-10 flex flex-col max-h-[90vh]"
          >
            <!-- Top Red Accent Bar -->
            <div class="h-1.5 w-full bg-red-600 dark:bg-red-500"></div>

            <!-- Header -->
            <div class="flex items-center justify-between px-6 pt-5 pb-3 relative">
              <div class="flex items-center gap-3 pr-8">
                <!-- Alert Icon -->
                <div
                  class="flex-shrink-0 flex items-center justify-center w-10 h-10 rounded-full bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400"
                >
                  <AlertTriangle class="w-5 h-5" />
                </div>
                <h3
                  id="error-dialog-title"
                  class="text-lg font-semibold tracking-tight truncate text-gray-900 dark:text-gray-100"
                >
                  {{ title }}
                </h3>
              </div>

              <!-- Close Button -->
              <button
                type="button"
                class="rounded-lg p-1.5 text-gray-400 hover:text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-700 dark:hover:text-gray-300 transition-colors focus:outline-none focus:ring-2 focus:ring-red-500"
                @click="handleClose"
              >
                <span class="sr-only">Close dialog</span>
                <X class="w-5 h-5" />
              </button>
            </div>

            <!-- Body Content -->
            <div class="px-6 py-3 overflow-y-auto flex-1 space-y-4">
              <!-- Default slot or message -->
              <div id="error-dialog-description">
                <slot>
                  <p v-if="errorMessage" class="text-sm text-gray-600 dark:text-gray-300">
                    {{ errorMessage }}
                  </p>
                </slot>
              </div>

              <!-- Collapsible Details Block -->
              <div v-if="errorDetails" class="pt-2">
                <button
                  type="button"
                  class="flex items-center gap-1.5 text-xs font-medium text-red-600 dark:text-red-400 hover:underline focus:outline-none"
                  @click="showDetails = !showDetails"
                >
                  <span>{{
                    showDetails ? 'Hide technical details' : 'Show technical details'
                  }}</span>
                  <ChevronDown
                    class="w-4 h-4 transition-transform duration-200"
                    :class="{ 'rotate-180': showDetails }"
                  />
                </button>

                <div v-if="showDetails" class="mt-2">
                  <pre
                    class="p-3 bg-gray-100 dark:bg-gray-900 text-red-800 dark:text-red-300 rounded-lg text-xs font-mono overflow-x-auto max-h-40 border border-gray-200 dark:border-gray-700 whitespace-pre-wrap break-all"
                    >{{ formattedErrorDetails }}</pre>
                </div>
              </div>
            </div>

            <!-- Footer -->
            <div
              class="px-6 py-4 bg-gray-50 dark:bg-gray-900/50 border-t border-gray-200 dark:border-gray-700 flex items-center justify-end gap-3"
            >
              <slot name="footer">
                <button
                  type="button"
                  class="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-red-500 transition-colors"
                  @click="handleClose"
                >
                  {{ dismissText }}
                </button>
                <button
                  v-if="showRetry"
                  type="button"
                  class="px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 transition-colors"
                  @click="handleRetry"
                >
                  {{ retryText }}
                </button>
              </slot>
            </div>
          </div>
        </transition>
      </div>
    </transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { X, AlertTriangle, ChevronDown } from '@lucide/vue'

interface Props {
  isOpen: boolean
  title?: string
  errorMessage?: string
  errorDetails?: string | Record<string, unknown> | Error | null
  showRetry?: boolean
  retryText?: string
  dismissText?: string
}

const props = withDefaults(defineProps<Props>(), {
  title: 'An Error Occurred',
  errorMessage: '',
  errorDetails: null,
  showRetry: false,
  retryText: 'Try Again',
  dismissText: 'Dismiss',
})

const emit = defineEmits<{
  (e: 'update:isOpen', value: boolean): void
  (e: 'close'): void
  (e: 'retry'): void
}>()

const showDetails = ref<boolean>(false)

// Properly format errorDetails for output inside <pre>
const formattedErrorDetails = computed(() => {
  if (!props.errorDetails) return ''
  if (props.errorDetails instanceof Error) {
    return props.errorDetails.stack || props.errorDetails.message
  }
  if (typeof props.errorDetails === 'object') {
    return JSON.stringify(props.errorDetails, null, 2)
  }
  return String(props.errorDetails)
})

const handleClose = (): void => {
  emit('update:isOpen', false)
  emit('close')
}

const handleRetry = (): void => {
  emit('retry')
}

// Prevent background scrolling when dialog is open
watch(
  () => props.isOpen,
  (newValue: boolean) => {
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
