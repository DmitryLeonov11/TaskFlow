<script setup lang="ts">
const props = defineProps<{
  selectedTags: any[];
  availableTags: any[];
}>();

const emit = defineEmits<{
  (e: 'update:selectedTags', tags: any[]): void;
}>();

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

const isSelected = (tagId: string) => {
  return props.selectedTags.some(t => t.id === tagId);
};
</script>

<template>
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
  </div>
</template>
