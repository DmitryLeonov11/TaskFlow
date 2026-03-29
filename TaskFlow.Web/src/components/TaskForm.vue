<script setup lang="ts">
import { ref, onMounted } from 'vue';
import type { Task } from '../stores/taskStore';
import { useTagStore } from '../stores/tagStore';
import TagSelector from './TagSelector.vue';

const props = defineProps<{
  initialData?: Partial<Task>;
  isEditing?: boolean;
}>();

const emit = defineEmits<{
  (e: 'save', data: Partial<Task> & { tagIds: string[] }): void;
  (e: 'delete', id: string): void;
  (e: 'cancel'): void;
}>();

const tagStore = useTagStore();

// Convert ISO deadline to YYYY-MM-DD for <input type="date">
const toDateInput = (iso?: string | null) => {
  if (!iso) return '';
  return iso.substring(0, 10);
};

const form = ref({
  title: props.initialData?.title ?? '',
  description: props.initialData?.description ?? '',
  priority: props.initialData?.priority ?? 1,
  status: props.initialData?.status ?? 0,
  deadline: toDateInput(props.initialData?.deadline),
});

const selectedTags = ref<{ id: string; name: string; color: string | null }[]>(
  props.initialData?.tags ? [...props.initialData.tags] : []
);

const confirmDelete = ref(false);

const handleCreateTag = async (name: string) => {
  const tag = await tagStore.createTag(name);
  selectedTags.value = [...selectedTags.value, tag];
};

onMounted(async () => {
  if (!tagStore.tags.length) {
    await tagStore.fetchTags();
  }
});

const submit = () => {
  emit('save', {
    ...form.value,
    deadline: form.value.deadline || undefined,
    tagIds: selectedTags.value.map(t => t.id),
  } as any);
};

const handleDelete = () => {
  if (!confirmDelete.value) {
    confirmDelete.value = true;
    return;
  }
  emit('delete', props.initialData?.id as string);
};
</script>

<template>
  <div class="bg-white p-6 rounded-xl shadow-lg w-full max-w-lg mx-auto border border-gray-100">
    <div class="flex items-center justify-between mb-5">
      <h2 class="text-xl font-bold text-gray-800">{{ isEditing ? 'Edit Task' : 'New Task' }}</h2>
      <button type="button" @click="emit('cancel')" class="text-gray-400 hover:text-gray-600 p-1 rounded-lg hover:bg-gray-100">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
        </svg>
      </button>
    </div>

    <form @submit.prevent="submit" class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-gray-700 mb-1">Title <span class="text-red-500">*</span></label>
        <input
          v-model="form.title"
          type="text"
          required
          autofocus
          class="w-full border border-gray-300 rounded-lg px-4 py-2 bg-gray-50 text-gray-900 placeholder-gray-400 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors text-sm"
          placeholder="Task title"
        />
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
        <textarea
          v-model="form.description"
          rows="3"
          class="w-full border border-gray-300 rounded-lg px-4 py-2 bg-gray-50 text-gray-900 placeholder-gray-400 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors text-sm resize-none"
          placeholder="Add a description..."
        ></textarea>
      </div>

      <div class="grid grid-cols-2 gap-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Priority</label>
          <select
            v-model="form.priority"
            class="w-full border border-gray-300 rounded-lg px-3 py-2 bg-gray-50 text-gray-900 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm"
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
            class="w-full border border-gray-300 rounded-lg px-3 py-2 bg-gray-50 text-gray-900 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm"
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
          class="w-full border border-gray-300 rounded-lg px-4 py-2 bg-gray-50 text-gray-900 focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm"
        />
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700 mb-2">Tags</label>
        <TagSelector
          :available-tags="tagStore.tags"
          :selected-tags="selectedTags"
          @update:selected-tags="selectedTags = $event"
          @create-tag="handleCreateTag"
        />
      </div>

      <div class="pt-4 border-t flex items-center justify-between gap-3">
        <!-- Delete (only when editing) -->
        <div v-if="isEditing">
          <button
            v-if="!confirmDelete"
            type="button"
            @click="handleDelete"
            class="px-3 py-2 text-red-600 hover:bg-red-50 rounded-lg text-sm font-medium transition-colors"
          >
            Delete
          </button>
          <div v-else class="flex items-center gap-2">
            <span class="text-sm text-red-600">Are you sure?</span>
            <button
              type="button"
              @click="handleDelete"
              class="px-3 py-1.5 bg-red-600 text-white rounded-lg text-sm font-medium hover:bg-red-700 transition-colors"
            >
              Yes, delete
            </button>
            <button
              type="button"
              @click="confirmDelete = false"
              class="px-3 py-1.5 border border-gray-300 rounded-lg text-sm text-gray-600 hover:bg-gray-50 transition-colors"
            >
              Cancel
            </button>
          </div>
        </div>
        <span v-else></span>

        <div class="flex gap-2">
          <button
            type="button"
            @click="emit('cancel')"
            class="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors text-sm font-medium"
          >
            Cancel
          </button>
          <button
            type="submit"
            class="px-4 py-2 text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors text-sm font-medium shadow-sm"
          >
            {{ isEditing ? 'Save Changes' : 'Create Task' }}
          </button>
        </div>
      </div>
    </form>
  </div>
</template>
