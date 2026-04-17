import * as signalR from '@microsoft/signalr';
import { useTaskStore } from '../stores/taskStore';

class SignalRService {
  private connection: signalR.HubConnection | null = null;

  public async startConnection() {
    if (this.connection) {
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(
        (import.meta.env.VITE_SIGNALR_HUB_URL || 'http://localhost:5000/hubs') + '/tasks',
        {
          accessTokenFactory: () => localStorage.getItem('auth_token') || ''
        }
      )
      .withAutomaticReconnect()
      .build();

    this.connection.on('TaskCreated', (task) => {
      // Disabled: task is added manually in createTask() to avoid duplicates
      // const taskStore = useTaskStore();
      // taskStore.tasks.push(task);
      console.log('[SignalR] TaskCreated event received (ignored):', task.title);
    });

    this.connection.on('TaskUpdated', (task) => {
      const taskStore = useTaskStore();
      const index = taskStore.tasks.findIndex(t => t.id === task.id);
      if (index !== -1) {
        taskStore.tasks[index] = task;
      }
    });

    this.connection.on('TaskMoved', (data) => {
      const taskStore = useTaskStore();
      const task = taskStore.tasks.find(t => t.id === data.taskId);
      if (task) {
        task.status = data.newStatus;
        task.orderIndex = data.newOrderIndex;
      }
    });

    try {
      await this.connection.start();
      console.log('Tasks SignalR connected.');
    } catch (err) {
      console.error('Tasks SignalR Connection Error: ', err);
      this.connection = null;
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

export default new SignalRService();
