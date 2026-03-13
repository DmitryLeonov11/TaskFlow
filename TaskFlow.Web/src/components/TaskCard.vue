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
    case 0: return 'bg-gray-100 text-gray-800'; // Low
    case 1: return 'bg-blue-100 text-blue-800'; // Medium
    case 2: return 'bg-orange-100 text-orange-800'; // High
    case 3: return 'bg-red-100 text-red-800'; // Urgent
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
</script>

<template>
  <div 
    class="bg-white p-4 rounded-lg shadow-sm border border-gray-200 cursor-grab hover:shadow-md transition-shadow"
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
          <i class="fas fa-clock mr-1"></i>
          {{ new Date(task.deadline).toLocaleDateString() }}
        </span>
      </div>
      <div>
        <!-- Any extra info, like comment count or attachment count -->
      </div>
    </div>
  </div>
</template>
