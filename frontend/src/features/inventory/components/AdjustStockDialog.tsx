'use client';

import { useI18n } from '@/components/providers/LanguageProvider';

import { useState } from 'react';

import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { Input } from '@/components/ui/Input';
import type { Item } from '@/features/masterdata/api/types';
import type { TranslationKey } from '@/lib/i18n';

import type { AdjustStockRequest, MovementType, Warehouse } from '../api/types';

type Props = {
  isOpen: boolean;
  warehouses: readonly Warehouse[];
  items: readonly Item[];
  initialWarehouseId?: string;
  initialItemId?: string;
  isPending: boolean;
  error: unknown;
  onConfirm: (request: AdjustStockRequest) => void;
  onCancel: () => void;
};

const MOVEMENT_OPTIONS: readonly { value: MovementType; labelKey: TranslationKey }[] = [
  { value: 'Receive', labelKey: 'inventory.stock.movement.receive' },
  { value: 'Issue', labelKey: 'inventory.stock.movement.issue' },
  { value: 'Transfer', labelKey: 'inventory.stock.movement.transfer' },
  { value: 'Adjustment', labelKey: 'inventory.stock.movement.adjustment' },
];

const SELECT_CLASS =
  'h-10 w-full rounded-md border border-line bg-surface px-3 text-sm text-charcoal shadow-sm focus-visible:border-teal-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary';

export function AdjustStockDialog({
  isOpen,
  warehouses,
  items,
  initialWarehouseId = '',
  initialItemId = '',
  isPending,
  error,
  onConfirm,
  onCancel,
}: Props) {
  const { t } = useI18n();
  const [warehouseId, setWarehouseId] = useState(initialWarehouseId);
  const [itemId, setItemId] = useState(initialItemId);
  const [movementType, setMovementType] = useState<MovementType>('Adjustment');
  const [quantityDelta, setQuantityDelta] = useState('');
  const [referenceNumber, setReferenceNumber] = useState(() => `ADJ-${Date.now()}`);
  const hasItemCatalogue = items.length > 0;

  const submit = () => {
    const quantity = Number(quantityDelta);
    const reference = referenceNumber.trim();

    if (!warehouseId || !itemId.trim() || reference.length === 0 || reference.length > 100) {
      return;
    }

    if (!Number.isFinite(quantity) || quantity === 0) {
      return;
    }

    onConfirm({
      warehouseId,
      itemId: itemId.trim(),
      quantityDelta: quantity,
      movementType,
      referenceNumber: reference,
    });
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    submit();
  };

  return (
    <ConfirmDialog
      isOpen={isOpen}
      title={t('inventory.stock.adjust.title')}
      description={t('inventory.stock.adjust.description')}
      confirmLabel={
        isPending ? t('inventory.stock.adjust.submitting') : t('inventory.stock.adjust.submit')
      }
      pendingLabel={t('inventory.stock.adjust.submitting')}
      cancelLabel={t('common.cancel')}
      isPending={isPending}
      error={error}
      errorDescription={t('inventory.stock.adjust.error')}
      onConfirm={submit}
      onCancel={onCancel}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label
              htmlFor="adjustWarehouseId"
              className="mb-1 block text-sm font-medium text-charcoal"
            >
              {t('inventory.stock.adjust.warehouse')}
            </label>
            <select
              id="adjustWarehouseId"
              value={warehouseId}
              onChange={(event) => setWarehouseId(event.target.value)}
              className={SELECT_CLASS}
              required
            >
              <option value="">{t('inventory.stock.adjust.warehousePlaceholder')}</option>
              {warehouses.map((warehouse) => (
                <option key={warehouse.id} value={warehouse.id}>
                  {warehouse.code} — {warehouse.name}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label
              htmlFor="adjustMovementType"
              className="mb-1 block text-sm font-medium text-charcoal"
            >
              {t('inventory.stock.adjust.movementType')}
            </label>
            <select
              id="adjustMovementType"
              value={movementType}
              onChange={(event) => setMovementType(event.target.value as MovementType)}
              className={SELECT_CLASS}
              required
            >
              {MOVEMENT_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {t(option.labelKey)}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div>
          <label htmlFor="adjustItemId" className="mb-1 block text-sm font-medium text-charcoal">
            {t('inventory.stock.adjust.item')}
          </label>
          {hasItemCatalogue ? (
            <select
              id="adjustItemId"
              value={itemId}
              onChange={(event) => setItemId(event.target.value)}
              className={SELECT_CLASS}
              required
            >
              <option value="">{t('inventory.stock.adjust.itemPlaceholder')}</option>
              {items.map((item) => (
                <option key={item.id} value={item.id}>
                  {item.sku} — {item.name}
                </option>
              ))}
            </select>
          ) : (
            <>
              <Input
                id="adjustItemId"
                type="text"
                placeholder="00000000-0000-0000-0000-000000000000"
                value={itemId}
                onChange={(event) => setItemId(event.target.value)}
                autoComplete="off"
                required
              />
              <p className="mt-1 text-xs text-charcoal-light">
                {t('inventory.stock.adjust.itemIdHint')}
              </p>
            </>
          )}
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label
              htmlFor="adjustQuantityDelta"
              className="mb-1 block text-sm font-medium text-charcoal"
            >
              {t('inventory.stock.adjust.quantityDelta')}
            </label>
            <Input
              id="adjustQuantityDelta"
              type="number"
              step="0.0001"
              placeholder="10"
              value={quantityDelta}
              onChange={(event) => setQuantityDelta(event.target.value)}
              required
            />
          </div>
          <div>
            <label
              htmlFor="adjustReferenceNumber"
              className="mb-1 block text-sm font-medium text-charcoal"
            >
              {t('inventory.stock.adjust.referenceNumber')}
            </label>
            <Input
              id="adjustReferenceNumber"
              type="text"
              placeholder={t('inventory.stock.adjust.referencePlaceholder')}
              value={referenceNumber}
              onChange={(event) => setReferenceNumber(event.target.value)}
              maxLength={100}
              autoComplete="off"
              required
            />
          </div>
        </div>

        <p className="text-xs text-charcoal-light">{t('inventory.stock.adjust.quantityHint')}</p>
      </form>
    </ConfirmDialog>
  );
}
