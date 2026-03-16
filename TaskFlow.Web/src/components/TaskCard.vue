<script setup lang="ts">
import { computed } from 'vue';
import type { Task } from '../stores/taskStore';

const props = defineProps<{
  task: Task;
}>();

const emit = defineEmits<{
  (e: 'click'): void;
}>();

const priorityColor = computed(() => {
  switch (props.task.priority) {
    case 0: return 'bg-gray-100 text-gray-800';
    case 1: return 'bg-blue-100 text-blue-800';
    case 2: return 'bg-orange-100 text-orange-800';
    case 3: return 'bg-red-100 text-red-800';
    default: return 'bg-gray-100 text-gray-800';
  }
});

const priorityLabel = computed(() => {
  switch (props.task.priority) {
    case 0: return 'Low';
    case 1: return 'Medium';
    case 2: return 'High';
    case 3: return 'Urgent';
    default: return 'Unknown';
  }
});

const isOverdue = computed(() => {
  if (!props.task.deadline) return false;
  return new Date(props.task.deadline) < new Date();
});

const onDragStart = (e: DragEvent) => {
  if (e.dataTransfer) {
    e.dataTransfer.effectAllowed = 'move';
    e.dataTransfer.setData('taskId', props.task.id);
  }
};
</script>

<template>
  <div
    draggable="true"
    @dragstart="onDragStart"
    class="bg-white p-4 rounded-lg shadow-sm border border-gray-200 hover:shadow-md transition-shadow select-none cursor-grab active:cursor-grabbing"
    @click="emit('click')"
  >
    <div class="flex justify-between items-start mb-2">
      <h4 class="font-medium text-gray-900 truncate pr-2">{{ task.title }}</h4>
      <span :class="['text-xs px-2 py-1 rounded-full whitespace-nowrap', priorityColor]">
        {{ priorityLabel }}
      </span>
    </div>

    <p class="text-sm text-gray-600 line-clamp-2 mb-3">
      {{ task.description || 'No description provided.' }}
    </p>

    <div class="flex items-center justify-between text-xs text-gray-500">
      <div class="flex items-center gap-2">
        <span v-if="task.deadline" :class="{'text-red-500 font-medium': isOverdue}">
          <svg class="inline-block w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
          </svg>
          {{ new Date(task.deadline).toLocaleDateString() }}
        </span>
      </div>
      <div>
        <!-- Any extra info, like comment count or attachment count -->
      </div>
    </div>
  </div>
</template>
