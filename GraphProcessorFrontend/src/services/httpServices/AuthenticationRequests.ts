import type { IResponseOperationResult, ILoginObject, IRegisterObject, IAuthenticationResultObject, IJwtPayloadComponent } from "@/models/interfacesAndTypes";
import axios, {type AxiosInstance} from "axios";
import { ApiClientConfigurator, ErrorHandler } from "@/services/httpServices/ApiClientConfigurator.ts";

export interface ILoginRequests {
    login() : Promise<IResponseOperationResult<IAuthenticationResultObject>>
    logout(accessToken: string): Promise<void>
}

export interface IRegistrationRequests {
    register(): Promise<IResponseOperationResult<IAuthenticationResultObject>>
}

export class LoginRequests implements ILoginRequests {
    private readonly _loginClient: AxiosInstance
    private readonly _username: string
    private readonly _password: string

    constructor(username: string = "", password: string = "") {
        this._username = username
        this._password = password
        const apiInstance = ApiClientConfigurator.getInstance()
        this._loginClient = apiInstance.getClient()
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
            const response = await this._loginClient.post(`api/User/login`, loginObject);
            return {
                operation : {
                    isValid: true,
                    errorMessage: ""
                }, 
                responseData: response.data
            }
        } catch(error) {
            return {
                operation : {
                    isValid: true,
                    errorMessage: ErrorHandler.handleError(error)
                },
                responseData: null
            }
        }
    }
    public async logout(): Promise<void> {
        try {
            await this._loginClient.get(`logout`);
        } catch (error) {
            console.error(ErrorHandler.handleError(error));
        }
    }
}

export class RegistrationRequests implements IRegistrationRequests {
    private readonly _registerClient: AxiosInstance
    private readonly _userDataObject: IRegisterObject
    
    constructor(apiUrl: string, userDataObject: IRegisterObject) {
        this._userDataObject = userDataObject
        const apiInstance = ApiClientConfigurator.getInstance()
        this._registerClient = apiInstance.getClient()
    }
    
    public async register(): Promise<IResponseOperationResult<IAuthenticationResultObject>> {
        try {
            const response = await this._registerClient.post(`api/User/register`, this._userDataObject);
            return {
                operation : {
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