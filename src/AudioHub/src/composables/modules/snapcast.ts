import type { useApiTransport } from '../useApiTransport'
import type {
  RpcVersion,
  SetClientLatencyRequest,
  SetClientNameRequest,
  SetGroupClientsRequest,
  SetGroupMuteRequest,
  SetGroupStreamRequest,
  SetVolumeRequest,
  SnapClient,
  SnapServer,
  VolumeState,
} from '@/types'

export function createSnapcastModule(transport: ReturnType<typeof useApiTransport>) {
  return {
    connect: () =>
      transport.request<void>('/api/v1/mixer/snapcast/connect', {
        method: 'POST',
      }),

    disconnect: () =>
      transport.request<void>('/api/v1/mixer/snapcast/disconnect', {
        method: 'POST',
      }),

    getRpcVersion: () => transport.request<RpcVersion>('/api/v1/mixer/snapcast/rpc-version'),

    getServer: () => transport.request<SnapServer>('/api/v1/mixer/snapcast/status'),

    getClients: () => transport.request<SnapClient[]>('/api/v1/mixer/snapcast/clients'),

    deleteClient: (clientId: string) =>
      transport.request<void>(`/api/v1/mixer/snapcast/clients/${clientId}`, {
        method: 'DELETE',
      }),

    setClientLatency: (clientId: string, payload: SetClientLatencyRequest) =>
      transport.request<number>(`/api/v1/mixer/snapcast/clients/${clientId}/latency`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setClientName: (clientId: string, payload: SetClientNameRequest) =>
      transport.request<string>(`/api/v1/mixer/snapcast/clients/${clientId}/name`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setClientVolume: (clientId: string, payload: SetVolumeRequest) =>
      transport.request<VolumeState>(`/api/v1/mixer/snapcast/clients/${clientId}/volume`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setAllClientVolume: (payload: SetVolumeRequest) =>
      transport.request<Record<string, VolumeState>>('/api/v1/mixer/snapcast/clients/volume', {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setGroupMute: (groupId: string, payload: SetGroupMuteRequest) =>
      transport.request<boolean>(`/api/v1/mixer/snapcast/groups/${groupId}/mute`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setGroupStream: (groupId: string, payload: SetGroupStreamRequest) =>
      transport.request<string>(`/api/v1/mixer/snapcast/groups/${groupId}/stream`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),

    setGroupClients: (groupId: string, payload: SetGroupClientsRequest) =>
      transport.request<string[]>(`/api/v1/mixer/snapcast/groups/${groupId}/clients`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),
  }
}
