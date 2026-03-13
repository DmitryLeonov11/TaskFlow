<script setup lang="ts">
import { computed } from 'vue';
import { RouterView, RouterLink, useRoute } from 'vue-router';

const route = useRoute();

const navigation = [
  { name: 'Board', href: '/board', icon: 'fas fa-columns' },
  { name: 'Today', href: '/today', icon: 'fas fa-calendar-day' },
];

const isCurrentRoute = (path: string) => route.path === path;
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

      <div class="px-4 mt-auto">
        <div class="flex items-center gap-3 px-3 py-3 rounded-lg hover:bg-gray-100 cursor-pointer transition-colors">
          <div class="w-8 h-8 rounded-full bg-gradient-to-tr from-blue-500 to-purple-500 flex items-center justify-center text-white font-medium text-sm">
            US
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 truncate">User</p>
            <p class="text-xs text-gray-500 truncate">user@taskflow.local</p>
          </div>
        </div>
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
