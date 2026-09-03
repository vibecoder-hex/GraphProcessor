import type { IResponseOperationResult, ILoginObject, IRegisterObject, IAuthenticationResultObject, IJwtPayloadComponent } from "@/models/interfacesAndTypes";
import axios, {type AxiosInstance} from "axios";
import { ApiClientConfigurator } from "@/services/httpServices/ApiClientConfigurator.ts";

export interface ILoginRequests {
    login() : Promise<IResponseOperationResult<IAuthenticationResultObject>>
    logout(accessToken: string): Promise<void>
}

export interface IRegistrationRequests {
    register(): Promise<IResponseOperationResult<IAuthenticationResultObject>>
}

export class LoginRequests implements ILoginRequests {
    private readonly _loginClient: AxiosInstance
    private readonly _apiUrl: string
    private readonly _username: string
    private readonly _password: string

    constructor(apiUrl: string, username: string = "", password: string = "") {
        this._apiUrl = apiUrl
        this._username = username
        this._password = password
        const apiConfigurator = new ApiClientConfigurator(this._apiUrl)
        this._loginClient = apiConfigurator.getInstance()
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
            const response = await this._loginClient.post(`login`, loginObject);
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
    public async logout(): Promise<void> {
        await this._loginClient.get(`${this._apiUrl}/logout`);
    }
}

export class RegistrationRequests implements IRegistrationRequests {
    private readonly _registerClient: AxiosInstance
    private readonly _apiUrl: string
    private readonly _userDataObject: IRegisterObject
    
    constructor(apiUrl: string, userDataObject: IRegisterObject) {
        this._apiUrl = apiUrl
        this._userDataObject = userDataObject
        const apiConfigurator = new ApiClientConfigurator(this._apiUrl)
        this._registerClient = apiConfigurator.getInstance()
    }
    
    public async register(): Promise<IResponseOperationResult<IAuthenticationResultObject>> {
        try {
            const response = await this._registerClient.post(`register`, this._userDataObject);
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