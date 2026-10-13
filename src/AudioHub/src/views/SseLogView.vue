<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted, nextTick, watch } from 'vue'
import { useServerEvents } from '@/composables/useServerEvents'

interface LogEntry {
  id: string
  timestamp: string
  raw: any
  type?: string
}

// State
const logs = ref<LogEntry[]>([])
const isPaused = ref(false)
const searchQuery = ref('')
const selectedLog = ref<LogEntry | null>(null)
const logContainerRef = ref<HTMLElement | null>(null)

// Resizable panel state (split percentage for bottom inspector or side-by-side)
const inspectorHeight = ref(280) // height in px
const isResizing = ref(false)

// Use server events composable focusing purely on global onMessage/offMessage
const { isConnected, isReconnecting, error, retryCount, onMessage, offMessage } = useServerEvents()

const handleScroll = () => {
  if (!logContainerRef.value) return
  const { scrollTop, scrollHeight, clientHeight } = logContainerRef.value
  // Optional: automatically pause autoscroll if user scrolls up
  const isNearBottom = scrollHeight - clientHeight - scrollTop < 30
  if (!isNearBottom && !isPaused.value) {
    // Uncomment below if you want auto-pause on manual scroll up
    // isPaused.value = true
  }
}

// Global message handler
const handleGlobalMessage = (event: any) => {
  const entry: LogEntry = {
    id: Math.random().toString(36).substring(2, 9),
    timestamp: new Date().toISOString(),
    raw: event,
    type: event?.type || event?.event || 'message',
  }

  logs.value.push(entry)

  // Cap logs to prevent memory bloat in long diagnostics
  if (logs.value.length > 500) {
    logs.value.shift()
  }

  if (!isPaused.value) {
    nextTick(() => {
      scrollToBottom()
    })
  }
}

onMounted(() => {
  onMessage(handleGlobalMessage)
})

onUnmounted(() => {
  offMessage(handleGlobalMessage)
})

// Scroll helper
const scrollToBottom = () => {
  if (logContainerRef.value && !isPaused.value) {
    logContainerRef.value.scrollTop = logContainerRef.value.scrollHeight
  }
}

// Filtered logs
const filteredLogs = computed(() => {
  if (!searchQuery.value.trim()) return logs.value
  const query = searchQuery.value.toLowerCase()
  return logs.value.filter((log) => {
    return (
      log.type?.toLowerCase().includes(query) ||
      JSON.stringify(log.raw).toLowerCase().includes(query) ||
      log.timestamp.includes(query)
    )
  })
})

// Actions
const clearLogs = () => {
  logs.value = []
  selectedLog.value = null
}

const togglePause = () => {
  isPaused.value = !isPaused.value
  if (!isPaused.value) {
    scrollToBottom()
  }
}

const selectLog = (log: LogEntry) => {
  selectedLog.value = selectedLog.value?.id === log.id ? null : log
}

// Resizer logic
const startResize = (e: MouseEvent) => {
  isResizing.value = true
  window.addEventListener('mousemove', onResize)
  window.addEventListener('mouseup', stopResize)
  e.preventDefault()
}

const onResize = (e: MouseEvent) => {
  if (!isResizing.value) return
  const newHeight = window.innerHeight - e.clientY
  if (newHeight > 150 && newHeight < window.innerHeight - 200) {
    inspectorHeight.value = newHeight
  }
}

const stopResize = () => {
  isResizing.value = false
  window.removeEventListener('mousemove', onResize)
  window.removeEventListener('mouseup', stopResize)
}
</script>

