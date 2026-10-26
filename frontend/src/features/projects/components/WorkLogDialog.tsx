'use client';

import { useState } from 'react';

import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { Input } from '@/components/ui/Input';
import { t } from '@/lib/i18n';
import { toDateOnlyString } from '@/lib/utils/date';
import type { RecordWorkLogRequest } from '../types';

type Props = {
  isOpen: boolean;
  taskId: string;
  taskTitle: string;
  isPending: boolean;
  error: unknown;
  onConfirm: (request: RecordWorkLogRequest) => void;
  onCancel: () => void;
};

export function WorkLogDialog({
  isOpen,
  taskId,
  taskTitle,
  isPending,
  error,
  onConfirm,
  onCancel,
}: Props) {
  const [formData, setFormData] = useState<RecordWorkLogRequest>({
    employeeId: '',
    loggedDate: toDateOnlyString(),
    hoursSpent: 0,
    description: '',
    completeTask: false,
  });

  const submit = () => {
    if (!formData.employeeId || formData.hoursSpent <= 0) {
      return;
    }

    onConfirm({ ...formData, loggedDate: formData.loggedDate || toDateOnlyString() });
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    submit();
  };

  return (
    <ConfirmDialog
      isOpen={isOpen}
      title={t('projects.tasks.workLog.title')}
      description={`${t('projects.tasks.workLog.description')}: ${taskTitle}`}
      confirmLabel={
        isPending ? t('projects.tasks.workLog.submitting') : t('projects.tasks.workLog.submit')
      }
      pendingLabel={t('projects.tasks.workLog.submitting')}
      cancelLabel={t('common.cancel')}
      isPending={isPending}
      error={error}
      onConfirm={submit}
      onCancel={onCancel}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <input type="hidden" name="taskId" value={taskId} />
        <div>
          <label htmlFor="employeeId" className="mb-1 block text-sm font-medium text-charcoal">
            {t('projects.tasks.workLog.employeeId')}
          </label>
          <Input
            id="employeeId"
            type="text"
            placeholder="EMP-001"
            value={formData.employeeId}
            onChange={(e) => setFormData({ ...formData, employeeId: e.target.value })}
            required
          />
        </div>
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label htmlFor="loggedDate" className="mb-1 block text-sm font-medium text-charcoal">
              {t('projects.tasks.workLog.loggedDate')}
            </label>
            <Input
              id="loggedDate"
              type="date"
              value={formData.loggedDate}
              onChange={(e) => setFormData({ ...formData, loggedDate: e.target.value })}
              required
            />
          </div>
          <div>
            <label htmlFor="hoursSpent" className="mb-1 block text-sm font-medium text-charcoal">
              {t('projects.tasks.workLog.hoursSpent')}
            </label>
            <Input
              id="hoursSpent"
              type="number"
              min="0.5"
              step="0.5"
              placeholder="4"
              value={formData.hoursSpent}
              onChange={(e) => setFormData({ ...formData, hoursSpent: parseFloat(e.target.value) })}
              required
            />
          </div>
        </div>
        <div>
          <label htmlFor="description" className="mb-1 block text-sm font-medium text-charcoal">
            {t('projects.tasks.workLog.descriptionField')}
          </label>
          <Input
            id="description"
            type="text"
            placeholder="Worked on authentication module"
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
          />
        </div>
        <div className="flex items-center gap-2">
          <input
            type="checkbox"
            id="completeTask"
            checked={formData.completeTask}
            onChange={(e) => setFormData({ ...formData, completeTask: e.target.checked })}
            className="h-4 w-4 rounded border-gray-300 text-mint-accent focus:ring-mint-accent"
          />
          <label htmlFor="completeTask" className="text-sm text-charcoal">
            {t('projects.tasks.workLog.completeTask')}
          </label>
        </div>
      </form>
    </ConfirmDialog>
  );
}
