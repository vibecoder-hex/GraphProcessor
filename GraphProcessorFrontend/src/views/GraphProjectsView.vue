<template>
    <div class="graph-projects-view" v-if="projectList.length > 0">
        <RouterLink :to="{name: 'ProjectDetails', params: {graphName: project.graphName}}" v-for="project in projectList" class="card">
            <div class="card-image">
              <figure class="image is-3by2">
                <img
                    :src="project.imagePresignedUrl"
                    alt="Placeholder image"
                />
              </figure>
            </div>
            <div class="card-content">
              <div class="media">
                <div class="media-content">
                    <p class="title is-4">{{ project.graphName }}</p>
                </div>
              </div>

              <div class="content">
                {{ project.graphDescription }}
                <br />
                <time>{{ new Date(project.createdAt).toUTCString() }}</time>
              </div>
            </div>
          </RouterLink>
    </div>
    <div v-else>
        <h1 class="is-size-4">The list of projects is empty</h1>
        <button class="button is-success" @click="router.push('/')">Create new project</button>
    </div>
    <p>{{ errorMessage }}</p>
</template>

<script setup lang="ts">
    import { GraphProjectRequests } from "@/services/httpServices/GraphProjectRequests.ts";
    import { ref } from "vue"
    import type { IGraphProjectObject } from "@/models/interfacesAndTypes.ts";
    import  { apiClient } from "@/services/httpServices/ApiClientConfigurator.ts";
    import router from "@/router/index.ts";
    
    const errorMessage = ref<string>("");
    const projectList = ref<IGraphProjectObject[]>([]);

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
    @media(min-width: 800px) {
        .graph-projects-view {
            display: grid;
            grid-template-columns: auto auto auto;
            grid-gap: 20px;
        }
    }
  
  @media(max-width: 720px) {
      .graph-projects-view {
          display: flex;
          flex-direction: column;
      }
  }
  
  .graph-projects-view {
      margin: 0 auto;
      width: 90%;
      gap: 10px;
  }

</style>