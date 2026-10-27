import { forwardRef, type ComponentPropsWithoutRef } from 'react';

import { cn } from '@/lib/utils/cn';

type Props = ComponentPropsWithoutRef<'input'> & {
  hasError?: boolean;
};

/**
 * Text input styled with design tokens. Consumers are responsible for
 * associating a `<label>` and `aria-describedby` helper/error text.
 */
export const Input = forwardRef<HTMLInputElement, Props>(function Input(
  { className, hasError = false, ...props },
  ref,
) {
  return (
    <input
      ref={ref}
      aria-invalid={hasError || undefined}
      className={cn(
        'h-10 w-full rounded-md border bg-surface px-3 text-base text-charcoal shadow-sm placeholder:text-charcoal-light focus-visible:border-teal-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary',
        hasError ? 'border-danger' : 'border-line',
        className,
      )}
      {...props}
    />
  );
});
