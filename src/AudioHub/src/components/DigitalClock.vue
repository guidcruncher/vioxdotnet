TL:DR Converted the standalone HTML/JS digital clock into a modular Vue 3 Single File Component
(SFC) where formatting preferences (24-hour mode, seconds visibility, timezone, and locale) are
fully controlled via props.
<template>
  <main
    class="w-full max-w-4xl mx-auto px-4 py-8 flex flex-col items-center justify-center flex-grow z-10"
  >
    <!-- Glassmorphism Card -->
    <div
      class="w-full bg-slate-900/40 backdrop-blur-2xl border border-slate-700/40 rounded-3xl p-8 sm:p-12 glow-box relative overflow-hidden transition-all duration-500"
    >
      <!-- Ambient glowing blur elements behind card -->
      <div
        class="absolute -top-24 -left-24 w-72 h-72 bg-indigo-600/20 rounded-full blur-3xl pointer-events-none"
      ></div>
      <div
        class="absolute -bottom-24 -right-24 w-72 h-72 bg-purple-600/20 rounded-full blur-3xl pointer-events-none"
      ></div>

      <!-- Date Display Header inside Card -->
      <div
        class="flex flex-col sm:flex-row justify-between items-center border-b border-slate-800/80 pb-6 mb-8 gap-4"
      >
        <div class="flex items-center space-x-3">
          <div class="p-2.5 rounded-xl bg-slate-800/60 border border-slate-700/50 text-indigo-400">
            <i class="fa-regular fa-calendar-days text-xl"></i>
          </div>
          <div>
            <div class="text-sm font-semibold tracking-wider text-indigo-300 uppercase">
              {{ dayName }}
            </div>
            <div class="text-xl sm:text-2xl font-bold tracking-tight text-slate-100">
              {{ fullDate }}
            </div>
          </div>
        </div>

        <!-- Timezone Badge -->
        <div
          class="flex items-center space-x-2 bg-slate-800/40 border border-slate-700/40 px-4 py-2 rounded-xl text-xs font-medium text-slate-300 backdrop-blur-sm"
        >
          <i class="fa-solid fa-globe text-indigo-400"></i>
          <span>{{ resolvedTimezone }}</span>
        </div>
      </div>

      <!-- Main Clock Time Digits -->
      <div class="flex flex-col items-center justify-center my-6 sm:my-10">
        <div class="flex items-center justify-center space-x-2 sm:space-x-4">
          <!-- Hours -->
          <div class="flex flex-col items-center">
            <div
              class="bg-slate-950/60 border border-slate-800/80 rounded-2xl px-4 sm:px-8 py-4 sm:py-6 backdrop-blur-md shadow-2xl"
            >
              <span
                class="font-mono text-5xl sm:text-8xl font-bold tracking-tight glow-text text-white"
                >{{ formattedHours }}</span
              >
            </div>
            <span
              class="text-[10px] sm:text-xs uppercase tracking-widest text-slate-400 mt-2 font-semibold"
              >Hours</span
            >
          </div>

          <span class="font-mono text-4xl sm:text-7xl font-bold text-slate-600 pb-6 animate-pulse"
            >:</span
          >

          <!-- Minutes -->
          <div class="flex flex-col items-center">
            <div
              class="bg-slate-950/60 border border-slate-800/80 rounded-2xl px-4 sm:px-8 py-4 sm:py-6 backdrop-blur-md shadow-2xl"
            >
              <span
                class="font-mono text-5xl sm:text-8xl font-bold tracking-tight glow-text text-white"
                >{{ formattedMinutes }}</span
              >
            </div>
            <span
              class="text-[10px] sm:text-xs uppercase tracking-widest text-slate-400 mt-2 font-semibold"
              >Minutes</span
            >
          </div>

          <!-- Seconds Container -->
          <div
            v-if="showSeconds"
            class="flex items-center space-x-2 sm:space-x-4 transition-all duration-300"
          >
            <span class="font-mono text-4xl sm:text-7xl font-bold text-slate-600 pb-6 animate-pulse"
              >:</span
            >
            <div class="flex flex-col items-center">
              <div
                class="bg-slate-950/60 border border-slate-800/80 rounded-2xl px-4 sm:px-8 py-4 sm:py-6 backdrop-blur-md shadow-2xl"
              >
                <span
                  class="font-mono text-5xl sm:text-8xl font-bold tracking-tight text-indigo-400"
                  >{{ formattedSeconds }}</span
                >
              </div>
              <span
                class="text-[10px] sm:text-xs uppercase tracking-widest text-slate-400 mt-2 font-semibold"
                >Seconds</span
              >
            </div>
          </div>

          <!-- AM / PM Indicator -->
          <div v-if="!is24Hour" class="flex flex-col justify-center pl-2 sm:pl-4">
            <div
              class="bg-indigo-950/40 border border-indigo-500/30 rounded-xl px-3 sm:px-5 py-3 sm:py-4 backdrop-blur-md flex flex-col items-center justify-center shadow-lg shadow-indigo-950/50"
            >
              <span class="font-mono text-lg sm:text-2xl font-bold text-indigo-300">{{
                ampm
              }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </main>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  is24Hour: {
    type: Boolean,
    default: false,
  },
  showSeconds: {
    type: Boolean,
    default: true,
  },
  timezone: {
    type: String,
    default: '',
  },
  locale: {
    type: String,
    default: 'en-US',
  },
})

