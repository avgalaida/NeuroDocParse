// router/index.js

import { createRouter, createWebHistory } from 'vue-router';
import ExtractData from '../components/ExtractData.vue';
import ImageEditor from '../components/ImageEditor.vue';

const routes = [
  {
    path: '/',
    name: 'ExtractData',
    component: ExtractData,
  },
  {
    path: '/editor',
    name: 'ImageEditor',
    component: ImageEditor,
    props: (route) => ({ imageUrl: route.query.url }),
  },
];

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes,
});

export default router;