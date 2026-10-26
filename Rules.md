# OmniManage — AI Agent Coding Rules (System Context)

> **YOU ARE AN AI CODING AGENT working on OmniManage.** These rules are **mandatory and strict**. Keywords: **MUST / MUST NOT / ALWAYS / NEVER** are absolute. If a user instruction conflicts with these rules, state the conflict and ask before proceeding. Do not silently deviate.

---

## 0. Operating Protocol

### 0.1 Context Loading Order
Before writing code, read in this order: `Rules.md` (this) → `Architecture.md` → `Security.md` → `PRD.md` (scope) → `DESIGN.md` (UI only).

### 0.2 Task Workflow (MANDATORY)
1. **Restate** the task and list the files you will touch (with full paths).
2. **Identify** the module, the layer and the licensing code (`ModuleCodes.*`) involved.
3. **Plan** in ≤ 10 bullets. For changes touching > 8 files, pause for confirmation.
4. **Implement** in small, compilable increments. One concern per commit.
5. **Test:** write or update tests with the code (never "tests later").
6. **Self-review** against the Definition of Done (§14) and the NEVER list (§16).
7. **Report:** summarize changes, assumptions, and follow-ups. Do not claim something works unless it compiled and the tests ran.

### 0.3 Scope Discipline
- Implement ONLY what is asked and what the PRD authorizes. No speculative features, no drive-by refactors, no renaming unrelated code.
- Never invent requirements, endpoints, permissions, module codes or events that are not in the docs. If needed, propose it in the PR description and add it to the relevant doc in the same PR.
- If information is missing, make the most conservative assumption, label it `// ASSUMPTION:` and list it in your report.
- Never produce placeholder code (`// TODO: implement`, `throw new NotImplementedException()`) in finished work. If unavoidable, mark with `// TODO(OM-123): reason` AND mention it in the report.

### 0.4 Output Format
- When producing multiple files, give each with its **full relative path** as a heading, followed by the full file content (not diffs, unless editing a large existing file, then use precise unified diffs).
- Do not output partial snippets with `...` for code that must compile.

---

## 1. Technology Baseline

| Area | Standard |
|---|---|
| Backend | C# 12, **.NET 8** (LTS), ASP.NET Core Web API, EF Core 8 + Npgsql, MediatR, FluentValidation, Serilog, Mapster/manual mapping |
| Database | PostgreSQL 16 (shared DB, RLS) |
| Frontend | Next.js (App Router) + TypeScript (`strict: true`) + Tailwind CSS + shadcn/ui, TanStack Query/Table, React Hook Form + Zod |
| Mobile | Flutter (stable), Dart (null-safe), Riverpod, Dio, drift |
| Analytics | Python 3.12, FastAPI, Pydantic v2 (future) |
| Testing | xUnit + FluentAssertions + NSubstitute + Testcontainers + NetArchTest; Vitest + Testing Library + Playwright; `flutter_test` |

- Nullable reference types **enabled**. `TreatWarningsAsErrors = true`. Use `Directory.Packages.props` (central versions).
- **Do not add a new NuGet/npm/pub dependency without justification** in the PR (why, alternatives, license, maintenance status). Prefer the platform and existing libraries.
- **Forbidden:** AutoMapper, Newtonsoft.Json (use `System.Text.Json`), Moment.js, Redux (use TanStack Query + Zustand only if needed), `any` in TypeScript, `dynamic` in C#.

---

## 2. Architecture Rules (Backend)

