import { fireEvent, render, screen, within } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import type { AttendanceLogDto, ClockInRequest } from '@/features/hris/types';
import { API_ERROR_CODES, ApiRequestError } from '@/lib/api/errors';
import { formatDateTime } from '@/lib/utils/format';
import { createSession, pageOf } from '@/test/fixtures';
import { mutationResult, queryResult } from '@/test/query';

import { AttendanceView } from './AttendanceView';

const api = vi.hoisted(() => ({
  useSession: vi.fn(),
  useGetAttendanceLogs: vi.fn(),
  useClockIn: vi.fn(),
}));

vi.mock('@/components/providers/SessionProvider', () => ({
  useSession: api.useSession,
}));

vi.mock('@/features/hris/api/useGetAttendanceLogs', () => ({
  useGetAttendanceLogs: api.useGetAttendanceLogs,
}));

vi.mock('@/features/hris/api/useClockIn', () => ({
  useClockIn: api.useClockIn,
}));

const EMPLOYEE_ID = '3f2504e0-4f89-41d3-9a0c-0305e82c3301';
const OTHER_EMPLOYEE_ID = '5c2b5e6e-6f4d-4c8b-9a1e-2d7c8b0f4321';
const CLOCK_OUT_AT = '2026-10-19T09:30:00Z';

const OPEN_LOG: AttendanceLogDto = {
  id: 'att-1',
  employeeId: EMPLOYEE_ID,
  clockInUtc: '2026-10-20T01:05:00Z',
  clockOutUtc: null,
  latitude: '-6.2088',
  longitude: '106.8456',
  photoUrl: null,
  status: 'ClockedIn',
};

const CLOSED_LOG: AttendanceLogDto = {
  id: 'att-2',
  employeeId: OTHER_EMPLOYEE_ID,
  clockInUtc: '2026-10-19T00:55:00Z',
  clockOutUtc: CLOCK_OUT_AT,
  latitude: '-6.9175',
  longitude: '107.6191',
  photoUrl: 'https://example.com/photo.jpg',
  status: 'ClockedOut',
};

/** Row-scoped access keeps cell assertions unambiguous. */
function rowFor(employeeId: string): HTMLElement {
  const row = screen.getByText(employeeId).closest('tr');

  if (!row) {
    throw new Error(`No table row was rendered for employee "${employeeId}"`);
  }

  return row;
}

beforeEach(() => {
  api.useSession.mockReturnValue(
    createSession({
      activeModules: [MODULE_CODES.hrisAttendance],
      permissions: [PERMISSIONS.hris.attendanceRead, PERMISSIONS.hris.attendanceClockIn],
    }),
  );
  api.useGetAttendanceLogs.mockReturnValue(queryResult({ data: pageOf([OPEN_LOG, CLOSED_LOG]) }));
  api.useClockIn.mockReturnValue(mutationResult());
});

