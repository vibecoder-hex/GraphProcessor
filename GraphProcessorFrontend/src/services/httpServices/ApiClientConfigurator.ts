import axios, { type AxiosInstance, type InternalAxiosRequestConfig } from 'axios'
import { useAuthenticationStore } from "@/stores";
import type { IJwtPayloadComponent } from "@/models/interfacesAndTypes.ts";

export interface IApiClientConfigurator {
    getInstance(): AxiosInstance
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
        return expire.getTime() >= Date.now();

    }
}

export class ApiClientConfigurator implements IApiClientConfigurator {
    private readonly _apiUrl: string;
    private readonly _instance: AxiosInstance;
    private readonly _accessToken: string | null;
    
    constructor (apiUrl: string) {
        this._apiUrl = apiUrl;
        this._instance = axios.create({
            baseURL: apiUrl,
            withCredentials: true,
            
        });
        const authStore = useAuthenticationStore();
        this._accessToken = authStore.token;
        this.setupInterceptors()
    }
    
    public getInstance(): AxiosInstance {
        return this._instance;
    }
    
    private setupInterceptors() {
        this._instance.interceptors.request.use((config: InternalAxiosRequestConfig) => {
            if (this._accessToken) {
                config.headers.Authorization = `Bearer ${this._accessToken}`;
            }
            return config;
        });
    }
}