<script setup lang="ts">
import {reactive, ref} from 'vue'
    import UserInputVertexField from './form_components/fields/UserInputVertexField.vue'
    import PathSearchField from "@/components/forms/form_components/fields/PathSearchField.vue";
    import AlgorithmSelector from "@/components/forms/form_components/selectors/AlgorithmSelector.vue";
    import GraphTypeSelector from "@/components/forms/form_components/selectors/GraphTypeSelector.vue";
    import DistanceProcessingResult from "./form_components/submit_results/DistanceProcessingResult.vue";
    import type {
      IDistanceProcessingRootObject,
      IGraphParametersObject,
      IResponseOperationResult,
      Algorithm,
      GraphType,
    } from "@/models/interfacesAndTypes.ts"
    import  { GraphAlgorithmsRequests } from "@/services/httpServices/GraphAlgorithmsRequests.ts";
    import {NetworkCanvasProcessor} from "@/services/graphServices/networkCanvasService.ts";
    import { DataSet, type Edge, type Node } from "vis-network/standalone"
    import GraphProjectInputfield from "@/components/forms/form_components/fields/GraphProjectInputfield.vue";
    import { GraphProjectRequests } from "@/services/httpServices/GraphProjectRequests.ts";
    import { apiClient } from "@/services/httpServices/ApiClientConfigurator.ts";
    import type { AxiosInstance } from "axios";
    
    const selectedAlgorithm = ref<Algorithm>("dijkstra")

    const startVertex = ref<string>("")
    const targetVertex = ref<string>("")

    const distanceMap = ref<Map<string, Map<string, number>>>(new Map())
    const graphProcessingResult = ref<IDistanceProcessingRootObject | null>(null)
    const errorMessage = ref<string>("")

    const visNodes = new DataSet<Node>()
    const visEdges = new DataSet<Edge>()

    const selectedGraphType = ref<GraphType>("oriented")
    const isGraphTypeSelected = ref<boolean>(false)

    const graphName = ref<string>("")
    const graphDescription = ref<string>("")
    
    const selectedGraphCanvas = ref<HTMLCanvasElement | null>(null)

    function getObjectFromMap(): IGraphParametersObject {
        const distanceObject: IGraphParametersObject = { Distances: {} }
        distanceMap.value.forEach((neighbors: Map<string, number>, node: string) => {
            distanceObject.Distances[node] = Object.fromEntries(neighbors)
        })
        return distanceObject
    }
      
    async function handleRequestedPath(): Promise<void> {
        const pathRequest = await GraphAlgorithmsRequests.getPathFromRequest(apiClient, startVertex.value, targetVertex.value, selectedAlgorithm.value, getObjectFromMap());
        if (pathRequest.operation.isValid) {
            const shortestPath: IDistanceProcessingRootObject | null  = pathRequest.responseData
            if (shortestPath !== null) {
                graphProcessingResult.value = shortestPath
                errorMessage.value = ""
                NetworkCanvasProcessor.ResetColors(visEdges);
                NetworkCanvasProcessor.UpdateColor(visEdges, shortestPath.result.shortestPath);
            }
        }
        else {
             errorMessage.value = pathRequest.operation.errorMessage
             graphProcessingResult.value = null
        }
    }
    
    async function handleCreateProject(): Promise<void> {
        const blobGraphImage: Blob = await canvasToBlob();
        console.log(graphName.value)
        const projRequest: IResponseOperationResult<null> = await GraphProjectRequests.createProject(
            apiClient,
            graphName.value,
            graphDescription.value,
            getObjectFromMap(),
            selectedGraphType.value,
            blobGraphImage);
        if (projRequest.operation.isValid) {
            errorMessage.value = "Succesfully created";
        } else {
            errorMessage.value = projRequest.operation.errorMessage;
        }
    }
    
    function canvasToBlob(): Promise<Blob> {
        return new Promise<Blob>((resolve, reject) => {
            if (selectedGraphCanvas.value) {
                selectedGraphCanvas.value.toBlob((blob: Blob | null) => {
                    blob ? resolve(blob) : reject(new Error("Blob not found"))
                }, 'image/png')
            } else {
              reject(new Error("Canvas is not valid"))
          }
        });
    }
    
</script>

<template>
    <form class="graph_processor_form" @submit.prevent>
        <GraphTypeSelector v-model:selectedGraphType="selectedGraphType" v-model:isGraphTypeSelected="isGraphTypeSelected"></GraphTypeSelector>
        <UserInputVertexField v-if="isGraphTypeSelected"
                              v-model:selectedGraphCanvas="selectedGraphCanvas"
                              v-model:selectedGraphType="selectedGraphType"
                              v-model:distanceMap="distanceMap"
                              v-model:visEdges="visEdges"
                              v-model:visNodes="visNodes"/>
        <div v-if="distanceMap.size > 0" class="graph-structure">
           <AlgorithmSelector v-model:selectedAlgorithm="selectedAlgorithm" />
           <PathSearchField v-model:selectedAlgorithm="selectedAlgorithm"
                                     v-model:startVertex="startVertex"
                                     v-model:targetVertex="targetVertex"
            />
            <button class="button is-primary" @click="handleRequestedPath()">Send path</button>
            <div v-if="graphProcessingResult" class="graph-result">
                <DistanceProcessingResult :result="graphProcessingResult.result"/>
                <GraphProjectInputfield v-model:graphName="graphName" v-model:graphDescription="graphDescription"/>
                <button class="button is-success" @click="handleCreateProject()">Save graph with path result</button>
            </div>
            <div>{{ errorMessage }}</div>
        </div>
    </form>
</template>


<style scoped>
    .graph-structure {
        display: flex;
        flex-direction: column;
        gap: 20px;
    }

    @media (max-width: 720px) {
        .graph-structure {
            padding: 0 1.5rem;
            gap: 1rem;
        }
    }
    .graph_processor_form {
        width: 90%;
        margin: 0 auto;
    }
        .graph-result {
          display: flex;
          flex-direction: column;
          gap: 20px;
        }

</style>