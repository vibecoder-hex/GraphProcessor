import type { IResponseOperationResult, ILoginObject, IRegisterObject, IAuthenticationResultObject, IJwtPayloadComponent } from "@/models/interfacesAndTypes";
import axios, {type AxiosInstance} from "axios";
import { ApiClientConfigurator, ErrorHandler } from "@/services/httpServices/ApiClientConfigurator.ts";

export interface IRegistrationRequests {
    register(): Promise<IResponseOperationResult<IAuthenticationResultObject>>
}

export class LoginRequests {
    private static getLoginObject(username: string, password: string): ILoginObject {
        return {
            username: username,
            password: password
        }
    }
    

    public static async login(loginClient: AxiosInstance, username: string, password: string): Promise<IResponseOperationResult<IAuthenticationResultObject>> {
        const loginObject: ILoginObject = this.getLoginObject(username, password);
        try {
            const response = await loginClient.post<IAuthenticationResultObject>(`api/User/login`, loginObject);
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
    public static async logout(loginClient: AxiosInstance): Promise<void> {
        try {
            await loginClient.get(`api/User/logout`);
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