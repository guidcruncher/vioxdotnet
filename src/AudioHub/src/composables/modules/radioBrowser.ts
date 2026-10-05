import type { useApiTransport } from '../useApiTransport'
import type { MediaMetaData } from '@/types'

export function createRadioBrowserModule(transport: ReturnType<typeof useApiTransport>) {
  return {
    getByCountry: (country: string): Promise<MediaMetaData[]> =>
      transport.request<MediaMetaData[]>(`/api/v1/media/radiobrowser/stations/country/${country}`),
  }
}
