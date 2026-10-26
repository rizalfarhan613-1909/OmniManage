import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import type { PagedResult } from '@/lib/api/pagination';
import { toQueryString } from '@/lib/api/pagination';
import type { QueryHookOptions } from '@/lib/api/query-keys';

import type { AttendanceLogDto } from '../types';
import { hrisKeys, type AttendanceLogQueryParams } from './keys';

/** `GET /api/v1/hris/attendance` — paginated smart-attendance log. */
export function useGetAttendanceLogs(
  params: AttendanceLogQueryParams = {},
  options: QueryHookOptions = {},
) {
  return useQuery({
    queryKey: hrisKeys.attendanceLogList(params),
    queryFn: ({ signal }) =>
      apiClient
        .get<PagedResult<AttendanceLogDto>>(`/api/v1/hris/attendance${toQueryString(params)}`, {
          signal,
        })
        .then((response) => response.data),
    enabled: options.enabled ?? true,
  });
}
