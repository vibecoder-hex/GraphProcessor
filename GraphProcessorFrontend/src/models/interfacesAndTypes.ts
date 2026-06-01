export type allPossibleDataType = string | number | boolean | string[] | number[] | boolean[] | null
export type Algorithm = "bfs" | "dfs" | "dijkstra"
export type GraphType = "oriented" | "non-oriented"

export interface IDistanceProcessingResultObject {
    algorithm: string,
    startVertex: string,
    shortestPath: string[],
    timeNs: number,
    [anotherProperties: string]: allPossibleDataType
}

export interface IDistanceProcessingRootObject {
    result: IDistanceProcessingResultObject
}

export interface IGraphParametersObject {
    Distances: IDistanceStructureObject
}

export interface IDistanceStructureObject {
        [vertex: string]: {
            [neighbor: string]: number
    }
}

export interface IAuthenticationResultObject {
    tokenString: string
}

export interface IOperationResult {
    isValid: boolean,
    errorMessage: string,
}

export interface IResponseOperationResult<T> {
    operation: IOperationResult,
    responseData: T | null
}

export interface ILoginObject {
    username: string,
    password: string
}

export interface IJwtPayloadComponent {
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": string,
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": string,
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": string,
    exp: number,
    iss: string,
    aud: string
    }