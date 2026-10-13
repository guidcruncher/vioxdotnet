import { ref, onMounted, onUnmounted, computed } from 'vue'

export interface Breakpoints {
  sm: number
  md: number
  lg: number
  xl: number
  '2xl': number
}

const defaultBreakpoints: Breakpoints = {
  sm: 640,
  md: 768,
  lg: 1024,
  xl: 1280,
  '2xl': 1536,
}

export function useBreakpoint(customBreakpoints: Partial<Breakpoints> = {}) {
  const breakpoints = { ...defaultBreakpoints, ...customBreakpoints }

  const width = ref(typeof window !== 'undefined' ? window.innerWidth : 0)
  const height = ref(typeof window !== 'undefined' ? window.innerHeight : 0)

  const matches = ref({
    sm: false,
    md: false,
    lg: false,
    xl: false,
    '2xl': false,
  })

  let mediaQueryLists: Record<string, MediaQueryList> = {}
  let handlers: Record<string, (e: MediaQueryListEvent) => void> = {}

  const updateDimensions = () => {
    if (typeof window !== 'undefined') {
      width.value = window.innerWidth
      height.value = window.innerHeight
    }
  }

  onMounted(() => {
    updateDimensions()
    window.addEventListener('resize', updateDimensions)

    const keys = Object.keys(breakpoints) as (keyof Breakpoints)[]

    keys.forEach((key) => {
      const query = `(min-width: ${breakpoints[key]}px)`
      const mql = window.matchMedia(query)

      matches.value[key] = mql.matches

      const handler = (e: MediaQueryListEvent) => {
        matches.value[key] = e.matches
      }

      mql.addEventListener('change', handler)
      mediaQueryLists[key] = mql
      handlers[key] = handler
    })
  })

  onUnmounted(() => {
    window.removeEventListener('resize', updateDimensions)
    Object.keys(mediaQueryLists).forEach((key) => {
      mediaQueryLists[key].removeEventListener('change', handlers[key])
    })
  })

  const breakpoint = computed(() => {
    if (matches.value['2xl']) return '2xl'
    if (matches.value.xl) return 'xl'
    if (matches.value.lg) return 'lg'
    if (matches.value.md) return 'md'
    if (matches.value.sm) return 'sm'
    return 'xs'
  })

  const isXs = computed(() => breakpoint.value === 'xs')
  const isSm = computed(() => matches.value.sm)
  const isMd = computed(() => matches.value.md)
  const isLg = computed(() => matches.value.lg)
  const isXl = computed(() => matches.value.xl)
  const is2Xl = computed(() => matches.value['2xl'])

  const isSmallerThan = (key: keyof Breakpoints) => !matches.value[key]
  const isLargerThan = (key: keyof Breakpoints) => matches.value[key]

  return {
    width,
    height,
    breakpoint,
    isXs,
    isSm,
    isMd,
    isLg,
    isXl,
    is2Xl,
    isSmallerThan,
    isLargerThan,
  }
}
