/** Presentation helpers for identity strings in the app chrome. */

export function formatPersonName(email: string): string {
  const localPart = email.split('@')[0] ?? email;
  const parts = localPart.split(/[._-]+/).filter(Boolean);

  if (parts.length === 0) {
    return email;
  }

  return parts
    .map((part) => `${part.charAt(0).toUpperCase()}${part.slice(1).toLowerCase()}`)
    .join(' ');
}

export function formatRoleLabel(role: string | undefined): string {
  if (!role) {
    return 'Workspace Member';
  }

  return role
    .replace(/[_-]+/g, ' ')
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replace(/\b\w/g, (character) => character.toUpperCase());
}
