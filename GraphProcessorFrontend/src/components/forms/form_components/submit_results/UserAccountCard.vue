<script setup lang="ts">
    import type { IUserProfileData } from '@/models/interfacesAndTypes';

    interface IProps {
        accountDataObject: IUserProfileData | null,
        handleLogout: () => void
    }
    const props = defineProps<IProps>()
</script>
    
<template>
    <div class="card">
      <div class="card-content">
        <div class="content">
          <ul v-if="props.accountDataObject">
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
    .content {
        display: flex;
        flex-direction: column;
    }
</style>