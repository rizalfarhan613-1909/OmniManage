# API Contract Audit

Phase 5B contract audit, performed on 2026-10-22 against the Development host.

## Method

- Build the solution and start `OmniManage.Api` with
  `ASPNETCORE_ENVIRONMENT=Development` against the local PostgreSQL 16
  container (host port `5433`, runtime role `omni_app`).
- Read the generated OpenAPI document from
  `http://localhost:5000/swagger/v1/swagger.json`.
- Inventory every backend controller and every Next.js BFF route.
- Exercise login, session, validation, authentication and not-found paths over
  HTTP and compare the bodies with `ApiResponse<T>` from `Rules.md §5.4`.
- Cross-check every frontend `/api/v1/**` call against the OpenAPI paths.

## Contract invariants

| Rule | Status | Evidence |
| --- | --- | --- |
| Success and failure bodies use `ApiResponse<T>` | Met | All controller actions inherit `ApiControllerBase`; `ApiResponseWriter` and `GlobalExceptionMiddleware` write failures. |
| JSON is camelCase with string enums, ISO-8601 UTC dates and string decimals | Met | ASP.NET defaults plus the module DTOs; verified in live login and list responses. |
| `meta.traceId` is present on every envelope | Met | Live responses and `ErrorEnvelopeTests`. |
| Validation/binding failures return `400 VALIDATION.FAILED` with `details[]` | Met | Malformed JSON and wrong field types both returned the envelope. |
| Unhandled exceptions return `500 SYSTEM.UNEXPECTED` with a generic message | Met | `GlobalExceptionMiddleware` and `UnexpectedException_ReturnsSystemUnexpectedEnvelope`. |
| Unknown authenticated routes return a JSON envelope | Met | `404 SYSTEM.NOT_FOUND` via `UseStatusCodePages`; covered by `UnknownRoute_ForAuthenticatedCaller_ReturnsNotFoundEnvelope`. |
| Authentication failures return `401 AUTH.UNAUTHENTICATED` or `AUTH.TOKEN_EXPIRED` | Met | JWT `OnChallenge` writer; live unauthenticated request returned `AUTH.UNAUTHENTICATED`. |
| Authorization failures return `403 AUTH.FORBIDDEN` or `PLATFORM.MODULE_NOT_LICENSED` | Met | JWT `OnForbidden`, authorization handlers and `ModuleLicenseMiddleware`; covered by integration tests. |
| Health endpoint is anonymous | Met | `GET /health/live` returned `200 Healthy` without a token. It is a liveness probe, not a business API, so it is intentionally outside the envelope. |

`GET /health/live` is the only non-enveloped response, by design.

## Backend endpoint inventory

The OpenAPI document contains 18 operations. All are under `/api/v1`, except
the anonymous liveness probe.

### Authentication and session

| Method | Path | Authorization | Success | Failure responses |
| --- | --- | --- | --- | --- |
| `POST` | `/api/v1/auth/login` | Anonymous | `200` login payload | `400`, `401` |
| `POST` | `/api/v1/auth/refresh` | Anonymous (refresh token in body) | `200` login payload | `400`, `401` |
| `POST` | `/api/v1/auth/logout` | Anonymous (refresh token in body) | `200` `{ revoked }` | none observed |
| `GET` | `/api/v1/auth/session` | Bearer | `200` session | `401` |

### Master Data

| Method | Path | Success | Failure responses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/masterdata/business-partners` | `200` paged list | envelope errors |
| `POST` | `/api/v1/masterdata/business-partners` | `201` | `400`, `409` |
| `GET` | `/api/v1/masterdata/items` | `200` paged list | envelope errors |
| `POST` | `/api/v1/masterdata/items` | `201` | `400`, `409` |

### Inventory

| Method | Path | Success | Failure responses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/inventory/warehouses` | `200` paged list | envelope errors |
| `POST` | `/api/v1/inventory/warehouses` | `201` | `400`, `409` |
| `GET` | `/api/v1/inventory/stock/balances` | `200` paged list | envelope errors |
| `POST` | `/api/v1/inventory/stock/adjustments` | `201` | `400`, `404` |

