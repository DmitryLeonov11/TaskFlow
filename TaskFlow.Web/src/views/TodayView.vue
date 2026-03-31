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

const todayTasks = computed(() => {
  const todayEnd = new Date();
  todayEnd.setHours(23, 59, 59, 999);

  return taskStore.tasks
    .filter(t => {
      if (!t.deadline) return false;
      const dl = new Date(t.deadline);
      return dl <= todayEnd && t.status !== 3; // not Done
    })
    .sort((a, b) => {
      const da = new Date(a.deadline!).getTime();
      const db = new Date(b.deadline!).getTime();
      return da - db;
    });
});

const overdueTasks = computed(() =>
  todayTasks.value.filter(t => new Date(t.deadline!) < new Date())
);

const dueTodayTasks = computed(() => {
  const todayStart = new Date();
  todayStart.setHours(0, 0, 0, 0);
  return todayTasks.value.filter(t => new Date(t.deadline!) >= todayStart);
});

const handleTaskClick = (task: Task) => {
  editingTask.value = task;
  isFormOpen.value = true;
};

const handleSaveTask = async (taskData: any) => {
  try {
    if (editingTask.value) {
      await taskStore.updateTask(editingTask.value.id, taskData);
    } else {
      await taskStore.createTask(taskData);
    }
    isFormOpen.value = false;
  } catch (error) {
    console.error('Error saving task:', error);
    alert('Failed to save task.');
  }
};

const handleDeleteTask = async (id: string) => {
  try {
    await taskStore.deleteTask(id);
    isFormOpen.value = false;
  } catch (error) {
    console.error('Error deleting task:', error);
    alert('Failed to delete task.');
  }
};
</script>

<template>
  <div class="h-full bg-base-200 overflow-y-auto">
    <div class="max-w-3xl mx-auto px-6 py-8">
      <header class="mb-8">
        <h1 class="text-2xl font-bold text-base-content mb-1">Today</h1>
        <p class="text-base-content/60 text-sm">
          {{ new Date().toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' }) }}
        </p>
      </header>

      <div v-if="taskStore.loading" class="flex justify-center py-12">
        <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-primary"></div>
      </div>

      <template v-else-if="todayTasks.length === 0">
        <div class="bg-base-100 rounded-xl border border-base-200 shadow-sm p-12 text-center">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-14 w-14 mx-auto mb-4 text-success" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <h3 class="text-base font-semibold text-base-content mb-1">All caught up!</h3>
          <p class="text-sm text-base-content/50">No tasks due today. Enjoy your day.</p>
        </div>
      </template>

      <template v-else>
        <!-- Overdue section -->
        <div v-if="overdueTasks.length" class="mb-6">
          <h2 class="text-xs font-semibold text-error uppercase tracking-wider mb-3 flex items-center gap-1.5">
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/>
            </svg>
            Overdue ({{ overdueTasks.length }})
          </h2>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <TaskCard
              v-for="task in overdueTasks"
              :key="task.id"
              :task="task"
              @click="handleTaskClick(task)"
            />
          </div>
        </div>

        <!-- Due today section -->
        <div v-if="dueTodayTasks.length">
          <h2 class="text-xs font-semibold text-base-content/50 uppercase tracking-wider mb-3">
            Due Today ({{ dueTodayTasks.length }})
          </h2>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <TaskCard
              v-for="task in dueTodayTasks"
              :key="task.id"
              :task="task"
              @click="handleTaskClick(task)"
            />
          </div>
        </div>
      </template>
    </div>

    <!-- Modal -->
    <div v-if="isFormOpen" class="fixed inset-0 bg-black/40 backdrop-blur-sm z-50 flex items-center justify-center p-4" @click.self="isFormOpen = false">
      <div class="w-full max-w-lg">
        <TaskForm
          :initial-data="editingTask ? { ...editingTask } : {}"
          :is-editing="!!editingTask"
          @save="handleSaveTask"
          @delete="handleDeleteTask"
          @cancel="isFormOpen = false"
        />
      </div>
    </div>
  </div>
</template>
