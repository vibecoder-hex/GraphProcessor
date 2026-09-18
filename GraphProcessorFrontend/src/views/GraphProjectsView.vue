<template>
    <div class="graph-projects-view">
        <div v-for="project in projectList">
            <p>{{ project.name }}</p>
            <p> {{ project.description }} </p>
            <p> {{ project.type }} </p>
            <p> {{ JSON.stringify(project.structure) }} </p>
            <p> {{ new Date(project.creationat).toUTCString()}}</p>
        </div>
    </div>
    <p>{{ errorMessage }}</p>
</template>

<script setup lang="ts">
    import { GraphProjectRequests } from "@/services/httpServices/GraphProjectRequests.ts";
    import { ref } from "vue"
    import type { IGraphProjectObject } from "@/models/interfacesAndTypes.ts";
    import  { ApiClientConfigurator} from "@/services/httpServices/ApiClientConfigurator.ts";
    import type {AxiosInstance} from "axios";
    
    const errorMessage = ref<string>("");
    const projectList = ref<IGraphProjectObject[]>([]);
    
    const apiInstance: ApiClientConfigurator = ApiClientConfigurator.getInstance();
    const apiClient: AxiosInstance = apiInstance.getClient();

    async function handleLoadProjects(): Promise<void> {
        const response = await GraphProjectRequests.getProjects(apiClient);
        if (response.operation.isValid && response.responseData) {
              projectList.value = response.responseData;
              errorMessage.value = "";
        } else {
            errorMessage.value = response.operation.errorMessage
        }
    }
    handleLoadProjects()
</script>

<style scoped>

</style>