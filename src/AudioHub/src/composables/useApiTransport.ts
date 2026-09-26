import { ref, type Ref } from 'vue'

export interface TransportOptions extends RequestInit {
  params?: Record<string, any>
}

export interface ApiState<T> {
  data: Ref<T | null>
  error: Ref<Error | null>
  loading: Ref<boolean>
}

export function useApiTransport(baseUrl: string = '', debug: boolean = true) {
  async function request<T>(endpoint: string, options: TransportOptions = {}): Promise<T> {
    const { params, headers, ...customConfig } = options

    // Determine target host base URL safely
    const effectiveBaseUrl =
      baseUrl.trim() !== '' ? baseUrl : `${window.location.protocol}//${window.location.host}`

    // Clean up slash collisions between baseUrl and endpoint
    const cleanBase = effectiveBaseUrl.replace(/\/+$/, '')
    const cleanEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`

    let targetUrl = `${cleanBase}${cleanEndpoint}`

    // Append query parameters safely
    if (params) {
      const searchParams = new URLSearchParams()
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          searchParams.append(key, String(value))
        }
      })
      const queryString = searchParams.toString()
      if (queryString) {
        const separator = targetUrl.includes('?') ? '&' : '?'
        targetUrl += `${separator}${queryString}`
      }
    }

    // Preserve custom HTTP method if passed (e.g. DELETE, PUT, PATCH)
    const method = options.method ?? (options.body ? 'POST' : 'GET')

    const config: RequestInit = {
      method,
      headers: {
        'Content-Type': 'application/json',
        ...headers,
      },
      ...customConfig,
    }

    const response = await fetch(targetUrl, config)

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}))
      const errorMessage = errorData.detail || errorData.title || response.statusText
      console.error(`${targetUrl}\n ${errorMessage || `HTTP Error: ${response.status}`}`)
      if (debug) {
        alert(`${targetUrl}\n ${errorMessage || `HTTP Error: ${response.status}`}`)
      }
      throw new Error(errorMessage || `HTTP Error: ${response.status}`)
    }

    if (response.status === 204) {
      return {} as T
    }

    return response.json()
  }

  function createApiState<T>() {
    const data = ref<T | null>(null) as Ref<T | null>
    const error = ref<Error | null>(null) as Ref<Error | null>
    const loading = ref<boolean>(false)

    const execute = async (apiCall: () => Promise<T>): Promise<T | null> => {
      loading.value = true
      error.value = null
      try {
        const result = await apiCall()
        data.value = result
        return result
      } catch (err) {
        error.value = err instanceof Error ? err : new Error(String(err))
        return null
      } finally {
        loading.value = false
      }
    }

    return { data, error, loading, execute }
  }

  return {
    request,
    createApiState,
  }
}
