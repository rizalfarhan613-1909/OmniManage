'use client';

import { Plus, Clock } from 'lucide-react';
import { useState } from 'react';

import { useSession } from '@/components/providers/SessionProvider';
import { DataTable, type DataTableColumn } from '@/components/shared/DataTable';
import { PageHeader } from '@/components/shared/PageHeader';
import { QuerySection } from '@/components/shared/QuerySection';
import { TableSkeleton } from '@/components/shared/Skeletons';
import { StatusBadge, type StatusTone } from '@/components/shared/StatusBadge';
import { Button } from '@/components/ui/Button';
import { PERMISSIONS } from '@/config/permissions';
import { useGetAttendanceLogs } from '@/features/hris/api/useGetAttendanceLogs';
import { useClockIn } from '@/features/hris/api/useClockIn';
import type { AttendanceLogDto, AttendanceStatus, ClockInRequest } from '@/features/hris/types';
import { toSortParam, type SortState } from '@/lib/api/pagination';
import { t, type TranslationKey } from '@/lib/i18n';
import { formatDateTime } from '@/lib/utils/format';

import { ClockInDialog } from './ClockInDialog';

const PAGE_SIZE = 10;

const STATUS_TONE: Record<AttendanceStatus, StatusTone> = {
  ClockedIn: 'info',
  ClockedOut: 'success',
};

const STATUS_LABEL = {
  ClockedIn: 'hris.attendance.status.clockedIn',
  ClockedOut: 'hris.attendance.status.clockedOut',
} as const;
export function AttendanceView() {
  const session = useSession();
  const [page, setPage] = useState(1);
  const [sort, setSort] = useState<SortState>({ field: 'clockInUtc', direction: 'desc' });
  const [isClockInOpen, setIsClockInOpen] = useState(false);
  const [lastClockIn, setLastClockIn] = useState<TranslationKey | null>(null);

  const canClockIn = session.permissions.includes(PERMISSIONS.hris.attendanceClockIn);

  const attendanceQuery = useGetAttendanceLogs({
    page,
    pageSize: PAGE_SIZE,
    sort: toSortParam(sort),
  });

  const clockInMutation = useClockIn();

  const handleOpenClockIn = () => {
    clockInMutation.reset();
    setIsClockInOpen(true);
  };

  const handleCloseClockIn = () => {
    if (!clockInMutation.isPending) {
      setIsClockInOpen(false);
    }
  };

  const handleClockIn = (request: ClockInRequest) => {
    clockInMutation.mutate(request, {
      onSuccess: () => {
        setLastClockIn('hris.attendance.clockIn.success');
        setIsClockInOpen(false);
      },
    });
  };

  const columns: DataTableColumn<AttendanceLogDto>[] = [
    {
      id: 'employeeId',
      header: t('hris.attendance.employee'),
      sortable: true,
      cell: (log) => (
        <span className="font-mono text-xs font-semibold text-charcoal">{log.employeeId}</span>
      ),
    },
    {
      id: 'clockInUtc',
      header: t('hris.attendance.clockIn'),
      sortable: true,
      cell: (log) => (
        <span className="whitespace-nowrap text-sm text-charcoal">
          {formatDateTime(log.clockInUtc)}
        </span>
      ),
    },
    {
      id: 'clockOutUtc',
      header: t('hris.attendance.clockOut'),
      sortable: true,
      cell: (log) => (
        <span className="whitespace-nowrap text-sm text-charcoal-light">
          {log.clockOutUtc ? formatDateTime(log.clockOutUtc) : '—'}
        </span>
      ),
    },
    {
      id: 'status',
      header: t('hris.attendance.status'),
      sortable: true,
      cell: (log) => (
        <StatusBadge label={t(STATUS_LABEL[log.status])} tone={STATUS_TONE[log.status]} />
      ),
    },
    {
      id: 'location',
      header: t('hris.attendance.location'),
      cell: (log) => (
        <span className="font-mono text-sm text-charcoal-light">
          {parseFloat(log.latitude).toFixed(4)}, {parseFloat(log.longitude).toFixed(4)}
        </span>
      ),
    },
    {
      id: 'actions',
      header: t('hris.attendance.actions'),
      align: 'right',
      cell: () => (
        <span aria-hidden="true" className="text-charcoal-light">
          &mdash;
        </span>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('hris.attendance.title')}
        description={t('hris.attendance.description')}
        actions={
          canClockIn ? (
            <Button onClick={handleOpenClockIn}>
              <Plus className="h-4 w-4" aria-hidden="true" />
              {t('hris.attendance.clockIn.submit')}
            </Button>
          ) : null
        }
      />

      {lastClockIn ? (
        <div
          role="status"
          aria-live="polite"
          className="flex items-start gap-2 rounded-lg border border-mint-accent bg-mint-accent/30 px-4 py-3 text-sm text-charcoal"
        >
          <Clock className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
          <p>{t(lastClockIn)}</p>
        </div>
      ) : null}

      <QuerySection
        query={attendanceQuery}
        skeleton={<TableSkeleton columns={columns.length} rows={7} />}
        isEmpty={(data) => data.items.length === 0}
        emptyTitle={t('hris.attendance.empty.title')}
        emptyDescription={t('hris.attendance.empty.body')}
        emptyActionLabel={t('hris.attendance.empty.cta')}
      >
        {(data) => (
          <DataTable
            data={data.items}
            columns={columns}
            getRowId={(log) => log.id}
            caption={t('hris.attendance.tableCaption')}
            sort={sort}
            onSortChange={(next) => {
              setSort(next);
              setPage(1);
            }}
            pagination={{
              page: data.page,
              pageSize: data.pageSize,
              totalItems: data.totalItems,
              totalPages: data.totalPages,
            }}
            onPageChange={setPage}
          />
        )}
      </QuerySection>

      <ClockInDialog
        isOpen={isClockInOpen}
        isPending={clockInMutation.isPending}
        error={clockInMutation.error}
        onConfirm={handleClockIn}
        onCancel={handleCloseClockIn}
      />
    </div>
  );
}
