<script setup lang="ts">
    const username = defineModel<string>("username", { required: true });
    const password = defineModel<string>("password", { required: true });
    
</script>

<template>
    <div class="login_form">
        <p>Username</p>
        <input class="input" placeholder="Enter username" type="text" v-model="username">
        <p>Password</p>
        <input class="input" placeholder="Enter password" type="password" v-model="password">
    </div>
</template>

<style scoped>

</style>