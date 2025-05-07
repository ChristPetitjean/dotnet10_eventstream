<template>
  <div class="p-4 bg-white rounded shadow">
    <p class="text-lg font-bold text-gray-800">Résumé : {{ message.summary }}</p>
    <p class="text-gray-600">Date : {{ new Date(message.date).toLocaleString() }}</p>
    <p class="text-gray-600">Température : {{ message.temperatureC }}°C ({{ message.temperatureF }}°F)</p>
  </div>
</template>

<script setup lang="ts">
defineProps({
  message: {
    type: Object as () => { summary: string; date: string; temperatureC: number; temperatureF: number },
    required: true,
  },
});
</script>

<style scoped>
/* Ajoutez ici des styles spécifiques si nécessaire */
</style>
