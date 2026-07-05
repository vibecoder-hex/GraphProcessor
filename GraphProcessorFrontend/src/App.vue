<template>
    <nav class="navbar" role="navigation">
        <RouterLink class="navbar-item is-size-3" to="/">GraphProcessor</RouterLink>
        <RouterLink v-if="!authStore.isAuthenticated" class="navbar-item" to="/login">Sign in</RouterLink>
        <RouterLink v-if="!authStore.isAuthenticated" class="navbar-item" to="/register">Sign up</RouterLink>
        <RouterLink v-if="authStore.isAuthenticated" class="navbar-item" to="/account">Account</RouterLink>
        <RouterLink class="navbar-item" to="/about">About</RouterLink>
    </nav>
    <main>
        <RouterView/>
    </main>
</template>

<script setup lang="ts">
    import { useAuthenticationStore } from './stores';
    import { onMounted } from 'vue';
    import { TokenProcessor } from './services/httpServices/AuthenticationRequests';

    const authStore = useAuthenticationStore()
    
    onMounted(() => {
        const jwtToken = authStore.token
        if (jwtToken !== null) {
            const tokenProcessor = new TokenProcessor(jwtToken)
            if (!tokenProcessor.isTokenValid()) {
                authStore.deleteToken()
            }
        }
    })


</script>


<style scoped>
  nav a {
      margin: 10px;
  }

</style>
