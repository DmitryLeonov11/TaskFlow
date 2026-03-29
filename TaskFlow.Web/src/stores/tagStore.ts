import { defineStore } from 'pinia';
import { ref } from 'vue';
import api from '../services/apiService';

export interface Tag {
    id: string;
    name: string;
    color: string | null;
}

export const useTagStore = defineStore('tags', () => {
    const tags = ref<Tag[]>([]);
    const loading = ref(false);

    const fetchTags = async () => {
        loading.value = true;
        try {
            const response = await api.get<Tag[]>('/tags');
            tags.value = response.data;
        } finally {
            loading.value = false;
        }
    };

    const createTag = async (name: string, color?: string) => {
        const response = await api.post<Tag>('/tags', { name, color });
        tags.value.push(response.data);
        return response.data;
    };

    const deleteTag = async (id: string) => {
        await api.delete(`/tags/${id}`);
        tags.value = tags.value.filter(t => t.id !== id);
    };

    return { tags, loading, fetchTags, createTag, deleteTag };
});
