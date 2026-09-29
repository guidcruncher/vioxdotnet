import { ref, onMounted, onUnmounted, readonly, type DeepReadonly, type Ref } from 'vue'

export interface UseQueryParamsOptions {
  /**
   * Automatically update query parameters when browser history changes via popstate.
   * @default true
   */
  listenToPopState?: boolean
}

/**
 * Utility function to convert URLSearchParams into a key-value record.
 */
function parseQueryString(searchString: string): Record<string, string> {
  const params = new URLSearchParams(searchString)
  const result: Record<string, string> = {}

  params.forEach((value, key) => {
    result[key] = value
  })

  return result
}

/**
 * Vue 3 Composable to parse current URL query string parameters into a Record<string, string>.
 *
 * @param options - Configuration options for tracking URL state changes.
 * @returns Readonly reactive Ref containing key-value query parameters.
 */
export function useQueryParams(
  options: UseQueryParamsOptions = {}
): DeepReadonly<Ref<Record<string, string>>> {
  const { listenToPopState = true } = options

  const getCurrentParams = (): Record<string, string> => {
    if (typeof window === 'undefined') {
      return {}
    }
    return parseQueryString(window.location.search)
  }

  const queryParams = ref<Record<string, string>>(getCurrentParams())

  const handleUrlChange = (): void => {
    queryParams.value = getCurrentParams()
  }

  if (listenToPopState) {
    onMounted(() => {
      window.addEventListener('popstate', handleUrlChange)
    })

    onUnmounted(() => {
      window.removeEventListener('popstate', handleUrlChange)
    })
  }

  return readonly(queryParams)
}
