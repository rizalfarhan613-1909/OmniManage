'use client';

import { useState } from 'react';

import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { Input } from '@/components/ui/Input';
import { t } from '@/lib/i18n';
import type { RequestMaintenanceRequest } from '../types';

type Props = {
  isOpen: boolean;
  assetId: string;
  assetName: string;
  isPending: boolean;
  error: unknown;
  onConfirm: (request: RequestMaintenanceRequest) => void;
  onCancel: () => void;
};

export function MaintenanceDialog({
  isOpen,
  assetId,
  assetName,
  isPending,
  error,
  onConfirm,
  onCancel,
}: Props) {
  const [formData, setFormData] = useState<RequestMaintenanceRequest>(() => ({
    requestNumber: `MNT-${Date.now()}`,
    scheduledDate: null,
    description: '',
    maintenanceType: 'PREVENTIVE',
    priority: 'MEDIUM',
  }));

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (!formData.requestNumber || !formData.description) {
      return;
    }
    onConfirm(formData);
  };

  return (
    <ConfirmDialog
      isOpen={isOpen}
      title={t('assets.register.maintenance.title')}
      description={`${t('assets.register.maintenance.description')}: ${assetName}`}
      confirmLabel={
        isPending
          ? t('assets.register.maintenance.submitting')
          : t('assets.register.maintenance.submit')
      }
      pendingLabel={t('assets.register.maintenance.submitting')}
      cancelLabel={t('common.cancel')}
      isPending={isPending}
      error={error}
      onConfirm={() => onConfirm(formData)}
      onCancel={onCancel}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <input type="hidden" name="assetId" value={assetId} />
        <div>
          <label htmlFor="requestNumber" className="mb-1 block text-sm font-medium text-charcoal">
            {t('assets.register.maintenance.requestNumber')}
          </label>
          <Input
            id="requestNumber"
            type="text"
            value={formData.requestNumber}
            onChange={(e) => setFormData({ ...formData, requestNumber: e.target.value })}
            required
          />
        </div>
        <div>
          <label htmlFor="scheduledDate" className="mb-1 block text-sm font-medium text-charcoal">
            {t('assets.register.maintenance.scheduledDate')}
          </label>
          <Input
            id="scheduledDate"
            type="date"
            value={formData.scheduledDate ?? ''}
            onChange={(e) => setFormData({ ...formData, scheduledDate: e.target.value || null })}
          />
        </div>
        <div>
          <label htmlFor="description" className="mb-1 block text-sm font-medium text-charcoal">
            {t('assets.register.maintenance.descriptionField')}
          </label>
          <Input
            id="description"
            type="text"
            placeholder="Replace battery and clean cooling system"
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            required
          />
        </div>
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label
              htmlFor="maintenanceType"
              className="mb-1 block text-sm font-medium text-charcoal"
            >
              {t('assets.register.maintenance.maintenanceType')}
            </label>
            <Input
              id="maintenanceType"
              type="text"
              placeholder="PREVENTIVE"
              value={formData.maintenanceType ?? ''}
              onChange={(e) => setFormData({ ...formData, maintenanceType: e.target.value })}
            />
          </div>
          <div>
            <label htmlFor="priority" className="mb-1 block text-sm font-medium text-charcoal">
              {t('assets.register.maintenance.priority')}
            </label>
            <Input
              id="priority"
              type="text"
              placeholder="MEDIUM"
              value={formData.priority ?? ''}
              onChange={(e) => setFormData({ ...formData, priority: e.target.value })}
            />
          </div>
        </div>
      </form>
    </ConfirmDialog>
  );
}
