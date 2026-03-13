import { defineStore } from 'pinia';
import api from '../services/apiService';
import { ref } from 'vue';

export const useAuthStore = defineStore('auth', () => {
    const token = ref<string | null>(localStorage.getItem('token'));
    const user = ref<any>(null);

    const isAuthenticated = () => !!token.value;

    const login = async (credentials: any) => {
        const response = await api.post('/auth/login', credentials);
        token.value = response.data.token;
        localStorage.setItem('token', response.data.token);
        // fetch user profile if needed
    };

    const register = async (userData: any) => {
        await api.post('/auth/register', userData);
    };

    const logout = () => {
        token.value = null;
        user.value = null;
        localStorage.removeItem('token');
    };

    return { token, user, isAuthenticated, login, register, logout };
});
