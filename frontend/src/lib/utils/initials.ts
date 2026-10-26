/** Derives up to two initials from an email address or display name. */
export function getInitials(value: string): string {
  const parts = value.split(/[\s@._-]+/).filter(Boolean);
  const first = parts[0]?.[0] ?? '?';
  const last = parts.length > 1 ? (parts[parts.length - 1]?.[0] ?? '') : '';

  return `${first}${last}`.toUpperCase();
}
