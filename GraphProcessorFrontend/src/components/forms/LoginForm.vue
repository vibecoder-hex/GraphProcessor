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
    <div class="login-container">
        <article class="message is-primary">
            <div class="message-header">
                <p>Sign In</p>
            </div>
            <div class="message-body">
                <div class="card">
                    <div class="card-content">
                        <div class="form-wrapper">
                            <LoginDataField v-model:username="username" v-model:password="password"/>
                        </div>

                        <!-- Status Messages -->
                        <div v-if="errorMessage" class="notification is-danger is-light">
                            <button class="delete"></button>
                            {{ errorMessage }}
                        </div>

                        <div v-if="authResultMessage" class="notification is-success is-light">
                            <button class="delete"></button>
                            {{ authResultMessage }}
                        </div>

                        <!-- Authentication Status -->
                        <div v-if="authStore.isAuthenticated" class="notification is-success is-light">
                            <p class="has-text-success">✓ User authenticated successfully</p>
                        </div>
                    </div>

                    <!-- Card Footer with Button -->
                    <div class="card-footer">
                        <a href="#" class="card-footer-item">
                            <button 
                                class="button is-primary is-fullwidth" 
                                @click="handleLogin()"
                            >
                                Sign In
                            </button>
                        </a>
                    </div>
                </div>
            </div>
        </article>
    </div>
  

</template>

<style scoped>
    /* Container - centered with 50% width */
    .login-container {
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 100%;
        padding: 2rem 1rem;
    }

    /* Message container */
    .message {
        width: 50%;
        min-width: 300px;
        max-width: 600px;
    }

    /* Card styling */
    .card {
        box-shadow: none;
        border: none;
    }

    .card-content {
        padding: 2rem;
        display: flex;
        flex-direction: column;
        gap: 1.5rem;
    }

    /* Form wrapper */
    .form-wrapper {
        display: flex;
        flex-direction: column;
        gap: 1rem;
    }

    /* Notification styling */
    .notification {
        margin-bottom: 0;
        border-radius: 4px;
    }

    /* Card Footer */
    .card-footer {
        border-top: 1px solid #dbdbdb;
        padding: 0;
    }

    .card-footer-item {
        padding: 1rem;
        flex-grow: 1;
    }

    .card-footer-item .button {
        margin: 0;
    }

    /* Responsive Design */
    @media (max-width: 768px) {
        .message {
            width: 100%;
        }

        .card-content {
            padding: 1.5rem;
        }
    }

</style>