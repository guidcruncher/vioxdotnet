export function useAlert(message: any) {
  const isDevelopment = import.meta.env.VITE_DEVELOPMENT ?? 'false'

  if (!isDevelopment) {
    console.log(message)
    return
  }

  if (message !== null && typeof message === 'object') {
    alert(JSON.stringify(message))
    return
  }

  alert(message)
}
