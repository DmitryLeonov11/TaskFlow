import * as signalR from '@microsoft/signalr';
import { useNotificationStore, type Notification } from '../stores/notificationStore';

class NotificationsSignalRService {
  private connection: signalR.HubConnection | null = null;

  public async startConnection() {
    if (this.connection) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(
        (import.meta.env.VITE_SIGNALR_HUB_URL || 'http://localhost:5000/hubs') + '/notifications',
        {
          accessTokenFactory: () => localStorage.getItem('auth_token') || ''
        }
      )
      .withAutomaticReconnect()
      .build();

    this.connection.on('NotificationReceived', (event: any) => {
      const store = useNotificationStore();
      const notification: Notification = {
        id: event.notificationId,
        type: event.type,
        message: event.message,
        relatedTaskId: event.relatedTaskId,
        isRead: false,
        createdAt: event.createdAt
      };
      store.addNotification(notification);
    });

    try {
      await this.connection.start();
      console.log('Notifications SignalR connected.');
    } catch (err) {
      console.error('Notifications SignalR Connection Error: ', err);
      setTimeout(() => this.startConnection(), 5000);
    }
  }

  public async stopConnection() {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }
}

export default new NotificationsSignalRService();

