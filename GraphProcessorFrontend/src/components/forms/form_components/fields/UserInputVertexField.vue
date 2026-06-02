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
    <article class="message is-dark">
        <div class="message-header">
            <p>Graph Input Parameters</p>
        </div>
        <div class="message-body">
            <div class="user-input">
                <!-- Node Input Section -->
                <div class="node-section">
                    <div class="card">
                        <div class="card-content">
                            <p class="is-size-5 mb-3">Add Nodes</p>
                            <div class="field">
                                <label class="label">Node name:</label>
                                <div class="control">
                                    <input class="input" v-model="nodeNameValue" type="text">
                                </div>
                            </div>
                            <div class="field is-grouped">
                                <div class="control">
                                    <button :disabled="!isValidNodeName" class="button is-info" @click="handleNodesOperation(NodeMethods.addNode(nodeNameValue, distanceMap, visNodes))">Add Node</button>
                                </div>
                                <div class="control">
                                    <button :disabled="!isValidNodeName" class="button is-warning" @click="handleNodesOperation(NodeMethods.deleteNode(nodeNameValue, distanceMap, visNodes))" >Delete Node</button>
                                </div>
                            </div>
                            <div v-if="nodeCardMessage" class="notification is-danger is-light">
                                {{ nodeCardMessage }}
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Edge Input Section -->
                <div class="edge-section">
                    <div class="card">
                        <div class="card-content">
                            <p class="is-size-5 mb-3">Add Edges</p>
                            <div class="field">
                                <label class="label">From:</label>
                                <div class="control">
                                    <input class="input" v-model="fromNodeValue" type="text">
                                </div>
                            </div>
                            <div class="field">
                                <label class="label">To:</label>
                                <div class="control">
                                    <input class="input" v-model="toNodeValue" type="text">
                                </div>
                            </div>
                            <div class="field">
                                <label class="label">Distance:</label>
                                <div class="control">
                                    <input @change="showInvalidDistanceNumberError()" class="input" v-model="distanceNumber" type="number">
                                </div>
                            </div>
                            <div class="field is-grouped">
                                <div class="control">
                                    <button :disabled="!isValidFromNodeValue || !isValidToNodeValue || !isValidDistanceNumber" class="button is-success" @click="handleEdgeMethods(EdgeMethods.addEdge(fromNodeValue, toNodeValue, distanceNumber, distanceMap, visEdges, selectedGraphType))">Add Edge</button>
                                </div>
                                <div class="control">
                                    <button :disabled="!isValidFromNodeValue || !isValidToNodeValue" class="button is-warning" @click="handleEdgeMethods(EdgeMethods.deleteEdge(fromNodeValue, toNodeValue, distanceMap, visEdges, selectedGraphType))">Delete Edge</button>
                                </div>
                            </div>
                            <div v-if="edgeCardMessage" class="notification is-danger is-light">
                                {{ edgeCardMessage }}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </article>

    <!-- Graph Canvas Section -->
    <div v-if="distanceMap.size > 0" class="canvas-section">
        <div class="field">
            <div class="control">
                <label class="checkbox">
                    <input type="checkbox" v-model="showCanvas">
                    Show graph canvas
                </label>
            </div>
        </div>
        <div v-if="showCanvas">
            <NetworkVisualizationCanvas :visNodes="visNodes" :visEdges="visEdges"/>
            <div class="mt-3">
                <button class="button is-danger" @click="NetworkCanvasProcessor.ResetColors(visEdges)">Reset edge colors</button>
            </div>
        </div>
    </div>
    
</template>

<style scoped>
    .mb-3 {
        margin-bottom: 1.5rem;
    }

    .mt-3 {
        margin-top: 1.5rem;
    }

    .card {
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .canvas-section {
        margin-top: 2rem;
    }

    @media (min-width: 1000px) {
        .user-input {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
            margin-top: 0;
        }
    }

    @media (max-width: 999px) {
        .user-input {
            display: flex;
            flex-direction: column;
            gap: 20px;
            margin-top: 0;
        }
    }
</style>