<script setup lang="ts">
import { onMounted } from 'vue';
import { useNotificationStore } from '../stores/notificationStore';

const notificationStore = useNotificationStore();

const emit = defineEmits<{
  (e: 'close'): void;
}>();

onMounted(async () => {
  await notificationStore.fetchNotifications(true);
});

const formatTime = (iso: string) => {
  const date = new Date(iso);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMin = Math.floor(diffMs / 60000);
  if (diffMin < 1) return 'just now';
  if (diffMin < 60) return `${diffMin}m ago`;
  const diffH = Math.floor(diffMin / 60);
  if (diffH < 24) return `${diffH}h ago`;
  return date.toLocaleDateString();
};

const handleMarkAsRead = async (id: string) => {
  await notificationStore.markAsRead(id);
};

const handleMarkAllRead = () => {
  notificationStore.markAllAsReadLocal();
};
</script>

<template>
  <div class="absolute bottom-full left-0 mb-2 w-80 bg-white border border-gray-200 rounded-xl shadow-xl z-50 overflow-hidden">
    <!-- Header -->
    <div class="flex items-center justify-between px-4 py-3 border-b border-gray-100">
      <h3 class="text-sm font-semibold text-gray-900">Notifications</h3>
      <div class="flex items-center gap-2">
        <button
          v-if="notificationStore.hasUnread"
          type="button"
          @click="handleMarkAllRead"
          class="text-xs text-blue-600 hover:text-blue-700 font-medium"
        >
          Mark all read
        </button>
        <button type="button" @click="emit('close')" class="text-gray-400 hover:text-gray-600">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
          </svg>
        </button>
      </div>
    </div>

    <!-- List -->
    <div class="max-h-72 overflow-y-auto">
      <div v-if="notificationStore.loading" class="flex justify-center py-6">
        <div class="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-600"></div>
      </div>

      <div v-else-if="!notificationStore.items.length" class="py-8 text-center text-sm text-gray-400">
        No notifications yet
      </div>

      <div v-else>
        <div
          v-for="notif in notificationStore.items"
          :key="notif.id"
          :class="[
            'px-4 py-3 border-b border-gray-50 last:border-0 transition-colors hover:bg-gray-50 cursor-pointer',
            !notif.isRead ? 'bg-blue-50/40' : ''
          ]"
          @click="!notif.isRead && handleMarkAsRead(notif.id)"
        >
          <div class="flex items-start gap-2.5">
            <span
              :class="['w-2 h-2 rounded-full mt-1.5 flex-shrink-0', !notif.isRead ? 'bg-blue-500' : 'bg-transparent']"
            ></span>
            <div class="flex-1 min-w-0">
              <p class="text-sm text-gray-800 leading-snug">{{ notif.message }}</p>
              <p class="text-xs text-gray-400 mt-0.5">{{ formatTime(notif.createdAt) }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
