<script setup lang="ts">
import { onMounted, ref, computed } from 'vue';
import { useTaskStore, type Task } from '../stores/taskStore';
import { useTagStore } from '../stores/tagStore';
import KanbanColumn from '../components/KanbanColumn.vue';
import TaskForm from '../components/TaskForm.vue';

const taskStore = useTaskStore();
const tagStore = useTagStore();

const isFormOpen = ref(false);
const editingTask = ref<Task | undefined>(undefined);
const defaultStatusForNew = ref(0);

const searchTerm = ref('');
const selectedFilterTags = ref<string[]>([]);

const columns = [
  { id: 0, title: 'To Do' },
  { id: 1, title: 'In Progress' },
  { id: 2, title: 'Review' },
  { id: 3, title: 'Done' },
];

onMounted(async () => {
  await Promise.all([taskStore.fetchTasks(), tagStore.fetchTags()]);
});

// Client-side filter (tasks are already loaded in full; search only re-fetches on explicit submit)
const filteredTasks = computed(() => {
  let result = taskStore.tasks;
  const term = searchTerm.value.trim().toLowerCase();
  if (term) {
    result = result.filter(
      t =>
        t.title.toLowerCase().includes(term) ||
        (t.description?.toLowerCase().includes(term))
    );
  }
  if (selectedFilterTags.value.length) {
    result = result.filter(t =>
      selectedFilterTags.value.every(tagId => t.tags?.some(tag => tag.id === tagId))
    );
  }
  return result;
});

const handleTaskClick = (task: Task) => {
  editingTask.value = task;
  isFormOpen.value = true;
};

const openNewTaskForm = (status: number = 0) => {
  editingTask.value = undefined;
  defaultStatusForNew.value = status;
  isFormOpen.value = true;
};

const handleSaveTask = async (taskData: any) => {
  try {
    if (editingTask.value) {
      await taskStore.updateTask(editingTask.value.id, taskData);
    } else {
      await taskStore.createTask({ ...taskData, status: taskData.status ?? defaultStatusForNew.value, orderIndex: 0 });
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

const closeForm = () => {
  isFormOpen.value = false;
};

const handleDropTask = async (taskId: string, newStatus: number) => {
  const task = taskStore.tasks.find(t => t.id === taskId);
  if (!task) return;

  const columnTasks = taskStore.tasks.filter(t => t.status === newStatus);
  const newOrderIndex = columnTasks.length;

  if (task.status !== newStatus || task.orderIndex !== newOrderIndex) {
    task.status = newStatus;
    task.orderIndex = newOrderIndex;
    try {
      await taskStore.moveTask(taskId, newStatus, newOrderIndex);
    } catch {
      await taskStore.fetchTasks();
    }
  }
};

const toggleFilterTag = (tagId: string) => {
  const idx = selectedFilterTags.value.indexOf(tagId);
  if (idx === -1) selectedFilterTags.value.push(tagId);
  else selectedFilterTags.value.splice(idx, 1);
};

const clearFilters = () => {
  searchTerm.value = '';
  selectedFilterTags.value = [];
};

const hasActiveFilters = computed(() => searchTerm.value.trim() || selectedFilterTags.value.length > 0);
</script>

<template>
  <div class="h-full flex flex-col bg-base-200">
    <!-- Header -->
    <header class="bg-base-100 border-b border-base-300 px-6 py-3 shadow-sm z-10">
      <div class="flex items-center justify-between mb-3">
        <div>
          <h1 class="text-xl font-bold text-base-content">Board</h1>
        </div>
        <button
          @click="openNewTaskForm(0)"
          class="bg-primary hover:bg-primary/90 text-primary-content px-4 py-2 rounded-lg text-sm font-medium transition-colors shadow-sm flex items-center gap-1.5"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clip-rule="evenodd" />
          </svg>
          New Task
        </button>
      </div>

      <!-- Search + tag filters -->
      <div class="flex items-center gap-3 flex-wrap">
        <div class="relative">
          <svg class="absolute left-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-base-content/40" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
          </svg>
          <input
            v-model="searchTerm"
            type="search"
            placeholder="Search tasks..."
            class="pl-8 pr-3 py-1.5 text-sm border border-base-300 rounded-lg bg-base-200 text-base-content focus:ring-2 focus:ring-primary focus:border-primary w-52"
          />
        </div>

        <div v-if="tagStore.tags.length" class="flex items-center gap-1.5 flex-wrap">
          <button
            v-for="tag in tagStore.tags"
            :key="tag.id"
            type="button"
            @click="toggleFilterTag(tag.id)"
            :class="[
              'px-2.5 py-1 text-xs rounded-full border transition-colors',
              selectedFilterTags.includes(tag.id)
                ? 'bg-primary text-primary-content border-primary'
                : 'bg-base-100 text-base-content/70 border-base-300 hover:border-primary/70 hover:text-primary'
            ]"
          >
            {{ tag.name }}
          </button>
        </div>

        <button
          v-if="hasActiveFilters"
          type="button"
          @click="clearFilters"
          class="text-xs text-base-content/60 hover:text-base-content flex items-center gap-1 px-2 py-1 rounded hover:bg-base-200 transition-colors"
        >
          <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
          </svg>
          Clear filters
        </button>
      </div>
    </header>

    <!-- Board -->
    <div class="flex-1 overflow-x-auto p-6">
      <div v-if="taskStore.loading" class="flex justify-center items-center h-full">
        <div class="animate-spin rounded-full h-10 w-10 border-b-2 border-primary"></div>
      </div>

      <div v-else class="flex gap-5 h-full items-start">
        <KanbanColumn
          v-for="column in columns"
          :key="column.id"
          :title="column.title"
          :status="column.id"
          :tasks="filteredTasks.filter(t => t.status === column.id).sort((a, b) => a.orderIndex - b.orderIndex)"
          @task-click="handleTaskClick"
          @add-task="openNewTaskForm"
          @drop-task="handleDropTask"
        />
      </div>
    </div>

    <!-- Modal -->
    <div v-if="isFormOpen" class="fixed inset-0 bg-black/40 backdrop-blur-sm z-50 flex items-center justify-center p-4" @click.self="closeForm">
      <div class="w-full max-w-lg">
        <TaskForm
          :initial-data="editingTask ? { ...editingTask } : { status: defaultStatusForNew }"
          :is-editing="!!editingTask"
          @save="handleSaveTask"
          @delete="handleDeleteTask"
          @cancel="closeForm"
        />
      </div>
    </div>
  </div>
</template>
