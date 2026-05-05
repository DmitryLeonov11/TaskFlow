import { createSignalRService } from './signalrFactory';
import { useTaskStore } from '../stores/taskStore';

const signalrService = createSignalRService('/tasks', (connection) => {
  connection.on('TaskCreated', (task) => {
    useTaskStore.getState().upsertTask(task);
  });

  connection.on('TaskUpdated', (task) => {
    useTaskStore.getState().upsertTask(task);
  });

  connection.on('TaskMoved', (data) => {
    const store = useTaskStore.getState();
    const task = store.tasks.find((t) => t.id === data.taskId);
    if (task) {
      store.upsertTask({ ...task, status: data.newStatus, orderIndex: data.newOrderIndex });
    }
  });
});

export default signalrService;
