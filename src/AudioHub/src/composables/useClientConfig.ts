import { ref, readonly, type Ref, type DeepReadonly } from 'vue'
import type { ClientConfiguration } from '@/types'
import { useApiClient } from '@/composables/useApiClient'

export interface UseClientConfigOptions {
  /**
   * If true, automatically fetches the configuration when the composable is initialized.
   * @default false
   */
  immediate?: boolean
}

export interface UseClientConfigReturn {
  config: DeepReadonly<Ref<ClientConfiguration | null>>
  isLoading: DeepReadonly<Ref<boolean>>
  error: DeepReadonly<Ref<Error | null>>
  fetchConfig: () => Promise<ClientConfiguration | null>
  updateConfig: (newConfig: ClientConfiguration) => Promise<boolean>
}

export function useClientConfig(options: UseClientConfigOptions = {}): UseClientConfigReturn {
  const { immediate = false } = options

  const apiClient = useApiClient()

  const config = ref<ClientConfiguration | null>(null)
  const isLoading = ref<boolean>(false)
  const error = ref<Error | null>(null)

  const fetchConfig = async (): Promise<ClientConfiguration | null> => {
    isLoading.value = true
    error.value = null

    try {
      const data = await apiClient.config.get()
      config.value = data
      return data
    } catch (err) {
      const normalizedError = err instanceof Error ? err : new Error(String(err))
      error.value = normalizedError
      return null
    } finally {
      isLoading.value = false
    }
  }

  const updateConfig = async (newConfig: ClientConfiguration): Promise<boolean> => {
    isLoading.value = true
    error.value = null

    try {
      await apiClient.config.update(newConfig)
      config.value = newConfig
      return true
    } catch (err) {
      const normalizedError = err instanceof Error ? err : new Error(String(err))
      error.value = normalizedError
      return false
    } finally {
      isLoading.value = false
    }
  }

  if (immediate) {
    void fetchConfig()
  }

  return {
    config: readonly(config),
    isLoading: readonly(isLoading),
    error: readonly(error),
    fetchConfig,
    updateConfig,
  }
}
