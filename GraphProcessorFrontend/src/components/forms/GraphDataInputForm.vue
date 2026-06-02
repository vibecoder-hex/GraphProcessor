<script setup lang="ts">
    import { ref } from 'vue'
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
      GraphType
    } from "@/models/interfacesAndTypes.ts"
    import  { type IGraphAlgorithmsRequests, GraphAlgorithmsRequests } from "@/services/httpServices/GraphAlgorithmsRequests.ts";
    import {NetworkCanvasProcessor} from "@/services/graphServices/networkCanvasService.ts";
    import { DataSet, type Edge, type Node } from "vis-network/standalone"

    const APIURL: string = "/api/GraphAlgorithms"
    
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

    function getObjectFromMap(): IGraphParametersObject {
        const distanceObject: IGraphParametersObject = { Distances: {} }
        distanceMap.value.forEach((neighbors: Map<string, number>, node: string) => {
            distanceObject.Distances[node] = Object.fromEntries(neighbors)
        })
        return distanceObject
    }
      
    async function handleRequestedPath(): Promise<void> {
        const graphAlgorithmsRequests: IGraphAlgorithmsRequests = new GraphAlgorithmsRequests(APIURL, getObjectFromMap(), selectedAlgorithm.value, startVertex.value, targetVertex.value)
        const pathRequest: IResponseOperationResult<IDistanceProcessingRootObject> = await graphAlgorithmsRequests.getPathFromRequest();
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
    
</script>

<template>
    <form @submit.prevent class="form-wrapper">
        <div class="form-container">
            <!-- Graph Type Selection -->
            <GraphTypeSelector v-model:selectedGraphType="selectedGraphType" v-model:isGraphTypeSelected="isGraphTypeSelected"></GraphTypeSelector>

            <!-- User Input Section -->
            <UserInputVertexField v-if="isGraphTypeSelected"
                                  v-model:selectedGraphType="selectedGraphType"
                                  v-model:distanceMap="distanceMap"
                                  v-model:visEdges="visEdges"
                                  v-model:visNodes="visNodes"/>

            <!-- Graph Processing Section -->
            <div v-if="distanceMap.size > 0" class="graph-structure">
                <AlgorithmSelector v-model:selectedAlgorithm="selectedAlgorithm" />
                <PathSearchField v-model:selectedAlgorithm="selectedAlgorithm"
                                         v-model:startVertex="startVertex"
                                         v-model:targetVertex="targetVertex"
                />
                <button class="button is-primary is-fullwidth" @click="handleRequestedPath()">Find Shortest Path</button>

                <!-- Error Message -->
                <div v-if="errorMessage" class="notification is-danger is-light">
                    <button class="delete"></button>
                    {{ errorMessage }}
                </div>

                <!-- Results -->
                <div v-if="graphProcessingResult">
                    <DistanceProcessingResult :result="graphProcessingResult.result"/>
                </div>
            </div>
        </div>
    </form>
</template>


<style scoped>
    .form-wrapper {
        width: 100%;
    }

    .form-container {
        background-color: #e8e8e8;
        border-radius: 12px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        padding: 2rem;
    }

    .graph-structure {
        display: flex;
        flex-direction: column;
        gap: 1.5rem;
        margin-top: 1.5rem;
        padding-top: 1.5rem;
        border-top: 2px solid #d0d0d0;
    }

    .notification {
        margin: 0;
    }

    /* Tablet layout */
    @media (max-width: 1000px) and (min-width: 641px) {
        .form-container {
            padding: 1.5rem;
        }

        .graph-structure {
            gap: 1rem;
            margin-top: 1rem;
            padding-top: 1rem;
        }
    }

    /* Mobile layout with edge padding */
    @media (max-width: 640px) {
        .form-wrapper {
            padding: 0 1rem;
        }

        .form-container {
            padding: 1rem;
            border-radius: 8px;
        }

        .graph-structure {
            gap: 1rem;
            margin-top: 1rem;
            padding-top: 1rem;
        }
    }
</style>