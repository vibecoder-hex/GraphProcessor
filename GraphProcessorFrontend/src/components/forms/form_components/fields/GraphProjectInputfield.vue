<template>
    <input class="input" type="text" v-model="graphName" placeholder="Graph name">
    <textarea class="textarea is-info" type="text" v-model="graphDescription" placeholder="Graph description"></textarea>
</template>

<script setup lang="ts">
    const graphName = defineModel<string>("graphName", {required: true});
    const graphDescription = defineModel<string>("graphDescription");
</script>

<style scoped>

</style>