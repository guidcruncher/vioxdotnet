import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '@/layouts/AppLayout.vue'
import CleanLayout from '@/layouts/CleanLayout.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: CleanLayout,
      children: [{ path: 'clock', component: () => import('@/views/DigitalClockView.vue') }],
    },
    {
      path: '/',
      component: AppLayout,
      children: [
        { path: '', component: () => import('@/views/NowPlayingView.vue') },
        { path: 'library/playlists', component: () => import('@/views/FilePlaylistView.vue') },
        { path: 'library/:source', component: () => import('@/views/LibraryView.vue') },
        { path: 'spotify/show/:id', component: () => import('@/views/ShowView.vue') },
        { path: 'spotify/playlist/:id', component: () => import('@/views/PlaylistView.vue') },
        { path: 'spotify/album/:id', component: () => import('@/views/AlbumView.vue') },
        { path: 'radio', component: () => import('@/views/RadioView.vue') },
        { path: 'podverse/podcast/:id', component: () => import('@/views/PodcastView.vue') },
        { path: 'tunein', component: () => import('@/views/TuneInView.vue') },
        { path: 'search', component: () => import('@/views/SearchView.vue') },
        { path: 'playlists', component: () => import('@/views/LocalPlaylistView.vue') },
        { path: 'config', component: () => import('@/views/ClientConfigView.vue') },
        { path: 'equalizer', component: () => import('@/views/EqualizerView.vue') },
      ],
    },
  ],
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    }

    // Target the main scrollable viewport instead of .content-container
    const container = document.querySelector('#main-viewport')
    if (container) {
      container.scrollTo({ top: 0, behavior: 'smooth' })
    }

    return false
  },
})
