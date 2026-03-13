import * as signalR from '@microsoft/signalr';
import { useTaskStore } from '../stores/taskStore';

class SignalRService {
    private connection: signalR.HubConnection | null = null;

    public async startConnection() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(import.meta.env.VITE_SIGNALR_HUB_URL || 'http://localhost:5000/hubs/tasks')
            .withAutomaticReconnect()
            .build();

        this.connection.on('TaskCreated', (task) => {
            const taskStore = useTaskStore();
            taskStore.tasks.push(task);
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
            console.log('SignalR Connected.');
        } catch (err) {
            console.error('SignalR Connection Error: ', err);
            setTimeout(() => this.startConnection(), 5000);
        }
    }

    public stopConnection() {
        if (this.connection) {
            this.connection.stop();
        }
    }
}

export default new SignalRService();
