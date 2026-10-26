export interface PayloadChange {
  field: string;
  before: unknown;
  after: unknown;
}

function stringify(value: unknown): string {
  try {
    return JSON.stringify(value) ?? '';
  } catch {
    return String(value);
  }
}

function isEqual(left: unknown, right: unknown): boolean {
  return stringify(left) === stringify(right);
}

/**
 * Builds a changed-fields-only diff from the redacted `before_data` /
 * `after_data` JSONB payloads stored by the backend (Security.md §6.1).
 */
export function buildPayloadDiff(
  before: Record<string, unknown> | null,
  after: Record<string, unknown> | null,
): PayloadChange[] {
  const beforeRecord = before ?? {};
  const afterRecord = after ?? {};
  const fields = new Set([...Object.keys(beforeRecord), ...Object.keys(afterRecord)]);

  return Array.from(fields)
    .sort((left, right) => left.localeCompare(right))
    .filter((field) => !isEqual(beforeRecord[field], afterRecord[field]))
    .map((field) => ({
      field,
      before: beforeRecord[field],
      after: afterRecord[field],
    }));
}
