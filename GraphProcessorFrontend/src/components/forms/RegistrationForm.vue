<script setup lang="ts">
    import RegistrationDataField from "@/components/forms/form_components/fields/RegistrationDataField.vue";
    import type {
      IAuthenticationResultObject,
      IRegisterObject,
      IResponseOperationResult
    } from "@/models/interfacesAndTypes.ts";
    import { RegistrationRequests } from "@/services/httpServices/AuthenticationRequests.ts";
    import { useAuthenticationStore } from "@/stores";
    import router from "@/router/index.ts"
    import { reactive, ref } from "vue";

    const API_URL = "api/User"
    
    const authStore = useAuthenticationStore()
    
    const registerObject = reactive<IRegisterObject>({
        username: "",
        password: "",
        repeatPassword: "",
        firstname: "",
        lastname: "",
        phone: "",
        email: "",
    })
    const errorMessage = ref<string>("")
    
    async function handleRegister() {
        const registrationRequests = new RegistrationRequests(API_URL, registerObject);
        const response: IResponseOperationResult<IAuthenticationResultObject> = await registrationRequests.register();
        if (response.operation.isValid) {
            const accessToken: IAuthenticationResultObject | null = response.responseData
            if (accessToken) {
                authStore.setToken(accessToken.tokenString)
                await router.push("/account")
                errorMessage.value = ""
            } else {
                errorMessage.value = "Access token is empty"
            }
        } else {
            errorMessage.value = response.operation.errorMessage
        }
    }
</script>

<template>
    <div class="registration-form">
        <h1 class="is-size-3">Registration</h1>
        <RegistrationDataField v-model:registerObject="registerObject" />
        <button class="button is-success" @click="handleRegister">Sign up</button>
        <p class="has-text-danger">{{ errorMessage }}</p>
    </div>
</template>

<style scoped>
    @media(max-width: 720px) {
        .registration-form {
            width: 80%;
        }
    }
    @media (min-width: 1000px) {
        .registration-form {
            width: 40%;
        }
    }
    .registration-form {
        display: flex;
        flex-direction: column;
        margin: 0 auto;
        gap: 25px;
    }
</style>