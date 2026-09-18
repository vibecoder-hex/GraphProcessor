import axios, {type AxiosInstance} from 'axios';
import type {
    GraphType,
    ICreateProjectObject,
    IGraphParametersObject, IGraphProjectObject,
    IResponseOperationResult
} from "@/models/interfacesAndTypes.ts";
import {ApiClientConfigurator, ErrorHandler} from "@/services/httpServices/ApiClientConfigurator.ts";

export class GraphProjectRequests {

    public static async createProject(projectClient: AxiosInstance, graphName: string, graphDescription: string, graphObject: IGraphParametersObject, graphType: GraphType): Promise<IResponseOperationResult<null>> {
        const newProjectObject: ICreateProjectObject = {
            graphName: graphName,
            graphDescription: graphDescription,
            graphType: graphType,
            graphStructure: graphObject
        }
        try {
            const response = await projectClient.post<null>("api/GraphProject", newProjectObject);
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
    
    public static async getProjects(projectClient: AxiosInstance): Promise<IResponseOperationResult<IGraphProjectObject[]>> {
        try {
            const response = await projectClient.get<IGraphProjectObject[]>('api/GraphProject');
            return {
                operation: {
                    isValid: true,
                    errorMessage: ""
                },
                responseData: response.data
            }
        } catch (error) {
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