import { useApiClient } from '@/composables/useApiClient'
import type { MediaMetaData } from '@/types'

const api = useApiClient()

export async function playAllTracks(tracks?: MediaMetaData[]) {
  if (!tracks || tracks.length == 0) {
    return
  }

  const state = await api.media.getPlaybackState()

  if (state && !state.playing) {
    await api.queue.clear()
  }

  const batch = []
  for (const item of tracks) {
    batch.push({ uri: item.rawUri })
  }
  await api.queue.enqueueBatch(batch)
}