1. **Layering** follows `Architecture.md §4.1`. Violations break the build.
2. **Modules reference other modules ONLY via `*.Contracts` projects.** NEVER reference another module's Domain/Application/Infrastructure/Presentation.
3. **One `DbContext` per module**, mapped to its own PostgreSQL schema. NEVER query another module's tables or DbContext. NEVER create cross-schema foreign keys (except to `platform.tenants`).
4. **Controllers are thin:** map the request → send a MediatR command/query → map the result → return the `ApiResponse` envelope. No business logic, no `DbContext`, no repositories in controllers.
5. **Business rules live in the Domain** (aggregate methods), not in handlers or controllers. Aggregates enforce invariants and raise domain events. No public setters on aggregates, so use factory methods and behavior methods.
6. **Handlers** orchestrate: load aggregate → call domain behavior → persist. One handler = one use case, folder-per-feature (`Orders/ConfirmOrder/…`).
7. **Commands return `Result`/`Result<T>`.** Queries return DTOs, never entities.
8. **Cross-module side effects ONLY via integration events** (Outbox). Never call another module's handler directly. Sync cross-module reads only via `Contracts` facades and must tolerate the target module being OFF.
9. **Integration events:** immutable `record`, name in past tense + `IntegrationEvent` suffix, versioned, carry `TenantId`/`EventId`, contain all data needed by consumers.
10. **Every integration event handler** declares its `RequiredModuleCode` and MUST be idempotent (inbox).
11. **Async all the way:** every I/O method is `async`, takes a `CancellationToken` (last parameter), and is awaited. NEVER use `.Result`, `.Wait()` or `async void` (except event handlers in UI frameworks).
12. **Dependency injection:** constructor injection (primary constructors allowed). NEVER use the service locator pattern (`IServiceProvider.GetService` in business code). Register through each module's `IModule`.
13. **Time and IDs:** inject `TimeProvider`/`IClock` and `IGuidGenerator`. NEVER call `DateTime.Now`/`DateTime.UtcNow`/`Guid.NewGuid()` directly in Domain/Application code.
14. **Money = `decimal`** (never `double`/`float`). Currency travels with the amount (`Money` value object). Rounding is explicit (`MidpointRounding.AwayFromZero` unless a policy says otherwise).

---

## 3. Multi-Tenancy & Licensing Rules (CRITICAL)

1. **Every business entity MUST implement `ITenantEntity`** and have a `tenant_id uuid NOT NULL` column + RLS policy (created by the migration helper `EnableTenantRls(table)`).
2. **NEVER** use `IgnoreQueryFilters()` outside `PlatformDbContext` or an explicitly reviewed, commented exception.
3. **NEVER** take `TenantId` from request bodies, query strings or headers on authenticated endpoints. Use `ITenantContext`.
4. Composite indexes **start with `tenant_id`**. Unique constraints include `tenant_id` (e.g., `UNIQUE (tenant_id, order_number)`).
5. **Every controller (and each action that differs) MUST have** `[RequireModule(ModuleCodes.X)]` or `[AllowWithoutModule]`. Every endpoint MUST have `[HasPermission("module.resource.action")]` or `[AllowAnonymous]`.
6. Module codes and permission codes are **constants** (`ModuleCodes`, `Permissions`). NEVER use string literals in attributes/handlers.
7. Cross-tenant background work MUST set `ITenantContext` per tenant iteration and check the license before running module jobs.
8. Any new feature **MUST declare** which `module_code` it belongs to, its hard/soft dependencies, and its behavior when a soft dependency is OFF (document the degraded behavior in code comments and the PR).
9. Add/extend the **tenant-isolation test** and **license-matrix test** for each new aggregate/endpoint.

---

## 4. Naming Conventions