describe('AttendanceView', () => {
  it('requests the canonical pagination and sort parameters', () => {
    render(<AttendanceView />);

    expect(api.useGetAttendanceLogs).toHaveBeenCalledWith({
      page: 1,
      pageSize: 10,
      sort: '-clockInUtc',
    });
  });

  it('renders logs with localized statuses, coordinates and clock-out placeholders', () => {
    render(<AttendanceView />);

    expect(screen.getByRole('table', { name: 'Attendance logs' })).toBeInTheDocument();

    const openRow = rowFor(EMPLOYEE_ID);

    expect(within(openRow).getByText('Clocked in')).toBeInTheDocument();
    expect(within(openRow).getByText(formatDateTime(OPEN_LOG.clockInUtc))).toBeInTheDocument();
    expect(within(openRow).getByText('-6.2088, 106.8456')).toBeInTheDocument();
    // An open shift renders an em dash for clock-out (and for its row actions).
    expect(within(openRow).getAllByText('—').length).toBeGreaterThan(0);

    const closedRow = rowFor(OTHER_EMPLOYEE_ID);

    expect(within(closedRow).getByText('Clocked out')).toBeInTheDocument();
    expect(within(closedRow).getByText(formatDateTime(CLOCK_OUT_AT))).toBeInTheDocument();
    expect(within(closedRow).getByText('-6.9175, 107.6191')).toBeInTheDocument();
  });

  it('re-queries from the first page when the sort changes', () => {
    render(<AttendanceView />);

    fireEvent.click(screen.getByRole('button', { name: 'Sort by Clock-in ascending' }));

    expect(api.useGetAttendanceLogs).toHaveBeenLastCalledWith({
      page: 1,
      pageSize: 10,
      sort: 'clockInUtc',
    });
  });

  it('posts a clock-in through the dialog with trimmed input and parsed coordinates', () => {
    const mutate = vi.fn((request: ClockInRequest, options?: { onSuccess?: () => void }) => {
      expect(request).toEqual({
        employeeId: EMPLOYEE_ID,
        latitude: -6.2088,
        longitude: 106.8456,
        photoUrl: null,
        attendanceMode: 'GPS_PHOTO',
        wifiSsid: 'Office-WiFi',
      });
      options?.onSuccess?.();
    });
    api.useClockIn.mockReturnValue(mutationResult({ mutate }));

    render(<AttendanceView />);

    fireEvent.click(screen.getByRole('button', { name: 'Clock in' }));

    const dialog = screen.getByRole('dialog');

    fireEvent.change(within(dialog).getByLabelText('Employee ID'), {
      target: { value: `  ${EMPLOYEE_ID}  ` },
    });
    fireEvent.change(within(dialog).getByLabelText('Latitude'), { target: { value: '-6.2088' } });
    fireEvent.change(within(dialog).getByLabelText('Longitude'), { target: { value: '106.8456' } });
    fireEvent.change(within(dialog).getByLabelText('WiFi SSID (optional)'), {
      target: { value: ' Office-WiFi ' },
    });
    fireEvent.click(within(dialog).getByRole('button', { name: 'Clock in' }));

    expect(mutate).toHaveBeenCalledTimes(1);
    expect(screen.getByText('Clocked in successfully.')).toBeInTheDocument();
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  it('blocks the dialog confirm until the payload is complete and in range', () => {
    const mutate = vi.fn();
    api.useClockIn.mockReturnValue(mutationResult({ mutate }));

    render(<AttendanceView />);

    fireEvent.click(screen.getByRole('button', { name: 'Clock in' }));

    const dialog = screen.getByRole('dialog');

    // Empty employee id and coordinates must not reach the API as `NaN`.
    fireEvent.click(within(dialog).getByRole('button', { name: 'Clock in' }));
    expect(mutate).not.toHaveBeenCalled();

    fireEvent.change(within(dialog).getByLabelText('Employee ID'), {
      target: { value: EMPLOYEE_ID },
    });
    fireEvent.change(within(dialog).getByLabelText('Latitude'), { target: { value: '91' } });
    fireEvent.change(within(dialog).getByLabelText('Longitude'), { target: { value: '106.8456' } });
    fireEvent.click(within(dialog).getByRole('button', { name: 'Clock in' }));

    expect(mutate).not.toHaveBeenCalled();
    expect(screen.getByRole('dialog')).toBeInTheDocument();
  });

  it('hides the clock-in action without the clock-in permission', () => {
    api.useSession.mockReturnValue(
      createSession({
        activeModules: [MODULE_CODES.hrisAttendance],
        permissions: [PERMISSIONS.hris.attendanceRead],
      }),
    );

    render(<AttendanceView />);

    expect(screen.queryByRole('button', { name: 'Clock in' })).not.toBeInTheDocument();
  });

  it('renders the loading skeleton while the log loads', () => {
    api.useGetAttendanceLogs.mockReturnValue(queryResult({ isLoading: true }));

    render(<AttendanceView />);

    expect(screen.getByRole('status')).toHaveTextContent('Loading content');
  });

  it('renders the empty state with a refresh CTA', () => {
    api.useGetAttendanceLogs.mockReturnValue(queryResult({ data: pageOf([]) }));

    render(<AttendanceView />);

    expect(screen.getByText('No attendance logs yet')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Refresh logs' })).toBeInTheDocument();
  });

  it('renders the error state with the traceId', () => {
    api.useGetAttendanceLogs.mockReturnValue(
      queryResult({
        error: new ApiRequestError({
          code: API_ERROR_CODES.unexpected,
          message: 'The attendance log could not be loaded.',
          status: 500,
          traceId: 'trace-attendance-500',
        }),
      }),
    );

    render(<AttendanceView />);

    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText('trace-attendance-500')).toBeInTheDocument();
  });

  it('renders forbidden when the API rejects the attendance read permission', () => {
    api.useGetAttendanceLogs.mockReturnValue(
      queryResult({
        error: new ApiRequestError({
          code: API_ERROR_CODES.forbidden,
          message: 'Forbidden.',
          status: 403,
          traceId: 'trace-attendance-403',
        }),
      }),
    );

    render(<AttendanceView />);

    expect(screen.getByText("You don't have access")).toBeInTheDocument();
  });
});
