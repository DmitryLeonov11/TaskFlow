import { createSignalRService } from './signalrFactory';
import { useNotificationStore } from '../stores/notificationStore';

const notificationsSignalrService = createSignalRService('/notifications', (connection) => {
  connection.on('NotificationReceived', (notification) => {
    useNotificationStore.getState().addNotification(notification);
  });
});

export default notificationsSignalrService;
