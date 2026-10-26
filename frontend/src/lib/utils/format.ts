import { DEFAULT_CURRENCY, DEFAULT_LOCALE } from '@/config/locale';
import { toDecimal, type DecimalLike } from '@/lib/utils/decimal';

const groupingFormatter = new Intl.NumberFormat(DEFAULT_LOCALE, {
  useGrouping: true,
  maximumFractionDigits: 0,
});

const DECIMAL_SEPARATOR =
  new Intl.NumberFormat(DEFAULT_LOCALE).formatToParts(1.1).find((part) => part.type === 'decimal')
    ?.value ?? '.';

const dateFormatter = new Intl.DateTimeFormat(DEFAULT_LOCALE, { dateStyle: 'medium' });
const dateTimeFormatter = new Intl.DateTimeFormat(DEFAULT_LOCALE, {
  dateStyle: 'medium',
  timeStyle: 'short',
});
const monthFormatter = new Intl.DateTimeFormat(DEFAULT_LOCALE, { month: 'short' });
const relativeFormatter = new Intl.RelativeTimeFormat(DEFAULT_LOCALE, { numeric: 'auto' });

function groupIntegerPart(value: string): string {
  return groupingFormatter.format(BigInt(value));
}

function formatFixed(
  value: DecimalLike,
  fractionDigits: number,
  trimTrailingZeros: boolean,
): string {
  const decimal = toDecimal(value);
  const isNegative = decimal.isNegative();
  const fixed = decimal.abs().toFixed(fractionDigits);
  const [integerPart = '0', rawFraction = ''] = fixed.split('.');
  const fraction = trimTrailingZeros ? rawFraction.replace(/0+$/, '') : rawFraction;
  const sign = isNegative ? '-' : '';
  const grouped = groupIntegerPart(integerPart);

  return fraction.length > 0
    ? `${sign}${grouped}${DECIMAL_SEPARATOR}${fraction}`
    : `${sign}${grouped}`;
}

/** `IDR 1,250,000.00` with locale-aware grouping, safe for large decimals. */
export function formatMoney(value: DecimalLike): string {
  const decimal = toDecimal(value);
  const sign = decimal.isNegative() ? '-' : '';

  return `${sign}${DEFAULT_CURRENCY} ${formatFixed(decimal.abs(), 2, false)}`;
}

/** Monetary amount without the currency code, for dense table columns. */
export function formatDecimal(value: DecimalLike): string {
  return formatFixed(value, 2, false);
}

/** Quantities keep up to four decimal places but never show noisy padding. */
export function formatQuantity(value: DecimalLike): string {
  return formatFixed(value, 4, true);
}

/** Compact axis/legend labels (K/M/B) for chart scales. */
export function formatCompactMoney(value: number): string {
  return new Intl.NumberFormat(DEFAULT_LOCALE, {
    notation: 'compact',
    maximumFractionDigits: 1,
    style: 'currency',
    currency: DEFAULT_CURRENCY,
    currencyDisplay: 'narrowSymbol',
  }).format(value);
}

export function formatDate(value: string): string {
  const date = new Date(value);

  return Number.isNaN(date.getTime()) ? value : dateFormatter.format(date);
}

export function formatDateTime(value: string): string {
  const date = new Date(value);

  return Number.isNaN(date.getTime()) ? value : dateTimeFormatter.format(date);
}

/** `YYYY-MM` (UTC) → `Oct`. */
export function formatMonthKey(monthKey: string): string {
  const [year, month] = monthKey.split('-').map(Number);

  if (!year || !month) {
    return monthKey;
  }

  return monthFormatter.format(new Date(Date.UTC(year, month - 1, 1)));
}

export function formatRelativeTime(value: string, now = Date.now()): string {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  const deltaMs = date.getTime() - now;
  const minute = 60_000;
  const hour = 60 * minute;
  const day = 24 * hour;

  if (Math.abs(deltaMs) < hour) {
    return relativeFormatter.format(Math.round(deltaMs / minute), 'minute');
  }

  if (Math.abs(deltaMs) < day) {
    return relativeFormatter.format(Math.round(deltaMs / hour), 'hour');
  }

  return relativeFormatter.format(Math.round(deltaMs / day), 'day');
}

export function formatCount(value: number): string {
  return groupingFormatter.format(BigInt(Math.trunc(value)));
}

/** Alias for formatMoney for currency formatting. */
export function formatCurrency(value: number): string {
  return formatMoney(value);
}

/** Alias for formatCount for number formatting. */
export function formatNumber(value: number): string {
  return formatCount(value);
}
