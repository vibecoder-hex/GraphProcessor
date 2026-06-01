import axios from "axios";
import type { IOperationResult, IResponseOperationResult, IUserProfileData } from "@/models/interfacesAndTypes";

export interface IProfileRequests {
    getAccountData(): Promise<IResponseOperationResult<IUserProfileData>>
}

export class ProfileRequests implements IProfileRequests {
    private readonly _apiUrl: string
    private readonly _accessToken: string

    constructor(apiUrl: string, accessToken: string) {
        this._apiUrl = apiUrl,
        this._accessToken = accessToken
    }

    public async getAccountData(): Promise<IResponseOperationResult<IUserProfileData>> {
        try {
            const request = await axios.get(`${this._apiUrl}/profile`, { 
                headers: {
                    "Authorization": `Bearer ${this._accessToken}`
                } });
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