### 4.1 C# / .NET
| Element | Convention | Example |
|---|---|---|
| Solution/Projects | `OmniManage.Modules.<Module>.<Layer>` | `OmniManage.Modules.Sales.Application` |
| Namespace | Mirrors folder/project | `OmniManage.Modules.Sales.Domain.Orders` |
| Class / record / struct / enum | PascalCase, noun | `SalesOrder`, `Money` |
| Interface | `I` + PascalCase | `ISalesOrderRepository` |
| Method | PascalCase, verb phrase | `Confirm()`, `GetByIdAsync()` |
| Async method | `Async` suffix (except handlers' `Handle` and controller actions) | `SaveAsync` |
| Property | PascalCase | `OrderNumber` |
| Private field | `_camelCase` | `_clock` |
| Parameter / local | camelCase | `salesOrderId` |
| Constant / static readonly | PascalCase | `MaxLineItems` |
| Generic type parameter | `T` or `TName` | `TEvent` |
| Command | `<Verb><Noun>Command` | `ConfirmOrderCommand` |
| Query | `Get<Noun>[By…]Query`, `List<Noun>Query` | `GetOrderByIdQuery` |
| Handler | `<Request>Handler` | `ConfirmOrderCommandHandler` |
| Validator | `<Request>Validator` | `ConfirmOrderCommandValidator` |
| Domain event | `<Noun><PastVerb>DomainEvent` | `SalesOrderConfirmedDomainEvent` |
| Integration event | `<Noun><PastVerb>IntegrationEvent` | `SalesOrderConfirmedIntegrationEvent` |
| DTO | `<Noun>Dto` / `<Verb><Noun>Request` / `<Noun>Response` | `OrderDto`, `CreateOrderRequest` |
| EF configuration | `<Entity>Configuration` | `SalesOrderConfiguration` |
| Controller | `<Plural>Controller` | `SalesOrdersController` |
| Test class / method | `<Subject>Tests` / `Method_Scenario_ExpectedResult` | `Confirm_WhenNoLines_ReturnsError` |
| Module code constant | `ModuleCodes.PascalCase` = `"UPPER_SNAKE"` | `ModuleCodes.HrisPayroll = "HRIS_PAYROLL"` |
| Permission code | `lower.dot.snake` | `finance.invoice.approve` |

**C# style:** file-scoped namespaces. One top-level type per file (small related records allowed). `sealed` by default on classes. `var` only when the type is obvious. Use `required`/`init` for DTOs. Prefer `record` for DTOs, commands, events and value objects. Expression-bodied members only for trivial members. No regions. Guard clauses first, with early returns. Methods ≤ 40 lines, classes ≤ 400 lines, cyclomatic complexity ≤ 10.

### 4.2 Database (PostgreSQL)
- `snake_case` for schemas, tables (plural), columns, indexes, constraints. Examples: `sales.sales_orders`, `tenant_id`, `ix_sales_orders_tenant_id_order_number`, `uq_…`, `fk_…`, `ck_…`.
- Boolean columns: `is_`/`has_` prefix. Timestamps: `_at` suffix (`timestamptz`). Dates: `_on` suffix (`date`).
- Use EF Core `UseSnakeCaseNamingConvention()`. Migrations are **forward-only**, reviewed, named `YYYYMMDDHHmm_<ModuleCode>_<Description>`, and backward-compatible (expand → migrate → contract). NEVER edit a merged migration.
- Never `SELECT *` in raw SQL. Never run DDL from application code.

### 4.3 TypeScript / Next.js
| Element | Convention | Example |
|---|---|---|
| Files (components) | `PascalCase.tsx` | `SalesOrderForm.tsx` |
| Files (non-component) | `kebab-case.ts` | `build-sidebar.ts`, `use-session.ts` |
| Folders | `kebab-case` | `features/sales/` |
| Components, types, interfaces, enums | PascalCase. **No `I` prefix** | `SalesOrder`, `OrderStatus` |
| Variables, functions, hooks | camelCase. Hooks start with `use` | `useSalesOrders` |
| Constants | `UPPER_SNAKE_CASE` | `MAX_PAGE_SIZE` |
| Zod schemas | `camelCase` + `Schema` | `createOrderSchema` |
| Event handlers | `handleX` (definition), `onX` (props) | `handleConfirm`, `onConfirm` |
| Booleans | `is/has/can/should` prefix | `isLoading`, `canApprove` |
| Route segments | `kebab-case` | `/sales/orders/[orderId]` |
| API query keys | factory in `features/<m>/api/keys.ts` | `salesKeys.order(id)` |

**TS rules:** `strict: true`, `noUncheckedIndexedAccess: true`. **No `any`** (use `unknown` + narrowing). No non-null assertions (`!`) without a justification comment. Named exports (default exports only for Next.js pages/layouts). Function components only, props typed with an explicit `type Props`. Server state via TanStack Query only (never `useEffect` + `fetch`). Forms via React Hook Form + Zod (`zodResolver`). **No inline styles, no hard-coded colors**, use Tailwind tokens from `DESIGN.md`. All user-visible strings go through i18n. No tokens in `localStorage`. Generated API types in `src/lib/api/generated/` are never edited by hand. Imports are ordered: external → `@/` aliases → relative. ESLint + Prettier are mandatory with zero warnings.

### 4.4 Dart / Flutter
`lower_snake_case` files and packages. `UpperCamelCase` types. `lowerCamelCase` members. Private members with a `_` prefix. Feature-first structure `features/<name>/{presentation,domain,data}`. Riverpod providers named `<thing>Provider`. No business logic in widgets. `const` constructors wherever possible. `flutter analyze` with zero issues.

---

## 5. API Conventions

### 5.1 Routing & Verbs
- Base: `/api/v{major}/<module>/<resource>`. Resources are plural kebab-case nouns: `/api/v1/sales/orders`, `/api/v1/hris/leave-requests`.
- State transitions are sub-resource actions (POST): `POST /sales/orders/{id}/confirm`. No verbs in resource names otherwise.
- `GET` safe/idempotent, `POST` create/action, `PUT` full replace, `PATCH` partial, `DELETE` remove/void. Return **201 + `Location`** on create.
- Versioning in the URL path. Breaking changes → new major version. Additive changes are allowed within a version.
- IDs in routes are `Guid` with a `:guid` constraint.

### 5.2 Pagination, Filtering, Sorting
- Query: `?page=1&pageSize=25&sort=-createdAt,orderNumber&search=…&status=Confirmed`. `pageSize` max **100** (server-enforced). Lists ALWAYS paginate.
- Response includes `meta.pagination { page, pageSize, totalItems, totalPages }`.
- Sort/filter fields are allow-listed. Never pass user-supplied field names to SQL/LINQ unchecked.

### 5.3 Idempotency & Concurrency
- Transaction-creating POSTs from mobile REQUIRE `Idempotency-Key` header (UUID). Duplicate → return the original response.
- Updates use optimistic concurrency (`rowVersion`/ETag with `If-Match`). Conflict → `409 CONCURRENCY_CONFLICT`.

### 5.4 Standard Response Envelope (MANDATORY for ALL responses, including 4xx/5xx)

```json
// Success
{
  "success": true,
  "data": { },
  "error": null,
  "meta": {
    "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
    "timestamp": "2026-10-21T08:15:30Z",
    "pagination": { "page": 1, "pageSize": 25, "totalItems": 134, "totalPages": 6 }
  }
}

// Error
{
  "success": false,
  "data": null,
  "error": {
    "code": "SALES.ORDER.INSUFFICIENT_CREDIT",
    "message": "Customer credit limit exceeded.",
    "details": [ { "field": "customerId", "code": "CREDIT_LIMIT", "message": "Limit 50,000,000; exposure 62,500,000" } ]
  },
  "meta": { "traceId": "…", "timestamp": "…" }
}
```

```csharp
// BuildingBlocks.Contracts
public sealed record ApiResponse<T>(bool Success, T? Data, ApiError? Error, ApiMeta Meta)
{
    public static ApiResponse<T> Ok(T data, ApiMeta meta) => new(true, data, null, meta);
    public static ApiResponse<T> Fail(ApiError error, ApiMeta meta) => new(false, default, error, meta);
}
public sealed record ApiError(string Code, string Message, IReadOnlyList<ApiErrorDetail>? Details = null);
public sealed record ApiErrorDetail(string? Field, string Code, string Message);
public sealed record ApiMeta(string TraceId, DateTime Timestamp, PaginationMeta? Pagination = null);
public sealed record PaginationMeta(int Page, int PageSize, long TotalItems, int TotalPages);
```
```ts
// frontend/src/lib/api/types.ts
export type ApiResponse<T> =
  | { success: true; data: T; error: null; meta: ApiMeta }
  | { success: false; data: null; error: ApiError; meta: ApiMeta };
export interface ApiError { code: string; message: string; details?: { field?: string; code: string; message: string }[] }
export interface ApiMeta { traceId: string; timestamp: string; pagination?: { page: number; pageSize: number; totalItems: number; totalPages: number } }
```
- Controllers inherit `ApiControllerBase`, which exposes `Ok<T>(…)`, `Created<T>(…)`, `Problem(Error)`. **NEVER return raw objects, entities, `ProblemDetails` or `IActionResult` without the envelope.**
- JSON: `camelCase`, enums as **strings**, dates ISO-8601 UTC, money/decimal as **strings** with fixed scale, ids as strings (GUID).

### 5.5 HTTP Status ↔ Error Mapping
| Situation | Status | Error code pattern |
|---|---|---|
| Validation failed | 400 | `VALIDATION.FAILED` (+ `details[]`) |
| Unauthenticated / token invalid | 401 | `AUTH.UNAUTHENTICATED`, `AUTH.TOKEN_EXPIRED` |
| Permission denied | 403 | `AUTH.FORBIDDEN` |
| **Module OFF for tenant** | **403** | **`PLATFORM.MODULE_NOT_LICENSED`** |
| Not found (incl. other tenant's resource) | 404 | `<MODULE>.<ENTITY>.NOT_FOUND` |
| Conflict / business-rule / state violation | 409 | `<MODULE>.<ENTITY>.<RULE>` |
| Concurrency | 409 | `CONCURRENCY_CONFLICT` |
| Unprocessable semantic (rare) | 422 | `<MODULE>.<RULE>` |
| Rate limited | 429 | `RATE_LIMIT.EXCEEDED` (+ `Retry-After`) |
| Unexpected | 500 | `SYSTEM.UNEXPECTED` (generic message + traceId) |
| Dependency down (license/DB) | 503 | `PLATFORM.LICENSE_UNAVAILABLE`, `SYSTEM.UNAVAILABLE` |

Error codes format: `MODULE.ENTITY.REASON` in `UPPER_SNAKE` segments. Every code is declared once in `<Module>.Domain/Errors/<Entity>Errors.cs` and documented in `docs/error-codes.md`. Do not return ad hoc strings.

---

## 6. Error Handling Rules

1. **Expected failures are values, not exceptions.** Use `Result<T>` with an `Error(Code, Message, Type)` for validation, not-found, conflict and business-rule outcomes.
```csharp
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
}
public enum ErrorType { None, Validation, NotFound, Conflict, Forbidden, Unauthorized, Failure }

public class Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }
    protected Result(bool ok, Error error) { /* invariant: ok ⇔ Error == None */ IsSuccess = ok; Error = error; }
    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error e) => new(false, e);
}
public sealed class Result<T> : Result { public T Value => IsSuccess ? _value! : throw new InvalidOperationException(); /* … */ }
```
2. **Exceptions are for the unexpected** (bugs, infrastructure faults). A single `GlobalExceptionMiddleware` converts them to the envelope (`500 SYSTEM.UNEXPECTED`) and logs with the traceId. Map known exceptions: `ValidationException` → 400, `DbUpdateConcurrencyException` → 409, `OperationCanceledException` → 499/aborted (no error log).
3. **NEVER** swallow exceptions (`catch { }`), `catch (Exception)` without rethrow/log/handle, or use exceptions for control flow. NEVER `throw ex;` (use `throw;`).
4. **Domain invariants** return `Result` from aggregate methods. Throw `DomainException` only for impossible/corrupt states.
5. **Never expose** internals: no stack traces, SQL, file paths, or entity names in client messages. The `message` is user-safe and localizable. Details go to logs by `traceId`.
6. **Validation layering:** syntactic (FluentValidation in Application) → business rules (Domain) → DB constraints (last line of defense). Constraint violations are mapped to 409 with a friendly code.
7. **Retries** only for idempotent operations, with exponential backoff + jitter (Polly). Outbox consumers: 5 attempts → dead-letter.
8. **Frontend:** a central `apiClient` parses the envelope. Throw a typed `ApiRequestError(code, message, traceId)` on `success: false`. Map `PLATFORM.MODULE_NOT_LICENSED` → `ModuleNotLicensedState` + session refresh. `AUTH.TOKEN_EXPIRED` → silent refresh once, then redirect to login. Every error UI shows a `traceId` copy action. Error boundaries per route segment (`error.tsx`).
9. **Mobile:** map the envelope to a sealed `Failure` hierarchy. Network failures on queued commands are retried automatically. 4xx business failures move the command to "Needs attention".

---

## 7. Logging & Observability Rules
- Use **structured logging** with message templates: `_logger.LogInformation("Order {OrderId} confirmed", id)`. NEVER string interpolation in log calls.
- Levels: `Debug` (dev only), `Information` (business milestones), `Warning` (handled anomalies, license denials), `Error` (failed operations), `Critical` (system integrity).
- NEVER log: passwords, tokens, secrets, full card/bank numbers, national IDs, salary figures, or full request bodies. Use `[Sensitive]` redaction.
- Every log carries `TenantId`, `UserId`, `TraceId` (enricher). New outbox/consumer code logs `EventId`, `EventType`, `Consumer`.

---

## 8. Validation & Security Coding Rules
(Full detail in `Security.md`. The following are enforced during code generation.)
- Validate ALL inputs with FluentValidation (backend) and Zod (frontend). Frontend validation is UX only. The backend is the authority.
- NEVER build SQL by string concatenation. NEVER bind request bodies to entities.
- NEVER store secrets, keys or connection strings in code, config files or tests. Use configuration providers and `.env.example` placeholders.
- NEVER disable TLS validation, CORS restrictions, CSRF protections or RLS "temporarily".
- Sensitive entity fields MUST use `[Encrypted]` (field encryption) and `[Sensitive]` (audit/log redaction). Mask in DTOs unless the user has `*.view_sensitive`.
- Authorization is checked **server-side, always**. Hiding a UI element is not authorization.
- Auditable aggregates MUST implement `IAuditable`. State-changing business actions MUST call `IAuditLogger`.

---

## 9. Persistence Rules
- Use repositories or `DbContext` only in Infrastructure/handlers per module convention. Queries use projection (`Select` to DTO) and `AsNoTracking()`.
- Avoid N+1 (use `Include` deliberately or projections). Always paginate. No unbounded `ToList()` on tenant tables.
- Each command handler = **one transaction** (`UnitOfWorkBehavior`). NEVER call `SaveChanges` inside loops.
- Financial & stock records are **append-only/reversible**: never `UPDATE`/`DELETE` posted ledger rows. Post a reversal.
- Numbering sequences use the `INumberSequence` service (DB-locked). Never `MAX()+1`.

---

## 10. Frontend Rules (Next.js)
1. Follow the structure in `DESIGN.md §8`. Feature code lives in `features/<module>/`. Shared UI in `components/shared`.
2. **Every route group declares its `moduleCode`** and the shell guards it. Missing module → `ModuleNotLicensedState`.
3. Sidebar is produced ONLY by `buildSidebar(NAVIGATION_REGISTRY, activeModules, permissions)`. New menu items MUST be added to the registry with `moduleCode` + `permission`.
4. Use `DataTable`, `LookupSelect`, `MoneyInput`, `StatusBadge`, etc. from the shared kit. NEVER re-implement them.
5. Every screen implements loading / empty / error / forbidden / not-licensed states.
6. Server Components by default. Use `"use client"` only when interactivity requires it.
7. Money values are strings from the API. Use `decimal.js`/`big.js` helpers for arithmetic. NEVER use JS floats for money.
8. Accessibility: semantic HTML, labels, keyboard support, focus management (see `DESIGN.md §2.6`).
9. Tests: component tests for logic-bearing components. Playwright for critical flows.

## 11. Mobile Rules (Flutter)
- Offline-capable flows (warehouse scanning, clock-in) MUST write to the local command queue first, then sync. Each command gets a UUIDv7 `Idempotency-Key`.
- Tokens only in secure storage. Offline DB encrypted. Biometric gate for payslips.
- Navigation tabs derive from `activeModules` + `permissions` from the session (same rules as the web).
- No business rules duplicated from the backend. The client displays server-provided state.

---

## 12. Testing Rules
- **Every new handler/aggregate behavior has unit tests**: success, each failure path, and edge cases. Name: `Method_Scenario_Expected`.
- **Every new endpoint has an integration test** (Testcontainers + WebApplicationFactory) covering: 200 path, validation 400, 401, 403 (permission), **403 `MODULE_NOT_LICENSED`**, and cross-tenant 404.
- **Every new integration event** has: a schema snapshot test, a consumer idempotency test (process twice → one effect), and a consumer test with the module OFF (→ `Skipped_ModuleDisabled`).
- Tests are deterministic (no real clock/network/random). Arrange-Act-Assert. One logical assertion focus per test.
- Coverage targets: Domain ≥ 90%, Application ≥ 80%. Do not lower the thresholds.
- Never delete or weaken a failing test to make a build pass. Fix the cause.

---

## 13. Git & Collaboration Standards

### 13.1 Branching
- Trunk-based with short-lived branches off `main`. `main` is always deployable and protected (PR + CI + 1 review).
- Branch name: `<type>/<ticket>-<kebab-description>` → `feat/OM-142-sales-order-confirm`, `fix/OM-201-ar-rounding`, `chore/OM-9-bump-efcore`.
- Types: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `build`, `ci`, `chore`, `revert`.

### 13.2 Commit Messages — Conventional Commits (MANDATORY)
```
<type>(<scope>): <imperative summary ≤ 72 chars, no period>

<body: WHAT and WHY (not how), wrapped at 100 cols>

<footer: BREAKING CHANGE: … | Refs: OM-142 | Co-authored-by: …>
```
- **Scope = module or area (lowercase):** `platform`, `masterdata`, `finance`, `procurement`, `sales`, `inventory`, `manufacturing`, `hris`, `crm`, `assets`, `web`, `mobile`, `analytics`, `infra`, `docs`, `deps`, `buildingblocks`.
- Examples:
  - `feat(sales): add confirm order command and outbox event`
  - `fix(finance): round AR tax lines half-away-from-zero`
  - `feat(platform)!: require reason when toggling tenant module` + `BREAKING CHANGE: PATCH /modules requires "reason"`
  - `test(inventory): cover shortage event when stock is insufficient`
- One logical change per commit. The build and tests pass at every commit. NEVER commit commented-out code, secrets, generated artifacts, or `.env` files. Do not mix refactors with behavior changes. Do not use `--no-verify`.
- Migrations and the code that needs them go in the same PR (separate commits OK).

### 13.3 Pull Requests
Title follows Conventional Commits. Description MUST include: **Summary · Module & `module_code` · Dependencies / behavior when soft deps OFF · Tenant-isolation & license tests added (Y/N) · Security considerations · Migrations (Y/N, backward-compatible) · Screenshots (UI) · Assumptions made**. Keep PRs ≤ 400 changed lines where feasible. Squash-merge with a Conventional Commit title.

---

## 14. Definition of Done (checklist the agent MUST self-verify)
- [ ] Compiles with zero warnings. Lint/format pass (`dotnet format`, ESLint, Prettier, `flutter analyze`).
- [ ] Layering and module-boundary architecture tests pass.
- [ ] Endpoint has `[RequireModule]` + `[HasPermission]`. Uses constants.
- [ ] Entities implement `ITenantEntity`. RLS enabled in the migration. Indexes lead with `tenant_id`.
- [ ] Response uses the `ApiResponse` envelope. Errors use catalogued codes.
- [ ] Inputs validated. No sensitive data logged. Sensitive fields encrypted/masked.
- [ ] Auditable actions audited.
- [ ] Unit + integration tests added (incl. 403 module test and cross-tenant test).
- [ ] Integration events versioned, idempotent consumers, module-OFF behavior defined.
- [ ] UI has all required states and i18n strings, and uses design tokens.
- [ ] OpenAPI regenerated, docs updated (`error-codes.md`, event catalogue, permissions seed).
- [ ] Conventional Commit messages. PR description complete.

---

## 15. Canonical Reference Snippets

**Controller skeleton**
```csharp
[ApiController]
[Route("api/v1/sales/orders")]
[RequireModule(ModuleCodes.SalesCore)]
public sealed class SalesOrdersController(ISender sender) : ApiControllerBase
{
    [HttpPost("{id:guid}/confirm")]
    [HasPermission(Permissions.Sales.OrderConfirm)]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
        => FromResult(await sender.Send(new ConfirmOrderCommand(id), ct));
}
```

**Handler skeleton**
```csharp
internal sealed class ConfirmOrderCommandHandler(
    ISalesOrderRepository orders, IFinanceModuleApi finance, TimeProvider clock)
    : ICommandHandler<ConfirmOrderCommand, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(ConfirmOrderCommand cmd, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(cmd.OrderId, ct);
        if (order is null) return SalesOrderErrors.NotFound(cmd.OrderId);

        var exposure = await finance.TryGetCreditExposureAsync(order.CustomerId, ct); // tolerant: null if FIN_AR OFF
        var result = order.Confirm(exposure, clock.GetUtcNow());                       // domain rules + raises domain event
        if (result.IsFailure) return result.Error;

        await orders.UpdateAsync(order, ct);   // UoW behavior commits order + outbox atomically
        return order.ToDto();
    }
}
```

**Aggregate skeleton**
```csharp
public sealed class SalesOrder : AggregateRoot<Guid>, ITenantEntity, IAuditable
{
    private readonly List<SalesOrderLine> _lines = [];
    public Guid TenantId { get; private set; }
    public SalesOrderStatus Status { get; private set; }
    public IReadOnlyList<SalesOrderLine> Lines => _lines;

    public Result Confirm(CreditExposure? exposure, DateTimeOffset now)
    {
        if (Status != SalesOrderStatus.Draft) return SalesOrderErrors.InvalidState(Status);
        if (_lines.Count == 0) return SalesOrderErrors.NoLines;
        if (exposure is { LimitExceeded: true }) return SalesOrderErrors.InsufficientCredit(exposure);
        Status = SalesOrderStatus.Confirmed;
        Raise(new SalesOrderConfirmedDomainEvent(Id, TenantId, now));
        return Result.Success();
    }
}
```

---

## 16. NEVER List (Hard Stops)
1. NEVER reference another module's internals (only `Contracts`).
2. NEVER read/write another module's tables or `DbContext`.
3. NEVER omit `tenant_id`, RLS, `[RequireModule]` or `[HasPermission]`.
4. NEVER take tenant identity from client-supplied input on authenticated routes.
5. NEVER put business logic in controllers, EF configurations or the frontend.
6. NEVER return entities or non-enveloped responses from the API.
7. NEVER use `float`/`double` for money. NEVER `DateTime.Now`/`Guid.NewGuid()` in domain/application code.
8. NEVER swallow exceptions, log secrets/PII, or leak internals in errors.
9. NEVER store tokens in `localStorage`/`sessionStorage` or log tokens.
10. NEVER hard-code module codes, permission strings, colors, or user-visible strings.
11. NEVER delete/update posted financial or stock-ledger rows. Reverse them instead.
12. NEVER add dependencies, modules, permissions, endpoints or events outside the documented scope without flagging it.
13. NEVER disable security controls, tests, analyzers or hooks to get unblocked.
14. NEVER use `any` (TS), `dynamic` (C#), `.Result`/`.Wait()` or `async void`.
15. NEVER force-push `main`, rewrite shared history, or commit secrets/`.env`.
16. NEVER claim completion without compiling and running the relevant tests.