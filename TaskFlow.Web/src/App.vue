<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { RouterView, RouterLink, useRoute } from 'vue-router';
import { useNotificationStore } from './stores/notificationStore';
import { useAuthStore } from './stores/authStore';
import NotificationsPanel from './components/NotificationsPanel.vue';

const route = useRoute();
const notificationStore = useNotificationStore();
const authStore = useAuthStore();

const showNotifications = ref(false);

onMounted(() => {
  const storedToken = localStorage.getItem('auth_token');
  const storedUser = localStorage.getItem('auth_user');
  if (storedToken && storedUser) {
    authStore.setAuth(storedToken, JSON.parse(storedUser));
  }
});

const isAuthenticated = computed(() => !!localStorage.getItem('auth_token'));

const user = computed(() => {
  const userStr = localStorage.getItem('auth_user');
  if (userStr) {
    try { return JSON.parse(userStr); } catch { return null; }
  }
  return null;
});

const navigation = [
  { name: 'Board', href: '/board' },
  { name: 'Today', href: '/today' },
];

const isCurrentRoute = (path: string) => route.path === path;

const unreadCount = computed(() => notificationStore.unreadCount);
const hasUnread = computed(() => notificationStore.hasUnread);

const handleLogout = () => {
  localStorage.removeItem('auth_token');
  localStorage.removeItem('auth_user');
  window.location.href = '/auth';
};

const toggleNotifications = async () => {
  showNotifications.value = !showNotifications.value;
};
</script>

<template>
  <!-- Desktop layout -->
  <div class="h-screen sm:flex hidden bg-white overflow-hidden font-sans text-gray-900">

    <!-- Sidebar -->
    <aside class="w-60 flex-shrink-0 flex flex-col pt-5 pb-4 bg-gray-50 border-r border-gray-200">
      <!-- Logo -->
      <div class="flex items-center px-5 mb-8 gap-2.5">
        <div class="w-7 h-7 rounded-lg bg-blue-600 text-white flex items-center justify-center font-bold text-sm shadow-sm">
          T
        </div>
        <span class="text-lg font-bold tracking-tight text-gray-900">TaskFlow</span>
      </div>

      <!-- Nav -->
      <nav class="flex-1 px-3 space-y-0.5">
        <RouterLink
          v-for="item in navigation"
          :key="item.name"
          :to="item.href"
          :class="[
            isCurrentRoute(item.href)
              ? 'bg-blue-50 text-blue-700 font-semibold'
              : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900',
            'flex items-center px-3 py-2 text-sm rounded-lg transition-colors'
          ]"
        >
          {{ item.name }}
        </RouterLink>
      </nav>

      <!-- Bottom: notifications + user -->
      <div class="px-3 mt-auto space-y-2">

        <!-- Notifications button -->
        <div class="relative">
          <button
            type="button"
            class="w-full flex items-center justify-between px-3 py-2 rounded-lg border border-gray-200 hover:bg-gray-100 transition-colors text-sm"
            @click="toggleNotifications"
          >
            <div class="flex items-center gap-2">
              <span class="relative inline-flex">
                <svg class="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
                </svg>
                <span
                  v-if="hasUnread"
                  class="absolute -top-1 -right-1 inline-flex items-center justify-center min-w-[16px] h-4 px-1 rounded-full bg-red-500 text-white text-[10px] font-semibold"
                >
                  {{ unreadCount > 9 ? '9+' : unreadCount }}
                </span>
              </span>
              <span class="text-gray-700 font-medium">Notifications</span>
            </div>
          </button>

          <NotificationsPanel
            v-if="showNotifications"
            @close="showNotifications = false"
          />
        </div>

        <!-- User -->
        <div v-if="isAuthenticated" class="space-y-1.5">
          <div class="flex items-center gap-2.5 px-3 py-2.5 rounded-lg bg-gray-50 border border-gray-100">
            <div class="w-7 h-7 rounded-full bg-gradient-to-tr from-blue-500 to-purple-500 flex items-center justify-center text-white font-medium text-xs flex-shrink-0">
              {{ user?.firstName?.[0]?.toUpperCase() || user?.email?.[0]?.toUpperCase() || 'U' }}{{ user?.lastName?.[0]?.toUpperCase() || '' }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-gray-900 truncate">{{ user?.firstName || user?.email?.split('@')[0] || 'User' }}</p>
              <p class="text-xs text-gray-400 truncate">{{ user?.email }}</p>
            </div>
          </div>
          <button
            @click="handleLogout"
            class="w-full flex items-center justify-center gap-1.5 px-3 py-1.5 rounded-lg border border-gray-200 text-xs text-gray-600 hover:bg-gray-100 transition-colors"
          >
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
            </svg>
            Logout
          </button>
        </div>
        <RouterLink
          v-else
          to="/auth"
          class="flex items-center gap-2.5 px-3 py-2.5 rounded-lg hover:bg-gray-100 cursor-pointer transition-colors"
        >
          <div class="w-7 h-7 rounded-full bg-gray-200 flex items-center justify-center text-gray-500 text-xs">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
            </svg>
          </div>
          <span class="text-sm font-medium text-gray-700">Login</span>
        </RouterLink>
      </div>
    </aside>

    <!-- Main content -->
    <main class="flex-1 flex flex-col relative focus:outline-none min-w-0 overflow-hidden">
      <RouterView />
    </main>

  </div>

  <!-- Mobile fallback -->
  <div class="flex sm:hidden h-screen items-center justify-center p-8 text-center bg-gray-50 flex-col">
    <div class="w-12 h-12 rounded-xl bg-blue-600 text-white flex items-center justify-center font-bold text-xl mb-4">T</div>
    <h2 class="text-xl font-bold mb-2 text-gray-900">Desktop recommended</h2>
    <p class="text-gray-500 text-sm">TaskFlow works best on larger screens.</p>
  </div>
</template>
