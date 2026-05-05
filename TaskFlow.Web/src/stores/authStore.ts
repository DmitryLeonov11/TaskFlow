import { create } from 'zustand';
import api from '../api/client';
import signalrService from '../services/signalrService';
import notificationsSignalrService from '../services/notificationsSignalrService';
import type { User } from '../types';

interface AuthUser extends User {
  roles?: string[];
  role?: string;
}

interface AuthState {
  token: string | null;
  user: AuthUser | null;
  setAuth: (token: string, user: AuthUser) => void;
  login: (credentials: { email: string; password: string }) => Promise<void>;
  register: (data: {
    email: string; password: string; confirmPassword: string;
    firstName?: string; lastName?: string;
  }) => Promise<void>;
  logout: () => Promise<void>;
  restoreFromStorage: () => void;
}

const parseUser = (str: string | null): AuthUser | null => {
  if (!str) return null;
  try { return JSON.parse(str); } catch { return null; }
};

const isTokenValid = (token: string): boolean => {
  try {
    const base64url = token.split('.')[1];
    const base64 = base64url.replace(/-/g, '+').replace(/_/g, '/');
    const padding = '='.repeat((4 - (base64.length % 4)) % 4);
    const payload = JSON.parse(atob(base64 + padding));
    if (typeof payload.exp !== 'number') return false;
    return payload.exp * 1000 > Date.now();
  } catch {
    return false;
  }
};

export const useAuthStore = create<AuthState>((set, get) => ({
  token: localStorage.getItem('auth_token'),
  user: parseUser(localStorage.getItem('auth_user')),

  setAuth: (token, user) => {
    localStorage.setItem('auth_token', token);
    localStorage.setItem('auth_user', JSON.stringify(user));
    set({ token, user });
  },

  login: async (credentials) => {
    const { data } = await api.post('/auth/login', credentials);
    const authData: AuthUser = {
      id: data.userId,
      email: data.email,
      firstName: data.firstName,
      lastName: data.lastName,
      roles: data.roles,
      role: data.role,
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
    if (!token || !user || !isTokenValid(token)) {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('auth_user');
      return;
    }
    set({ token, user });
  },
}));

export const useIsAuthenticated = () => useAuthStore((s) => !!s.token);
export const useIsAdmin = () =>
  useAuthStore((s) => {
    const u = s.user;
    return !!(u?.roles?.includes('Admin') || u?.role === 'Admin');
  });
