<script setup lang="ts">
    import { ref } from 'vue'
    import { apiClient } from "@/services/httpServices/ApiClientConfigurator.ts";
    import type {IGraphProjectObject, IResponseOperationResult} from "@/models/interfacesAndTypes.ts";
    import {GraphProjectRequests} from "@/services/httpServices/GraphProjectRequests.ts";
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
    <div class="selected-graph-project" v-if="graphProjectObject">
        <h1 class="is-size-3">{{ graphProjectObject.graphName }}</h1>
        <figure class="image is-3by2">
            <img :src="graphProjectObject.imagePresignedUrl" alt="placeholder image">
        </figure>
        <p>{{ graphProjectObject.graphDescription }}</p>
        <time>{{graphProjectObject.createdAt}}</time>
        <button @click="handleDeleteProject()" class="button is-danger">Delete project</button>
    </div>
    <p>{{ errorMessage }}</p>
</template>

<style scoped>

</style>