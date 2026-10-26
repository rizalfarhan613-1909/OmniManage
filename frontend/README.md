# OmniManage Web (Frontend)

Next.js App Router frontend for OmniManage. Phase 4A delivers the application
shell, authentication, design tokens, API foundation and dynamic navigation.
Phase 4B adds the TanStack Query data layer, the executive BI dashboard and the
first Sales, Inventory and Finance module views.

## Prerequisites

- Node.js 20.9+ (developed on Node 24)
- The OmniManage API running locally (default `http://localhost:5000`)

## Setup

```bash
cd frontend
npm install
cp .env.example .env.local   # set API_BASE_URL if the API is not on :5000
npm run dev                  # http://localhost:3000
```

| Script              | Purpose                             |
| ------------------- | ----------------------------------- |
| `npm run dev`       | Development server (Turbopack)      |
| `npm run build`     | Production build                    |
| `npm run start`     | Serve the production build          |
| `npm run lint`      | ESLint with `--max-warnings=0`      |
| `npm run typecheck` | `tsc --noEmit` (`strict`, no `any`) |
| `npm test`          | Vitest unit/component tests         |
| `npm run format`    | Prettier check                      |

## Architecture

```
src/
├── app/
│   ├── (auth)/login/            # public sign-in
│   ├── (app)/                   # authenticated shell + 10 core routes
│   └── api/                     # BFF: auth, session, backend proxy
├── components/
│   ├── providers/               # SessionProvider + QueryProvider
│   ├── shared/                  # shell, states, DataTable, KPI/chart cards
│   └── ui/                      # Button, Input primitives
├── config/                      # navigation registry, module/permission/role codes
├── features/
│   ├── auth/                    # login form + Zod schema
│   ├── dashboard/               # executive BI aggregation + view
│   ├── finance/                 # A/R invoices + journal ledger
│   ├── inventory/               # warehouses + stock balances
│   ├── masterdata/              # business partners + items read APIs
│   └── sales/                   # sales orders + UC-01 confirmation
├── lib/
│   ├── api/                     # envelope types, client, BFF helpers, pagination
│   ├── auth/                    # cookies, backend calls, server session
│   ├── navigation/              # buildSidebar + collapse preference
│   ├── utils/                   # Decimal-safe money math + formatting
│   └── i18n/                    # typed English dictionary
├── test/                        # query/session/API test helpers
└── styles/tokens.css            # DESIGN.md §2.2 tokens
```

### Authentication (BFF)

Per `Security.md §2.5`, JWTs never reach browser JavaScript.

1. `POST /api/auth/login` (Next route handler) forwards credentials to
   `POST /api/v1/auth/login`, stores the access/refresh tokens in
   `httpOnly; Secure (prod); SameSite=Lax` cookies, and returns only the
   sanitized session payload.
2. `GET /api/auth/session` re-validates the session, transparently rotating the
   cookie pair when the access token has expired.
3. `POST /api/auth/refresh` and `POST /api/auth/logout` rotate/revoke tokens.
4. `ALL /api/proxy/*` forwards business calls to the backend with the access
   token attached, refreshing once on `401`. `auth/*` paths are blocked so
   tokens can never be read through the proxy.
5. Mutating BFF routes verify the `Origin` header (CSRF defense in addition to
   `SameSite=Lax`).

The browser-side `apiClient` (`src/lib/api/client.ts`) accepts documented
backend paths (e.g. `/api/v1/sales/orders`), rewrites them onto the proxy,
parses the standard `ApiResponse<T>` envelope and throws a typed
`ApiRequestError(code, message, traceId)` on `success: false`.

- `401` → silent refresh once, then retry once; on failure emit
  `omni:auth-unauthorized` (shell redirects to `/login`).
- `403 PLATFORM.MODULE_NOT_LICENSED` → emit `omni:module-not-licensed` and
  `omni:session-refresh`; render `ModuleNotLicensedState`.
- No tokens are written to `localStorage`/`sessionStorage`. The only stored
  value is the sidebar collapse preference.

### Dynamic navigation

`NAVIGATION_REGISTRY` declares all 10 core views with a Lucide icon, the
licensing `moduleCode`, and optional `permission`/`roles`. `buildSidebar`
filters these against `activeModules`, `permissions` and `roles` from the
session payload; the sidebar renders only its output. Filtering is UX only —
the backend re-enforces every check.

### Route guards

