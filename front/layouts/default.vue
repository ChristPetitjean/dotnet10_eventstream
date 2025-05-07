<template>
  <div class="flex flex-col min-h-screen">
    <!-- Header -->
    <header :class="['bg-blue-600 text-white py-4 shadow-md fixed top-0 left-0 w-full transition-transform duration-300', { '-translate-y-full': isScrollingDown }]">
      <div class="container mx-auto flex justify-between items-center">
        <h1 class="text-xl font-bold">Mon Application</h1>
        <nav>
          <ul class="flex space-x-4">
            <li><NuxtLink to="/" class="hover:underline">Accueil</NuxtLink></li>
            <li><NuxtLink to="/about" class="hover:underline">À propos</NuxtLink></li>
            <li><NuxtLink to="/contact" class="hover:underline">Contact</NuxtLink></li>
          </ul>
        </nav>
      </div>
    </header>

    <!-- Main Content -->
    <main class="flex-grow container mx-auto py-8 mt-16">
      <slot />
    </main>

    <!-- Footer -->
    <footer class="bg-gray-800 text-white py-4 mt-auto">
      <div class="container mx-auto text-center">
        <p>&copy; 2025 Mon Application. Tous droits réservés.</p>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';

const isScrollingDown = ref(false);
let lastScrollPosition = 0;

const handleScroll = () => {
  const currentScrollPosition = window.scrollY;
  isScrollingDown.value = currentScrollPosition > lastScrollPosition;
  lastScrollPosition = currentScrollPosition;
};

onMounted(() => {
  window.addEventListener('scroll', handleScroll);
});

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll);
});
</script>

<style>
/* Ajoutez ici des styles personnalisés si nécessaire */
</style>
