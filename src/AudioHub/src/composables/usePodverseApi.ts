import { useApiTransport } from './useApiTransport';
import type { Podcast, Episode, User } from '../types';

export function usePodverseApi(transport = useApiTransport('/api/v1/media/podverse')) {
  return {
    getPodcastEpisodes: (podcastId: string) =>
      transport.request<Podcast>(`podcast/${encodeURIComponent(podcastId)}/episodes`, { method: 'GET' }),
    getPodcast: (podcastId: string) =>
      transport.request<Podcast>(`podcasts/${encodeURIComponent(podcastId)}`, { method: 'GET' }),
    searchPodcasts: (params?: { searchTitle?: string; page?: number }) =>
      transport.request<Podcast[]>('podcasts', { method: 'GET', params }),
    getEpisode: (episodeId: string) =>
      transport.request<Episode>(`episodes/${encodeURIComponent(episodeId)}`, { method: 'GET' }),
    getCurrentUser: () =>
      transport.request<User>('user/me', { method: 'GET' }),
    toggleSubscription: (podcastId: string) =>
      transport.request<any>(`podcasts/${encodeURIComponent(podcastId)}/toggle-subscription`, { method: 'POST' }),
  };
}
