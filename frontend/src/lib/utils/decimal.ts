import Decimal from 'decimal.js';

/**
 * Money and quantity arithmetic MUST use decimal.js (Rules.md §10.7). The API
 * transmits decimals as strings; no feature code converts a monetary value to
 * a JS number except at the Recharts presentation boundary.
 */
Decimal.set({ precision: 40, rounding: Decimal.ROUND_HALF_UP });

export type DecimalLike = string | number | Decimal;

export function toDecimal(value: DecimalLike | null | undefined): Decimal {
  if (value === null || value === undefined || value === '') {
    return new Decimal(0);
  }

  if (value instanceof Decimal) {
    return value;
  }

  return new Decimal(value);
}

export function sumDecimals(values: Iterable<DecimalLike | null | undefined>): Decimal {
  let total = new Decimal(0);

  for (const value of values) {
    total = total.plus(toDecimal(value));
  }

  return total;
}

export function multiplyDecimals(left: DecimalLike, right: DecimalLike): Decimal {
  return toDecimal(left).times(toDecimal(right));
}

export function subtractDecimals(left: DecimalLike, right: DecimalLike): Decimal {
  return toDecimal(left).minus(toDecimal(right));
}

/**
 * Percentage change used for KPI trends. Returns null when there is no prior
 * baseline so the UI never renders a misleading infinite/NaN value.
 */
export function percentageChange(current: DecimalLike, previous: DecimalLike): Decimal | null {
  const baseline = toDecimal(previous);

  if (baseline.isZero()) {
    return null;
  }

  return toDecimal(current).minus(baseline).dividedBy(baseline.abs()).times(100);
}

/**
 * Chart coordinates require numbers. This is the only sanctioned conversion;
 * all aggregation has already happened in Decimal space.
 */
export function toChartNumber(value: DecimalLike): number {
  return Number(toDecimal(value).toFixed(4));
}
