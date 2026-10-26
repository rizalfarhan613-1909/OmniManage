/**
 * Wire-format date helpers.
 *
 * `DateOnly` contract fields travel as `yyyy-MM-dd` strings (Rules.md §5) and are
 * declared non-nullable in the DTOs, so form defaults must be produced here:
 * `date.toISOString().split('T')[0]` widens to `string | undefined` under
 * `noUncheckedIndexedAccess` and cannot be assigned to those fields.
 */
export function toDateOnlyString(date: Date = new Date()): string {
  return date.toISOString().slice(0, 10);
}
