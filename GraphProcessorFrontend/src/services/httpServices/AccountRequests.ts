import axios, {type AxiosInstance} from "axios";
import type { IOperationResult, IResponseOperationResult, IUserProfileData } from "@/models/interfacesAndTypes";
import {ApiClientConfigurator, ErrorHandler} from "@/services/httpServices/ApiClientConfigurator.ts";

export class ProfileRequests  {
    public static async getAccountData(profileClient: AxiosInstance): Promise<IResponseOperationResult<IUserProfileData>> {
       try {
            const request = await profileClient.get<IUserProfileData>(`api/User/profile`);
            return {
                operation: {
                    isValid: true,
                    errorMessage: "",
                },
                responseData: request.data
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