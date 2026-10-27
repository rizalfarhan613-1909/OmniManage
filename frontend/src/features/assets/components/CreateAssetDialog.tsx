'use client';

import { useI18n } from '@/components/providers/LanguageProvider';

import { useState } from 'react';

import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { Input } from '@/components/ui/Input';

import { toDateOnlyString } from '@/lib/utils/date';
import type { CreateAssetRequest } from '../types';

type Props = {
  isOpen: boolean;
  isPending: boolean;
  error: unknown;
  onConfirm: (request: CreateAssetRequest) => void;
  onCancel: () => void;
};

type AssetForm = {
  assetCode: string;
  name: string;
  category: string;
  serialNumber: string;
  qrCodeUrl: string;
  purchaseValue: number;
  acquisitionDate: string;
};

export function CreateAssetDialog({ isOpen, isPending, error, onConfirm, onCancel }: Props) {
  const { t } = useI18n();
  const [formData, setFormData] = useState<AssetForm>({
    assetCode: '',
    name: '',
    category: '',
    serialNumber: '',
    qrCodeUrl: '',
    purchaseValue: 0,
    acquisitionDate: toDateOnlyString(),
  });

  const submit = () => {
    if (
      !formData.assetCode ||
      !formData.name ||
      !formData.category ||
      formData.purchaseValue <= 0
    ) {
      return;
    }

    onConfirm({
      assetCode: formData.assetCode,
      name: formData.name,
      category: formData.category,
      serialNumber: formData.serialNumber || null,
      qrCodeUrl: formData.qrCodeUrl || null,
      purchaseValue: formData.purchaseValue,
      acquisitionDate: formData.acquisitionDate || toDateOnlyString(),
    });
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    submit();
  };

  return (
    <ConfirmDialog
      isOpen={isOpen}
      title={t('assets.register.create.title')}
      description={t('assets.register.create.description')}
      confirmLabel={
        isPending ? t('assets.register.create.submitting') : t('assets.register.create.submit')
      }
      pendingLabel={t('assets.register.create.submitting')}
      cancelLabel={t('common.cancel')}
      isPending={isPending}
      error={error}
      onConfirm={submit}
      onCancel={onCancel}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="assetCode" className="mb-1 block text-sm font-medium text-charcoal">
            {t('assets.register.create.assetCode')}
          </label>
          <Input
            id="assetCode"
            type="text"
            placeholder="AST-001"
            value={formData.assetCode}
            onChange={(e) => setFormData({ ...formData, assetCode: e.target.value })}
            required
          />
        </div>
        <div>
          <label htmlFor="name" className="mb-1 block text-sm font-medium text-charcoal">
            {t('assets.register.create.name')}
          </label>
          <Input
            id="name"
            type="text"
            placeholder="MacBook Pro 16"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            required
          />
        </div>
        <div>
          <label htmlFor="category" className="mb-1 block text-sm font-medium text-charcoal">
            {t('assets.register.create.category')}
          </label>
          <Input
            id="category"
            type="text"
            placeholder="Laptop"
            value={formData.category}
            onChange={(e) => setFormData({ ...formData, category: e.target.value })}
            required
          />
        </div>
        <div>
          <label htmlFor="serialNumber" className="mb-1 block text-sm font-medium text-charcoal">
            {t('assets.register.create.serialNumber')}
          </label>
          <Input
            id="serialNumber"
            type="text"
            placeholder="SN123456789"
            value={formData.serialNumber}
            onChange={(e) => setFormData({ ...formData, serialNumber: e.target.value })}
          />
        </div>
        <div>
          <label htmlFor="qrCodeUrl" className="mb-1 block text-sm font-medium text-charcoal">
            {t('assets.register.create.qrCodeUrl')}
          </label>
          <Input
            id="qrCodeUrl"
            type="url"
            placeholder="https://example.com/qr/AST-001"
            value={formData.qrCodeUrl}
            onChange={(e) => setFormData({ ...formData, qrCodeUrl: e.target.value })}
          />
        </div>
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label htmlFor="purchaseValue" className="mb-1 block text-sm font-medium text-charcoal">
              {t('assets.register.create.purchaseValue')}
            </label>
            <Input
              id="purchaseValue"
              type="number"
              min="0"
              step="0.01"
              placeholder="2500.00"
              value={formData.purchaseValue}
              onChange={(e) =>
                setFormData({ ...formData, purchaseValue: parseFloat(e.target.value) })
              }
              required
            />
          </div>
          <div>
            <label
              htmlFor="acquisitionDate"
              className="mb-1 block text-sm font-medium text-charcoal"
            >
              {t('assets.register.create.acquisitionDate')}
            </label>
            <Input
              id="acquisitionDate"
              type="date"
              value={formData.acquisitionDate}
              onChange={(e) => setFormData({ ...formData, acquisitionDate: e.target.value })}
              required
            />
          </div>
        </div>
      </form>
    </ConfirmDialog>
  );
}
