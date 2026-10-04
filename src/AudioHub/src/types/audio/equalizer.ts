export interface EqualizerBand {
  index: number

  controlName: string

  frequencyLabel: string

  leftPercentage: number

  rightPercentage: number
}

export interface SetLevelRequest {
  percentage: number
}

export interface SetAllBandsRequest {
  percentages: number[]
}
