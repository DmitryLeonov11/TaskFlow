import { create } from 'zustand';
import api from '../api/client';
import type { Task, CreateTaskInput, UpdateTaskInput } from '../types';

interface FetchParams {
  searchTerm?: string;
  tagIds?: string[];
  status?: number;
  projectId?: string | null;
  pageNumber?: number;
  pageSize?: number;
}

interface TaskState {
  tasks: Task[];
  loading: boolean;
  error: string | null;
  totalCount: number;
  fetchTasks: (params?: FetchParams) => Promise<void>;
  createTask: (data: CreateTaskInput) => Promise<Task>;
  updateTask: (id: string, data: UpdateTaskInput) => Promise<void>;
  moveTask: (id: string, status: number, orderIndex: number) => Promise<void>;
  deleteTask: (id: string) => Promise<void>;
  getTasksForStatus: (status: number) => Task[];
  upsertTask: (task: Task) => void;
  removeTask: (id: string) => void;
}

export const useTaskStore = create<TaskState>((set, get) => ({
  tasks: [],
  loading: false,
  error: null,
  totalCount: 0,

  fetchTasks: async (params = {}) => {
    set({ loading: true, error: null });
    try {
      const queryParams: Record<string, unknown> = {
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 200,
      };
      if (params.searchTerm) queryParams.searchTerm = params.searchTerm;
      if (params.status !== undefined) queryParams.status = params.status;
      if (params.tagIds?.length) queryParams.tagIds = params.tagIds;
      if (params.projectId) queryParams.projectId = params.projectId;

      const { data } = await api.get('/tasks', { params: queryParams });
      set({ tasks: data.tasks ?? [], totalCount: data.totalCount ?? 0 });
    } catch (e: unknown) {
      const msg = (e as any)?.response?.data?.message ?? 'Failed to fetch tasks';
      set({ error: msg });
      throw e;
    } finally {
      set({ loading: false });
    }
  },

  createTask: async (taskData) => {
    const { data } = await api.post('/tasks', taskData);
    const task: Task = data;
    if (!task.tags) task.tags = [];
    if (!task.comments) task.comments = [];
    if (!task.attachments) task.attachments = [];
    if (!task.subtasks) task.subtasks = [];
    set((s) => ({ tasks: [...s.tasks, task] }));
    return task;
  },

  updateTask: async (id, taskData) => {
    const { data } = await api.put(`/tasks/${id}`, taskData);
    const updated: Task = data;
    set((s) => ({
      tasks: s.tasks.map((t) =>
        t.id === id
          ? { ...t, ...updated, tags: updated.tags ?? t.tags, subtasks: updated.subtasks ?? t.subtasks }
          : t
      ),
    }));
  },

  moveTask: async (id, newStatus, newOrderIndex) => {
    await api.put(`/tasks/${id}/move`, { newStatus, newOrderIndex });
    await get().fetchTasks();
  },

  deleteTask: async (id) => {
    await api.delete(`/tasks/${id}`);
    set((s) => ({ tasks: s.tasks.filter((t) => t.id !== id) }));
  },

  getTasksForStatus: (status) =>
    get()
      .tasks.filter((t) => t.status === status)
      .sort((a, b) => a.orderIndex - b.orderIndex),

  upsertTask: (task) =>
    set((s) => {
      const idx = s.tasks.findIndex((t) => t.id === task.id);
      if (idx !== -1) {
        const tasks = [...s.tasks];
        tasks[idx] = task;
        return { tasks };
      }
      return { tasks: [...s.tasks, task] };
    }),

  removeTask: (id) =>
    set((s) => ({ tasks: s.tasks.filter((t) => t.id !== id) })),
}));
