<script setup lang="ts">
    import { ref, computed } from 'vue';
    import NetworkVisualizationCanvas from "../../../graph_view/NetworkVisualisationCanvas.vue";
    import { NetworkCanvasProcessor } from "@/services/graphServices/networkCanvasService.ts";
    import { NodeMethods, EdgeMethods } from "@/services/graphServices/graphOperationsService.ts"
    import type { GraphType, IOperationResult } from "@/models/interfacesAndTypes.ts";
    import { DataSet, type Node, type Edge } from "vis-network/standalone"

    const distanceMap = defineModel<Map<string, Map<string, number>>>("distanceMap", {required: true});
    const visNodes = defineModel<DataSet<Node>>("visNodes", {required: true});
    const visEdges = defineModel<DataSet<Edge>>("visEdges", {required: true});
    const selectedGraphType = defineModel<GraphType>("selectedGraphType", { required: true });
    
    const nodeNameValue = ref<string>("")
    const fromNodeValue = ref<string>("")
    const toNodeValue = ref<string>("")
    const distanceNumber = ref(0)
    
    const showCanvas = ref<boolean>(false)
    
    const nodeCardMessage = ref<string>("")
    const edgeCardMessage = ref<string>("")
    
    const isValidNodeName = computed(() => nodeNameValue.value.trim().length > 0)
    const isValidFromNodeValue = computed(() => fromNodeValue.value.trim().length > 0)
    const isValidToNodeValue = computed(() => toNodeValue.value.trim().length > 0)
    const isValidDistanceNumber = computed(() => distanceNumber.value >= 0)
    
    function showInvalidDistanceNumberError(): void {
        if (!isValidDistanceNumber.value) {
            edgeCardMessage.value = "Invalid distance number"
        } else {
            edgeCardMessage.value = ""
        }
    }
    
    function handleNodesOperation(operationResult: IOperationResult): void {
        if (!operationResult.isValid) {
            nodeCardMessage.value = operationResult.errorMessage
        }
        else {
            nodeCardMessage.value = ""
            nodeNameValue.value = ""
        }
    }
    
    function handleEdgeMethods(operationResult: IOperationResult): void {
        if (!operationResult.isValid) {
            edgeCardMessage.value = operationResult.errorMessage
        }
        else {
            edgeCardMessage.value = ""
            fromNodeValue.value = ""
            toNodeValue.value = ""
            distanceNumber.value = 0
              
        }
    }
    
</script>

