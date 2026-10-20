import { reactive, readonly } from 'vue'

interface AlertOptions {
  title?: string
  errorMessage?: string | object | null
  errorDetails?: any
  retryText?: string
  onRetry?: () => void
  onClose?: () => void
}

interface AlertState {
  isOpen: boolean
  title: string
  errorMessage: string
  errorDetails: any
  showRetry: boolean
  retryText: string
  onRetry: (() => void) | null
  onClose: (() => void) | null
}

const state = reactive<AlertState>({
  isOpen: false,
  title: 'An Error Occurred',
  errorMessage: '',
  errorDetails: null,
  showRetry: false,
  retryText: 'Try Again',
  onRetry: null,
  onClose: null,
})

export function useAlert() {
  const showError = (options: AlertOptions = {}) => {
    let message = options.errorMessage || ''
    if (message !== null && typeof message === 'object') {
      message = JSON.stringify(message)
    }
    state.title = options.title || 'An Error Occurred'
    state.errorMessage = message
    state.errorDetails = options.errorDetails || null
    state.showRetry = !!options.onRetry
    state.retryText = options.retryText || 'Try Again'
    state.onRetry = options.onRetry || null
    state.onClose = options.onClose || null
    state.isOpen = true
  }

  const closeError = () => {
    state.isOpen = false
    if (state.onClose) state.onClose()
  }

  const handleRetry = () => {
    if (state.onRetry) {
      state.onRetry()
    }
    state.isOpen = false
  }

  return {
    state: readonly(state),
    showError,
    closeError,
    handleRetry,
  }
}
