<template>
  <div class="app-container">
    <ConverterZone />
  </div>
</template>

<script setup>
import ConverterZone from "./components/ConverterZone.vue";
</script>

<style>
/* Global Reset */
body {
  margin: 0;
  padding: 0;
  font-family: "Inter", sans-serif;
  -webkit-font-smoothing: antialiased;
}

.app-container {
  min-height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  /* Modern Gradyan Arka Plan */
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  background-size: 400% 400%;
  animation: gradientBG 15s ease infinite;
}

@keyframes gradientBG {
  0% {
    background-position: 0% 50%;
  }
  50% {
    background-position: 100% 50%;
  }
  100% {
    background-position: 0% 50%;
  }
}
</style>
