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
  updateTask: (id: string, data: UpdateTaskInput) => Promise<Task>;
  moveTask: (id: string, status: number, orderIndex: number) => Promise<void>;
  deleteTask: (id: string) => Promise<void>;
  upsertTask: (task: Task) => void;
  removeTask: (id: string) => void;
}

const normalizeTask = (task: Task): Task => ({
  ...task,
  tags: task.tags ?? [],
  comments: task.comments ?? [],
  attachments: task.attachments ?? [],
  subtasks: task.subtasks ?? [],
});

export const useTaskStore = create<TaskState>((set) => ({
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
      const tasks: Task[] = (data.tasks ?? []).map(normalizeTask);
      set({ tasks, totalCount: data.totalCount ?? 0 });
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to fetch tasks';
      set({ error: msg });
      throw e;
    } finally {
      set({ loading: false });
    }
  },

  createTask: async (taskData) => {
    const { data } = await api.post('/tasks', taskData);
    const task = normalizeTask(data);
    set((s) => ({ tasks: [...s.tasks, task] }));
    return task;
  },

  updateTask: async (id, taskData) => {
    const { data } = await api.put(`/tasks/${id}`, taskData);
    const updated = normalizeTask(data);
    set((s) => ({
      tasks: s.tasks.map((t) => (t.id === id ? { ...t, ...updated } : t)),
    }));
    return updated;
  },

  moveTask: async (id, newStatus, newOrderIndex) => {
    await api.put(`/tasks/${id}/move`, { newStatus, newOrderIndex });
    set((s) => ({
      tasks: s.tasks.map((t) =>
        t.id === id ? { ...t, status: newStatus, orderIndex: newOrderIndex } : t
      ),
    }));
  },

  deleteTask: async (id) => {
    await api.delete(`/tasks/${id}`);
    set((s) => ({ tasks: s.tasks.filter((t) => t.id !== id) }));
  },

  upsertTask: (task) =>
    set((s) => {
      const normalized = normalizeTask(task);
      const idx = s.tasks.findIndex((t) => t.id === task.id);
      if (idx !== -1) {
        const tasks = [...s.tasks];
        tasks[idx] = normalized;
        return { tasks };
      }
      return { tasks: [...s.tasks, normalized] };
    }),

  removeTask: (id) =>
    set((s) => ({ tasks: s.tasks.filter((t) => t.id !== id) })),
}));
