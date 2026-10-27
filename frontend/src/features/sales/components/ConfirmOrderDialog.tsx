'use client';

import { useI18n } from '@/components/providers/LanguageProvider';

import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import type { SalesOrder } from '@/features/sales/api/types';

type Props = {
  isOpen: boolean;
  order: SalesOrder | null;
  isPending: boolean;
  error: unknown;
  onConfirm: () => void;
  onCancel: () => void;
};

/** UC-01 confirmation. The order number is part of the accessible title. */
export function ConfirmOrderDialog({
  isOpen,
  order,
  isPending,
  error,
  onConfirm,
  onCancel,
}: Props) {
  const { t } = useI18n();
  return (
    <ConfirmDialog
      isOpen={isOpen}
      title={t('sales.confirm.title', { orderNumber: order?.orderNumber ?? '' })}
      description={t('sales.confirm.body')}
      confirmLabel={t('sales.confirm.confirm')}
      pendingLabel={t('sales.confirm.pending')}
      cancelLabel={t('sales.confirm.cancel')}
      isPending={isPending}
      error={error}
      onConfirm={onConfirm}
      onCancel={onCancel}
    />
  );
}
