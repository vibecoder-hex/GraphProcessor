import axios, {type AxiosInstance} from "axios";
import type { IOperationResult, IResponseOperationResult, IUserProfileData } from "@/models/interfacesAndTypes";
import {ApiClientConfigurator, ErrorHandler} from "@/services/httpServices/ApiClientConfigurator.ts";

export interface IProfileRequests {
    getAccountData(): Promise<IResponseOperationResult<IUserProfileData>>
}

export class ProfileRequests implements IProfileRequests {
    private readonly _profileClient: AxiosInstance

    constructor() {
        const apiInstance = ApiClientConfigurator.getInstance()
        this._profileClient = apiInstance.getClient()
    }

    public async getAccountData(): Promise<IResponseOperationResult<IUserProfileData>> {
       try {
            const request = await this._profileClient.get(`api/User/profile`);
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