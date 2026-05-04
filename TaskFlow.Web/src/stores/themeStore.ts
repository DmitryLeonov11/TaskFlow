import { create } from 'zustand';

interface ThemeState {
  isDark: boolean;
  toggle: () => void;
}

const initial = localStorage.getItem('theme') === 'dark';

export const useThemeStore = create<ThemeState>((set) => ({
  isDark: initial,
  toggle: () =>
    set((s) => {
      const next = !s.isDark;
      const theme = next ? 'dark' : 'light';
      localStorage.setItem('theme', theme);
      document.documentElement.setAttribute('data-theme', theme);
      return { isDark: next };
    }),
}));
