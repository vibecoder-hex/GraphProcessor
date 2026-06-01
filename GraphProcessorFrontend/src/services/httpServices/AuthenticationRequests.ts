import type { IResponseOperationResult, ILoginObject, IAuthenticationResultObject, IJwtPayloadComponent } from "@/models/interfacesAndTypes";
import axios from "axios";


export interface ILoginRequests {
    login() : Promise<IResponseOperationResult<IAuthenticationResultObject>>
}

export interface IRegistrationRequests {
    register(): Promise<IResponseOperationResult<IAuthenticationResultObject>>
}

export interface ITokenProcessor {
    getUserId(): string | undefined,
    getUsername(): string | undefined,
    getUserRole(): string | undefined,
    getTokenExpire(): Date | undefined,
    isTokenValid(): boolean | undefined
}

export class TokenProcessor implements ITokenProcessor  {
    private readonly _webToken: string

    constructor(webToken: string) {
        this._webToken = webToken
    }

    private decodeJsonWebToken(): IJwtPayloadComponent | null {
        const BASE64_SYMBOL_COUNT = 4;
        const tokenParts: string[] = this._webToken.split('.')
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
            try {
                const decodedString: string = atob(payloadPart)
                const payloadObject: IJwtPayloadComponent = JSON.parse(decodedString)
                return payloadObject
            } catch (error) {
                console.error("failed to parse/decode token", error)
            }

        }
        return null
    }

    public getUserId(): string | undefined {
        return this.decodeJsonWebToken()?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]
    }

    public getUsername(): string | undefined {
        return this.decodeJsonWebToken()?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]
    }

    public getUserRole(): string | undefined {
        return this.decodeJsonWebToken()?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]
    }

    public getTokenExpire(): Date | undefined {
        const tokenExpire = this.decodeJsonWebToken()?.exp
        if (tokenExpire !== undefined) {
            return new Date(tokenExpire * 1000);
        }
        return undefined;
    }

    public isTokenValid(): boolean {
        const expire = this.getTokenExpire()
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