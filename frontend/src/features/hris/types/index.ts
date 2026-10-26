/**
 * Wire types for the HRIS module (Phase 6).
 *
 * Mirrors the presentation contracts and domain aggregates on the backend.
 * Following Rules.md §7, decimal values travel as fixed-scale strings while
 * dates use ISO-8601 (`DateOnly` values are `yyyy-MM-dd`).
 */

/** Mirrors `AttendanceStatus`. */
export type AttendanceStatus = 'ClockedIn' | 'ClockedOut';

/** Mirrors `PayrollStatus`. */
export type PayrollStatus = 'Draft' | 'Finalized' | 'Cancelled';

/** `POST /api/v1/hris/attendance/clock-in` request body. */
export interface ClockInRequest {
  employeeId: string;
  latitude: number;
  longitude: number;
  photoUrl?: string | null;
  /** Defaults to `GPS_PHOTO` on the backend when omitted. */
  attendanceMode?: string | null;
  wifiSsid?: string | null;
}

/**
 * Command payload for `POST /api/v1/hris/payroll/runs/{id}/finalize`.
 *
 * The identifier is carried in the route; no request body is sent.
 */
export interface PayrollFinalizeRequest {
  payrollRunId: string;
}

/** Mirrors `SmartAttendanceLog`. */
export interface AttendanceLogDto {
  id: string;
  employeeId: string;
  clockInUtc: string;
  clockOutUtc: string | null;
  latitude: string;
  longitude: string;
  photoUrl: string | null;
  status: AttendanceStatus;
}

/** Mirrors `PayrollRun`. */
export interface PayrollRunDto {
  id: string;
  periodCode: string;
  processedAtUtc: string | null;
  totalBasicSalary: string;
  totalAllowances: string;
  totalDeductions: string;
  netPayTotal: string;
  status: PayrollStatus;
}

/** Mirrors `Employee`. */
export interface EmployeeDto {
  id: string;
  employeeCode: string;
  fullName: string;
  email: string;
  jobTitle: string;
  departmentId: string;
  joinedDate: string;
  isActive: boolean;
}
