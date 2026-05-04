import { create } from 'zustand';
import api from '../api/client';
import type { Project, CreateProjectInput, UpdateProjectInput } from '../types';

interface ProjectState {
  projects: Project[];
  selectedProjectId: string | null;
  fetchProjects: () => Promise<void>;
  createProject: (data: CreateProjectInput) => Promise<Project>;
  updateProject: (id: string, data: UpdateProjectInput) => Promise<Project>;
  deleteProject: (id: string) => Promise<void>;
  selectProject: (id: string | null) => void;
}

export const useProjectStore = create<ProjectState>((set, get) => ({
  projects: [],
  selectedProjectId: null,

  fetchProjects: async () => {
    const { data } = await api.get('/projects');
    set({ projects: data });
  },

  createProject: async (projectData) => {
    const { data } = await api.post('/projects', projectData);
    set((s) => ({ projects: [...s.projects, data] }));
    return data;
  },

  updateProject: async (id, projectData) => {
    const { data } = await api.put(`/projects/${id}`, projectData);
    set((s) => ({ projects: s.projects.map((p) => (p.id === id ? data : p)) }));
    return data;
  },

  deleteProject: async (id) => {
    await api.delete(`/projects/${id}`);
    const { selectedProjectId } = get();
    set((s) => ({
      projects: s.projects.filter((p) => p.id !== id),
      selectedProjectId: selectedProjectId === id ? null : selectedProjectId,
    }));
  },

  selectProject: (id) => set({ selectedProjectId: id }),
}));
