import { createRouter, createWebHistory } from 'vue-router';
import KanbanView from '../views/KanbanView.vue';
import TodayView from '../views/TodayView.vue';
import AuthView from '../views/AuthView.vue';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/board'
    },
    {
      path: '/board',
      name: 'kanban',
      component: KanbanView,
      meta: { requiresAuth: true }
    },
    {
      path: '/today',
      name: 'today',
      component: TodayView,
      meta: { requiresAuth: true }
    },
    {
      path: '/auth',
      name: 'auth',
      component: AuthView
    }
  ]
});

router.beforeEach((to, _from, next) => {
  const hasToken = !!localStorage.getItem('auth_token');

  if (to.meta.requiresAuth && !hasToken) {
    // Redirect to auth with full page reload to reset app state
    window.location.href = '/auth';
  } else if (to.name === 'auth' && hasToken) {
    // If already authenticated and trying to go to auth page, redirect to board
    window.location.href = '/board';
  } else {
    next();
  }
});

export default router;
