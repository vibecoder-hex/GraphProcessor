<script setup lang="ts">
    import LoginDataField from './form_components/fields/LoginDataField.vue';
    import { LoginRequests } from '@/services/httpServices/AuthenticationRequests.ts';
    import type { IAuthenticationResultObject, IResponseOperationResult } from '@/models/interfacesAndTypes.ts';
    import { useAuthenticationStore } from '@/stores/index.ts';
    import { ref } from "vue";

    const API_URL = "api/User"

    const authStore = useAuthenticationStore();

    const authResultMessage = ref<string>("")
    const errorMessage = ref<string>("")
    const username = ref<string>("")
    const password = ref<string>("")


    async function handleLogin(): Promise<void> {
        const loginRequests = new LoginRequests(API_URL, username.value, password.value)
        const loginResponse: IResponseOperationResult<IAuthenticationResultObject> = await loginRequests.login();
        if (loginResponse.operation.isValid) {
            const accessToken: IAuthenticationResultObject | null = loginResponse.responseData;
            if (accessToken != null) {
                authStore.setToken(accessToken.tokenString)
                authResultMessage.value = "Authentication successfull";
                errorMessage.value = "";
                

            } else {
                authResultMessage.value = '';
                errorMessage.value = 'Authentication data is empty'
            }
        } else {
            authResultMessage.value = "";
            errorMessage.value = loginResponse.operation.errorMessage;
        }
    }

</script>

<template>
    <div class="login-form">
        <h1 class="is-size-3">Login</h1>
        <LoginDataField v-model:username="username" v-model:password="password"/>
        <p class="has-text-danger">{{ errorMessage }}</p>
        <p class="has-text-success">{{ authResultMessage }}</p>
        <button class="button is-success" @click="handleLogin()">Sign in</button>
        <div v-if="authStore.isAuthenticated">
            <p class="has-text-success">User has been authenticated</p>
        </div>
    </div>
  

</template>

<style scoped>
    .login-form {
        
        display: flex;
        flex-direction: column;
        gap: 15px;
        margin: 0 auto;
    }
    @media (min-width: 1000px) {
        .login-form {
            width: 40%;
        }
    }
    @media (max-width: 720px) {
        .login-form {
            width: 90%;
        }
    }

</style>