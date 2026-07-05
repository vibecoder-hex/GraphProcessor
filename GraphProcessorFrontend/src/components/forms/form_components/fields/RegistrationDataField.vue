<script setup lang="ts">
    import type { IRegisterObject } from "@/models/interfacesAndTypes.ts";
    
    const registerObject = defineModel<IRegisterObject>("registerObject", {required: true});
</script>

<template>
    <div class="registration_field">
        <p>Username</p>
        <input class="input" placeholder="Enter username" v-model="registerObject.username" type="text">
        <p>Password</p>
        <input class="input" placeholder="Enter password" v-model="registerObject.password" type="password">
        <p>Repeat password</p>
        <input class="input" placeholder="Repeat password" v-model="registerObject.repeatPassword" type="password">
        <p>FirstName</p>
        <input class="input" placeholder="Enter firstname" v-model="registerObject.firstname" type="text">
        <p>LastName</p>
        <input class="input" placeholder="Enter lastname" v-model="registerObject.lastname" type="text">
        <p>Email</p>
        <input class="input" placeholder="Enter email" v-model="registerObject.email" type="text">
        <p>Phone</p>
        <input class="input" placeholder="Enter lastname" v-model="registerObject.phone" type="text">
    </div>
</template>

<style scoped>
    
</style>