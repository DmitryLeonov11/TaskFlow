<script setup lang="ts">
import { onMounted, computed, ref } from 'vue';
import { useTaskStore, type Task } from '../stores/taskStore';
import TaskCard from '../components/TaskCard.vue';
import TaskForm from '../components/TaskForm.vue';

const taskStore = useTaskStore();
const isFormOpen = ref(false);
const editingTask = ref<Task | undefined>(undefined);

onMounted(async () => {
  await taskStore.fetchTasks();
});

// Filter tasks due today or overdue
const todayTasks = computed(() => {
  const todayStart = new Date();
  todayStart.setHours(0, 0, 0, 0);
  
  const todayEnd = new Date();
  todayEnd.setHours(23, 59, 59, 999);

  return taskStore.tasks.filter(t => {
    if (!t.deadline) return false;
    const dl = new Date(t.deadline);
    // Overdue or due today
    return dl <= todayEnd && t.status !== 3; // Not done
  });
});

const handleTaskClick = (task: Task) => {
  editingTask.value = task;
  isFormOpen.value = true;
};

const handleSaveTask = async (taskData: Partial<Task>) => {
  try {
    if (editingTask.value) {
      await taskStore.updateTask(editingTask.value.id, taskData);
    } else {
      await taskStore.createTask(taskData);
    }
    isFormOpen.value = false;
  } catch (error) {
    console.error('Error saving task:', error);
    alert('Failed to save task. Please check your authentication and try again.');
  }
};
</script>

<template>
  <div class="h-full bg-slate-50 p-6 overflow-y-auto">
    <div class="max-w-4xl mx-auto">
      <header class="mb-8 flex items-end justify-between">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 mb-2">Today</h1>
          <p class="text-gray-500">{{ new Date().toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' }) }}</p>
        </div>
      </header>

      <div v-if="taskStore.loading" class="flex justify-center py-12">
        <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-600"></div>
      </div>
      
      <div v-else-if="todayTasks.length === 0" class="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center text-gray-500">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 mx-auto mb-4 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
        </svg>
        <h3 class="text-lg font-medium text-gray-900 mb-1">You're all caught up!</h3>
        <p>No tasks due today. Enjoy your day or plan ahead.</p>
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <TaskCard 
          v-for="task in todayTasks" 
          :key="task.id" 
          :task="task" 
          @click="handleTaskClick(task)"
        />
      </div>
    </div>

    <!-- Modal Form Layer -->
    <div v-if="isFormOpen" class="fixed inset-0 bg-black/40 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div class="w-full max-w-lg">
        <TaskForm 
          :initial-data="editingTask ? { ...editingTask } : {}"
          :is-editing="!!editingTask"
          @save="handleSaveTask"
          @cancel="isFormOpen = false"
        />
      </div>
    </div>
  </div>
</template>
