<template>
    <nav class="navbar" role="navigation">
        <RouterLink class="navbar-item is-size-3" to="/">GraphProcessor</RouterLink>
        <RouterLink v-if="!authStore.isAuthenticated" class="navbar-item" to="/login">Sign in</RouterLink>
        <RouterLink v-if="!authStore.isAuthenticated" class="navbar-item" to="#">Sign up</RouterLink>
        <RouterLink v-if="authStore.isAuthenticated" class="navbar-item" to="#">Sign out</RouterLink>
        <RouterLink v-if="authStore.isAuthenticated" class="navbar-item" to="#">Account</RouterLink>
        <RouterLink class="navbar-item" to="/about">About</RouterLink>
    </nav>
    <main>
        <RouterView/>
    </main>
</template>

<script setup lang="ts">
    import { useAuthenticationStore } from './stores';
    import { TokenProvider } from './services/httpServices/AuthenticationRequests';
    import { onMounted } from 'vue';

    const authStore = useAuthenticationStore()
    
    onMounted(() => {
        const jwtToken = TokenProvider.getToken()
        if (jwtToken !== null) {
            if (!TokenProvider.isTokenValid(jwtToken)) {
                authStore.deleteTokenFromState()
                TokenProvider.removeToken()
            }
        }
    })

</script>


<style scoped>
  nav a {
      margin: 10px;
  }

</style>
