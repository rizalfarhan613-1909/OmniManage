import { z } from 'zod';

import { t } from '@/lib/i18n';

/**
 * Request contract for `POST /api/auth/login` (BFF) which forwards to
 * `POST /api/v1/auth/login`. Frontend validation is UX only.
 */
export const loginRequestSchema = z.object({
  tenantCode: z
    .string()
    .trim()
    .min(1, t('validation.required'))
    .max(50, t('validation.maxLength', { max: 50 })),
  email: z
    .string()
    .trim()
    .min(1, t('validation.required'))
    .max(254, t('validation.maxLength', { max: 254 }))
    .email(t('validation.email')),
  password: z
    .string()
    .min(1, t('validation.required'))
    .max(200, t('validation.maxLength', { max: 200 })),
});

export type LoginRequest = z.infer<typeof loginRequestSchema>;
