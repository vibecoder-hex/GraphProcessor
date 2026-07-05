<script setup lang="ts">
    import type { ILoginObject } from "@/models/interfacesAndTypes.ts";
  
    const loginObject = defineModel<ILoginObject>("loginObject", {required: true});
    
    
</script>

<template>
    <div class="login_form">
        <p>Username</p>
        <input class="input" placeholder="Enter username" type="text" v-model="loginObject.username">
        <p>Password</p>
        <input class="input" placeholder="Enter password" type="password" v-model="loginObject.password">
    </div>
</template>

<style scoped>

</style>