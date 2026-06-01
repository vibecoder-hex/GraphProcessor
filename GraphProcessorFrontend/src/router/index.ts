import { createRouter, createWebHistory } from 'vue-router'
import GraphDataInputView from "../views/GraphDataInputView.vue"
import AboutPageView from "../views/AboutPageView.vue"
import LoginView from '@/views/LoginView.vue'
import AccountPageView from '@/views/AccountPageView.vue'
import { useAuthenticationStore } from '@/stores/index.ts'

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        { path: '/', component: GraphDataInputView },
        { path: '/about', component: AboutPageView },
        { path: '/login', component: LoginView },
        { path: '/account', component: AccountPageView, meta: { requiresAuth: true } }
    ],
})

router.beforeEach((to) => {
    const authStore = useAuthenticationStore()
    if (!authStore.isAuthenticated && to.meta.requiresAuth) {
        return {
            path: '/'
        }
    }
})

export default router
