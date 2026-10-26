'use client';

import { Plus, Trash2 } from 'lucide-react';
import { useRef, useState } from 'react';

import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import type { Item } from '@/features/masterdata/api/types';
import type { CreatePurchaseOrderRequest, Supplier } from '@/features/procurement/api/types';
import { t } from '@/lib/i18n';
import { toDateOnlyString } from '@/lib/utils/date';
import { toDecimal } from '@/lib/utils/decimal';
import { formatMoney } from '@/lib/utils/format';

type Props = {
  isOpen: boolean;
  suppliers: readonly Supplier[];
  items: readonly Item[];
  hasItemCatalogue: boolean;
  hasSupplierAccess: boolean;
  isPending: boolean;
  error: unknown;
  onConfirm: (request: CreatePurchaseOrderRequest) => void;
  onCancel: () => void;
};

type LineForm = {
  key: string;
  itemId: string;
  quantity: string;
  unitPrice: string;
};

type ParsedLine = {
  itemId: string;
  quantity: number;
  unitPrice: number;
  isValid: boolean;
};

const ORDER_NUMBER_PATTERN = /^[A-Z0-9][A-Z0-9._-]*$/;

const SELECT_CLASS =
  'h-10 w-full rounded-md border border-sage bg-surface px-3 text-sm text-charcoal shadow-sm focus-visible:border-teal-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary';

const TEXTAREA_CLASS =
  'min-h-20 w-full rounded-md border border-sage bg-surface px-3 py-2 text-sm text-charcoal shadow-sm placeholder:text-charcoal-light focus-visible:border-teal-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary';

const EMPTY_LINE = { itemId: '', quantity: '', unitPrice: '' } as const;

const INITIAL_LINE: LineForm = { key: 'po-line-1', ...EMPTY_LINE };

function parseLine(line: LineForm): ParsedLine {
  const itemId = line.itemId.trim();
  const quantity = Number(line.quantity);
  const unitPrice = Number(line.unitPrice);
  const hasQuantity = line.quantity.trim() !== '';
  const hasUnitPrice = line.unitPrice.trim() !== '';

  return {
    itemId,
    quantity,
    unitPrice,
    isValid:
      itemId !== '' &&
      hasQuantity &&
      Number.isFinite(quantity) &&
      quantity > 0 &&
      hasUnitPrice &&
      Number.isFinite(unitPrice) &&
      unitPrice >= 0,
  };
}

