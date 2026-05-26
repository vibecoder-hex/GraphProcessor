import type { IResponseOperationResult, ILoginObject, IAuthenticationResultObject } from "@/models/interfacesAndTypes";
import axios from "axios";

export interface ILoginRequests {
    login() : Promise<IResponseOperationResult<IAuthenticationResultObject>>
}

export interface IRegistrationRequests {
    register(): Promise<IResponseOperationResult<IAuthenticationResultObject>>
}

export class LoginRequests implements ILoginRequests {
    private readonly _apiUrl: string
    private readonly _username: string
    private readonly _password: string

    constructor(apiUrl: string, username: string, password: string) {
        this._apiUrl = apiUrl
        this._username = username
        this._password = password
    }

    private getLoginObject(): ILoginObject {
        return {
            username: this._username,
            password: this._password
        }
    }

    public async login(): Promise<IResponseOperationResult<IAuthenticationResultObject>> {
        const loginObject: ILoginObject = this.getLoginObject();
        try {
            const response = await axios.post(`${this._apiUrl}/login`, loginObject);
            return {
                operation : {
                    isValid: true,
                    errorMessage: ""
                }, 
                responseData: response.data
            }
        } catch(error) {
            if (axios.isAxiosError(error)) {
                return {
                    operation : {
                        isValid: false,
                        errorMessage: `Error : ${error.response?.data.error}`
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