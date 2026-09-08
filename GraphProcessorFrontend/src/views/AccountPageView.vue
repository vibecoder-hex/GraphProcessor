<script setup lang="ts">
    import UserAccountCard from '@/components/forms/form_components/submit_results/UserAccountCard.vue';
    import type { IResponseOperationResult, IUserProfileData } from '@/models/interfacesAndTypes';

    import { ProfileRequests } from '@/services/httpServices/AccountRequests';
    import { LoginRequests } from "@/services/httpServices/AuthenticationRequests.ts";
    import router  from '@/router';
    import { ref } from 'vue'
    import { useAuthenticationStore } from '@/stores';
    

    const errorMessage = ref<string>("")
    const accountDataObject = ref<IUserProfileData | null>(null)
    
    const authStore = useAuthenticationStore()

    async function loadProfile() {
        const profileRequest = new ProfileRequests();
        const response: IResponseOperationResult<IUserProfileData> = await profileRequest.getAccountData();
        
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
        const loginRequests = new LoginRequests();
        const token: string | null = authStore.token;
        if (token !== null) {
            authStore.deleteToken()
            window.location.href = "/"
            await loginRequests.logout()
        }
    }

</script>

<template>
    <UserAccountCard :handleLogout="handleLogout" :accountDataObject="accountDataObject"/>
    <div>{{ errorMessage }}</div>
</template>

<style scoped>

</style>