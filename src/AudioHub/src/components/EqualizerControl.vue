<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import type { EqualizerBand } from '@/types'

// Initialize API client and state
const api = useApiClient()

const bands = ref<EqualizerBand[]>([])
const presets = ref<Record<string, number[]>>({})
const selectedPreset = ref<string>('')
const isLoading = ref<boolean>(true)
const isSaving = ref<boolean>(false)
const errorMessage = ref<string | null>(null)
const linkChannels = ref<boolean>(true)

// Debounce timer registry per band index using browser-safe return type
const debounceTimers = new Map<number, ReturnType<typeof setTimeout>>()

// Load equalizer bands and dynamic presets on mount
onMounted(async () => {
  await Promise.all([fetchBands(), fetchPresets()])
})

async function fetchBands(): Promise<void> {
  isLoading.value = true
  errorMessage.value = null
  try {
    bands.value = await api.equalizer.getBands()
  } catch (err) {
    errorMessage.value = 'Failed to load equalizer settings.'
  } finally {
    isLoading.value = false
  }
}

async function fetchPresets(): Promise<void> {
  try {
    presets.value = await api.equalizer.getPresets()
  } catch (err) {
    errorMessage.value = 'Failed to load equalizer presets.'
  }
}

// Handle slider changes for individual bands with debounced API execution
function handleBandInput(index: number, channel: 'left' | 'right', value: number): void {
  const band = bands.value.find((b) => b.index === index)
  if (!band) return

  // Unset selected preset if user manually adjusts sliders
  selectedPreset.value = ''

  if (channel === 'left') {
    band.leftPercentage = value
    if (linkChannels.value) {
      band.rightPercentage = value
    }
  } else {
    band.rightPercentage = value
    if (linkChannels.value) {
      band.leftPercentage = value
    }
  }

  // Target value sent to API (averages channels if unlinked, or uses left if linked)
  const targetValue = linkChannels.value
    ? band.leftPercentage
    : Math.round((band.leftPercentage + band.rightPercentage) / 2)

  // Debounce API calls per band
  const existingTimer = debounceTimers.get(index)
  if (existingTimer !== undefined) {
    clearTimeout(existingTimer)
  }

  const timer = setTimeout(async () => {
    try {
      await api.equalizer.setBand(index, targetValue)
    } catch (err) {
      errorMessage.value = `Failed to update band ${band.frequencyLabel}`
    } finally {
      debounceTimers.delete(index)
    }
  }, 300)

  debounceTimers.set(index, timer)
}

// Dynamic preset application triggered by dropdown selection
async function handlePresetChange(event: Event): Promise<void> {
  const presetName = (event.target as HTMLSelectElement).value
  if (!presetName || bands.value.length === 0) return

  const targetValues = presets.value[presetName]
  if (!targetValues) return

  // Map preset values onto local bands array
  bands.value.forEach((band, idx) => {
    const val = targetValues[idx] ?? 50
    band.leftPercentage = val
    band.rightPercentage = val
  })

  // Apply batch update via API
  isSaving.value = true
  errorMessage.value = null
  try {
    const allPercentages = bands.value.map((b) => b.leftPercentage)
    await api.equalizer.setBands(allPercentages)
  } catch (err) {
    errorMessage.value = 'Failed to apply equalizer preset.'
  } finally {
    isSaving.value = false
  }
}

// Capitalize preset names for UI display (e.g. 'bass' -> 'Bass')
function formatPresetName(name: string): string {
  return name.charAt(0).toUpperCase() + name.slice(1)
}

// Reset all bands to default mid point (50%)
async function resetEqualizer(): Promise<void> {
  selectedPreset.value = ''
  bands.value.forEach((band) => {
    band.leftPercentage = 50
    band.rightPercentage = 50
  })

  isSaving.value = true
  errorMessage.value = null
  try {
    const allPercentages = bands.value.map(() => 50)
    await api.equalizer.setBands(allPercentages)
  } catch (err) {
    errorMessage.value = 'Failed to reset equalizer.'
  } finally {
    isSaving.value = false
  }
}
</script>

