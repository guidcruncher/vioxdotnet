import { useApiTransport } from './useApiTransport';
import type {
  RpcVersion,
  SnapServer,
  SnapClient,
  SetVolumeRequest,
  VolumeState,
  SetClientNameRequest,
  SetClientLatencyRequest,
  SetGroupMuteRequest,
  SetGroupStreamRequest,
  SetGroupClientsRequest,
} from '../types';

export function useSnapcastApi(transport = useApiTransport('/api/v1/mixer/snapcast')) {
  return {
    connect: () => transport.request<any>('connect', { method: 'POST' }),
    disconnect: () => transport.request<any>('disconnect', { method: 'POST' }),
    getRpcVersion: () => transport.request<RpcVersion>('rpc-version', { method: 'GET' }),
    getStatus: () => transport.request<SnapServer>('status', { method: 'GET' }),
    getClients: () => transport.request<SnapClient[]>('clients', { method: 'GET' }),
    setClientVolume: (clientId: string, data: SetVolumeRequest) =>
      transport.request<VolumeState>(`clients/${encodeURIComponent(clientId)}/volume`, { method: 'PUT', body: data }),
    setAllClientsVolume: (data: SetVolumeRequest) =>
      transport.request<Record<string, VolumeState>>('clients/volume', { method: 'PUT', body: data }),
    setClientName: (clientId: string, data: SetClientNameRequest) =>
      transport.request<string>(`clients/${encodeURIComponent(clientId)}/name`, { method: 'PUT', body: data }),
    setClientLatency: (clientId: string, data: SetClientLatencyRequest) =>
      transport.request<number>(`clients/${encodeURIComponent(clientId)}/latency`, { method: 'PUT', body: data }),
    deleteClient: (clientId: string) =>
      transport.request<void>(`clients/${encodeURIComponent(clientId)}`, { method: 'DELETE' }),
    setGroupMute: (groupId: string, data: SetGroupMuteRequest) =>
      transport.request<boolean>(`groups/${encodeURIComponent(groupId)}/mute`, { method: 'PUT', body: data }),
    setGroupStream: (groupId: string, data: SetGroupStreamRequest) =>
      transport.request<string>(`groups/${encodeURIComponent(groupId)}/stream`, { method: 'PUT', body: data }),
    setGroupClients: (groupId: string, data: SetGroupClientsRequest) =>
      transport.request<string[]>(`groups/${encodeURIComponent(groupId)}/clients`, { method: 'PUT', body: data }),
  };
}
