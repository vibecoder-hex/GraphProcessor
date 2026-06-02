<script setup lang="ts">
    import type { IUserProfileData } from '@/models/interfacesAndTypes';

    interface IProps {
        accountDataObject: IUserProfileData | null,
        handleLogout: () => void
    }
    const props = defineProps<IProps>()
</script>
    
<template>
    <div class="account-container">
        <article v-if="props.accountDataObject" class="message is-success">
            <div class="message-header">
                <p>User Profile</p>
            </div>
            <div class="message-body">
                <div class="card">
                    <div class="card-content">
                        <!-- Avatar Section -->
                        <div class="avatar-section">
                            <figure class="avatar">
                                <div class="avatar-placeholder">
                                    {{ props.accountDataObject.username.charAt(0).toUpperCase() }}
                                </div>
                            </figure>
                        </div>

                        <!-- User Data List -->
                        <div class="user-data">
                            <ul class="user-list">
                                <li class="user-list-item">
                                    <span class="label">Username:</span>
                                    <span class="value">{{ props.accountDataObject.username }}</span>
                                </li>
                                <li class="user-list-item">
                                    <span class="label">First Name:</span>
                                    <span class="value">{{ props.accountDataObject.firstName }}</span>
                                </li>
                                <li class="user-list-item">
                                    <span class="label">Last Name:</span>
                                    <span class="value">{{ props.accountDataObject.lastName }}</span>
                                </li>
                                <li class="user-list-item">
                                    <span class="label">Email:</span>
                                    <span class="value">{{ props.accountDataObject.email }}</span>
                                </li>
                                <li class="user-list-item">
                                    <span class="label">Phone Number:</span>
                                    <span class="value">{{ props.accountDataObject.phoneNumber }}</span>
                                </li>
                            </ul>
                        </div>
                    </div>

                    <!-- Card Footer with Button -->
                    <div class="card-footer">
                        <a href="#" class="card-footer-item">
                            <button 
                                class="button is-danger is-fullwidth" 
                                @click="props.handleLogout()"
                            >
                                Sign Out
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
.account-container {
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
    align-items: center;
    gap: 2rem;
}

/* Avatar Section */
.avatar-section {
    width: 100%;
    display: flex;
    justify-content: center;
}

.avatar {
    width: auto;
    margin: 0;
}

.avatar-placeholder {
    width: 120px;
    height: 120px;
    background: linear-gradient(135deg, #48c774, #3273dc);
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 2.5rem;
    font-weight: bold;
    color: white;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

/* User Data List */
.user-data {
    width: 100%;
}

.user-list {
    list-style: none;
    padding: 0;
    margin: 0;
}

.user-list-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1rem 0;
    border-bottom: 1px solid #dbdbdb;
}

.user-list-item:last-child {
    border-bottom: none;
}

.user-list-item .label {
    font-weight: 600;
    color: #363636;
    flex: 1;
}

.user-list-item .value {
    color: #3273dc;
    text-align: right;
    flex: 1;
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
@media screen and (max-width: 768px) {
    .message {
        width: 100%;
    }

    .avatar-placeholder {
        width: 100px;
        height: 100px;
        font-size: 2rem;
    }

    .user-list-item {
        flex-direction: column;
        align-items: flex-start;
    }

    .user-list-item .value {
        text-align: left;
        margin-top: 0.5rem;
    }
}
</style>