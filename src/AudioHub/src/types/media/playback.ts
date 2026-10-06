import type { MediaMetaData } from './media'

export interface PlayRequest {
  uri: string
}

export interface SeekRequest {
  position: string
}

export interface PlaybackState {
  activeBackend?: string

  track?: MediaMetaData | null

  position?: number

  playing?: boolean

  isLive?: boolean
}

export enum PlaybackRepeatMode {
  Off = 0,
  Track = 1,
  Queue = 2,
}

export interface QueueStatusResponse {
  items: MediaMetaData[]
  currentIndex: number
  currentItem: MediaMetaData | null
  isShuffleEnabled: boolean
  repeatMode: PlaybackRepeatMode
}
