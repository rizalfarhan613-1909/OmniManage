import type { Metadata } from 'next';

import { ModuleGuard } from '@/components/shared/ModuleGuard';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { t } from '@/lib/i18n';

import { AttendanceView } from '@/features/hris/components/AttendanceView';

export const metadata: Metadata = { title: t('hris.attendance.title') };

/** Mirrors `[RequireModule(ModuleCodes.HrisAttendance)]` on the controller. */
export default function AttendancePage() {
  return (
    <ModuleGuard
      moduleCode={MODULE_CODES.hrisAttendance}
      permission={PERMISSIONS.hris.attendanceRead}
    >
      <AttendanceView />
    </ModuleGuard>
  );
}
