<script setup lang="ts">
import { computed } from 'vue';
import type { Task } from '../stores/taskStore';

const props = defineProps<{
  task: Task;
}>();

const emit = defineEmits<{
  (e: 'click'): void;
}>();

const priorityConfig = computed(() => {
  const configs = [
    { label: 'Low',    classes: 'bg-base-300 text-base-content/70' },
    { label: 'Medium', classes: 'bg-primary/20 text-primary' },
    { label: 'High',   classes: 'bg-orange-100 text-orange-700' },
    { label: 'Urgent', classes: 'bg-red-100 text-red-700' },
  ];
  return configs[props.task.priority] ?? configs[0];
});

const isOverdue = computed(() => {
  if (!props.task.deadline) return false;
  return new Date(props.task.deadline) < new Date();
});

const formattedDeadline = computed(() => {
  if (!props.task.deadline) return null;
  return new Date(props.task.deadline).toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
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
    class="bg-base-100 p-3.5 rounded-lg shadow-sm border border-base-300 hover:shadow-md hover:border-base-content/20 transition-all select-none cursor-pointer active:cursor-grabbing group"
    @click="emit('click')"
  >
    <!-- Header: title + priority -->
    <div class="flex items-start justify-between gap-2 mb-2">
      <h4 class="font-medium text-base-content text-sm leading-snug line-clamp-2 flex-1">{{ task.title }}</h4>
      <span :class="['text-xs px-2 py-0.5 rounded-full whitespace-nowrap font-medium flex-shrink-0', priorityConfig.classes]">
        {{ priorityConfig.label }}
      </span>
    </div>

    <!-- Description -->
    <p v-if="task.description" class="text-xs text-base-content/60 line-clamp-2 mb-2.5">
      {{ task.description }}
    </p>

    <!-- Tags -->
    <div v-if="task.tags?.length" class="flex flex-wrap gap-1 mb-2.5">
      <span
        v-for="tag in task.tags"
        :key="tag.id"
        class="inline-flex items-center px-1.5 py-0.5 rounded text-[11px] font-medium"
        :style="tag.color ? `background-color: ${tag.color}22; color: ${tag.color}` : ''"
        :class="!tag.color ? 'bg-secondary/10 text-secondary' : ''"
      >
        {{ tag.name }}
      </span>
    </div>

    <!-- Footer: deadline + counts -->
    <div class="flex items-center justify-between text-xs text-base-content/50">
      <span
        v-if="task.deadline"
        :class="['flex items-center gap-1', isOverdue ? 'text-error font-medium' : 'text-base-content/60']"
      >
        <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
            d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
        </svg>
        {{ formattedDeadline }}
      </span>
      <span v-else></span>

      <div class="flex items-center gap-2.5">
        <span v-if="task.comments?.length" class="flex items-center gap-0.5">
          <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"/>
          </svg>
          {{ task.comments.length }}
        </span>
        <span v-if="task.attachments?.length" class="flex items-center gap-0.5">
          <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13"/>
          </svg>
          {{ task.attachments.length }}
        </span>
      </div>
    </div>
  </div>
</template>
