import * as signalR from '@microsoft/signalr';
import { useTaskStore } from '../stores/taskStore';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private isReconnecting = false;

  async startConnection() {
    if (this.connection || this.isReconnecting) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(
        (import.meta.env.VITE_SIGNALR_HUB_URL || 'http://localhost:5000/hubs') + '/tasks',
        { accessTokenFactory: () => localStorage.getItem('auth_token') || '' }
      )
      .withAutomaticReconnect()
      .build();

    this.connection.on('TaskCreated', (_task) => {
      // ignored — createTask() adds optimistically
    });

    this.connection.on('TaskUpdated', (task) => {
      useTaskStore.getState().upsertTask(task);
    });

    this.connection.on('TaskMoved', (data) => {
      const store = useTaskStore.getState();
      const task = store.tasks.find((t) => t.id === data.taskId);
      if (task) {
        store.upsertTask({ ...task, status: data.newStatus, orderIndex: data.newOrderIndex });
      }
    });

    try {
      await this.connection.start();
    } catch (err) {
      console.error('Tasks SignalR error:', err);
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

export default new SignalRService();
