<script setup lang="ts">
import { onMounted, computed, ref } from 'vue';
import { useTaskStore, type Task } from '../stores/taskStore';
import KanbanColumn from '../components/KanbanColumn.vue';
import TaskForm from '../components/TaskForm.vue';

const taskStore = useTaskStore();
const isFormOpen = ref(false);
const editingTask = ref<Task | undefined>(undefined);
const defaultStatusForNew = ref(0);

const columns = [
  { id: 0, title: 'To Do' },
  { id: 1, title: 'In Progress' },
  { id: 2, title: 'Review' },
  { id: 3, title: 'Done' }
];

onMounted(async () => {
  await taskStore.fetchTasks();
});

const getTasksForStatus = (status: number) => {
  return taskStore.tasks
    .filter(t => t.status === status)
    .sort((a, b) => a.orderIndex - b.orderIndex);
};

const handleTaskClick = (task: Task) => {
  editingTask.value = task;
  isFormOpen.value = true;
};

const openNewTaskForm = (status: number = 0) => {
  editingTask.value = undefined;
  defaultStatusForNew.value = status;
  isFormOpen.value = true;
};

const handleSaveTask = async (taskData: Partial<Task>) => {
  if (editingTask.value) {
    await taskStore.updateTask(editingTask.value.id, taskData);
  } else {
    await taskStore.createTask({ ...taskData, status: taskData.status ?? defaultStatusForNew.value, orderIndex: 0 });
  }
  isFormOpen.value = false;
};

const closeForm = () => {
  isFormOpen.value = false;
};
</script>

<template>
  <div class="h-full flex flex-col bg-slate-50">
    <header class="bg-white border-b px-6 py-4 flex items-center justify-between shadow-sm z-10">
      <div>
        <h1 class="text-2xl font-bold text-gray-800">Board</h1>
        <p class="text-sm text-gray-500 mt-1">Manage your team's tasks</p>
      </div>
      <button 
        @click="openNewTaskForm(0)"
        class="bg-blue-600 hover:bg-blue-700 text-white px-5 py-2.5 rounded-lg font-medium transition-colors shadow-sm flex items-center gap-2"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
          <path fill-rule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clip-rule="evenodd" />
        </svg>
        New Task
      </button>
    </header>

    <div class="flex-1 overflow-x-auto p-6">
      <div v-if="taskStore.loading" class="flex justify-center items-center h-full">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
      
      <div v-else class="flex gap-6 h-full items-start">
        <KanbanColumn 
          v-for="column in columns" 
          :key="column.id"
          :title="column.title"
          :status="column.id"
          :tasks="getTasksForStatus(column.id)"
          @task-click="handleTaskClick"
          @add-task="openNewTaskForm"
        />
      </div>
    </div>

    <!-- Modal Form Layer -->
    <div v-if="isFormOpen" class="fixed inset-0 bg-black/40 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div class="w-full max-w-lg">
        <TaskForm 
          :initial-data="editingTask ? { ...editingTask } : { status: defaultStatusForNew }"
          :is-editing="!!editingTask"
          @save="handleSaveTask"
          @cancel="closeForm"
        />
      </div>
    </div>
  </div>
</template>