const currentTime = ref(new Date())
let timer = null

onMounted(() => {
  timer = setInterval(() => {
    currentTime.value = new Date()
  }, 1000)
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})

const resolvedTimezone = computed(() => {
  if (props.timezone) return props.timezone.replace('_', ' ')
  return Intl.DateTimeFormat().resolvedOptions().timeZone.replace('_', ' ')
})

const dayName = computed(() => {
  try {
    return new Intl.DateTimeFormat(props.locale, {
      weekday: 'long',
      timeZone: props.timezone || undefined,
    }).format(currentTime.value)
  } catch {
    return currentTime.value.toLocaleDateString(props.locale, { weekday: 'long' })
  }
})

const fullDate = computed(() => {
  try {
    return new Intl.DateTimeFormat(props.locale, {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      timeZone: props.timezone || undefined,
    }).format(currentTime.value)
  } catch {
    return currentTime.value.toLocaleDateString(props.locale, {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    })
  }
})

const hours = computed(() => {
  let h = currentTime.value.getHours()
  if (props.timezone) {
    try {
      const tzString = new Intl.DateTimeFormat('en-US', {
        timeZone: props.timezone,
        hour: 'numeric',
        hour12: false,
      }).format(currentTime.value)
      h = parseInt(tzString, 10)
    } catch {
      // Fallback to local
    }
  }

  if (!props.is24Hour) {
    h = h % 12
    h = h ? h : 12
  }
  return h
})

const minutes = computed(() => {
  let m = currentTime.value.getMinutes()
  if (props.timezone) {
    try {
      const tzString = new Intl.DateTimeFormat('en-US', {
        timeZone: props.timezone,
        minute: 'numeric',
      }).format(currentTime.value)
      m = parseInt(tzString, 10)
    } catch {
      // Fallback
    }
  }
  return m
})

const seconds = computed(() => {
  let s = currentTime.value.getSeconds()
  if (props.timezone) {
    try {
      const tzString = new Intl.DateTimeFormat('en-US', {
        timeZone: props.timezone,
        second: 'numeric',
      }).format(currentTime.value)
      s = parseInt(tzString, 10)
    } catch {
      // Fallback
    }
  }
  return s
})

const ampm = computed(() => {
  let h = currentTime.value.getHours()
  if (props.timezone) {
    try {
      const tzString = new Intl.DateTimeFormat('en-US', {
        timeZone: props.timezone,
        hour: 'numeric',
        hour12: false,
      }).format(currentTime.value)
      h = parseInt(tzString, 10)
    } catch {
      // Fallback
    }
  }
  return h >= 12 ? 'PM' : 'AM'
})

const formattedHours = computed(() => String(hours.value).padStart(2, '0'))
const formattedMinutes = computed(() => String(minutes.value).padStart(2, '0'))
const formattedSeconds = computed(() => String(seconds.value).padStart(2, '0'))
</script>

<style scoped>
@keyframes meshGradient {
  0% {
    background-position: 0% 50%;
  }
  50% {
    background-position: 100% 50%;
  }
  100% {
    background-position: 0% 50%;
  }
}

.mesh-bg {
  background: linear-gradient(135deg, #09090b, #18181b, #27272a, #0f172a, #1e1b4b);
  background-size: 300% 300%;
  animation: meshGradient 18s ease infinite;
}

.glow-box {
  box-shadow:
    0 0 40px -10px rgba(99, 102, 241, 0.25),
    0 0 80px -20px rgba(168, 85, 247, 0.2);
}

.glow-text {
  text-shadow: 0 0 20px rgba(129, 140, 248, 0.4);
}
</style>
