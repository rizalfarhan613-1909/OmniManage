import { fireEvent, render, screen } from '@testing-library/react';
import type { ComponentProps } from 'react';
import { describe, expect, it, vi } from 'vitest';

import { ConfirmDialog } from './ConfirmDialog';

type DialogProps = ComponentProps<typeof ConfirmDialog>;

function buildProps(overrides: Partial<DialogProps> = {}): DialogProps {
  return {
    isOpen: true,
    title: 'Disable General Ledger?',
    description: 'This removes access to the module.',
    confirmLabel: 'Disable module',
    pendingLabel: 'Disabling…',
    cancelLabel: 'Cancel',
    onConfirm: vi.fn(),
    onCancel: vi.fn(),
    ...overrides,
  };
}

function Dialog({ onCancel, isOpen = true }: { onCancel: () => void; isOpen?: boolean }) {
  return (
    <ConfirmDialog {...buildProps({ onCancel, isOpen })}>
      <label htmlFor="reason">Reason for change</label>
      <textarea id="reason" />
    </ConfirmDialog>
  );
}

describe('ConfirmDialog', () => {
  it('keeps focus inside the dialog content when the parent re-renders', () => {
    const { rerender } = render(<Dialog onCancel={vi.fn()} />);

    const textarea = screen.getByLabelText('Reason for change');
    textarea.focus();

    rerender(<Dialog onCancel={vi.fn()} />);

    expect(textarea).toHaveFocus();
  });

  it('calls the latest onCancel handler on Escape', () => {
    const onCancel = vi.fn();
    const { rerender } = render(<Dialog onCancel={vi.fn()} />);

    rerender(<Dialog onCancel={onCancel} />);
    fireEvent.keyDown(document, { key: 'Escape' });

    expect(onCancel).toHaveBeenCalledTimes(1);
  });

  it('ignores Escape while a command is pending', () => {
    const onCancel = vi.fn();

    render(
      <ConfirmDialog {...buildProps({ isPending: true, onCancel })}>
        <p>Working…</p>
      </ConfirmDialog>,
    );

    fireEvent.keyDown(document, { key: 'Escape' });

    expect(onCancel).not.toHaveBeenCalled();
  });
});