export function CreatePurchaseOrderDialog({
  isOpen,
  suppliers,
  items,
  hasItemCatalogue,
  hasSupplierAccess,
  isPending,
  error,
  onConfirm,
  onCancel,
}: Props) {
  const nextLineKey = useRef(1);
  const [orderNumber, setOrderNumber] = useState('');
  const [supplierId, setSupplierId] = useState('');
  const [expectedDeliveryDate, setExpectedDeliveryDate] = useState('');
  const [notes, setNotes] = useState('');
  const [lines, setLines] = useState<LineForm[]>([INITIAL_LINE]);

  const normalizedOrderNumber = orderNumber.trim().toUpperCase();
  const isOrderNumberValid =
    normalizedOrderNumber.length > 0 &&
    normalizedOrderNumber.length <= 50 &&
    ORDER_NUMBER_PATTERN.test(normalizedOrderNumber);
  const isDeliveryValid = expectedDeliveryDate === '' || expectedDeliveryDate >= toDateOnlyString();
  const parsedLines = lines.map(parseLine);
  const areLinesValid = parsedLines.length > 0 && parsedLines.every((line) => line.isValid);
  const canSubmit = isOrderNumberValid && supplierId !== '' && isDeliveryValid && areLinesValid;

  const total = parsedLines.reduce(
    (sum, line) =>
      line.isValid ? sum.plus(toDecimal(line.quantity).times(toDecimal(line.unitPrice))) : sum,
    toDecimal(0),
  );

  const updateLine = (key: string, patch: Partial<Omit<LineForm, 'key'>>) => {
    setLines((current) => current.map((line) => (line.key === key ? { ...line, ...patch } : line)));
  };

  const addLine = () => {
    nextLineKey.current += 1;
    const key = `po-line-${nextLineKey.current}`;

    setLines((current) => [...current, { ...EMPTY_LINE, key }]);
  };

  const removeLine = (key: string) => {
    setLines((current) =>
      current.length > 1 ? current.filter((line) => line.key !== key) : current,
    );
  };

  const submit = () => {
    if (!canSubmit) {
      return;
    }

    onConfirm({
      orderNumber: normalizedOrderNumber,
      supplierId,
      expectedDeliveryDate: expectedDeliveryDate || null,
      notes: notes.trim() || null,
      lines: parsedLines.map((line) => ({
        itemId: line.itemId,
        quantity: line.quantity,
        unitPrice: line.unitPrice,
      })),
    });
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    submit();
  };

  return (
    <ConfirmDialog
      isOpen={isOpen}
      title={t('procurement.create.title')}
      description={t('procurement.create.description')}
      confirmLabel={isPending ? t('procurement.create.submitting') : t('procurement.create.submit')}
      pendingLabel={t('procurement.create.submitting')}
      cancelLabel={t('common.cancel')}
      isPending={isPending}
      isConfirmDisabled={!canSubmit}
      error={error}
      errorDescription={t('procurement.create.error')}
      onConfirm={submit}
      onCancel={onCancel}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label htmlFor="poOrderNumber" className="mb-1 block text-sm font-medium text-charcoal">
              {t('procurement.create.orderNumber')}
            </label>
            <Input
              id="poOrderNumber"
              type="text"
              placeholder={t('procurement.create.orderNumberPlaceholder')}
              value={orderNumber}
              onChange={(event) => setOrderNumber(event.target.value)}
              maxLength={50}
              autoComplete="off"
              required
            />
            <p className="mt-1 text-xs text-charcoal-light">
              {t('procurement.create.orderNumberHint')}
            </p>
          </div>

          <div>
            <label
              htmlFor="poExpectedDeliveryDate"
              className="mb-1 block text-sm font-medium text-charcoal"
            >
              {t('procurement.create.expectedDelivery')}
            </label>
            <Input
              id="poExpectedDeliveryDate"
              type="date"
              min={toDateOnlyString()}
              value={expectedDeliveryDate}
              onChange={(event) => setExpectedDeliveryDate(event.target.value)}
            />
          </div>
        </div>

        <div>
          <label htmlFor="poSupplierId" className="mb-1 block text-sm font-medium text-charcoal">
            {t('procurement.create.supplier')}
          </label>
          <select
            id="poSupplierId"
            value={supplierId}
            onChange={(event) => setSupplierId(event.target.value)}
            className={SELECT_CLASS}
            disabled={suppliers.length === 0}
            required
          >
            <option value="">{t('procurement.create.supplierPlaceholder')}</option>
            {suppliers.map((supplier) => (
              <option key={supplier.id} value={supplier.id}>
                {supplier.code} — {supplier.name}
              </option>
            ))}
          </select>
          {suppliers.length === 0 ? (
            <p className="mt-1 text-xs text-charcoal-light">
              {hasSupplierAccess
                ? t('procurement.create.noSuppliers')
                : t('procurement.create.supplierUnavailable')}
            </p>
          ) : null}
        </div>

        <div>
          <label htmlFor="poNotes" className="mb-1 block text-sm font-medium text-charcoal">
            {t('procurement.create.notes')}
          </label>
          <textarea
            id="poNotes"
            value={notes}
            onChange={(event) => setNotes(event.target.value)}
            placeholder={t('procurement.create.notesPlaceholder')}
            maxLength={1000}
            className={TEXTAREA_CLASS}
          />
        </div>

        <fieldset className="space-y-3">
          <legend className="text-sm font-semibold text-charcoal">
            {t('procurement.create.lines')}
          </legend>
          <p className="text-xs text-charcoal-light">{t('procurement.create.linesHint')}</p>

          {lines.map((line, index) => {
            const lineNumber = index + 1;
            const idPrefix = `poLine-${line.key}`;

            return (
              <div
                key={line.key}
                className="space-y-3 rounded-lg border border-sage bg-sage-light/40 p-3"
              >
                <div className="flex items-center justify-between gap-2">
                  <p className="text-xs font-semibold uppercase tracking-wide text-charcoal-light">
                    {t('procurement.create.line', { line: lineNumber })}
                  </p>
                  <Button
                    variant="ghost"
                    size="sm"
                    aria-label={t('procurement.create.removeLine', { line: lineNumber })}
                    disabled={lines.length === 1 || isPending}
                    onClick={() => removeLine(line.key)}
                  >
                    <Trash2 className="h-4 w-4" aria-hidden="true" />
                  </Button>
                </div>

                <div>
                  <label
                    htmlFor={`${idPrefix}-item`}
                    className="mb-1 block text-xs font-medium text-charcoal"
                  >
                    {hasItemCatalogue
                      ? t('procurement.create.item')
                      : t('procurement.create.itemId')}
                  </label>
                  {hasItemCatalogue ? (
                    <select
                      id={`${idPrefix}-item`}
                      value={line.itemId}
                      onChange={(event) => updateLine(line.key, { itemId: event.target.value })}
                      className={SELECT_CLASS}
                      required
                    >
                      <option value="">{t('procurement.create.itemPlaceholder')}</option>
                      {items.map((item) => (
                        <option key={item.id} value={item.id}>
                          {item.sku} — {item.name}
                        </option>
                      ))}
                    </select>
                  ) : (
                    <>
                      <Input
                        id={`${idPrefix}-item`}
                        type="text"
                        placeholder="00000000-0000-0000-0000-000000000000"
                        value={line.itemId}
                        onChange={(event) => updateLine(line.key, { itemId: event.target.value })}
                        autoComplete="off"
                        required
                      />
                      <p className="mt-1 text-xs text-charcoal-light">
                        {t('procurement.create.itemIdHint')}
                      </p>
                    </>
                  )}
                </div>

                <div className="grid gap-3 sm:grid-cols-2">
                  <div>
                    <label
                      htmlFor={`${idPrefix}-quantity`}
                      className="mb-1 block text-xs font-medium text-charcoal"
                    >
                      {t('procurement.create.quantity')}
                    </label>
                    <Input
                      id={`${idPrefix}-quantity`}
                      type="number"
                      min="0.0001"
                      step="0.0001"
                      placeholder="1"
                      value={line.quantity}
                      onChange={(event) => updateLine(line.key, { quantity: event.target.value })}
                      required
                    />
                  </div>
                  <div>
                    <label
                      htmlFor={`${idPrefix}-unitPrice`}
                      className="mb-1 block text-xs font-medium text-charcoal"
                    >
                      {t('procurement.create.unitPrice')}
                    </label>
                    <Input
                      id={`${idPrefix}-unitPrice`}
                      type="number"
                      min="0"
                      step="0.0001"
                      placeholder="0.0000"
                      value={line.unitPrice}
                      onChange={(event) => updateLine(line.key, { unitPrice: event.target.value })}
                      required
                    />
                  </div>
                </div>

                {parsedLines[index]?.isValid ? (
                  <p className="text-right text-xs tabular-nums text-charcoal-light">
                    {t('procurement.create.lineTotal')}:{' '}
                    {formatMoney(
                      toDecimal(parsedLines[index]?.quantity ?? 0).times(
                        toDecimal(parsedLines[index]?.unitPrice ?? 0),
                      ),
                    )}
                  </p>
                ) : null}
              </div>
            );
          })}

          <Button variant="secondary" size="sm" onClick={addLine} disabled={isPending}>
            <Plus className="h-4 w-4" aria-hidden="true" />
            {t('procurement.create.addLine')}
          </Button>

          <div className="flex items-center justify-between gap-2 border-t border-sage pt-3 text-sm">
            <span className="font-medium text-charcoal">{t('procurement.create.total')}</span>
            <span className="font-semibold tabular-nums text-charcoal">{formatMoney(total)}</span>
          </div>
        </fieldset>
      </form>
    </ConfirmDialog>
  );
}
