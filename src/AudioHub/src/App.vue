<script setup lang="ts">
import DefaultLayout from './layouts/DefaultLayout.vue';
import { RouterView } from 'vue-router';
</script>

<template>
  <DefaultLayout>
    <template #default="slotProps">
      <RouterView v-bind="slotProps" />
    </template>
  </DefaultLayout>
</template>