<template>
  <div
    class="flex flex-col h-screen w-full bg-slate-950 text-slate-100 font-sans select-none overflow-hidden"
  >
    <!-- Top Diagnostic Header / Toolbar -->
    <header
      class="flex items-center justify-between px-4 py-3 bg-slate-900 border-b border-slate-800 shadow-md"
    >
      <div class="flex items-center space-x-4">
        <h1 class="text-lg font-semibold tracking-wide flex items-center space-x-2">
          <span
            class="inline-block w-2.5 h-2.5 rounded-full"
            :class="{
              'bg-emerald-500 animate-pulse': isConnected,
              'bg-amber-500 animate-ping': isReconnecting,
              'bg-rose-500': !isConnected && !isReconnecting,
            }"
          ></span>
          <span>Events</span>
        </h1>

        <!-- Connection Badge -->
        <div
          class="flex items-center space-x-2 text-xs px-2.5 py-1 rounded-md bg-slate-800 border border-slate-700"
        >
          <span class="text-slate-400">Status:</span>
          <span
            :class="{
              'text-emerald-400 font-medium': isConnected,
              'text-amber-400 font-medium': isReconnecting,
              'text-rose-400 font-medium': !isConnected && !isReconnecting,
            }"
          >
            {{
              isConnected
                ? 'Connected'
                : isReconnecting
                  ? `Reconnecting (${retryCount})`
                  : 'Disconnected'
            }}
          </span>
          <span
            v-if="error"
            class="text-rose-400 ml-2"
            :title="error instanceof Error ? error.message : String(error)"
          >
            Error</span
          >
        </div>
      </div>

      <!-- Toolbar Actions -->
      <div class="flex items-center space-x-3">
        <div class="relative">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search events or payloads..."
            class="bg-slate-950 text-slate-200 text-xs rounded-lg px-3 py-1.5 pl-8 border border-slate-700 focus:outline-none focus:border-indigo-500 w-64"
          />
          <svg
            class="w-4 h-4 text-slate-500 absolute left-2.5 top-2"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
            />
          </svg>
        </div>

        <button
          @click="togglePause"
          class="flex items-center space-x-1.5 px-3 py-1.5 text-xs font-medium rounded-lg border transition-colors"
          :class="
            isPaused
              ? 'bg-amber-500/20 text-amber-300 border-amber-500/40 hover:bg-amber-500/30'
              : 'bg-slate-800 text-slate-300 border-slate-700 hover:bg-slate-700'
          "
        >
          <svg
            v-if="isPaused"
            class="w-3.5 h-3.5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z"
            />
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <svg v-else class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M10 9v6m4-6v6m7-3a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          <span>{{ isPaused ? 'Resume Scroll' : 'Pause Scroll' }}</span>
        </button>

        <button
          @click="clearLogs"
          class="px-3 py-1.5 text-xs font-medium bg-slate-800 hover:bg-rose-950/40 hover:text-rose-400 hover:border-rose-900/50 text-slate-300 rounded-lg border border-slate-700 transition-colors"
        >
          Clear Logs
        </button>

        <div class="text-xs text-slate-400 pl-2 border-l border-slate-800">
          Total: <span class="text-slate-200 font-mono font-medium">{{ filteredLogs.length }}</span>
        </div>
      </div>
    </header>

    <!-- Main Log Stream Area -->
    <div
      ref="logContainerRef"
      class="flex-1 overflow-y-auto font-mono text-xs divide-y divide-slate-900/80 relative"
      @scroll="handleScroll"
    >
      <div
        v-if="filteredLogs.length === 0"
        class="flex flex-col items-center justify-center h-full text-slate-500 space-y-2"
      >
        <svg
          class="w-10 h-10 stroke-slate-700"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="1.5"
            d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"
          />
        </svg>
        <p>Listening for global SSE messages (`onMessage`)...</p>
      </div>

      <div
        v-for="log in filteredLogs"
        :key="log.id"
        @click="selectLog(log)"
        class="flex items-start px-4 py-2 hover:bg-slate-900/80 cursor-pointer transition-colors group"
        :class="{ 'bg-slate-900 border-l-2 border-indigo-500': selectedLog?.id === log.id }"
      >
        <span class="text-slate-500 w-24 shrink-0 select-none">{{
          log.timestamp.split('T')[1]?.slice(0, 12)
        }}</span>
        <span
          class="px-2 py-0.5 rounded text-[10px] font-semibold mr-3 uppercase shrink-0 tracking-wider"
          :class="{
            'bg-indigo-950 text-indigo-300 border border-indigo-800/50': log.type === 'message',
            'bg-cyan-950 text-cyan-300 border border-cyan-800/50': log.type !== 'message',
          }"
        >
          {{ log.type }}
        </span>
        <span class="text-slate-300 truncate flex-1 select-text">
          {{ JSON.stringify(log.raw) }}
        </span>
      </div>
    </div>

    <!-- Pause Indicator Badge Overlay -->
    <transition name="fade">
      <div
        v-if="isPaused"
        class="absolute bottom-16 right-6 bg-amber-500/95 text-slate-950 font-semibold px-3 py-1.5 rounded-full shadow-lg text-xs flex items-center space-x-2 cursor-pointer z-20 backdrop-blur"
        @click="togglePause"
      >
        <span class="w-2 h-2 rounded-full bg-slate-950 animate-ping"></span>
        <span>Autoscroll Paused</span>
      </div>
    </transition>

    <!-- Resizable Bottom Inspector Pane -->
    <div
      v-if="selectedLog"
      class="flex flex-col bg-slate-900 border-t border-slate-800 relative z-10"
      :style="{ height: `${inspectorHeight}px` }"
    >
      <!-- Drag Handle -->
      <div
        @mousedown="startResize"
        class="absolute top-0 left-0 right-0 h-1.5 cursor-row-resize hover:bg-indigo-500 transition-colors z-30"
        title="Drag to resize inspector"
      ></div>

      <!-- Inspector Header -->
      <div
        class="flex items-center justify-between px-4 py-2 bg-slate-900/90 border-b border-slate-800 text-xs"
      >
        <div class="flex items-center space-x-2 font-mono">
          <span class="text-slate-400">Inspecting Event:</span>
          <span class="text-indigo-400 font-semibold">{{ selectedLog.type }}</span>
          <span class="text-slate-600">|</span>
          <span class="text-slate-500">{{ selectedLog.timestamp }}</span>
        </div>
        <button
          @click="selectedLog = null"
          class="text-slate-400 hover:text-slate-100 p-1 rounded hover:bg-slate-800"
        >
          ✕
        </button>
      </div>

      <!-- Inspector Content -->
      <div
        class="flex-1 overflow-auto p-4 font-mono text-xs bg-slate-950 text-slate-300 select-text"
      >
        <pre class="whitespace-pre-wrap leading-relaxed">{{
          JSON.stringify(selectedLog.raw, null, 2)
        }}</pre>
      </div>
    </div>
  </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
