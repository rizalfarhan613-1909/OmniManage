'use client';

import { useI18n } from '@/components/providers/LanguageProvider';

import { useState } from 'react';

import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { Input } from '@/components/ui/Input';

import type { CreateWarehouseRequest } from '../api/types';

type Props = {
  isOpen: boolean;
  isPending: boolean;
  error: unknown;
  onConfirm: (request: CreateWarehouseRequest) => void;
  onCancel: () => void;
};

type WarehouseForm = {
  code: string;
  name: string;
  address: string;
};

const INITIAL_FORM: WarehouseForm = { code: '', name: '', address: '' };

export function CreateWarehouseDialog({ isOpen, isPending, error, onConfirm, onCancel }: Props) {
  const { t } = useI18n();
  const [formData, setFormData] = useState<WarehouseForm>(INITIAL_FORM);

  const submit = () => {
    const code = formData.code.trim();
    const name = formData.name.trim();
    const address = formData.address.trim();

    if (code.length < 2 || code.length > 50 || name.length === 0 || name.length > 200) {
      return;
    }

    if (address.length > 500) {
      return;
    }

    onConfirm({ code, name, address: address || null });
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    submit();
  };

  return (
    <ConfirmDialog
      isOpen={isOpen}
      title={t('inventory.warehouses.create.title')}
      description={t('inventory.warehouses.create.description')}
      confirmLabel={
        isPending
          ? t('inventory.warehouses.create.submitting')
          : t('inventory.warehouses.create.submit')
      }
      pendingLabel={t('inventory.warehouses.create.submitting')}
      cancelLabel={t('common.cancel')}
      isPending={isPending}
      error={error}
      errorDescription={t('inventory.warehouses.create.error')}
      onConfirm={submit}
      onCancel={onCancel}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="warehouseCode" className="mb-1 block text-sm font-medium text-charcoal">
            {t('inventory.warehouses.create.code')}
          </label>
          <Input
            id="warehouseCode"
            type="text"
            placeholder={t('inventory.warehouses.create.codePlaceholder')}
            value={formData.code}
            onChange={(event) => setFormData({ ...formData, code: event.target.value })}
            minLength={2}
            maxLength={50}
            autoComplete="off"
            required
          />
          <p className="mt-1 text-xs text-charcoal-light">
            {t('inventory.warehouses.create.invalidCode')}
          </p>
        </div>
        <div>
          <label htmlFor="warehouseName" className="mb-1 block text-sm font-medium text-charcoal">
            {t('inventory.warehouses.create.name')}
          </label>
          <Input
            id="warehouseName"
            type="text"
            placeholder={t('inventory.warehouses.create.namePlaceholder')}
            value={formData.name}
            onChange={(event) => setFormData({ ...formData, name: event.target.value })}
            maxLength={200}
            required
          />
        </div>
        <div>
          <label
            htmlFor="warehouseAddress"
            className="mb-1 block text-sm font-medium text-charcoal"
          >
            {t('inventory.warehouses.create.address')}
          </label>
          <Input
            id="warehouseAddress"
            type="text"
            placeholder={t('inventory.warehouses.create.addressPlaceholder')}
            value={formData.address}
            onChange={(event) => setFormData({ ...formData, address: event.target.value })}
            maxLength={500}
          />
        </div>
      </form>
    </ConfirmDialog>
  );
}
