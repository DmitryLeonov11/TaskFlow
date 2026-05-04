import { create } from 'zustand';
import api from '../api/client';
import type { Tag } from '../types';

interface TagState {
  tags: Tag[];
  fetchTags: () => Promise<void>;
  createTag: (name: string, color?: string) => Promise<Tag>;
  deleteTag: (id: string) => Promise<void>;
}

export const useTagStore = create<TagState>((set) => ({
  tags: [],

  fetchTags: async () => {
    const { data } = await api.get('/tags');
    set({ tags: data });
  },

  createTag: async (name, color) => {
    const { data } = await api.post('/tags', { name, color });
    set((s) => ({ tags: [...s.tags, data] }));
    return data;
  },

  deleteTag: async (id) => {
    await api.delete(`/tags/${id}`);
    set((s) => ({ tags: s.tags.filter((t) => t.id !== id) }));
  },
}));