### Sales

| Method | Path | Success | Failure responses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/sales/orders` | `200` paged list | envelope errors |
| `GET` | `/api/v1/sales/orders/{id}` | `200` order | `404` |
| `POST` | `/api/v1/sales/orders` | `201` | `400`, `409` |
| `POST` | `/api/v1/sales/orders/{id}/confirm` | `200` confirmed order | `404`, `409` |

### Finance

| Method | Path | Success | Failure responses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/finance/invoices` | `200` paged list | `400` |
| `GET` | `/api/v1/finance/journal-entries` | `200` paged list | `400` |

## BFF mapping

The browser never sends a backend JWT. Next.js route handlers hold the access
and refresh tokens in httpOnly cookies and call the API server-side
(`Security.md §2.5`).

| Browser route | Backend route | Behaviour |
| --- | --- | --- |
| `POST /api/auth/login` | `POST /api/v1/auth/login` | Validates input, stores both JWTs in httpOnly cookies, returns only the sanitized session. |
| `POST /api/auth/refresh` | `POST /api/v1/auth/refresh` | Rotates both cookies, returns the session. Tokens never enter the response body. |
| `POST /api/auth/logout` | `POST /api/v1/auth/logout` | Revokes the backend refresh-token family (best effort) and always clears cookies. |
| `GET /api/auth/session` | `GET /api/v1/auth/session` | Revalidates; on `401` it refreshes once and retries. |
| `* /api/proxy/{segments}` | `* /api/v1/{segments}` | Generic authenticated proxy. Preserves method, query, safe headers and body; refreshes once on `401`; forwards the envelope and `x-trace-id`. |

Proxy safeguards:

- `/api/proxy/auth/**` is rejected with `403 AUTH.FORBIDDEN`; authentication
  flows only use the dedicated routes above.
- Mutating methods (`POST`, `PUT`, `PATCH`, `DELETE`) require a same-origin
  request (CSRF defence).
- Path segments are rejected when empty, `.`, `..`, or containing slash or
  backslash characters.
- The proxy never reads or returns the tokens; it only sets rotated cookies.

## Frontend consumers

| Frontend hook / call | Backend operation | BFF path |
| --- | --- | --- |
| `useBusinessPartners` | `GET /api/v1/masterdata/business-partners` | `/api/proxy/masterdata/business-partners` |
| `useItems` | `GET /api/v1/masterdata/items` | `/api/proxy/masterdata/items` |
| `useWarehouses` | `GET /api/v1/inventory/warehouses` | `/api/proxy/inventory/warehouses` |
| `useStockBalances` | `GET /api/v1/inventory/stock/balances` | `/api/proxy/inventory/stock/balances` |
| `useSalesOrders` | `GET /api/v1/sales/orders` | `/api/proxy/sales/orders` |
| `useConfirmSalesOrder` | `POST /api/v1/sales/orders/{id}/confirm` | `/api/proxy/sales/orders/{id}/confirm` |
| `useArInvoices` | `GET /api/v1/finance/invoices` | `/api/proxy/finance/invoices` |
| `useJournalEntries` | `GET /api/v1/finance/journal-entries` | `/api/proxy/finance/journal-entries` |
| `useExecutiveDashboard` | Composes the six documented list reads above; no dedicated endpoint | Several proxy reads |
| `useTenants` | `GET /api/v1/platform/tenants` — **not implemented** | `/api/proxy/platform/tenants` |
| `useAuditLogs` | `GET /api/v1/platform/audit-logs` — **not implemented** | `/api/proxy/platform/audit-logs` |
| `useActiveSessions` | `GET /api/v1/platform/sessions` — **not implemented** | `/api/proxy/platform/sessions` |
| `useTerminateSession` | `DELETE /api/v1/platform/sessions/{id}` — **not implemented** | `/api/proxy/platform/sessions/{id}` |
| `useToggleTenantModule` | `PATCH /api/v1/platform/tenants/{id}/modules/{code}` — **not implemented** | `/api/proxy/platform/tenants/{id}/modules/{code}` |

