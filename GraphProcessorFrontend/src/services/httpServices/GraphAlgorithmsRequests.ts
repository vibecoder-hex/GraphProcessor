import axios, {type AxiosInstance} from 'axios'
import type { IGraphParametersObject, IResponseOperationResult, IDistanceProcessingRootObject, Algorithm } from "@/models/interfacesAndTypes.ts";
import {ApiClientConfigurator, ErrorHandler} from "@/services/httpServices/ApiClientConfigurator.ts";

export interface IGraphAlgorithmsRequests {
    getPathFromRequest(): Promise<IResponseOperationResult<IDistanceProcessingRootObject>>
}

export class GraphAlgorithmsRequests implements IGraphAlgorithmsRequests {
    private readonly _algoClient: AxiosInstance;
    private readonly _apiUrl: string;
    private readonly _selectedAlgorithm: Algorithm;
    private readonly _distanceJSONObject: IGraphParametersObject;
    private readonly _startVertex: string;
    private readonly _endVertex: string;
    
    constructor(apiUrl: string, distanceJSONObject: IGraphParametersObject, selectedAlgorithm: Algorithm, startVertex: string, endVertex: string) {
        this._apiUrl = apiUrl;
        this._distanceJSONObject = distanceJSONObject;
        this._selectedAlgorithm= selectedAlgorithm;
        this._startVertex = startVertex;
        this._endVertex = endVertex;
        const apiConfigurator = new ApiClientConfigurator(this._apiUrl);
        this._algoClient = apiConfigurator.getInstance();
    }
    
    private getSelectedUrl() {
        const baseUrl: string = `${this._selectedAlgorithm}/${this._startVertex}`
        switch (this._selectedAlgorithm) {
            case "bfs":
            case "dijkstra":
                return `${baseUrl}/${this._endVertex}`
            case "dfs":
                return baseUrl
        }
    }
    
    public async getPathFromRequest(): Promise<IResponseOperationResult<IDistanceProcessingRootObject>> {
        try {
            const response = await this._algoClient.post(this.getSelectedUrl(), this._distanceJSONObject)
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