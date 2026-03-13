<script setup lang="ts">
import { ref, onMounted } from 'vue';
import type { Task } from '../stores/taskStore';

const props = defineProps<{
  initialData?: Partial<Task>;
  isEditing?: boolean;
}>();

const emit = defineEmits<{
  (e: 'save', data: Partial<Task>): void;
  (e: 'cancel'): void;
}>();

const form = ref<Partial<Task>>({
  title: '',
  description: '',
  priority: 1, // Medium
  status: 0, // ToDo
  ...props.initialData
});

const submit = () => {
  emit('save', form.value);
};
</script>

<template>
  <div class="bg-white p-6 rounded-xl shadow-lg w-full max-w-lg mx-auto border border-gray-100">
    <h2 class="text-xl font-bold mb-5 text-gray-800">{{ isEditing ? 'Edit Task' : 'Create New Task' }}</h2>
    
    <form @submit.prevent="submit" class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-gray-700 mb-1">Title</label>
        <input 
          v-model="form.title" 
          type="text" 
          required
          class="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors"
          placeholder="Enter task title"
        />
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
        <textarea 
          v-model="form.description" 
          rows="3"
          class="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors"
          placeholder="Task description..."
        ></textarea>
      </div>

      <div class="grid grid-cols-2 gap-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Priority</label>
          <select 
            v-model="form.priority" 
            class="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 appearance-none bg-white"
          >
            <option :value="0">Low</option>
            <option :value="1">Medium</option>
            <option :value="2">High</option>
            <option :value="3">Urgent</option>
          </select>
        </div>
        
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
          <select 
            v-model="form.status" 
            class="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 appearance-none bg-white"
          >
            <option :value="0">To Do</option>
            <option :value="1">In Progress</option>
            <option :value="2">Review</option>
            <option :value="3">Done</option>
          </select>
        </div>
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700 mb-1">Deadline</label>
        <input 
          v-model="form.deadline" 
          type="date"
          class="w-full border border-gray-300 rounded-lg px-4 py-2 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
        />
      </div>

      <div class="pt-4 flex justify-end gap-3 border-t mt-6">
        <button 
          type="button" 
          @click="emit('cancel')"
          class="px-5 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors font-medium"
        >
          Cancel
        </button>
        <button 
          type="submit"
          class="px-5 py-2 text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors font-medium shadow-sm hover:shadow"
        >
          {{ isEditing ? 'Save Changes' : 'Create Task' }}
        </button>
      </div>
    </form>
  </div>
</template>
