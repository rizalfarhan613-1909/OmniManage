/**
 * Standard response envelope — Rules.md §5.4.
 * Every API response (including 4xx/5xx) uses this shape.
 */
export interface ApiErrorDetail {
  field?: string;
  code: string;
  message: string;
}

export interface ApiError {
  code: string;
  message: string;
  details?: ApiErrorDetail[];
}

export interface PaginationMeta {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface ApiMeta {
  traceId: string;
  timestamp: string;
  pagination?: PaginationMeta;
}

export type ApiResponse<T> =
  | { success: true; data: T; error: null; meta: ApiMeta }
  | { success: false; data: null; error: ApiError; meta: ApiMeta };

export type ApiSuccess<T> = Extract<ApiResponse<T>, { success: true }>;
