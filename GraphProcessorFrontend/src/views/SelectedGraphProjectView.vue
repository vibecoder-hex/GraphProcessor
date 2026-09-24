<script setup lang="ts">
    import { ref } from 'vue'
    import { apiClient } from "@/services/httpServices/ApiClientConfigurator.ts";
    import type {IGraphProjectObject, IResponseOperationResult} from "@/models/interfacesAndTypes.ts";
    import { GraphProjectRequests } from "@/services/httpServices/GraphProjectRequests.ts";
    import router from "@/router";

    interface IProps {
        graphName: string
    }
    
    const props = defineProps<IProps>();
    const graphProjectObject = ref<IGraphProjectObject | null>(null)

    const errorMessage = ref<string>("")
    
    async function handleSelectedProject(): Promise<void> {
        const response: IResponseOperationResult<IGraphProjectObject> = await GraphProjectRequests.getSelectedProject(apiClient, props.graphName);
        
        const responseData = response.responseData;
        const responseError = response.operation.errorMessage;
        
        if (response.operation.isValid && responseData) {
            graphProjectObject.value = responseData;
            errorMessage.value = "";
            console.log(graphProjectObject.value)
        } else {
          errorMessage.value = responseError
        }
    }
    
    handleSelectedProject()
    
    async function handleDeleteProject(): Promise<void> {
        const response: IResponseOperationResult<null> = await GraphProjectRequests.deleteSelectedProject(apiClient, props.graphName);
      
        const responseError = response.operation.errorMessage;
        
        if (response.operation.isValid) {
            await router.push('/projects');
        } else {
            errorMessage.value = responseError;
        }
    }
</script>

<template>
    <article class="message is-primary" v-if="graphProjectObject">
      <div class="message-header">
          <p>{{ graphProjectObject.graphName }}</p>
          <div>
              Delete project
              <button class="delete" aria-label="delete" @click="handleDeleteProject()"></button>
          </div>
      </div>
      <div class="message-body">
        <figure class="image is-3by2">
            <img :src="graphProjectObject.imagePresignedUrl" alt="placeholder image">
        </figure>
        {{ graphProjectObject.graphDescription }}
        <strong>Created at: {{graphProjectObject.createdAt}}</strong>
      </div>
    </article>
    <p>{{ errorMessage }}</p>
</template>

<style scoped>
    .message {
        width: 90%;
        margin: 0 auto;
    }
    .message-body {
        display: flex;
        flex-direction: column;
    }
</style>