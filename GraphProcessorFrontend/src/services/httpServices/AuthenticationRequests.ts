import type { IResponseOperationResult, ILoginObject, IRegisterObject, IAuthenticationResultObject, IJwtPayloadComponent } from "@/models/interfacesAndTypes";
import { type AxiosInstance } from "axios";
import {  ErrorHandler } from "@/services/httpServices/ApiClientConfigurator.ts";

export class LoginRequests {

    public static async login(loginClient: AxiosInstance, loginObject: ILoginObject): Promise<IResponseOperationResult<IAuthenticationResultObject>> {
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

export class RegistrationRequests {
    public static async register(registerClient: AxiosInstance, userDataObject: IRegisterObject): Promise<IResponseOperationResult<IAuthenticationResultObject>> {
        try {
            const response = await registerClient.post<IAuthenticationResultObject>(`api/User/register`, userDataObject);
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