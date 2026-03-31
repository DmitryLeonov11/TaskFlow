import { defineStore } from 'pinia';
import { ref, watch } from 'vue';

export const useThemeStore = defineStore('theme', () => {
  const isDark = ref(localStorage.getItem('theme') === 'dark');

  const applyTheme = (dark: boolean) => {
    const theme = dark ? 'dark' : 'light';
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('theme', theme);
  };

  watch(isDark, applyTheme, { immediate: true });

  const toggle = () => {
    isDark.value = !isDark.value;
  };

  return { isDark, toggle };
});
