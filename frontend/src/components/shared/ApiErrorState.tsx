'use client';

import { ApiRequestError } from '@/lib/api/errors';
import { ForbiddenState } from '@/components/shared/ForbiddenState';
import { ErrorState } from '@/components/shared/ErrorState';
import { ModuleNotLicensedState } from '@/components/shared/ModuleNotLicensedState';

type Props = {
  error: unknown;
  onRetry?: () => void;
  moduleCode?: string;
};

/**
 * Maps an API failure onto the correct required state:
 *  - 403 PLATFORM.MODULE_NOT_LICENSED → ModuleNotLicensedState
 *  - 403 (any other)                   → ForbiddenState
 *  - everything else                   → ErrorState with traceId + retry
 */
export function ApiErrorState({ error, onRetry, moduleCode }: Props) {
  if (error instanceof ApiRequestError) {
    if (error.isModuleNotLicensed()) {
      return <ModuleNotLicensedState moduleCode={moduleCode} />;
    }

    if (error.isForbidden()) {
      return <ForbiddenState />;
    }

    return <ErrorState description={error.message} traceId={error.traceId} onRetry={onRetry} />;
  }

  return <ErrorState onRetry={onRetry} />;
}
