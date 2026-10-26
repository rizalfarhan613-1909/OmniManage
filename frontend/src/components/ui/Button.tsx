import type { ComponentPropsWithoutRef } from 'react';

import { cn } from '@/lib/utils/cn';

type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger';
type ButtonSize = 'sm' | 'md';

type Props = ComponentPropsWithoutRef<'button'> & {
  variant?: ButtonVariant;
  size?: ButtonSize;
};

const VARIANT_CLASSES: Record<ButtonVariant, string> = {
  primary:
    'bg-teal-primary text-charcoal hover:bg-teal-hover active:bg-primary-700 focus-visible:ring-teal-primary',
  secondary:
    'border border-sage bg-surface text-charcoal hover:bg-sage-light focus-visible:ring-teal-primary',
  ghost: 'text-charcoal hover:bg-sage-light focus-visible:ring-teal-primary',
  danger: 'bg-danger text-surface hover:bg-danger/90 focus-visible:ring-danger',
};

const SIZE_CLASSES: Record<ButtonSize, string> = {
  sm: 'h-9 px-3 text-sm',
  md: 'h-10 px-4 text-sm',
};

export function Button({
  variant = 'primary',
  size = 'md',
  className,
  type = 'button',
  ...props
}: Props) {
  return (
    <button
      type={type}
      className={cn(
        'inline-flex items-center justify-center gap-2 rounded-md font-medium transition-colors duration-300 ease-in-out disabled:cursor-not-allowed disabled:opacity-60',
        VARIANT_CLASSES[variant],
        SIZE_CLASSES[size],
        className,
      )}
      {...props}
    />
  );
}
