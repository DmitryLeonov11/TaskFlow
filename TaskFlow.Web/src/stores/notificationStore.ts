import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import api from '../services/apiService';

export interface Notification {
  id: string;
  type: string;
  message: string;
  relatedTaskId?: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface GetNotificationsResponse {
  notifications: Notification[];
  totalCount: number;
  unreadCount: number;
}

export const useNotificationStore = defineStore('notifications', () => {
  const items = ref<Notification[]>([]);
  const loading = ref(false);
  const unreadCount = ref(0);
  const page = ref(1);
  const pageSize = ref(20);

  const hasUnread = computed(() => unreadCount.value > 0);

  const fetchNotifications = async (reset = false) => {
    try {
      loading.value = true;
      if (reset) {
        page.value = 1;
      }

      const response = await api.get<GetNotificationsResponse>('/notifications', {
        params: {
          page: page.value,
          pageSize: pageSize.value
        }
      });

      const data = response.data;

      if (reset) {
        items.value = data.notifications;
      } else {
        const existingIds = new Set(items.value.map(n => n.id));
        const merged = [...items.value];
        data.notifications.forEach(n => {
          if (!existingIds.has(n.id)) {
            merged.push(n);
          }
        });
        items.value = merged;
      }

      unreadCount.value = data.unreadCount;
    } finally {
      loading.value = false;
    }
  };

  const addNotification = (notification: Notification) => {
    const exists = items.value.some(n => n.id === notification.id);
    if (!exists) {
      items.value.unshift(notification);
      if (!notification.isRead) {
        unreadCount.value += 1;
      }
    }
  };

  const markAsRead = async (id: string) => {
    await api.put(`/notifications/${id}/mark-as-read`);
    const notif = items.value.find(n => n.id === id);
    if (notif && !notif.isRead) {
      notif.isRead = true;
      unreadCount.value = Math.max(0, unreadCount.value - 1);
    }
  };

  const markAllAsReadLocal = () => {
    items.value.forEach(n => (n.isRead = true));
    unreadCount.value = 0;
  };

  return {
    items,
    loading,
    unreadCount,
    hasUnread,
    fetchNotifications,
    addNotification,
    markAsRead,
    markAllAsReadLocal
  };
});

