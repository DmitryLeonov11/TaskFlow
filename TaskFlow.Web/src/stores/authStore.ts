import { create } from 'zustand';
import api from '../api/client';
import signalrService from '../services/signalrService';
import notificationsSignalrService from '../services/notificationsSignalrService';
import type { User } from '../types';

interface AuthState {
  token: string | null;
  user: User | null;
  isAuthenticated: boolean;
  isAdmin: boolean;
  setAuth: (token: string, user: User) => void;
  login: (credentials: { email: string; password: string }) => Promise<void>;
  register: (data: {
    email: string; password: string; confirmPassword: string;
    firstName?: string; lastName?: string;
  }) => Promise<void>;
  logout: () => Promise<void>;
  restoreFromStorage: () => void;
}

const parseUser = (str: string | null): User | null => {
  if (!str) return null;
  try { return JSON.parse(str); } catch { return null; }
};

export const useAuthStore = create<AuthState>((set, get) => ({
  token: localStorage.getItem('auth_token'),
  user: parseUser(localStorage.getItem('auth_user')),
  get isAuthenticated() { return !!get().token; },
  get isAdmin() {
    const u = get().user as any;
    return u?.roles?.includes('Admin') || u?.role === 'Admin' || false;
  },

  setAuth: (token, user) => {
    localStorage.setItem('auth_token', token);
    localStorage.setItem('auth_user', JSON.stringify(user));
    set({ token, user });
  },

  login: async (credentials) => {
    const { data } = await api.post('/auth/login', credentials);
    const authData: User = {
      id: data.userId,
      email: data.email,
      firstName: data.firstName,
      lastName: data.lastName,
    };
    get().setAuth(data.accessToken, authData);
  },

  register: async (data) => {
    await api.post('/auth/register', data);
  },

  logout: async () => {
    await signalrService.stopConnection();
    await notificationsSignalrService.stopConnection();
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_user');
    set({ token: null, user: null });
  },

  restoreFromStorage: () => {
    const token = localStorage.getItem('auth_token');
    const user = parseUser(localStorage.getItem('auth_user'));
    if (token && user) set({ token, user });
  },
}));
