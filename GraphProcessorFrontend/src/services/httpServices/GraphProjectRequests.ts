import axios, {type AxiosInstance} from 'axios';
import type {
    GraphType,
    ICreateProjectObject,
    IGraphParametersObject,
    IResponseOperationResult
} from "@/models/interfacesAndTypes.ts";
import {ApiClientConfigurator, ErrorHandler} from "@/services/httpServices/ApiClientConfigurator.ts";

export interface IGraphProjectRequests {
    createProject(): Promise<IResponseOperationResult<null>>
}

export class GraphProjectRequests implements IGraphProjectRequests {
    private readonly _projectClient: AxiosInstance
    private readonly _graphName: string
    private readonly _graphDescription: string
    private readonly _graphObject: IGraphParametersObject
    private readonly _graphType: GraphType
    
    constructor(graphName: string, graphDescription: string, graphObject: IGraphParametersObject, graphType: GraphType) {
        const apiInstance = ApiClientConfigurator.getInstance();
        this._projectClient = apiInstance.getClient()
        this._graphName = graphName;
        this._graphDescription = graphDescription;
        this._graphObject = graphObject;
        this._graphType = graphType;
    }
    
    private getNewProjectObject(): ICreateProjectObject {
        return {
            graphName: this._graphName,
            graphDescription: this._graphDescription,
            graphType: this._graphType,
            graphStructure: this._graphObject
        }
    }
    public async createProject(): Promise<IResponseOperationResult<null>> {
        try {
            const response = await this._projectClient.post("api/GraphProject", this.getNewProjectObject());
            return {
                operation: {
                    isValid: true,
                    errorMessage: ""
                },
                responseData: null
            }
        }
        catch (error) {
            return {
                operation: {
                    isValid: false,
                    errorMessage: ErrorHandler.handleError(error)
                },
                responseData: null
            }
        }
    }
}