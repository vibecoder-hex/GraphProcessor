<script setup lang="ts">
    import UserAccountCard from '@/components/forms/form_components/submit_results/UserAccountCard.vue';
    import type { IResponseOperationResult, IUserProfileData } from '@/models/interfacesAndTypes';

    import { ProfileRequests } from '@/services/httpServices/AccountRequests';
    import router  from '@/router';
    import { ref } from 'vue'
    import { useAuthenticationStore } from '@/stores';

    const apiUrl = '/api/User'

    const errorMessage = ref<string>("")
    const accountDataObject = ref<IUserProfileData | null>(null)
    
    const authStore = useAuthenticationStore()

    async function loadProfile() {
        if (!authStore.token) {
            accountDataObject.value = null;
            errorMessage.value = "Access token not found";
            return;
        }
        
        const profileRequest = new ProfileRequests(apiUrl, authStore.token);
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
        authStore.deleteToken()
        await router.push('/')
    }

</script>

<template>
    <UserAccountCard :handleLogout="handleLogout" :accountDataObject="accountDataObject"/>
    <div>{{ errorMessage }}</div>
</template>

<style scoped>

</style>