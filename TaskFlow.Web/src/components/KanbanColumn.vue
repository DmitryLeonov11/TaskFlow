<script setup lang="ts">
import { ref } from 'vue';
import type { Task } from '../stores/taskStore';
import TaskCard from './TaskCard.vue';

const props = defineProps<{
  title: string;
  status: number;
  tasks: Task[];
}>();

const emit = defineEmits<{
  (e: 'task-click', task: Task): void;
  (e: 'add-task', status: number): void;
}>();

// Using dnd-kit or vuedraggable would be implemented here in a real project
// For now, we stub the drop zone support. 
</script>

<template>
  <div class="flex flex-col flex-shrink-0 w-80 bg-gray-50 rounded-xl p-3 border border-gray-200 shadow-sm h-full">
    <div class="flex items-center justify-between mb-3 px-1">
      <h3 class="font-semibold text-gray-700 flex items-center gap-2">
        {{ title }}
        <span class="bg-gray-200 text-gray-600 text-xs px-2 py-0.5 rounded-full font-medium">
          {{ tasks.length }}
        </span>
      </h3>
      <button 
        @click="emit('add-task', status)"
        class="text-gray-400 hover:text-gray-600 p-1 rounded hover:bg-gray-200 transition-colors"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
          <path fill-rule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clip-rule="evenodd" />
        </svg>
      </button>
    </div>

    <div class="flex-1 overflow-y-auto min-h-0 custom-scrollbar space-y-3 p-1">
      <!-- Draggable tasks area -->
      <TaskCard 
        v-for="task in tasks" 
        :key="task.id" 
        :task="task" 
        @click="emit('task-click', task)"
      />
      
      <div v-if="tasks.length === 0" class="h-24 border-2 border-dashed border-gray-300 rounded-lg flex items-center justify-center text-gray-400 text-sm">
        Drag tasks here
      </div>
    </div>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: #cbd5e1;
  border-radius: 20px;
}
</style>
