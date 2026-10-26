/**
 * Presentation locale defaults.
 *
 * ASSUMPTION: Phase 1 API contracts expose monetary amounts as decimal strings
 * but do not yet carry a currency code. The MVP is therefore presented in a
 * single currency (IDR). When multi-currency lands, these constants are the
 * single replacement point; feature code never hard-codes the code or locale.
 */
export const DEFAULT_LOCALE = 'en-US';
export const DEFAULT_CURRENCY = 'IDR';
