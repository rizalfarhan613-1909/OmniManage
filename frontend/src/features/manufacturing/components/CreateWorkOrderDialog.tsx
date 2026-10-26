'use client';

import { useState } from 'react';

import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { Input } from '@/components/ui/Input';
import type { Item } from '@/features/masterdata/api/types';
import type { BillOfMaterial, CreateWorkOrderRequest } from '@/features/manufacturing/api/types';
import { t } from '@/lib/i18n';
import { toDateOnlyString } from '@/lib/utils/date';

type Props = {
  isOpen: boolean;
  billOfMaterials: readonly BillOfMaterial[];
  items: readonly Item[];
  hasBillOfMaterialAccess: boolean;
  isPending: boolean;
  error: unknown;
  onConfirm: (request: CreateWorkOrderRequest) => void;
  onCancel: () => void;
};

const ORDER_NUMBER_PATTERN = /^[A-Z0-9][A-Z0-9._-]*$/;
const MAX_QUANTITY = 999_999_999.999999;

const SELECT_CLASS =
  'h-10 w-full rounded-md border border-sage bg-surface px-3 text-sm text-charcoal shadow-sm focus-visible:border-teal-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary';

const TEXTAREA_CLASS =
  'min-h-20 w-full rounded-md border border-sage bg-surface px-3 py-2 text-sm text-charcoal shadow-sm placeholder:text-charcoal-light focus-visible:border-teal-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary';

export function CreateWorkOrderDialog({
  isOpen,
  billOfMaterials,
  items,
  hasBillOfMaterialAccess,
  isPending,
  error,
  onConfirm,
  onCancel,
}: Props) {
  const [orderNumber, setOrderNumber] = useState('');
  const [billOfMaterialId, setBillOfMaterialId] = useState('');
  const [quantityPlanned, setQuantityPlanned] = useState('');
  const [dueDate, setDueDate] = useState('');
  const [notes, setNotes] = useState('');

  const normalizedOrderNumber = orderNumber.trim().toUpperCase();
  const isOrderNumberValid =
    normalizedOrderNumber.length > 0 &&
    normalizedOrderNumber.length <= 50 &&
    ORDER_NUMBER_PATTERN.test(normalizedOrderNumber);
  const isQuantityValid =
    quantityPlanned.trim() !== '' &&
    Number.isFinite(Number(quantityPlanned)) &&
    Number(quantityPlanned) > 0 &&
    Number(quantityPlanned) <= MAX_QUANTITY;
  const isDueDateValid = dueDate === '' || dueDate >= toDateOnlyString();
  const canSubmit =
    isOrderNumberValid && billOfMaterialId !== '' && isQuantityValid && isDueDateValid;

  const selectedBillOfMaterial = billOfMaterials.find((bom) => bom.id === billOfMaterialId);
  const selectedProduct = selectedBillOfMaterial
    ? items.find((item) => item.id === selectedBillOfMaterial.productItemId)
    : undefined;
  const productLabel = selectedProduct
    ? `${selectedProduct.sku} \u2014 ${selectedProduct.name}`
    : (selectedBillOfMaterial?.productItemId ?? '');

  const submit = () => {
    if (!canSubmit) {
      return;
    }

    onConfirm({
      orderNumber: normalizedOrderNumber,
      billOfMaterialId,
      quantityPlanned: Number(quantityPlanned),
      dueDate: dueDate || null,
      notes: notes.trim() || null,
    });
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    submit();
  };

  return (
    <ConfirmDialog
      isOpen={isOpen}
      title={t('manufacturing.create.title')}
      description={t('manufacturing.create.description')}
      confirmLabel={
        isPending ? t('manufacturing.create.submitting') : t('manufacturing.create.submit')
      }
      pendingLabel={t('manufacturing.create.submitting')}
      cancelLabel={t('common.cancel')}
      isPending={isPending}
      isConfirmDisabled={!canSubmit}
      error={error}
      errorDescription={t('manufacturing.create.error')}
      onConfirm={submit}
      onCancel={onCancel}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label htmlFor="woOrderNumber" className="mb-1 block text-sm font-medium text-charcoal">
              {t('manufacturing.create.orderNumber')}
            </label>
            <Input
              id="woOrderNumber"
              type="text"
              placeholder={t('manufacturing.create.orderNumberPlaceholder')}
              value={orderNumber}
              onChange={(event) => setOrderNumber(event.target.value)}
              maxLength={50}
              autoComplete="off"
              required
            />
            <p className="mt-1 text-xs text-charcoal-light">
              {t('manufacturing.create.orderNumberHint')}
            </p>
          </div>

          <div>
            <label htmlFor="woDueDate" className="mb-1 block text-sm font-medium text-charcoal">
              {t('manufacturing.create.dueDate')}
            </label>
            <Input
              id="woDueDate"
              type="date"
              min={toDateOnlyString()}
              value={dueDate}
              onChange={(event) => setDueDate(event.target.value)}
            />
          </div>
        </div>

        <div>
          <label
            htmlFor="woBillOfMaterialId"
            className="mb-1 block text-sm font-medium text-charcoal"
          >
            {t('manufacturing.create.billOfMaterial')}
          </label>
          <select
            id="woBillOfMaterialId"
            value={billOfMaterialId}
            onChange={(event) => setBillOfMaterialId(event.target.value)}
            className={SELECT_CLASS}
            disabled={billOfMaterials.length === 0}
            required
          >
            <option value="">{t('manufacturing.create.billOfMaterialPlaceholder')}</option>
            {billOfMaterials.map((bom) => (
              <option key={bom.id} value={bom.id}>
                {bom.code} — {bom.name} (
                {t('manufacturing.bom.versionValue', { version: bom.version })})
              </option>
            ))}
          </select>
          {billOfMaterials.length === 0 ? (
            <p className="mt-1 text-xs text-charcoal-light">
              {hasBillOfMaterialAccess
                ? t('manufacturing.create.noBillOfMaterials')
                : t('manufacturing.create.billOfMaterialUnavailable')}
            </p>
          ) : null}
        </div>

        <div>
          <span className="mb-1 block text-sm font-medium text-charcoal">
            {t('manufacturing.create.product')}
          </span>
          <p
            aria-live="polite"
            className="rounded-md border border-sage bg-sage-light/40 px-3 py-2 text-sm text-charcoal"
          >
            {productLabel || t('manufacturing.create.billOfMaterialPlaceholder')}
          </p>
          <p className="mt-1 text-xs text-charcoal-light">
            {t('manufacturing.create.productHint')}
          </p>
        </div>

        <div>
          <label
            htmlFor="woQuantityPlanned"
            className="mb-1 block text-sm font-medium text-charcoal"
          >
            {t('manufacturing.create.quantityPlanned')}
          </label>
          <Input
            id="woQuantityPlanned"
            type="number"
            min="0.0001"
            max={MAX_QUANTITY}
            step="0.0001"
            placeholder="1"
            value={quantityPlanned}
            onChange={(event) => setQuantityPlanned(event.target.value)}
            required
          />
          <p className="mt-1 text-xs text-charcoal-light">
            {t('manufacturing.create.quantityHint')}
          </p>
        </div>

        <div>
          <label htmlFor="woNotes" className="mb-1 block text-sm font-medium text-charcoal">
            {t('manufacturing.create.notes')}
          </label>
          <textarea
            id="woNotes"
            value={notes}
            onChange={(event) => setNotes(event.target.value)}
            placeholder={t('manufacturing.create.notesPlaceholder')}
            maxLength={1000}
            className={TEXTAREA_CLASS}
          />
        </div>
      </form>
    </ConfirmDialog>
  );
}
