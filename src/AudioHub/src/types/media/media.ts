export interface MediaUri {
  source: string
  type: string
  id: string
  secondaryId?: string
}

export interface MediaMetaData {
  uri?: MediaUri

  rawUri: string

  album: string
  title: string
  artist: string

  url: string
  imageUrl: string

  duration?: number

  favourite?: boolean
  inPlaylist?: boolean
  releaseDate?: string
}

export interface MediaMetaDataPlaylist {
  id: string

  title?: string

  imageUrl?: string

  items?: MediaMetaData[]
}
