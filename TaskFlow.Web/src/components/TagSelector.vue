<script setup lang="ts">
import { ref } from 'vue';

const props = defineProps<{
  selectedTags: any[];
  availableTags: any[];
}>();

const emit = defineEmits<{
  (e: 'update:selectedTags', tags: any[]): void;
  (e: 'create-tag', name: string): void;
}>();

const newTagName = ref('');
const creating = ref(false);

const toggleTag = (tag: any) => {
  const index = props.selectedTags.findIndex(t => t.id === tag.id);
  const newTags = [...props.selectedTags];
  if (index === -1) {
    newTags.push(tag);
  } else {
    newTags.splice(index, 1);
  }
  emit('update:selectedTags', newTags);
};

const isSelected = (tagId: string) => props.selectedTags.some(t => t.id === tagId);

const submitNewTag = () => {
  const name = newTagName.value.trim();
  if (!name) return;
  emit('create-tag', name);
  newTagName.value = '';
  creating.value = false;
};
</script>

<template>
  <div class="space-y-2">
    <div class="flex flex-wrap gap-2">
      <button
        v-for="tag in availableTags"
        :key="tag.id"
        type="button"
        @click="toggleTag(tag)"
        :class="[
          'px-3 py-1 text-sm rounded-full border transition-colors focus:outline-none focus:ring-2 focus:ring-offset-1',
          isSelected(tag.id)
            ? 'bg-blue-100 text-blue-800 border-blue-200 focus:ring-blue-500'
            : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50 focus:ring-gray-300'
        ]"
      >
        {{ tag.name }}
      </button>

      <button
        v-if="!creating"
        type="button"
        @click="creating = true"
        class="px-3 py-1 text-sm rounded-full border border-dashed border-gray-300 text-gray-400 hover:border-blue-400 hover:text-blue-500 transition-colors"
      >
        + New tag
      </button>
    </div>

    <div v-if="creating" class="flex items-center gap-2">
      <input
        v-model="newTagName"
        type="text"
        placeholder="Tag name"
        autofocus
        maxlength="30"
        @keydown.enter.prevent="submitNewTag"
        @keydown.esc="creating = false; newTagName = ''"
        class="px-2.5 py-1 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 w-36"
      />
      <button
        type="button"
        @click="submitNewTag"
        class="px-2.5 py-1 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
      >
        Add
      </button>
      <button
        type="button"
        @click="creating = false; newTagName = ''"
        class="px-2.5 py-1 text-sm text-gray-500 hover:text-gray-700 rounded-lg hover:bg-gray-100 transition-colors"
      >
        Cancel
      </button>
    </div>
  </div>
</template>
