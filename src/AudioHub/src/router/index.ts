import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import DiscoverView from '../views/DiscoverView.vue';
import PlatformView from '../views/PlatformView.vue';

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Discover',
    component: DiscoverView
  },
  {
    path: '/platform/:name',
    name: 'Platform',
    component: PlatformView,
    props: true
  },
  {
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

export default router;
