import axios, {type AxiosInstance} from 'axios'
import type { IGraphParametersObject, IResponseOperationResult, IDistanceProcessingRootObject, Algorithm } from "@/models/interfacesAndTypes.ts";
import {ErrorHandler} from "@/services/httpServices/ApiClientConfigurator.ts";


export class GraphAlgorithmsRequests {
    private static getSelectedUrl(startVertex: string, targetVertex: string, selectedAlgorithm: string): string {
        const baseUrl: string = `api/GraphAlgorithms/${selectedAlgorithm}/${startVertex}`
        switch (selectedAlgorithm) {
            case "bfs":
            case "dijkstra":
                return `${baseUrl}/${targetVertex}`
            case "dfs":
                return baseUrl
        }
        return `${baseUrl}`
    }
    
    public static async getPathFromRequest(algoClient: AxiosInstance, startVertex: string, targetVertex: string, selectedAlgorithm: string, distanceJSONObject: IGraphParametersObject): Promise<IResponseOperationResult<IDistanceProcessingRootObject>> {
        try {
            const response = await algoClient.post<IDistanceProcessingRootObject>(this.getSelectedUrl(startVertex, targetVertex, selectedAlgorithm), distanceJSONObject)
            return {
                operation: {
                    isValid: true,
                    errorMessage: ""
                },
                responseData: response.data
            }
        } catch(error) {
            return {
                operation : {
                    isValid: false,
                    errorMessage: ErrorHandler.handleError(error)
                },
                responseData: null
            }
        }
    }
}