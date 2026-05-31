import type { IResponseOperationResult, ILoginObject, IAuthenticationResultObject, IJwtPayloadComponent } from "@/models/interfacesAndTypes";
import axios from "axios";


export interface ILoginRequests {
    login() : Promise<IResponseOperationResult<IAuthenticationResultObject>>
}

export interface IRegistrationRequests {
    register(): Promise<IResponseOperationResult<IAuthenticationResultObject>>
}

export class TokenProvider  {

    private static decodeJsonWebToken(webToken: string): IJwtPayloadComponent | null {
        const BASE64_SYMBOL_COUNT = 4;
        const tokenParts: string[] = webToken.split('.')
        if (tokenParts.length !== 3) {
            throw new Error("Invalid token parts count")
        }

        let payloadPart: string | undefined = tokenParts[1]?.replace(/_/g, '+').replace(/-/g, '/');
        if (payloadPart !== undefined) {
            const remainder: number = payloadPart.length % BASE64_SYMBOL_COUNT
            switch (remainder) {
                case 3:
                    payloadPart += "="
                    break;
                case 2:
                    payloadPart += "=="
                    break;
                case 0:
                    break;
            }
            const decodedString: string = atob(payloadPart)
            const payloadObject: IJwtPayloadComponent = JSON.parse(decodedString)
            return payloadObject
        }
        return null
    }

    public static setToken(webToken: string): void {
        localStorage.setItem("token", webToken)
    }

    public static getToken(): string | null {
        return localStorage.getItem("token");
    }
    
    public static removeToken(): void {
        localStorage.removeItem("token")
    }

    public static getUserId(webToken: string): number | undefined {
        return this.decodeJsonWebToken(webToken)?.userId
    }

    public static getUsername(webToken: string): string | undefined {
        return this.decodeJsonWebToken(webToken)?.username
    }

    public static getUserRole(webToken: string): string | undefined {
        return this.decodeJsonWebToken(webToken)?.userRole
    }

    public static getTokenExpire(webToken: string): Date | undefined {
        const tokenExpire = this.decodeJsonWebToken(webToken)?.expire
        if (tokenExpire !== undefined) {
            return new Date(tokenExpire * 1000);
        }
        return undefined;
    }

    public static isTokenValid(webToken: string): boolean {
        if (!webToken)
            return false
        const expire = this.getTokenExpire(webToken)
        if (!expire)
            return false
        if (expire.getTime() < Date.now())
            return false
        return true
    }
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