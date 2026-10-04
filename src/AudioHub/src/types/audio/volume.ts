export interface VolumeState {
  percent: number
  muted: boolean
}

export interface SetVolumeRequest {
  volumePercent: number
  muted?: boolean
}

export interface GetVolumeResponse {
  volumePercent: number
  muted?: boolean
}
