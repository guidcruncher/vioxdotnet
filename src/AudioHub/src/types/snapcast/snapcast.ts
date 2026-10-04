import type { VolumeState } from '../audio/volume'

export interface SnapClientConfig {
  instance: number | string

  latency: number | string

  name: string

  volume: VolumeState
}

export interface ClientHost {
  arch: string
  ip: string
  mac: string
  name: string
  os: string
}

export interface SnapTime {
  sec: number | string
  usec: number | string
}

export interface SnapClient {
  id: string

  connected: boolean

  config: SnapClientConfig

  host: ClientHost

  lastSeen: SnapTime
}

export interface SnapGroup {
  id: string
  name: string
  muted: boolean
  stream_id: string

  clients: SnapClient[]
}

export interface SnapStream {
  id: string
  status: string
  uri: unknown
}

export interface SnapServer {
  host: ClientHost

  groups: SnapGroup[]

  streams: SnapStream[]
}

export interface RpcVersion {
  major: number | string
  minor: number | string
  patch: number | string
}

export interface SetClientLatencyRequest {
  latency: number
}

export interface SetClientNameRequest {
  name: string
}

export interface SetGroupClientsRequest {
  clientIds: string[]
}

export interface SetGroupMuteRequest {
  mute: boolean
}

export interface SetGroupStreamRequest {
  streamId: string
}
