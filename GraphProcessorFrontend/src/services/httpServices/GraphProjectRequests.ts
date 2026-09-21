import { type AxiosInstance } from 'axios';
import type {
    GraphType,
    ICreateProjectObject,
    IGraphParametersObject, IGraphProjectObject,
    IResponseOperationResult
} from "@/models/interfacesAndTypes.ts";
import { ErrorHandler } from "@/services/httpServices/ApiClientConfigurator.ts";

export class GraphProjectRequests {
    
    private static createFormData(newProjectObject: ICreateProjectObject): FormData {
        const formData = new FormData();
        formData.append("graphName", newProjectObject.graphName);
        formData.append("graphDescription", newProjectObject.graphDescription);
        formData.append("graphType", newProjectObject.graphType);
        formData.append("graphStructure", JSON.stringify(newProjectObject.graphStructure));
        formData.append("image", newProjectObject.image, `${crypto.randomUUID()}.png`);
        return formData;
    }
    public static async createProject(projectClient: AxiosInstance, graphName: string, graphDescription: string, graphObject: IGraphParametersObject, graphType: GraphType, blobGraphImage: Blob): Promise<IResponseOperationResult<null>> {
        const newProjectObject: ICreateProjectObject = {
            graphName: graphName,
            graphDescription: graphDescription,
            graphType: graphType,
            graphStructure: graphObject,
            image: blobGraphImage
        }
        try {
             await projectClient.post<null>("api/GraphProject", this.createFormData(newProjectObject),{
                headers: {
                    "Content-Type": "multipart/form-data"
                }
            });
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