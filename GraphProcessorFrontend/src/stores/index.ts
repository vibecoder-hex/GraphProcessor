import { defineStore } from "pinia";
import { ref, computed } from "vue"
import { TokenProvider } from "@/services/httpServices/AuthenticationRequests";

export const useAuthenticationStore = defineStore('authStore', () => {
    const token = ref<string | null>(TokenProvider.getToken());
    const isAuthenticated = computed(() => token.value !== null)

    function setTokenInState(newToken: string) {
        token.value = newToken
    }

    function deleteTokenFromState() {
        token.value = null
    }

    return {
        isAuthenticated,
        setTokenInState,
        deleteTokenFromState
    }
})