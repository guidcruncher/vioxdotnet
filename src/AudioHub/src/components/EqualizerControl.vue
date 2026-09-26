<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useApiClient } from '@/composables/useApiClient'
import type { EqualizerBand } from '@/types/api'

// Initialize API client and state
const api = useApiClient()

const bands = ref<EqualizerBand[]>([])
const isLoading = ref<boolean>(true)
const isSaving = ref<boolean>(false)
const errorMessage = ref<string | null>(null)
const linkChannels = ref<boolean>(true)

// Debounce timer registry per band index using browser-safe return type
const debounceTimers = new Map<number, ReturnType<typeof setTimeout>>()

// Load equalizer bands on mount
onMounted(async () => {
  await fetchBands()
})

async function fetchBands(): Promise<void> {
  isLoading.value = true
  errorMessage.value = null
  try {
    bands.value = await api.equalizer.getEqualizerBands()
  } catch (err) {
    errorMessage.value = 'Failed to load equalizer settings.'
  } finally {
    isLoading.value = false
  }
}

// Handle slider changes for individual bands with debounced API execution
function handleBandInput(index: number, channel: 'left' | 'right', value: number): void {
  const band = bands.value.find((b) => b.index === index)
  if (!band) return

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
      await api.equalizer.setEqualizerBand(index, targetValue)
    } catch (err) {
      errorMessage.value = `Failed to update band ${band.frequencyLabel}`
    } finally {
      debounceTimers.delete(index)
    }
  }, 300)

  debounceTimers.set(index, timer)
}

// Preset application with batch API update
async function applyPreset(presetName: 'flat' | 'bass' | 'treble' | 'vocal'): Promise<void> {
  if (bands.value.length === 0) return

  const presets: Record<string, number[]> = {
    flat: [50, 50, 50, 50, 50, 50, 50, 50, 50, 50],
    bass: [80, 75, 70, 60, 50, 50, 50, 50, 50, 50],
    treble: [50, 50, 50, 50, 50, 60, 70, 75, 80, 85],
    vocal: [40, 45, 50, 65, 75, 75, 65, 50, 45, 40],
  }

  const targetValues = presets[presetName] || presets.flat

  // Map values onto local bands array
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
    await api.equalizer.setEqualizerBands(allPercentages)
  } catch (err) {
    errorMessage.value = 'Failed to apply equalizer preset.'
  } finally {
    isSaving.value = false
  }
}

// Reset all bands to default mid point (50%)
async function resetEqualizer(): Promise<void> {
  await applyPreset('flat')
}
</script>

<template>
  <div
    class="w-full max-w-4xl rounded-xl border border-slate-800 bg-slate-900 p-6 text-slate-100 shadow-2xl"
  >
    <!-- Header -->
    <div
      class="mb-6 flex flex-wrap items-center justify-between gap-4 border-b border-slate-800 pb-4"
    >
      <div>
        <h2 class="text-xl font-bold tracking-wide text-white">10-Band Equalizer</h2>
        <p class="text-xs text-slate-400">Adjust audio frequencies and balance</p>
      </div>

      <!-- Quick Preset Controls -->
      <div class="flex flex-wrap items-center gap-2">
        <button
          type="button"
          class="rounded-lg border border-slate-700 bg-slate-800 px-3 py-1.5 text-xs font-medium text-slate-300 transition-colors hover:bg-slate-700 hover:text-white disabled:opacity-50"
          :disabled="isLoading || isSaving"
          @click="applyPreset('flat')"
        >
          Flat
        </button>
        <button
          type="button"
          class="rounded-lg border border-slate-700 bg-slate-800 px-3 py-1.5 text-xs font-medium text-slate-300 transition-colors hover:bg-slate-700 hover:text-white disabled:opacity-50"
          :disabled="isLoading || isSaving"
          @click="applyPreset('bass')"
        >
          Bass Boost
        </button>
        <button
          type="button"
          class="rounded-lg border border-slate-700 bg-slate-800 px-3 py-1.5 text-xs font-medium text-slate-300 transition-colors hover:bg-slate-700 hover:text-white disabled:opacity-50"
          :disabled="isLoading || isSaving"
          @click="applyPreset('treble')"
        >
          Treble Boost
        </button>
        <button
          type="button"
          class="rounded-lg border border-slate-700 bg-slate-800 px-3 py-1.5 text-xs font-medium text-slate-300 transition-colors hover:bg-slate-700 hover:text-white disabled:opacity-50"
          :disabled="isLoading || isSaving"
          @click="applyPreset('vocal')"
        >
          Vocal
        </button>
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
      <div class="grid grid-cols-5 gap-4 sm:grid-cols-10">
        <div
          v-for="band in bands"
          :key="band.index"
          class="flex flex-col items-center rounded-lg bg-slate-950/50 p-3 transition-colors hover:bg-slate-950"
        >
          <!-- Value Readout -->
          <span class="mb-2 font-mono text-[11px] text-cyan-400">
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
          <span class="text-[10px] text-slate-500">
            {{ band.controlName }}
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
