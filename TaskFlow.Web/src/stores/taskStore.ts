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
            tasks.value = response.data;
        } finally {
            loading.value = false;
        }
    };

    const createTask = async (taskData: any) => {
        const response = await api.post('/tasks', taskData);
        tasks.value.push(response.data);
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
        if (task) {
            task.status = newStatus;
            task.orderIndex = newOrderIndex;
        }
    };

    const deleteTask = async (id: string) => {
        await api.delete(`/tasks/${id}`);
        tasks.value = tasks.value.filter(t => t.id !== id);
    };

    return { tasks, loading, fetchTasks, createTask, updateTask, moveTask, deleteTask };
});
