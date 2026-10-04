export type Sources = Record<string, Record<string, string>>

export interface ProblemDetails {
  type?: string | null
  title?: string | null
  status?: number | string | null
  detail?: string | null
  instance?: string | null
}

export interface ValidationProblemDetails extends ProblemDetails {
  errors?: Record<string, string[]>
}

export interface PagedList<T> {
  offset?: number | string
  pageNumber?: number | string
  totalPages?: number | string

  items?: T[] | null

  limit?: number | string
  totalCount?: number | string

  hasPreviousPage?: boolean
  hasNextPage?: boolean
}
