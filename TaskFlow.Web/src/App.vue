<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { RouterView, RouterLink, useRoute } from 'vue-router';
import { useNotificationStore } from './stores/notificationStore';
import { useAuthStore } from './stores/authStore';

const route = useRoute();
const notificationStore = useNotificationStore();
const authStore = useAuthStore();

// Load auth state from localStorage on app mount
onMounted(() => {
  const storedToken = localStorage.getItem('auth_token');
  const storedUser = localStorage.getItem('auth_user');
  if (storedToken && storedUser) {
    authStore.setAuth(storedToken, JSON.parse(storedUser));
  }
});

// Check localStorage directly for authentication
const isAuthenticated = computed(() => !!localStorage.getItem('auth_token'));

const getUserFromStorage = () => {
  const userStr = localStorage.getItem('auth_user');
  if (userStr) {
    try {
      return JSON.parse(userStr);
    } catch {
      return null;
    }
  }
  return null;
};

const user = computed(() => getUserFromStorage());

const navigation = [
  { name: 'Board', href: '/board', icon: 'fas fa-columns' },
  { name: 'Today', href: '/today', icon: 'fas fa-calendar-day' }
];

const isCurrentRoute = (path: string) => route.path === path;

const unreadCount = computed(() => notificationStore.unreadCount);
const hasUnread = computed(() => notificationStore.hasUnread);

const handleLogout = () => {
  localStorage.removeItem('auth_token');
  localStorage.removeItem('auth_user');
  window.location.href = '/auth';
};
</script>

<template>
  <div class="h-screen flex hidden sm:flex bg-white overflow-hidden font-sans text-gray-900 border-r border-gray-200">

    <!-- Sidebar -->
    <aside class="w-64 flex-shrink-0 flex flex-col pt-5 pb-4 bg-gray-50/50">
      <div class="flex items-center px-6 mb-8 gap-3">
        <div class="w-8 h-8 rounded-lg bg-blue-600 text-white flex items-center justify-center font-bold text-lg shadow-sm">
          T
        </div>
        <span class="text-xl font-bold tracking-tight text-gray-900">TaskFlow</span>
      </div>

      <nav class="flex-1 px-4 space-y-1">
        <RouterLink
          v-for="item in navigation"
          :key="item.name"
          :to="item.href"
          :class="[
            isCurrentRoute(item.href)
              ? 'bg-blue-50 text-blue-700 before:absolute before:inset-y-0 before:left-0 before:w-1 before:bg-blue-600 before:rounded-r'
              : 'text-gray-700 hover:bg-gray-100 hover:text-gray-900',
            'group flex items-center px-3 py-2.5 text-sm font-medium rounded-lg relative transition-colors'
          ]"
        >
          <span class="truncate">{{ item.name }}</span>
        </RouterLink>
      </nav>

      <div class="px-4 mt-auto space-y-3">
        <!-- Notifications -->
        <button
          type="button"
          class="w-full flex items-center justify-between px-3 py-2 rounded-lg border border-gray-200 hover:bg-gray-100 transition-colors text-sm"
          @click="notificationStore.fetchNotifications(true)"
        >
          <div class="flex items-center gap-2">
            <span class="relative inline-flex">
              <span class="inline-flex items-center justify-center h-7 w-7 rounded-full bg-blue-50 text-blue-600">
                <i class="fas fa-bell text-sm"></i>
              </span>
              <span
                v-if="hasUnread"
                class="absolute -top-0.5 -right-0.5 inline-flex items-center justify-center px-1.5 py-0.5 rounded-full bg-red-500 text-white text-[10px] font-semibold"
              >
                {{ unreadCount > 9 ? '9+' : unreadCount }}
              </span>
            </span>
            <span class="text-gray-800 font-medium">Notifications</span>
          </div>
        </button>

        <!-- User -->
        <div v-if="isAuthenticated" class="space-y-2">
          <div class="flex items-center gap-3 px-3 py-3 rounded-lg bg-gray-50 border border-gray-100 transition-colors">
            <div class="w-8 h-8 rounded-full bg-gradient-to-tr from-blue-500 to-purple-500 flex items-center justify-center text-white font-medium text-sm">
              {{ user?.firstName?.[0]?.toUpperCase() || user?.email?.[0]?.toUpperCase() || 'U' }}{{ user?.lastName?.[0]?.toUpperCase() || '' }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-gray-900 truncate">{{ user?.firstName || user?.email?.split('@')[0] || 'User' }}</p>
              <p class="text-xs text-gray-500 truncate">{{ user?.email || 'user@taskflow.local' }}</p>
            </div>
          </div>
          <button
            @click="handleLogout"
            class="w-full flex items-center justify-center gap-2 px-3 py-2 rounded-lg border border-gray-200 text-sm text-gray-700 hover:bg-gray-100 transition-colors"
          >
            <i class="fas fa-sign-out-alt"></i>
            Logout
          </button>
        </div>
        <RouterLink
          v-else
          to="/auth"
          class="flex items-center gap-3 px-3 py-3 rounded-lg hover:bg-gray-100 cursor-pointer transition-colors"
        >
          <div class="w-8 h-8 rounded-full bg-gray-200 flex items-center justify-center text-gray-600 font-medium text-sm">
            <i class="fas fa-user"></i>
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 truncate">Login</p>
            <p class="text-xs text-gray-500 truncate">Sign in to TaskFlow</p>
          </div>
        </RouterLink>
      </div>
    </aside>

    <!-- Main Content -->
    <main class="flex-1 flex flex-col relative focus:outline-none min-w-0 bg-white">
      <RouterView />
    </main>

  </div>

  <div class="block sm:hidden p-8 text-center bg-gray-50 h-screen flex flex-col justify-center">
    <h2 class="text-xl font-bold mb-2">Desktop recommended</h2>
    <p class="text-gray-600">TaskFlow works best on larger screens right now.</p>
  </div>
</template>

<style>
/* Global Tailwind reset is in style.css */
</style>
