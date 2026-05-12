import {Network, DataSet, type Node, type Edge, type Data, type Options } from 'vis-network/standalone'
import type { GraphType } from "@/models/interfacesAndTypes.ts";


export class NetworkCanvasProcessor {
    public static AddVisNode(id: string, node: string, nodes: DataSet<Node>): void {
        nodes.add({id: id, label: node})
    }
    
    public static RemoveVisNode(id: string, node: string, nodes: DataSet<Node>) {
        nodes.remove({id: id, label: node})
    }
    
    public static AddVisEdge(from: string, to: string, weight: number, edges: DataSet<Edge>, selectedGraphType: GraphType) {
        const arrowType: string = selectedGraphType === 'oriented' ? 'to': ''
        edges.add({from: from, to: to, label: weight.toString(), arrows: arrowType})
    }
    public static RemoveVisEdge(from: string, to: string, edges: DataSet<Edge>) {
        const edgeToDelete: Edge | undefined = edges.get({
            filter: (item) => (item.from === from && item.to === to) || (item.from === to && item.to === from)
        })[0]
        if (edgeToDelete) {
            edges.remove(edgeToDelete)
        }
    }
    public static DrawVis(container: HTMLElement, nodes: DataSet<Node>, edges: DataSet<Edge>, options: Options = {
        height: '100%', 
        width: '100%', 
        autoResize: true,
        edges: { color: "#000000" }
        
    }) {
        const data: Data = {
            nodes: nodes,
            edges: edges
        }
        return new Network(container, data, options)
    }
    
    public static ResetColors(edges: DataSet<Edge>) {
        const edgeIds = edges.getIds();
        const resetUpdates = edgeIds.map((id) => ({
            id: id,
            color: "#000000"
        }))
        edges.update(resetUpdates)
    }
    public static UpdateColor(edges: DataSet<Edge>, shortestPathArray: string[]) {
        for (let i = 0; i < shortestPathArray.length - 1; i++) {
            const fromNode: string | undefined = shortestPathArray[i]
            const toNode: string | undefined = shortestPathArray[i + 1]
            var connectedEdges = edges.get({
                filter: (edge: Edge) => (edge.from === fromNode && edge.to === toNode) || (edge.from === toNode && edge.to === fromNode),
                fields: ["id"]
            }).map((pickedEdge: Edge) => ({
                id: pickedEdge.id,
                color: "red"
            }))
            edges.update(connectedEdges)
        }
    }
}