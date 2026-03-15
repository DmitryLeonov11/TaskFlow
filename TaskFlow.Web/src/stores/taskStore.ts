import { defineStore } from 'pinia';
import api from '../services/apiService';
import { ref } from 'vue';

export interface Task {
    id: string;
    title: string;
    description: string;
    priority: number;
    status: number;
    orderIndex: number;
    deadline?: string;
    tags?: any[];
}

export const useTaskStore = defineStore('task', () => {
    const tasks = ref<Task[]>([]);
    const loading = ref(false);

    const fetchTasks = async () => {
        loading.value = true;
        try {
            const response = await api.get('/tasks');
            tasks.value = response.data.tasks ?? response.data;
        } finally {
            loading.value = false;
        }
    };

    const getTasksForStatus = (status: number) => {
        return tasks.value
            .filter(t => t.status === status)
            .sort((a, b) => a.orderIndex - b.orderIndex);
    };

    const createTask = async (taskData: any) => {
        const response = await api.post('/tasks', taskData);
        const newTask = response.data;
        tasks.value.push(newTask);
    };

    const updateTask = async (id: string, taskData: any) => {
        await api.put(`/tasks/${id}`, taskData);
        const index = tasks.value.findIndex(t => t.id === id);
        if (index !== -1) {
            tasks.value[index] = { ...tasks.value[index], ...taskData };
        }
    };

    const moveTask = async (id: string, newStatus: number, newOrderIndex: number) => {
        await api.put(`/tasks/${id}/move`, { newStatus, newOrderIndex });
        const task = tasks.value.find(t => t.id === id);
        if (!task) return;

        task.status = newStatus;
        task.orderIndex = newOrderIndex;

        const sameColumnTasks = tasks.value
            .filter(t => t.status === newStatus)
            .sort((a, b) => a.orderIndex - b.orderIndex);

        sameColumnTasks.forEach((t, index) => {
            t.orderIndex = index;
        });
    };

    const deleteTask = async (id: string) => {
        await api.delete(`/tasks/${id}`);
        tasks.value = tasks.value.filter(t => t.id !== id);
    };

    return { tasks, loading, fetchTasks, getTasksForStatus, createTask, updateTask, moveTask, deleteTask };
});
