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
