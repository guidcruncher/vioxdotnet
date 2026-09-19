import { createRouter, createWebHistory } from 'vue-router'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/spotify' },
    { path: '/spotify', component: () => import('@/views/SpotifyView.vue') },
    { path: '/radio', component: () => import('@/views/RadioView.vue') },
    { path: '/podverse', component: () => import('@/views/PodverseView.vue') },
    { path: '/podverse/podcast/:id', component: () => import('@/views/PodcastView.vue') },
    { path: '/tunein', component: () => import('@/views/TuneInView.vue') },
    { path: '/snapcast', component: () => import('@/views/SnapcastView.vue') },
    { path: '/mpd', component: () => import('@/views/MpdView.vue') },
    { path: '/librespot', component: () => import('@/views/LibrespotView.vue') },
    { path: '/search', component: () => import('@/views/SearchView.vue') },
  ],
})