Each route wraps its content in `<ModuleGuard>`, which re-reads the session and
renders `ModuleNotLicensedState` (missing license) or `ForbiddenState` (missing
permission/role) instead of the page.

### Data layer and module views (Phase 4B)

`QueryProvider` (`src/components/providers/QueryProvider.tsx`) mounts a single
TanStack Query client inside the authenticated shell. Feature hooks live beside
their API types and key factories, return the documented `PagedResult<T>` shape
and are gated by both the active license and the read permission, so an OFF
module never fires a request.

| Hook                    | Endpoint                                   |
| ----------------------- | ------------------------------------------ |
| `useExecutiveDashboard` | composes the list APIs below               |
| `useBusinessPartners`   | `GET /api/v1/masterdata/business-partners` |
| `useItems`              | `GET /api/v1/masterdata/items`             |
| `useWarehouses`         | `GET /api/v1/inventory/warehouses`         |
| `useStockBalances`      | `GET /api/v1/inventory/stock/balances`     |
| `useSalesOrders`        | `GET /api/v1/sales/orders`                 |
| `useConfirmSalesOrder`  | `POST /api/v1/sales/orders/{id}/confirm`   |
| `useArInvoices`         | `GET /api/v1/finance/invoices`             |
| `useJournalEntries`     | `GET /api/v1/finance/journal-entries`      |

- **Dashboard** — KPI cards, a Recharts area chart with the teal `#14D6E4`
  glow gradient, a latest-events feed and licensed-module utilization. The
  backend has no dashboard read model yet, so the hook aggregates the
  documented list endpoints (no invented routes) over the newest 100 records.
- **Sales** — order table with status badges and the UC-01 confirmation dialog.
  A successful confirmation seeds the order detail cache and invalidates both
  the sales lists and the dashboard aggregate.
- **Inventory** — warehouse cards and a server-paginated stock-balance table,
  with the documented warehouse filter.
- **Finance** — A/R invoice and journal-ledger tables; the A/R section owns its
  own license/permission state so the GL ledger remains usable without `FIN_AR`.

`QuerySection<T>` enforces the five required states (loading, empty, error,
forbidden, module-not-licensed). `DataTable` owns server-side sort/pagination
controls, accessible `aria-sort` headers and a visually hidden caption.
All money math uses `decimal.js`; locale/currency defaults live in
`src/config/locale.ts`.

### Tests

Vitest + Testing Library run in jsdom with a fresh Query client per test.
Coverage includes the sales hooks (URL, paging, cache invalidation), the five
`QuerySection` states, `DataTable` sorting/pagination, and every module view
(loading, empty, error with traceId, forbidden, module-not-licensed and
happy-path rendering).

## Design system

- Exact 4-color palette from `DESIGN.md §2.2` is defined once in
  `src/styles/tokens.css` and mapped in `tailwind.config.ts`.
- No hard-coded colors or inline styles in components.
- Sidebar: charcoal `#4A3A3A`, 264px ↔ 72px, 300ms transition, teal active
  border + mint indicator; off-canvas drawer below `768px`.
- All user-visible strings come from `src/lib/i18n`.
- Required states are implemented: skeleton loading, empty, error (with
  copyable traceId), forbidden, module-not-licensed.

## Assumptions

- The backend session payload currently exposes `user.email` and tenant
  `name/code` (no `displayName`/branding yet), so the topbar uses those fields.
- `NavItem.roles` is an additive, any-of role gate for administrative views.
  The documented `moduleCode` + `permission` gates remain authoritative.
- Admin views are gated to `TENANT_ADMIN` because the backend contract has no
  platform-admin permission codes yet.
- `API_BASE_URL` defaults to `http://localhost:5000`.
- A nonce-based CSP is deferred to a hardening task; it requires per-request
  nonce generation. The other security headers are set in `next.config.ts`.
- Money is presented in a single currency (`IDR`) because the DTOs do not yet
  carry a currency code; `src/config/locale.ts` is the single replacement point.
- Until a dashboard read model exists, executive metrics aggregate the newest
  100 records per licensed list endpoint. The hook signature is designed to stay
  stable when the projection endpoint lands.
- "Cash collected" counts Paid A/R invoices in the current month. The invoice
  DTO has no `paidAt`, so `createdAt` is used as the collection timestamp.
- `DataTable` only offers sorting for columns accepted by the backend
  allow-lists. The stock-balance endpoint supports paging and warehouse/item
  filters only, so that table is intentionally unsorted.