The contract is intentionally asymmetric: the backend exposes create/command
operations that the Phase 5B UI does not yet consume (`POST` Master Data,
`POST` warehouses, `POST` stock adjustments and `POST` sales orders). Those
operations are still covered by integration tests and OpenAPI.

## Live BFF verification

The production build and the Next.js development server were both exercised
against the running API:

| Check | Result |
| --- | --- |
| `POST /api/auth/login` | `200`; response body contained only the session, with no access or refresh token. |
| Session cookie attributes | `HttpOnly; SameSite=lax; Path=/`; `Secure` is added when `NODE_ENV=production`. |
| Refresh cookie lifetime | Capped at 43,200 seconds (12 hours) even though the backend refresh token lives 7 days. |
| `GET /api/auth/session` with the cookies | `200`, tenant `TEN-8492`, permissions and active modules present. |
| `GET /api/proxy/sales/orders` with the cookies | `200` envelope; the proxy attached the bearer token server-side. |
| `GET /api/proxy/auth/session` | `403 AUTH.FORBIDDEN`; authentication routes cannot be reached through the generic proxy. |
| Cross-site `Origin` on login | `403 AUTH.FORBIDDEN`; same-origin check rejected the request. |
| `POST /api/auth/logout` | `200`; backend revocation attempted and both cookies cleared. |

Production cookies are intentionally `Secure`, so they are only sent over
HTTPS. A plain-HTTP loopback test therefore uses the development server; the
header inspection above confirms the production flags.

## Verified failure envelopes

Observed against the live Development host on 2026-10-22:

| Scenario | Status | Envelope |
| --- | --- | --- |
| Valid tenant login | `200` | `success=true`, session and token metadata present |
| Empty login object | `401` | `AUTH.INVALID_CREDENTIALS` (no exception) |
| Login without password | `401` | `AUTH.INVALID_CREDENTIALS` |
| Malformed JSON | `400` | `VALIDATION.FAILED`, `details[]` populated |
| Wrong field type | `400` | `VALIDATION.FAILED`, `details[]` populated |
| Business endpoint without bearer token | `401` | `AUTH.UNAUTHENTICATED` |
| Authenticated unknown route | `404` | `SYSTEM.NOT_FOUND` |
| Unauthenticated unknown route | `401` | `AUTH.UNAUTHENTICATED` (fallback policy fails closed) |
| Unhandled exception (integration test) | `500` | `SYSTEM.UNEXPECTED`, generic message and trace id |
| Module disabled / permission missing (integration tests) | `403` | `PLATFORM.MODULE_NOT_LICENSED` / `AUTH.FORBIDDEN` |

## Known gaps and deviations

1. **Platform administration API is not implemented.** The five
   `/api/v1/platform/**` calls listed above have no controller, so the admin
   console receives `404 SYSTEM.NOT_FOUND`. Implementing them requires
   platform-realm tokens and MFA per `Security.md §3.4`; tenant-realm access was
   deliberately not added. This is the primary known limitation in `README.md`.
2. **Swagger has no security scheme or operation summaries.** Paths, methods,
   request bodies and status codes are generated, but the document does not
   advertise the bearer requirement and operations have no descriptions. This
   is a documentation gap, not a runtime contract difference.
3. **Unmatched paths are protected by the fallback authorization policy.**
   Anonymous callers get `401` before routing can report `404`. Authenticated
   callers get the `SYSTEM.NOT_FOUND` envelope. This is fail-closed and matches
   the API's default-deny posture.
4. **`POST /api/v1/auth/logout` returns `200` for an unknown or missing refresh
   token.** Logout is idempotent and never leaks whether a token existed.
5. **The dashboard is composed client-side.** No dashboard endpoint exists, so
   the audit treats the six documented list operations as its contract.

## Verification commands

```powershell
dotnet build backend/OmniManage.sln
dotnet test backend/OmniManage.sln

cd frontend
npm run lint
npm run typecheck
npm test
npm run build
```