<template>
  <div
    class="w-full max-w-4xl rounded-xl border border-slate-800 bg-slate-900 p-4 sm:p-6 text-slate-100 shadow-2xl"
  >
    <!-- Header -->
    <div
      class="mb-6 flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-800 pb-4"
    >
      <div>
        <h2 class="text-xl font-bold tracking-wide text-white">10-Band Equalizer</h2>
        <p class="text-xs text-slate-400">Adjust audio frequencies and balance</p>
      </div>

      <!-- Responsive EQ Preset Dropdown -->
      <div class="flex items-center gap-2 w-full sm:w-auto">
        <label for="eq-preset" class="text-xs font-medium text-slate-400 shrink-0"> Preset: </label>
        <div class="relative w-full sm:w-48">
          <select
            id="eq-preset"
            v-model="selectedPreset"
            class="w-full appearance-none rounded-lg border border-slate-700 bg-slate-800 py-2 pl-3 pr-8 text-xs font-medium text-slate-200 transition-colors hover:border-slate-600 focus:border-cyan-500 focus:outline-none focus:ring-1 focus:ring-cyan-500 disabled:cursor-not-allowed disabled:opacity-50"
            :disabled="isLoading || isSaving || Object.keys(presets).length === 0"
            @change="handlePresetChange"
          >
            <option value="" disabled selected>Select preset...</option>
            <option
              v-for="(values, name) in presets"
              :key="name"
              :value="name"
              class="bg-slate-900 text-slate-200"
            >
              {{ formatPresetName(name) }}
            </option>
          </select>

          <!-- Custom SVG Chevron Arrow -->
          <div
            class="pointer-events-none absolute inset-y-0 right-0 flex items-center px-2.5 text-slate-400"
          >
            <svg
              class="h-4 w-4 fill-current"
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 20 20"
            >
              <path
                fill-rule="evenodd"
                d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z"
                clip-rule="evenodd"
              />
            </svg>
          </div>
        </div>
      </div>
    </div>

    <!-- Error Alert -->
    <div
      v-if="errorMessage"
      class="mb-4 rounded-lg border border-red-500/30 bg-red-500/10 p-3 text-xs text-red-400"
    >
      {{ errorMessage }}
    </div>

    <!-- Loading Skeleton -->
    <div v-if="isLoading" class="flex h-64 items-center justify-center space-x-6">
      <div v-for="n in 10" :key="n" class="flex flex-col items-center space-y-3">
        <div class="h-40 w-3 animate-pulse rounded-full bg-slate-800"></div>
        <div class="h-3 w-8 animate-pulse rounded bg-slate-800"></div>
      </div>
    </div>

    <!-- Equalizer Grid -->
    <div v-else-if="bands.length > 0" class="flex flex-col gap-6">
      <!-- Sliders Container -->
      <div class="grid grid-cols-5 gap-2 sm:gap-4 sm:grid-cols-10">
        <div
          v-for="band in bands"
          :key="band.index"
          class="flex flex-col items-center rounded-lg bg-slate-950/50 p-2 sm:p-3 transition-colors hover:bg-slate-950"
        >
          <!-- Value Readout -->
          <span class="mb-2 font-mono text-[10px] sm:text-[11px] text-cyan-400">
            {{
              linkChannels
                ? `${band.leftPercentage}%`
                : `L:${band.leftPercentage}% R:${band.rightPercentage}%`
            }}
          </span>

          <!-- Slider Pair / Single Slider -->
          <div class="flex h-48 items-center gap-2">
            <!-- Left Channel Slider -->
            <input
              type="range"
              min="0"
              max="100"
              :value="band.leftPercentage"
              class="h-40 w-2 cursor-pointer appearance-none rounded-lg bg-slate-800 accent-cyan-500 [writing-mode:vertical-lr] [direction:rtl]"
              @input="
                (e) =>
                  handleBandInput(band.index, 'left', Number((e.target as HTMLInputElement).value))
              "
            />

            <!-- Right Channel Slider (When Unlinked) -->
            <input
              v-if="!linkChannels"
              type="range"
              min="0"
              max="100"
              :value="band.rightPercentage"
              class="h-40 w-2 cursor-pointer appearance-none rounded-lg bg-slate-800 accent-emerald-500 [writing-mode:vertical-lr] [direction:rtl]"
              @input="
                (e) =>
                  handleBandInput(band.index, 'right', Number((e.target as HTMLInputElement).value))
              "
            />
          </div>

          <!-- Frequency Label -->
          <span class="mt-3 text-xs font-semibold text-slate-300">
            {{ band.frequencyLabel }}
          </span>
        </div>
      </div>

      <!-- Controls & Footer Options -->
      <div class="flex items-center justify-between border-t border-slate-800 pt-4">
        <!-- Channel Link Toggle -->
        <label
          class="inline-flex cursor-pointer items-center gap-2 text-xs font-medium text-slate-400"
        >
          <input
            v-model="linkChannels"
            type="checkbox"
            class="h-4 w-4 rounded border-slate-700 bg-slate-800 text-cyan-500 focus:ring-cyan-500/20 focus:ring-offset-slate-900"
          />
          Link L/R Channels
        </label>

        <!-- Reset Button -->
        <button
          type="button"
          class="inline-flex items-center gap-1.5 rounded-lg border border-slate-700 bg-slate-800 px-3 py-1.5 text-xs font-medium text-slate-300 hover:bg-slate-700 hover:text-white"
          @click="resetEqualizer"
        >
          Reset Bands
        </button>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="py-12 text-center text-sm text-slate-500">No equalizer bands available.</div>
  </div>
</template>
