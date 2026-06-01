import { defineStore } from "pinia";
import { ref, computed } from "vue"

export const useAuthenticationStore = defineStore('authStore', () => {
    const token = ref<string | null>(localStorage.getItem("token"));
    const isAuthenticated = computed(() => token.value !== null)

    function setToken(newToken: string) {
        token.value = newToken
        localStorage.setItem("token", newToken)
    }

    function deleteToken() {
        localStorage.removeItem("token")
        token.value = null
    }

    return {
        isAuthenticated,
        setToken,
        deleteToken,
        token
    }
})