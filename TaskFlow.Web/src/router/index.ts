import { createRouter, createWebHistory } from 'vue-router';
import KanbanView from '../views/KanbanView.vue';
import TodayView from '../views/TodayView.vue';
import { useAuthStore } from '../stores/authStore';

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
        }
        // Auth routes could be added here
    ]
});

router.beforeEach((to, from, next) => {
    const authStore = useAuthStore();

    if (to.meta.requiresAuth && !authStore.isAuthenticated()) {
        // next({ name: 'login' });
        next(); // Bypass for now if login not fully implemented on front
    } else {
        next();
    }
});

export default router;
