import { defineStore } from 'pinia';
import api from '../services/apiService';
import signalrService from '../services/signalrService';
import { ref, computed } from 'vue';

export const useAuthStore = defineStore('auth', () => {
    const token = ref<string | null>(localStorage.getItem('auth_token'));
    const user = ref<any>(null);

    const userStr = localStorage.getItem('auth_user');
    if (userStr) {
        try {
            user.value = JSON.parse(userStr);
        } catch {
            user.value = null;
        }
    }

    const isAuthenticated = computed(() => !!token.value);

    const getUser = computed(() => user.value);

    const setAuth = (accessToken: string, userData: any) => {
        localStorage.setItem('auth_token', accessToken);
        localStorage.setItem('auth_user', JSON.stringify(userData));
        token.value = accessToken;
        user.value = userData;
    };

    const login = async (credentials: any) => {
        const response = await api.post('/auth/login', credentials);
        const userData = {
            id: response.data.id,
            email: response.data.email,
            firstName: response.data.firstName,
            lastName: response.data.lastName
        };
        setAuth(response.data.accessToken, userData);
    };

    const register = async (userData: any) => {
        const response = await api.post('/auth/register', userData);
        const authData = {
            id: response.data.id,
            email: response.data.email,
            firstName: response.data.firstName,
            lastName: response.data.lastName
        };
        setAuth(response.data.accessToken, authData);
    };

    const logout = async () => {
        await signalrService.stopConnection();
        localStorage.removeItem('auth_token');
        localStorage.removeItem('auth_user');
        token.value = null;
        user.value = null;
    };

    return { token, user, isAuthenticated, getUser, login, register, logout, setAuth };
});
