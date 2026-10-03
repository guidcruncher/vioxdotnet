export interface SpotifyResponse<T> {
  isSuccess: boolean

  statusCode: number | string

  data?: T | null

  error?: SpotifyErrorObject | null
}

export interface SpotifyErrorObject {
  status?: number | string
  message?: string
}

export interface SpotifyExternalUrls {
  spotify?: string
}

export interface SpotifyFollowers {
  href?: string | null
  total?: number | string
}

export interface SpotifyImage {
  url?: string

  height?: number | string | null

  width?: number | string | null
}

export interface SpotifyUserProfile {
  id?: string

  account_id?: string

  display_name?: string | null

  email?: string | null

  external_urls?: SpotifyExternalUrls | null

  followers?: SpotifyFollowers | null

  href?: string

  images?: SpotifyImage[]

  product?: string | null

  type?: string

  uri?: string
}
