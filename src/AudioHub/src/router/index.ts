import { createRouter, createWebHistory } from 'vue-router'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: () => import('@/views/NowPlayingView.vue') },
    { path: '/library/playlists', component: () => import('@/views/FilePlaylistView.vue') },
    { path: '/library/:source', component: () => import('@/views/LibraryView.vue') },
    { path: '/spotify/show/:id', component: () => import('@/views/ShowView.vue') },
    { path: '/spotify/playlist/:id', component: () => import('@/views/PlaylistView.vue') },
    { path: '/spotify/album/:id', component: () => import('@/views/AlbumView.vue') },
    { path: '/radio', component: () => import('@/views/RadioView.vue') },
    { path: '/podverse/podcast/:id', component: () => import('@/views/PodcastView.vue') },
    { path: '/tunein', component: () => import('@/views/TuneInView.vue') },
    { path: '/search', component: () => import('@/views/SearchView.vue') },
    { path: '/playlists', component: () => import('@/views/LocalPlaylistView.vue') },
    { path: '/config', component: () => import('@/views/ClientConfigView.vue') },
    { path: '/equalizer', component: () => import('@/views/EqualizerView.vue') },
  ],
  scrollBehavior(to, from, savedPosition) {
    // If the user used the browser back/forward buttons, restore their previous scroll position
    if (savedPosition) {
      return savedPosition
    }

    // Manually scroll your internal container to the top
    const container = document.querySelector('.content-container')
    if (container) {
      container.scrollTo({ top: 0, behavior: 'smooth' })
    }

    // Return false to let router know you handled it manually
    return false
  },
})