<template>
    <div class="user-input-wrapper">
        <p class="is-size-5 has-text-weight-bold">Enter Graph Parameters</p>
        <div class="user-input">
            <!-- Nodes Input Section -->
            <article class="node-input message is-dark">
                <div class="message-header">
                    <p>Add Nodes</p>
                </div>
                <div class="message-body">
                    <div class="field">
                        <label class="label">Node Name:</label>
                        <div class="control">
                            <input class="input" v-model="nodeNameValue" type="text" placeholder="Enter node name">
                        </div>
                    </div>
                    <div class="field is-grouped">
                        <p class="control">
                            <button :disabled="!isValidNodeName" class="button is-success" @click="handleNodesOperation(NodeMethods.addNode(nodeNameValue, distanceMap, visNodes))">Add Node</button>
                        </p>
                        <p class="control">
                            <button :disabled="!isValidNodeName" class="button is-danger" @click="handleNodesOperation(NodeMethods.deleteNode(nodeNameValue, distanceMap, visNodes))">Delete Node</button>
                        </p>
                    </div>
                    <div v-if="nodeCardMessage" class="notification is-warning is-light">
                        <button class="delete"></button>
                        {{ nodeCardMessage }}
                    </div>
                </div>
            </article>

            <!-- Edges Input Section -->
            <article class="edge-input message is-dark">
                <div class="message-header">
                    <p>Add Edges</p>
                </div>
                <div class="message-body">
                    <div class="field">
                        <label class="label">From Node:</label>
                        <div class="control">
                            <input class="input" v-model="fromNodeValue" type="text" placeholder="Enter source node">
                        </div>
                    </div>
                    <div class="field">
                        <label class="label">To Node:</label>
                        <div class="control">
                            <input class="input" v-model="toNodeValue" type="text" placeholder="Enter target node">
                        </div>
                    </div>
                    <div class="field">
                        <label class="label">Distance:</label>
                        <div class="control">
                            <input @change="showInvalidDistanceNumberError()" class="input" v-model="distanceNumber" type="number" placeholder="Enter distance value">
                        </div>
                    </div>
                    <div class="field is-grouped">
                        <p class="control">
                            <button :disabled="!isValidFromNodeValue || !isValidToNodeValue || !isValidDistanceNumber" class="button is-success" @click="handleEdgeMethods(EdgeMethods.addEdge(fromNodeValue, toNodeValue, distanceNumber, distanceMap, visEdges, selectedGraphType))">Add Edge</button>
                        </p>
                        <p class="control">
                            <button :disabled="!isValidFromNodeValue || !isValidToNodeValue" class="button is-danger" @click="handleEdgeMethods(EdgeMethods.deleteEdge(fromNodeValue, toNodeValue, distanceMap, visEdges, selectedGraphType))">Delete Edge</button>
                        </p>
                    </div>
                    <div v-if="edgeCardMessage" class="notification is-warning is-light">
                        <button class="delete"></button>
                        {{ edgeCardMessage }}
                    </div>
                </div>
            </article>
        </div>

        <!-- Graph Canvas Section -->
        <div v-if="distanceMap.size > 0" class="canvas-section">
            <label class="checkbox">
                <input type="checkbox" v-model="showCanvas">
                <span class="ml-2">Show Graph Visualization</span>
            </label>
            <div v-if="showCanvas" class="visualization-wrapper">
                <NetworkVisualizationCanvas :visNodes="visNodes" :visEdges="visEdges"/>
                <div class="mt-3">
                    <button class="button is-warning" @click="NetworkCanvasProcessor.ResetColors(visEdges)">Reset Edge Colors</button>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
    .user-input-wrapper {
        display: flex;
        flex-direction: column;
        gap: 1.5rem;
    }

    .user-input {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 1.5rem;
        margin-top: 1rem;
    }

    .message {
        border-radius: 6px;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .message-header {
        background-color: #363636;
        border-bottom: 1px solid #555;
    }

    .message-header p {
        color: #fff;
        font-weight: 600;
    }

    .message-body {
        padding: 1.5rem;
    }

    .field {
        margin-bottom: 1rem;
    }

    .field:last-child {
        margin-bottom: 0;
    }

    .is-grouped {
        display: flex;
        gap: 0.5rem;
        flex-wrap: wrap;
    }

    .is-grouped .control {
        flex: 1;
        min-width: 150px;
    }

    .canvas-section {
        margin-top: 1rem;
        padding: 1.5rem;
        background-color: #f5f5f5;
        border-radius: 6px;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .visualization-wrapper {
        margin-top: 1rem;
        background-color: #fff;
        border-radius: 6px;
        overflow: hidden;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .ml-2 {
        margin-left: 0.5rem;
    }

    .mt-3 {
        margin-top: 1rem;
    }

    /* Desktop layout - keep grid as specified */
    @media (min-width: 1001px) {
        .user-input {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 1.5rem;
        }
    }

    /* Tablet layout */
    @media (max-width: 1000px) and (min-width: 641px) {
        .user-input {
            display: flex;
            flex-direction: column;
            gap: 1.5rem;
        }
    }

    /* Mobile layout with padding */
    @media (max-width: 640px) {
        .user-input-wrapper {
            padding: 0 1rem;
        }

        .user-input {
            display: flex;
            flex-direction: column;
            gap: 1rem;
            margin-top: 1rem;
        }

        .message-body {
            padding: 1rem;
        }

        .is-grouped {
            flex-direction: column;
        }

        .is-grouped .control {
            width: 100%;
        }

        .canvas-section {
            padding: 1rem;
            margin-left: 0;
            margin-right: 0;
        }
    }
</style>