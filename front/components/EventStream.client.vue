<script setup lang="ts">
import { ref } from 'vue';

let evtSource: EventSource;
const messages = ref<{ summary: string; date: string; temperatureC: number; temperatureF: number; id: string }[]>([]);

const closeConnection = () => {
  evtSource.close();
  console.log('EventSource connection closed');
};

const startListening = () => {
  evtSource = new EventSource('http://localhost:5149/stream');
  evtSource.addEventListener('weatherforecast', (event) => {
    try {
      const data = JSON.parse(event.data);
      data.id = crypto.randomUUID(); // Generate a unique ID for each message
      if (messages.value.length >= 6) {
        messages.value = []; // Remove the oldest message if the list exceeds 6 items
      }
      messages.value.push(data); // Insert the new message at the beginning of the list
    } catch (error) {
      console.error('Invalid JSON received:', event.data);
    }
  });
};

const clearMessages = () => {
  messages.value = [];
};

onBeforeUnmount(() => {
  closeConnection();
});
</script>

<template>
  <div class="p-4 space-y-4">
    <div class="flex space-x-4">
      <button @click="closeConnection" class="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600">Close Connection</button>
      <button @click="startListening" class="px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600">Start Listening</button>
      <button @click="clearMessages" class="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600">Clear Messages</button>
    </div>
    <h1 class="text-2xl font-bold text-gray-800">Server-Sent Events</h1>
    <transition-group name="fade" tag="div" class="grid grid-cols-4 gap-4">
      <div v-for="message in messages" :key="message.id" class="p-2 bg-gray-100 rounded shadow">
        <MessageDisplay class="animate-highlight" :message="message" />
      </div>
    </transition-group>
  </div>
</template>

<style>
/* Ajoutez une animation de halo lumineux */
@keyframes highlight {
  0% {
    box-shadow: 0 0 10px 4px rgba(59, 130, 246, 0.5); /* Blue glow */
  }
  100% {
    box-shadow: 0 0 0 0 rgba(59, 130, 246, 0); /* Fade out */
  }
}

.animate-highlight {
  animation: highlight 1s ease-out;
}

/* Fade transition for removing items */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
