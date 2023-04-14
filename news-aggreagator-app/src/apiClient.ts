import {ApiNews} from "./App";

class ApiClient {
    private hostName

    constructor(hostName: string) {
        this.hostName = hostName
    }

    public async GetNews(keywords: string[] | null, take: number, skip: number, reverseDateOrder: boolean, leftBorder: Date | null, rightBorder: Date | null): Promise<ApiNews[] | null> {
        const keywordsParam = keywords === null || keywords.length === 0 ? "" : `keywords=${keywords.join(',')}&`
        const leftBorderParam = leftBorder === null ? "" : `&leftBorder=${leftBorder.toLocaleDateString()}`
        const rightBorderParam = rightBorder === null ? "" : `&rightBorder=${rightBorder.toLocaleDateString()}`
        const query = `/news?${keywordsParam}take=${take}&skip=${skip}&reverseOrder=${reverseDateOrder}${leftBorderParam}${rightBorderParam}`
        return await this.fetch<ApiNews[]>(query)
    }

    public async GetKeywords(): Promise<string[] | null> {
        const query = `/news/getKeywords`
        return await this.fetch<string[]>(query)
    }

    private async fetch<T>(query: string): Promise<T | null> {
        return await fetch(this.hostName + query).then(response => {
            if (!response.ok) {
                throw new Error(response.statusText)
            }
            return response.json().then(res => res as T)
        }).catch(e => {
            console.log(`Error occurred during request to ${query}: ${e} `)
            return null
        })
    }
}

export {ApiClient}