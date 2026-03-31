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
            ? 'bg-primary/20 text-primary border-primary/30 focus:ring-primary'
            : 'bg-base-100 text-base-content/80 border-base-300 hover:bg-base-200 focus:ring-base-300'
        ]"
      >
        {{ tag.name }}
      </button>

      <button
        v-if="!creating"
        type="button"
        @click="creating = true"
        class="px-3 py-1 text-sm rounded-full border border-dashed border-base-300 text-base-content/50 hover:border-primary/70 hover:text-primary transition-colors"
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
        class="px-2.5 py-1 text-sm border border-base-300 rounded-lg bg-base-200 text-base-content focus:ring-2 focus:ring-primary focus:border-primary w-36"
      />
      <button
        type="button"
        @click="submitNewTag"
        class="px-2.5 py-1 text-sm bg-primary text-primary-content rounded-lg hover:bg-primary/90 transition-colors"
      >
        Add
      </button>
      <button
        type="button"
        @click="creating = false; newTagName = ''"
        class="px-2.5 py-1 text-sm text-base-content/60 hover:text-base-content hover:bg-base-200 rounded-lg transition-colors"
      >
        Cancel
      </button>
    </div>
  </div>
</template>
