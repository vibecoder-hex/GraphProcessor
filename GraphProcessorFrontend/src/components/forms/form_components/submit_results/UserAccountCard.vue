<script setup lang="ts">
    import type { IUserProfileData } from '@/models/interfacesAndTypes';

    interface IProps {
        accountDataObject: IUserProfileData | null,
        handleLogout: () => void
    }
    const props = defineProps<IProps>()
    
</script>
    
<template>
    <div class="card" v-if="props.accountDataObject">
        <div class="card-content">
            <div class="media">
                <div class="media-left">
                    <figure class="image is-48x48">
                        <img src="/placeholder-48x48.png">
                    </figure>
                </div>
                <div class="media-content">
                  <strong class="is-size-4">{{ props.accountDataObject.firstName }}</strong>
                  <p>@{{ props.accountDataObject.username }}</p>
                </div>
            </div>
            <div class="content">
                <ul>
                  <li>{{ props.accountDataObject.username }}</li>
                  <li>{{ props.accountDataObject.firstName }}</li>
                  <li>{{ props.accountDataObject.phoneNumber }}</li>
                  <li>{{ props.accountDataObject.lastName }}</li>
                  <li>{{ props.accountDataObject.email }}</li>
                </ul>
                <button class="button button-danger" @click="props.handleLogout()">Sign out</button>
            </div>
        </div>
    </div>
</template>

<style scoped>
    @media(max-width: 720px) {
        .content {
          display: flex;
          flex-direction: column;
        }
    }

</style>