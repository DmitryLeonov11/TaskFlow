<script setup lang="ts">
import { computed, ref } from 'vue';
import type { Task } from '../stores/taskStore';
import { useDraggable, useDroppable } from '@dnd-kit/vue';

const props = defineProps<{
  task: Task;
}>();

const emit = defineEmits<{
  (e: 'click'): void;
}>();

const element = ref<HTMLElement | null>(null);

const draggable = useDraggable({
  id: props.task.id,
  element,
  data: {
    taskId: props.task.id,
    status: props.task.status,
    orderIndex: props.task.orderIndex
  }
});

const { isDragging } = draggable;

useDroppable({
  id: props.task.id,
  element
});

const style = computed(() => {
  const transform = (draggable as any).transform;
  if (!transform) return {};
  const { x, y, scaleX = 1, scaleY = 1 } = transform;
  return {
    transform: `translate3d(${x}px, ${y}px, 0) scale(${scaleX}, ${scaleY})`,
    zIndex: isDragging ? 50 : 1
  };
});

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
</script>

<template>
  <div
    ref="element"
    :style="style"
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
