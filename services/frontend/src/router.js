import { createRouter, createWebHistory } from 'vue-router';
import HomePage from './components/HomePage.vue';
import UserRegister from './components/UserRegister.vue';
import UserLogin from './components/UserLogin.vue';
import UploadImage from './components/UploadImage.vue';
import ExtractData from './components/ExtractData.vue';
import RequestHistory from './components/RequestHistory.vue';

const routes = [
  { path: '/', component: HomePage },
  { path: '/register', component: UserRegister },
  { path: '/login', component: UserLogin },
  { path: '/upload', component: UploadImage },
  { path: '/extract', component: ExtractData },
  { path: '/history', component: RequestHistory },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;