<script setup lang="ts">
import {onMounted, onUnmounted, ref} from 'vue'
import {NetworkCanvasProcessor} from "@/services/graphServices/networkCanvasService.ts";
import type {DataSet, Edge, Network, Node} from "vis-network/standalone"

    interface IProps {
        visNodes: DataSet<Node>,
        visEdges: DataSet<Edge>
    }

    const props = defineProps<IProps>()

    
    const selectedGraphCanvas = defineModel<HTMLCanvasElement | null>("selectedGraphCanvas");
    
    const networkContainer = ref<HTMLElement | null>(null)
    let network: Network | null = null
    
    onMounted(() => {
        if (networkContainer.value) {
            network = NetworkCanvasProcessor.DrawVis(networkContainer.value, props.visNodes, props.visEdges)
            network.setSize(networkContainer.value.offsetWidth.toString(), networkContainer.value.offsetHeight.toString());
            network.fit();
            
            if (network) {
                network.on("afterDrawing", () => {
                   selectedGraphCanvas.value = networkContainer.value ? networkContainer.value.querySelector("canvas") : null
                })
            }
        }
        
    })
    onUnmounted(() => {
        if (network) {
            network.destroy()
            network = null
        }
    })
    
</script>

<template>
    <div id="network-container" ref="networkContainer"></div>
</template>

<style scoped>
  #network-container {
    width: 100%;
    height: 100%;
    min-height: 500px;
    border: 1px solid black;
  }
  @media(max-width: 640px) {
    #network-container {
        width: 100%;
        height: 100%;
        min-height: 500px;
        max-width: 640px;
        border: 1px solid black;
      }
  }
</style>