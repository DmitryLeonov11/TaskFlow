import axios from 'axios';

const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api',
    headers: {
        'Content-Type': 'application/json'
    },
    withCredentials: true
});

api.interceptors.request.use(config => {
    const token = localStorage.getItem('auth_token');
    console.log('[API Request]', config.method?.toUpperCase(), config.url, 'Token present:', !!token);
    if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
    } else if (!token && config.url !== '/auth/login' && config.url !== '/auth/register') {
        console.warn('[API] No auth token found for request:', config.url);
    }
    return config;
});

api.interceptors.response.use(
    response => response,
    error => {
        // Don't clear localStorage here - let the router guard handle redirects
        // This prevents logout when a single API call fails
        return Promise.reject(error);
    }
);

export default api;
