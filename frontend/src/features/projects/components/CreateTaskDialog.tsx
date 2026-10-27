'use client';

import { useI18n } from '@/components/providers/LanguageProvider';

import { useState } from 'react';

import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { Input } from '@/components/ui/Input';

import type { CreateTaskRequest } from '../types';

type Props = {
  isOpen: boolean;
  isPending: boolean;
  error: unknown;
  onConfirm: (request: CreateTaskRequest) => void;
  onCancel: () => void;
};

type TaskForm = {
  projectId: string;
  title: string;
  assigneeEmployeeId: string;
  estimatedHours: number;
};

export function CreateTaskDialog({ isOpen, isPending, error, onConfirm, onCancel }: Props) {
  const { t } = useI18n();
  const [formData, setFormData] = useState<TaskForm>({
    projectId: '',
    title: '',
    assigneeEmployeeId: '',
    estimatedHours: 0,
  });

  const submit = () => {
    if (!formData.projectId || !formData.title || formData.estimatedHours <= 0) {
      return;
    }

    onConfirm({
      projectId: formData.projectId,
      title: formData.title,
      assigneeEmployeeId: formData.assigneeEmployeeId || null,
      estimatedHours: formData.estimatedHours,
    });
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    submit();
  };

  return (
    <ConfirmDialog
      isOpen={isOpen}
      title={t('projects.tasks.create.title')}
      description={t('projects.tasks.create.description')}
      confirmLabel={
        isPending ? t('projects.tasks.create.submitting') : t('projects.tasks.create.submit')
      }
      pendingLabel={t('projects.tasks.create.submitting')}
      cancelLabel={t('common.cancel')}
      isPending={isPending}
      error={error}
      onConfirm={submit}
      onCancel={onCancel}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="projectId" className="mb-1 block text-sm font-medium text-charcoal">
            {t('projects.tasks.create.projectId')}
          </label>
          <Input
            id="projectId"
            type="text"
            placeholder="PROJ-001"
            value={formData.projectId}
            onChange={(e) => setFormData({ ...formData, projectId: e.target.value })}
            required
          />
        </div>
        <div>
          <label htmlFor="title" className="mb-1 block text-sm font-medium text-charcoal">
            {t('projects.tasks.create.titleField')}
          </label>
          <Input
            id="title"
            type="text"
            placeholder="Implement user authentication"
            value={formData.title}
            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
            required
          />
        </div>
        <div>
          <label
            htmlFor="assigneeEmployeeId"
            className="mb-1 block text-sm font-medium text-charcoal"
          >
            {t('projects.tasks.create.assigneeEmployeeId')}
          </label>
          <Input
            id="assigneeEmployeeId"
            type="text"
            placeholder="EMP-001"
            value={formData.assigneeEmployeeId}
            onChange={(e) => setFormData({ ...formData, assigneeEmployeeId: e.target.value })}
          />
        </div>
        <div>
          <label htmlFor="estimatedHours" className="mb-1 block text-sm font-medium text-charcoal">
            {t('projects.tasks.create.estimatedHours')}
          </label>
          <Input
            id="estimatedHours"
            type="number"
            min="0.5"
            step="0.5"
            placeholder="8"
            value={formData.estimatedHours}
            onChange={(e) =>
              setFormData({ ...formData, estimatedHours: parseFloat(e.target.value) })
            }
            required
          />
        </div>
      </form>
    </ConfirmDialog>
  );
}
