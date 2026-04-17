<script setup lang="ts">
import { ref, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/authStore';
import api from '../services/apiService';

const authStore = useAuthStore();
const router = useRouter();

const email = ref('');
const password = ref('');
const confirmPassword = ref('');
const firstName = ref('');
const lastName = ref('');
const isResetMode = ref(false);
const isRegisterMode = ref(false);
const resetEmail = ref('');
const resetToken = ref('');
const newPassword = ref('');
const message = ref('');
const error = ref('');

const login = async () => {
  error.value = '';
  message.value = '';
  try {
    await authStore.login({ email: email.value, password: password.value });
    await router.push('/board');
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Login failed.';
  }
};

const register = async () => {
  error.value = '';
  message.value = '';
  if (password.value !== confirmPassword.value) {
    error.value = 'Passwords do not match.';
    return;
  }
  try {
    await authStore.register({
      email: email.value,
      password: password.value,
      confirmPassword: confirmPassword.value,
      firstName: firstName.value,
      lastName: lastName.value
    });
    // After registration, redirect to login
    message.value = 'Registration successful! Redirecting to login...';
    const redirectTimer = setTimeout(() => router.push('/auth'), 1000);
    onUnmounted(() => clearTimeout(redirectTimer));
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Registration failed.';
  }
};

const requestReset = async () => {
  error.value = '';
  message.value = '';
  try {
    await api.post('/auth/request-password-reset', { email: resetEmail.value });
    message.value = 'If the email exists, reset instructions were sent (token logged on server).';
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to request password reset.';
  }
};

const resetPassword = async () => {
  error.value = '';
  message.value = '';
  try {
    await api.post('/auth/reset-password', {
      email: resetEmail.value,
      token: resetToken.value,
      newPassword: newPassword.value
    });
    message.value = 'Password has been reset. You can login with new password.';
    isResetMode.value = false;
  } catch (e: any) {
    error.value = e.response?.data?.message || 'Failed to reset password.';
  }
};
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-base-200 px-4">
    <div class="w-full max-w-md bg-base-100 rounded-2xl shadow-lg border border-base-200 p-8 space-y-6">
      <h1 class="text-2xl font-bold text-base-content text-center mb-2">
        {{ isResetMode ? 'Reset Password' : isRegisterMode ? 'Create Account' : 'Login' }}
      </h1>
      <p class="text-sm text-base-content/60 text-center mb-6">
        {{ isResetMode ? 'Reset your TaskFlow account password.' : isRegisterMode ? 'Create your TaskFlow account.' : 'Sign in to your TaskFlow account.' }}
      </p>

      <div v-if="error" class="text-sm text-red-600 bg-red-50 border border-red-100 px-3 py-2 rounded-lg">
        {{ error }}
      </div>
      <div v-if="message" class="text-sm text-emerald-700 bg-emerald-50 border border-emerald-100 px-3 py-2 rounded-lg">
        {{ message }}
      </div>

      <!-- Login Form -->
      <form v-if="!isResetMode && !isRegisterMode" @submit.prevent="login" class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-base-content/80 mb-1">Email</label>
          <input
            v-model="email"
            type="email"
            required
            class="w-full border border-base-300 rounded-lg px-3 py-2 text-sm bg-base-200 text-base-content placeholder:text-base-content/40 focus:ring-2 focus:ring-primary focus:border-primary"
            placeholder="you@example.com"
          />
        </div>
        <div>
          <label class="block text-sm font-medium text-base-content/80 mb-1">Password</label>
          <input
            v-model="password"
            type="password"
            required
            class="w-full border border-base-300 rounded-lg px-3 py-2 text-sm bg-base-200 text-base-content placeholder:text-base-content/40 focus:ring-2 focus:ring-primary focus:border-primary"
            placeholder="••••••••"
          />
        </div>

        <button
          type="submit"
          class="w-full inline-flex justify-center items-center px-4 py-2.5 rounded-lg bg-primary text-primary-content text-sm font-medium hover:bg-primary/90 transition-colors shadow-sm"
        >
          Login
        </button>

        <div class="flex items-center justify-between">
          <button
            type="button"
            class="text-xs text-base-content/60 hover:text-base-content/80"
            @click="isRegisterMode = true"
          >
            Don't have an account? Register
          </button>
          <button
            type="button"
            class="text-xs text-base-content/60 hover:text-base-content/80"
            @click="isResetMode = true"
          >
            Forgot password?
          </button>
        </div>
      </form>

      <!-- Register Form -->
      <form v-else-if="isRegisterMode" @submit.prevent="register" class="space-y-4">
        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="block text-sm font-medium text-base-content/80 mb-1">First Name</label>
            <input
              v-model="firstName"
              type="text"
              class="w-full border border-base-300 rounded-lg px-3 py-2 text-sm bg-base-200 text-base-content placeholder:text-base-content/40 focus:ring-2 focus:ring-primary focus:border-primary"
              placeholder="John"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-base-content/80 mb-1">Last Name</label>
            <input
              v-model="lastName"
              type="text"
              class="w-full border border-base-300 rounded-lg px-3 py-2 text-sm bg-base-200 text-base-content placeholder:text-base-content/40 focus:ring-2 focus:ring-primary focus:border-primary"
              placeholder="Doe"
            />
          </div>
        </div>
        <div>
          <label class="block text-sm font-medium text-base-content/80 mb-1">Email</label>
          <input
            v-model="email"
            type="email"
            required
            class="w-full border border-base-300 rounded-lg px-3 py-2 text-sm bg-base-200 text-base-content placeholder:text-base-content/40 focus:ring-2 focus:ring-primary focus:border-primary"
            placeholder="you@example.com"
          />
        </div>
        <div>
          <label class="block text-sm font-medium text-base-content/80 mb-1">Password</label>
          <input
            v-model="password"
            type="password"
            required
            minlength="8"
            class="w-full border border-base-300 rounded-lg px-3 py-2 text-sm bg-base-200 text-base-content placeholder:text-base-content/40 focus:ring-2 focus:ring-primary focus:border-primary"
            placeholder="••••••••"
          />
          <p class="text-xs text-base-content/60 mt-1">
            Min 8 chars: 1 uppercase, 1 lowercase, 1 digit, 1 special character
          </p>
        </div>
        <div>
          <label class="block text-sm font-medium text-base-content/80 mb-1">Confirm Password</label>
          <input
            v-model="confirmPassword"
            type="password"
            required
            minlength="8"
            class="w-full border border-base-300 rounded-lg px-3 py-2 text-sm bg-base-200 text-base-content placeholder:text-base-content/40 focus:ring-2 focus:ring-primary focus:border-primary"
            placeholder="••••••••"
          />
        </div>

        <button
          type="submit"
          class="w-full inline-flex justify-center items-center px-4 py-2.5 rounded-lg bg-primary text-primary-content text-sm font-medium hover:bg-primary/90 transition-colors shadow-sm"
        >
          Register
        </button>

        <button
          type="button"
          class="w-full text-xs text-base-content/60 hover:text-base-content/80 text-center"
          @click="isRegisterMode = false"
        >
          Already have an account? Login
        </button>
      </form>

      <form v-else @submit.prevent="resetPassword" class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-base-content/80 mb-1">Email</label>
          <input
            v-model="resetEmail"
            type="email"
            required
            class="w-full border border-base-300 rounded-lg px-3 py-2 text-sm bg-base-200 text-base-content placeholder:text-base-content/40 focus:ring-2 focus:ring-primary focus:border-primary"
            placeholder="you@example.com"
          />
        </div>

        <div class="space-y-2">
          <button
            type="button"
            class="w-full inline-flex justify-center items-center px-4 py-2.5 rounded-lg bg-neutral text-neutral-content text-sm font-medium hover:bg-neutral/90 transition-colors shadow-sm"
            @click="requestReset"
          >
            Request reset link
          </button>
          <p class="text-xs text-base-content/60">
            In this demo, reset token is written to server logs. Copy it from API logs and paste below.
          </p>
        </div>

        <div>
          <label class="block text-sm font-medium text-base-content/80 mb-1">Reset token</label>
          <textarea
            v-model="resetToken"
            rows="3"
            required
            class="w-full border border-base-300 rounded-lg px-3 py-2 text-sm bg-base-200 text-base-content placeholder:text-base-content/40 focus:ring-2 focus:ring-primary focus:border-primary"
            placeholder="Paste token from email / logs"
          ></textarea>
        </div>

        <div>
          <label class="block text-sm font-medium text-base-content/80 mb-1">New password</label>
          <input
            v-model="newPassword"
            type="password"
            required
            minlength="8"
            class="w-full border border-base-300 rounded-lg px-3 py-2 text-sm bg-base-200 text-base-content placeholder:text-base-content/40 focus:ring-2 focus:ring-primary focus:border-primary"
            placeholder="New secure password"
          />
        </div>

        <div class="flex gap-3 pt-1">
          <button
            type="button"
            class="flex-1 inline-flex justify-center items-center px-4 py-2.5 rounded-lg border border-base-300 text-sm font-medium text-base-content/80 hover:bg-base-200"
            @click="isResetMode = false"
          >
            Back to login
          </button>
          <button
            type="submit"
            class="flex-1 inline-flex justify-center items-center px-4 py-2.5 rounded-lg bg-primary text-primary-content text-sm font-medium hover:bg-primary/90 transition-colors shadow-sm"
          >
            Reset password
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
