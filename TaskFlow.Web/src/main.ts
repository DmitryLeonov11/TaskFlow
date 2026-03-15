import { createApp } from 'vue';
import { createPinia } from 'pinia';
import router from './router';
import './style.css';
import App from './App.vue';
import signalrService from './services/signalrService';
import notificationsSignalrService from './services/notificationsSignalrService';

const app = createApp(App);

app.use(createPinia());
app.use(router);

app.mount('#app');

signalrService.startConnection();
notificationsSignalrService.startConnection();
