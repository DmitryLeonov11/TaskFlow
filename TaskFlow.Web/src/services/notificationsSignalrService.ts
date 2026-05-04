import * as signalR from '@microsoft/signalr';
import { useNotificationStore } from '../stores/notificationStore';

class NotificationsSignalRService {
  private connection: signalR.HubConnection | null = null;
  private isReconnecting = false;

  async startConnection() {
    if (this.connection || this.isReconnecting) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(
        (import.meta.env.VITE_SIGNALR_HUB_URL || 'http://localhost:5000/hubs') + '/notifications',
        { accessTokenFactory: () => localStorage.getItem('auth_token') || '' }
      )
      .withAutomaticReconnect()
      .build();

    this.connection.on('NotificationReceived', (notification) => {
      useNotificationStore.getState().addNotification(notification);
    });

    try {
      await this.connection.start();
    } catch (err) {
      console.error('Notifications SignalR error:', err);
      this.connection = null;
      this.isReconnecting = true;
      setTimeout(() => { this.isReconnecting = false; this.startConnection(); }, 5000);
    }
  }

  async stopConnection() {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }
}

export default new NotificationsSignalRService();
