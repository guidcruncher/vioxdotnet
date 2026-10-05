import type { useApiTransport } from '../useApiTransport'
import type { EqualizerBand } from '@/types'

export function createEqualizerModule(transport: ReturnType<typeof useApiTransport>) {
  return {
    getPresets: () =>
      transport.request<Record<string, number[]>>('/api/v1/audiocontrol/equalizer/presets'),

    getBands: () => transport.request<EqualizerBand[]>('/api/v1/audiocontrol/equalizer/bands'),

    setBand: (bandIndex: number, percentage: number) =>
      transport.request<void>(`/api/v1/audiocontrol/equalizer/bands/${bandIndex}`, {
        method: 'PUT',
        body: JSON.stringify({
          percentage,
        }),
      }),

    setBands: (percentages: number[]) =>
      transport.request<void>('/api/v1/audiocontrol/equalizer/bands', {
        method: 'PUT',
        body: JSON.stringify({
          percentages,
        }),
      }),
  }
}
