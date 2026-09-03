import axios, {type AxiosInstance} from "axios";
import type { IOperationResult, IResponseOperationResult, IUserProfileData } from "@/models/interfacesAndTypes";
import { ApiClientConfigurator } from "@/services/httpServices/ApiClientConfigurator.ts";

export interface IProfileRequests {
    getAccountData(): Promise<IResponseOperationResult<IUserProfileData>>
}

export class ProfileRequests implements IProfileRequests {
    private readonly _profileClient: AxiosInstance
    private readonly _apiUrl: string

    constructor(apiUrl: string) {
        this._apiUrl = apiUrl;
        const apiConfigurator = new ApiClientConfigurator(this._apiUrl);
        this._profileClient = apiConfigurator.getInstance()
    }

    public async getAccountData(): Promise<IResponseOperationResult<IUserProfileData>> {
        try {
            const request = await this._profileClient.get(`profile`);
            return {
                operation: {
                    isValid: true,
                    errorMessage: "",
                },
                responseData: request.data
            }
        } catch (error) {
            if (axios.isAxiosError(error)) {
                return {
                    operation : {
                        isValid: false,
                        errorMessage: `Error: ${error.response?.data.error}`
                    },
                    responseData: null
                }
            } else {
                return {
                    operation: {
                        isValid: false,
                        errorMessage: `Error: ${error}`
                    },
                    responseData: null
                }
            }
        }
    }
}