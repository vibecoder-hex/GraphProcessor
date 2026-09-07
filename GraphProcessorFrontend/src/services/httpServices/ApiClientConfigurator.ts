import axios, { type AxiosInstance, type InternalAxiosRequestConfig, AxiosError } from 'axios'
import { useAuthenticationStore } from "@/stores";
import type { IJwtPayloadComponent, IBadRequestBody, IAuthenticationResultObject } from "@/models/interfacesAndTypes.ts";


export interface IApiClientConfigurator {
    getClient(): AxiosInstance
    
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

export class ErrorHandler {
    private static handleBadRequest(error: AxiosError<IBadRequestBody>): string {
        const errorTitle = error.response?.data.title
        const errorObj = error.response?.data.errors
        if (errorTitle && errorObj) {
            const errorMessages: string = Object.entries(errorObj)
                .map(([key, value]) => `${key}: ${value.join(', ')}`)
                .join('; ')
            console.error(errorMessages)
            return `${errorTitle}: ${errorMessages}`
        }
        return "Validation error"
    }
    
    public static handleError(error: AxiosError | unknown): string {
        if (axios.isAxiosError(error)) {
            const errorStatus = error.response?.status;
            if (axios.isAxiosError<IBadRequestBody>(error) && errorStatus === 400) {
                return this.handleBadRequest(error)
            }
            if (errorStatus === 403) {
                return "Access denied error"
            }
            if (errorStatus === 500) {
                return "Internal Server Error"
            }
            if (errorStatus === 404) {
                return "Not Found error"
            }
        }
        return `${error}`;
    }
}

export class ApiClientConfigurator implements IApiClientConfigurator {
    private readonly _instance: AxiosInstance;
    private static _clientInstance: ApiClientConfigurator | null = null
    
    constructor () {
        this._instance = axios.create({
            withCredentials: true
        });
        this.setupAccessToken();
        this.tokenRefresher();
    }
    
    public static getInstance(): ApiClientConfigurator {
        if (!ApiClientConfigurator._clientInstance) {
            ApiClientConfigurator._clientInstance = new ApiClientConfigurator()
        }
        return ApiClientConfigurator._clientInstance;
    }
    public getClient(): AxiosInstance {
        return this._instance;
    }
    
    private tokenRefresher() {
        let isRefreshing: boolean = false;
        let failedQueue: { 
            resolve: (value: unknown) => void;
            reject: (reason?: any) => void;
        }[] = [];
        
        const processQueue = (error: AxiosError | null, token: string | null) => {
            failedQueue.forEach((prom: { resolve: (value: unknown) => void; reject: (reason?: any) => void; }) => {
                if (error) {
                    prom.reject(error);
                } else {
                    prom.resolve(token);
                }
            })
            failedQueue = []
        }
        
        this._instance.interceptors.response.use(
            (response) => {
                console.log('refresh response')
                return response;
            },
            async (error) => { // НЕ ВЫЗЫВАЕТСЯ хер знает почему
                const authStore = useAuthenticationStore();
                const originalRequest = error.config;
                if (error.response?.status === 401 && !originalRequest._retry) {
                    if (isRefreshing) {
                        return new Promise((resolve, reject) => {
                            failedQueue.push({ resolve, reject });
                        }).then((token) => {
                            originalRequest.headers["Authorization"] = `Bearer ${token}`
                            return this._instance(originalRequest)
                        }).catch((err) => Promise.reject(err));
                    }
                    originalRequest._retry = true;
                    isRefreshing = true;
                    
                    try {
                        const refreshRequest = await axios.get(`api/User/refresh`, {withCredentials: true});
                        const newToken: IAuthenticationResultObject = refreshRequest.data
                        console.log(newToken);
                        authStore.setToken(newToken.tokenString)
                        this._instance.defaults.headers.common["Authorization"] = `Bearer ${newToken}`
                        processQueue(null, newToken.tokenString)
                        return this._instance(originalRequest)
                    } catch (refreshError) {
                        if (axios.isAxiosError(refreshError)) {
                            processQueue(refreshError, null)
                            authStore.deleteToken()
                            console.log("refresh error", refreshError)
                            return Promise.reject(refreshError)
                        }

                    } finally {
                        isRefreshing = false;
                    }
                }
                return Promise.reject(error)
            }
        )
    }
    
    private setupAccessToken() {
        this._instance.interceptors.request.use(
            (config: InternalAxiosRequestConfig) => {
                const authStore = useAuthenticationStore();
                if (authStore.token) {
                    config.headers.Authorization = `Bearer ${authStore.token}`;
                }
                return config;
        }
        );
    }
}