<script setup lang="ts">
    import UserAccountCard from '@/components/forms/form_components/submit_results/UserAccountCard.vue';
    import type { IResponseOperationResult, IUserProfileData } from '@/models/interfacesAndTypes';

    import { ProfileRequests } from '@/services/httpServices/AccountRequests';
    import { LoginRequests } from "@/services/httpServices/AuthenticationRequests.ts";
    import { ref } from 'vue'
    import { useAuthenticationStore } from '@/stores';
    import {ApiClientConfigurator} from "@/services/httpServices/ApiClientConfigurator.ts";
    import type {AxiosInstance} from "axios";
    

    const errorMessage = ref<string>("")
    const accountDataObject = ref<IUserProfileData | null>(null)
    
    const authStore = useAuthenticationStore()

    const apiInstance: ApiClientConfigurator = ApiClientConfigurator.getInstance();
    const apiClient: AxiosInstance = apiInstance.getClient()

    async function loadProfile() {
        const response: IResponseOperationResult<IUserProfileData> = await ProfileRequests.getAccountData(apiClient);
        
        if (response.operation.isValid) {
            const profileData: IUserProfileData | null = response.responseData;
            if (profileData != null) {
                accountDataObject.value = profileData;
                errorMessage.value = "";
            } else {
                accountDataObject.value = null;
                errorMessage.value = "Error: Profile data object is empty";
            }
        } else {
            accountDataObject.value = null;
            errorMessage.value = response.operation.errorMessage;
        }
    }

    loadProfile();

    async function handleLogout() {
        const token: string | null = authStore.token;
        if (token !== null) {
            authStore.deleteToken()
            window.location.href = "/"
            await LoginRequests.logout(apiClient)
        }
    }

</script>

<template>
    <UserAccountCard :handleLogout="handleLogout" :accountDataObject="accountDataObject"/>
    <div>{{ errorMessage }}</div>
</template>

<style scoped>

</style>