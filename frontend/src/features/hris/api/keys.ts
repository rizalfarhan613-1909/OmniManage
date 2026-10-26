import type { ListQueryParams } from '@/lib/api/pagination';
import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

import type { AttendanceStatus, PayrollStatus } from '../types';

export interface AttendanceLogQueryParams extends ListQueryParams {
  employeeId?: string;
  status?: AttendanceStatus;
  fromUtc?: string;
  toUtc?: string;
}

export interface PayrollRunQueryParams extends ListQueryParams {
  periodCode?: string;
  status?: PayrollStatus;
}

export interface EmployeeQueryParams extends ListQueryParams {
  departmentId?: string;
  isActive?: boolean;
}

export const hrisKeys = {
  all: [QUERY_KEY_ROOTS.hris] as const,
  attendanceLogs: () => [...hrisKeys.all, 'attendance-logs'] as const,
  attendanceLogList: (params: AttendanceLogQueryParams) =>
    [...hrisKeys.attendanceLogs(), 'list', params] as const,
  payrollRuns: () => [...hrisKeys.all, 'payroll-runs'] as const,
  payrollRunList: (params: PayrollRunQueryParams) =>
    [...hrisKeys.payrollRuns(), 'list', params] as const,
  employees: () => [...hrisKeys.all, 'employees'] as const,
  employeeList: (params: EmployeeQueryParams) => [...hrisKeys.employees(), 'list', params] as const,
};
