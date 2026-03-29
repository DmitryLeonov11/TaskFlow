import { defineStore } from 'pinia';
import api from '../services/apiService';
import { ref } from 'vue';
import type { Tag } from './tagStore';

export interface TaskComment {
    id: string;
    userId: string;
    content: string;
    createdAt: string;
}

export interface TaskAttachment {
    id: string;
    fileName: string;
    fileSize: number;
    uploadedAt: string;
}

export interface Task {
    id: string;
    title: string;
    description: string | null;
    priority: number;
    status: number;
    orderIndex: number;
    deadline?: string | null;
    createdAt: string;
    updatedAt: string;
    tags: Tag[];
    comments: TaskComment[];
    attachments: TaskAttachment[];
}

export interface FetchTasksParams {
    searchTerm?: string;
    tagIds?: string[];
    status?: number;
    pageNumber?: number;
    pageSize?: number;
}

export const useTaskStore = defineStore('task', () => {
    const tasks = ref<Task[]>([]);
    const loading = ref(false);
    const totalCount = ref(0);

    const fetchTasks = async (params: FetchTasksParams = {}) => {
        loading.value = true;
        try {
            const queryParams: Record<string, any> = {
                pageNumber: params.pageNumber ?? 1,
                pageSize: params.pageSize ?? 200,
            };
            if (params.searchTerm) queryParams.searchTerm = params.searchTerm;
            if (params.status !== undefined) queryParams.status = params.status;
            // tagIds are not supported as query array by default in axios — send as repeated params
            if (params.tagIds?.length) {
                queryParams['tagIds'] = params.tagIds;
            }

            const response = await api.get('/tasks', { params: queryParams });
            const data = response.data;
            tasks.value = data.tasks ?? data;
            totalCount.value = data.totalCount ?? tasks.value.length;
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
        // Ensure tags/comments/attachments default to arrays
        if (!newTask.tags) newTask.tags = [];
        if (!newTask.comments) newTask.comments = [];
        if (!newTask.attachments) newTask.attachments = [];
        tasks.value.push(newTask);
        return newTask;
    };

    const updateTask = async (id: string, taskData: any) => {
        const response = await api.put(`/tasks/${id}`, taskData);
        const updated = response.data;
        const index = tasks.value.findIndex(t => t.id === id);
        if (index !== -1) {
            tasks.value[index] = {
                ...tasks.value[index],
                ...updated,
                tags: updated.tags ?? tasks.value[index].tags,
                comments: updated.comments ?? tasks.value[index].comments,
                attachments: updated.attachments ?? tasks.value[index].attachments,
            };
        }
    };

    const moveTask = async (id: string, newStatus: number, newOrderIndex: number) => {
        await api.put(`/tasks/${id}/move`, { newStatus, newOrderIndex });
        const task = tasks.value.find(t => t.id === id);
        if (!task) return;

        task.status = newStatus;
        task.orderIndex = newOrderIndex;

        tasks.value
            .filter(t => t.status === newStatus)
            .sort((a, b) => a.orderIndex - b.orderIndex)
            .forEach((t, index) => { t.orderIndex = index; });
    };

    const deleteTask = async (id: string) => {
        await api.delete(`/tasks/${id}`);
        tasks.value = tasks.value.filter(t => t.id !== id);
    };

    return { tasks, loading, totalCount, fetchTasks, getTasksForStatus, createTask, updateTask, moveTask, deleteTask };
});
