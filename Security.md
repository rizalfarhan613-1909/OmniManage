# OmniManage — Security Specification

> **AI agent directive:** Security requirements are non-negotiable and override convenience. If a task conflicts with this document, STOP and flag the conflict. Never weaken a control to make a test pass.

---

## 1. Security Principles & Threat Model

**Principles:** Zero trust between layers. Least privilege. Fail closed. Defense in depth. Secure by default. Everything auditable.

| STRIDE | Primary Threat | Controls |
|---|---|---|
| Spoofing | Stolen credentials/tokens, **GPS/Location spoofing, Fake attendance selfies** | MFA, short-lived JWT, refresh rotation + reuse detection, device binding (mobile), **payload signatures for attendance, EXIF metadata validation** |
| Tampering | Modified data/events | RLS, validation, signed tokens, append-only audit with hash chain, optimistic concurrency |
| Repudiation | "I didn't do that" | Immutable audit trail with actor, trace ID, IP, before/after |
| Information disclosure | **Cross-tenant leakage**, PII exposure | EF filter + RLS, field-level encryption, response DTO allow-lists, log scrubbing |
| Denial of service | Abusive tenants/clients | Rate limiting, per-tenant quotas, request size limits, pagination caps |
| Elevation of privilege | Accessing unlicensed modules or other roles' features | Module license middleware + permission policies + server-side re-checks |

**Top-priority risk:** cross-tenant data leakage. Tenant isolation has dedicated automated tests (see `Architecture.md §16`).

---

## 2. Authentication

### 2.1 Identity Model
- **Two identity realms, kept separate:**
  1. **Platform realm**: Superadmin users (`platform.platform_users`), audience `omnimanage-platform`. MFA mandatory.
  2. **Tenant realm**: tenant users (`platform.users` with `tenant_id`), audience `omnimanage-tenant`.
- Tokens from one realm are never valid in the other (separate `aud`, separate signing key IDs).
- MVP: first-party issuance with **ASP.NET Core Identity** (user store only) and JWT bearer tokens. Optional **OIDC federation** (Microsoft Entra ID / Google) per tenant, using Authorization Code + PKCE. The design must allow migrating issuance to a dedicated OIDC server (OpenIddict/Duende) without changing resource-server validation.

### 2.2 Password & Account Policy
- Hash: **Argon2id** (memory ≥ 64 MiB, iterations ≥ 3, parallelism ≥ 2) via a custom `IPasswordHasher<T>`. Transparent rehash on login if parameters are outdated.
- Minimum 12 characters, check against a breached-password list (k-anonymity API or offline list). No composition theater and no forced periodic rotation.
- Lockout: 5 failed attempts → 15-minute progressive lockout. Generic error messages ("Invalid credentials") to prevent user enumeration. Uniform response timing.
- **MFA:** TOTP (RFC 6238) with recovery codes. Mandatory for Superadmin, Tenant Admin and Finance Manager. Tenant-configurable for others. WebAuthn/passkeys in Phase 2.
- Password reset: single-use, 30-minute, high-entropy token, hashed at rest. Invalidates all sessions on completion.

### 2.3 JWT Specification
| Item | Value |
|---|---|
| Algorithm | **ES256** (or RS256). **Never** `none` or HS256 with shared secrets across services |
| Keys | Asymmetric key pair in a KMS/Key Vault, with `kid` header, rotated every 90 days (overlap window supported via JWKS) |
| Access token TTL | **15 minutes** |
| Refresh token TTL | **7 days** sliding, **30 days** absolute (mobile); web 12 h idle |
| Validation | issuer, audience, lifetime (clock skew ≤ 30 s), signature, `kid` |

**Access token claims (minimal, no PII):**
```json
{
  "iss": "[https://auth.omnimanage.io](https://auth.omnimanage.io)",
  "aud": "omnimanage-tenant",
  "sub": "user-uuid",
  "tenant_id": "tenant-uuid",
  "sid": "session-uuid",
  "roles": ["FINANCE_MANAGER"],
  "amr": ["pwd", "otp"],
  "iat": 0, "exp": 0, "jti": "uuid"
}

```

* **Permissions and active modules are NOT embedded in the JWT** (avoids stale/oversized tokens and stale licensing). They are resolved server-side per request from cache (`license:{tenant}`, `perm:{user}`), and returned to clients in the session payload for UX only.

### 2.4 Refresh Token Handling

* Opaque random 256-bit tokens, stored **hashed** (SHA-256) in `platform.refresh_tokens` with `session_id`, `device_id`, `ip`, `user_agent`, `expires_at`, `replaced_by`.
* **Rotation on every use.** Presenting an already-used token = theft signal → revoke the entire session family and force re-login and alert.
* Logout revokes the session. "Log out everywhere" revokes all sessions. Disabling a user or tenant suspension revokes all sessions immediately.

### 2.5 Client Token Storage

| Client | Storage | Rules |
| --- | --- | --- |
| Web (Next.js) | **BFF**: tokens only in `httpOnly; Secure; SameSite=Lax` cookies set by Next.js route handlers | **Never** `localStorage`/`sessionStorage` for tokens. CSRF: double-submit token or `SameSite` + Origin check on mutating requests |
| Mobile (Flutter) | iOS Keychain / Android Keystore via `flutter_secure_storage` | Biometric gate for sensitive screens (Payslips). Certificate pinning for production. Root/jailbreak detection (warn + policy) |

### 2.6 OAuth/OIDC Federation (optional per tenant)

Authorization Code + **PKCE** only (no implicit, no ROPC). Validate `state`, `nonce`, `iss`, `aud`. Map external identity → local user by verified email + provider subject (`sub`), with Tenant Admin pre-provisioning (no auto-create from unknown domains unless explicitly enabled).

---

## 3. Authorization — RBAC

### 3.1 Model

```
User ──< UserRole >── Role ──< RolePermission >── Permission (code: module.resource.action)

```

* **Permissions are fixed by the platform** (seeded, versioned, developer-defined). **Roles are tenant-defined** compositions of permissions. 8 **system roles** are seeded per tenant (immutable names, editable permission sets only by the Tenant Admin, except the Tenant Admin role, which always holds all tenant permissions).
* Permission code format: `<module>.<resource>.<action>`, e.g., `finance.invoice.approve`, `hris.employee.view_sensitive`, `inventory.stock.adjust`.
* Effective permissions = union of the roles' permissions **∩ permissions whose module is ACTIVE for the tenant**. A permission for a disabled module is never granted.

### 3.2 Default Role–Permission Matrix (system roles; C=create R=read U=update D=delete/void A=approve X=execute/special)

| Resource | Tenant Admin | HR Mgr | Finance Mgr | Sales Rep | Warehouse Op | Production Mgr | Project Mgr | General Staff |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Tenant settings / users / roles | CRUDA | – | – | – | – | – | – | – |
| Employee master (basic) | R | CRUD | R (limited) | – | – | – | R | R (self) |
| Employee sensitive fields (salary, ID, bank) | R | R/U | – | – | – | – | – | R (self) |
| Attendance | R | CRUDA | – | – | – | – | R (team) | C/R (self) |
| **Smart Attendance (GPS/Selfie)** | R | CRUDA | – | – | – | – | R (team) | C/R (self) |
| Leave | R | RA | – | – | – | – | RA (team) | C/R (self) |
| Payroll run / payslips | R | CRXA | R (journal) | – | – | – | – | R (own payslip) |
| Journals / GL | R | – | CRUA | – | – | – | – | – |
| A/R invoices & receipts | R | – | CRUDA | R (own customers) | – | – | – | – |
| A/P bills & payments | R | – | CRUDA | – | – | – | – | – |
| Customers / CRM | R | – | R | CRU (own) | – | – | – | – |
| Sales orders | R | – | R | CRU + confirm (own) | R | R | – | – |
| Stock ledger / levels | R | – | R | R | CRU (receive/pick/count) | R | – | – |
| Stock adjustments | R | – | A | – | C | – | – | – |
| Purchase orders | R | – | RA | – | R (receiving) | C (requisition) | – | – |
| BOM / MRP | R | – | – | – | R | CRUD | – | – |
| **Project Tasks & Work Logs** | R | – | – | – | – | – | CRUDA | CRU (own) |
| **Asset Registry & Maint.** | R | – | R | – | R | CRU | R | R (assigned) |
| Dashboards | Full | HR | Finance | Sales | Warehouse | Production | Projects | Self |
| Audit logs (tenant) | R | – | – | – | – | – | – | – |

> The authoritative matrix is the seed file `Permissions.seed.json` kept in the Platform module. This table is its human-readable summary.

### 3.3 Enforcement in .NET 8

```csharp
// Policy-based, dynamic: no per-permission policy registration.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute(string permission) : AuthorizeAttribute($"perm:{permission}")
{
    public string Permission { get; } = permission;
}

public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider { /* builds a policy from "perm:*" names */ }

public sealed class PermissionHandler(IPermissionService permissions, ITenantContext tenant)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext ctx, PermissionRequirement req)
    {
        var userId = ctx.User.GetUserId();
        if (await permissions.HasAsync(tenant.TenantId, userId, req.Permission)) ctx.Succeed(req);
        // else: no Succeed → 403
    }
}

```

* **Default deny:** a global fallback policy requires an authenticated user. Every endpoint declares `[HasPermission]` or an explicit `[AllowAnonymous]`. Architecture tests enforce it.
* **Resource/data-scope rules (ABAC layer)** applied in handlers, not controllers: `Own` (employee = self), `Team` (manager → direct reports), `Department`, `All`. Example: Sales Reps read only customers they own unless granted `crm.customer.read_all`.
* **Approval limits:** `finance.payment.approve` has a per-role monetary limit evaluated in the Approval engine.
* **Segregation of duties:** the creator of a payment/journal cannot approve it. Payroll preparer ≠ payroll approver.

### 3.4 Superadmin Boundaries

* Superadmin APIs live under `/api/v1/platform/**` and require platform-realm tokens, plus MFA.
* Superadmin **cannot read tenant business data**. Troubleshooting is through **Support Mode** (Phase 2): the Tenant Admin grants a time-boxed (≤ 4 h) read-only grant. Every action is tagged `impersonated_by` in the audit log. The tenant is notified.

---

## 4. Dynamic Module Authorization (.NET 8 Middleware)

### 4.1 Strategy

Three enforcement layers (all must agree):

1. **Edge (middleware):** blocks HTTP calls to endpoints decorated with `[RequireModule]` when the module is OFF → `403 MODULE_NOT_LICENSED`.
2. **Application (pipeline behavior / event-consumer guard):** blocks jobs, consumers and internal callers.
3. **UI:** hides navigation (convenience only).

### 4.2 Attribute (metadata only, no logic)

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireModuleAttribute(string moduleCode) : Attribute
{
    public string ModuleCode { get; } = moduleCode;   // constants from ModuleCodes.*
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowWithoutModuleAttribute : Attribute { }   // platform/auth/health only

```

Usage:

```csharp
[ApiController]
[Route("api/v1/hris/payroll")]
[RequireModule(ModuleCodes.HrisPayroll)]            // class-level default
public sealed class PayrollController(ISender sender) : ApiControllerBase
{
    [HttpPost("runs/{id:guid}/finalize")]
    [HasPermission("hris.payroll.approve")]
    public async Task<IActionResult> Finalize(Guid id, CancellationToken ct) => Ok(await sender.Send(new FinalizePayrollRunCommand(id), ct));
}

```

### 4.3 Middleware

```csharp
public sealed class ModuleLicenseMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx, ITenantContext tenant, ITenantModuleCache licenses, IApiResponseWriter writer)
    {
        var requirements = ctx.GetEndpoint()?.Metadata.GetOrderedMetadata<RequireModuleAttribute>();

        if (requirements is { Count: > 0 })
        {
            if (!ctx.User.Identity?.IsAuthenticated ?? true)      // let AuthN decide first
            { await next(ctx); return; }

            LicenseSnapshot snapshot;
            try { snapshot = await licenses.GetAsync(tenant.TenantId, ctx.RequestAborted); }
            catch (LicenseSourceUnavailableException)
            {
                await writer.WriteErrorAsync(ctx, 503, "PLATFORM.LICENSE_UNAVAILABLE", "License service unavailable."); // fail closed
                return;
            }

            foreach (var req in requirements)       // ALL declared modules must be active
            {
                if (!snapshot.IsActive(req.ModuleCode))
                {
                    await writer.WriteErrorAsync(ctx, StatusCodes.Status403Forbidden,
                        "PLATFORM.MODULE_NOT_LICENSED",
                        $"Module '{req.ModuleCode}' is not active for this organization.",
                        details: new { moduleCode = req.ModuleCode });
                    return;                          // short-circuit
                }
            }
        }
        await next(ctx);
    }
}

```

**Rules**

* `IsActive` = `is_active AND (expires_at IS NULL OR expires_at > UtcNow)`.
* Applied after authentication (so the tenant is trustworthy) and before authorization (cheap check first, no permission lookups for unlicensed modules).
* Module codes are **constants** (`ModuleCodes`), never string literals. Unknown codes fail the architecture test.
* Responses must not reveal other modules or pricing. Include only the requested `moduleCode`.
* Log every denial at `Information` with `tenant_id`, `module_code`, `user_id` (metric `license_denied_total`).
* `Every_Controller_Should_Have_RequireModule_Or_AllowWithoutModule` is a CI gate.

---

## 5. Data Protection

### 5.1 In Transit

* TLS 1.2+ (prefer 1.3) everywhere: client↔API, API↔DB (`sslmode=verify-full`), API↔Redis (TLS), API↔storage. HSTS (`max-age ≥ 31536000; includeSubDomains`). No mixed content.
* Mobile: certificate/SPKI pinning with a backup pin.

### 5.2 At Rest

* Database, backups and object storage encrypted with AES-256 (provider-managed keys minimum, **customer-managed keys (KMS)** for enterprise tier).
* **Object Storage Security (S3/MinIO):** All Phase 6 media (attendance selfies, receipt proofs, asset QR codes) stored in S3/MinIO are strictly isolated per tenant via pathing and accessed exclusively through short-lived signed URLs. Public bucket access is explicitly denied.
* **Field-level (application) encryption** for high-sensitivity columns: national ID, tax ID, bank account numbers, salary components, MFA secrets, third-party API credentials.
* **Envelope encryption:** per-tenant Data Encryption Key (DEK, AES-256-GCM), wrapped by a KEK in KMS. DEKs are cached in memory with short TTL.
* Ciphertext format: `v1:{keyId}:{nonce}:{ciphertext}:{tag}`. Supports key rotation and lazy re-encryption.
* Searchable fields: store a **blind index** (HMAC-SHA-256 with a separate key) alongside the ciphertext. Never use deterministic encryption for low-entropy data.



```csharp
public interface IFieldEncryptor { string Encrypt(string plaintext, Guid tenantId); string Decrypt(string ciphertext, Guid tenantId); }
// Applied via EF Core ValueConverter on properties marked [Encrypted]. Never inline ad hoc.

```

* Sensitive fields are **masked by default** in API responses (`****1234`) and revealed only with `*.view_sensitive` permission (each reveal is audited).

### 5.3 Secrets Management

* No secrets in source, images or CI logs. Use a secret manager (Azure Key Vault / AWS Secrets Manager / Vault) and environment injection. Rotate DB credentials and signing keys on schedule. Pre-commit and CI **secret scanning** (gitleaks).
* Local dev uses `dotnet user-secrets` / `.env` (git-ignored). Provide `.env.example` with placeholders only.

### 5.4 Data Privacy & Retention

* Classify data: **Public / Internal / Confidential / Restricted (PII, payroll, bank, biometric/facial data, GPS tracking logs)**. Restricted data requires field encryption and audited access.
* Support data-subject rights (access/export/rectification/erasure where legally permitted). Financial records may be legally retained, so use pseudonymization instead of deletion.
* Tenant offboarding: export → grace period → cryptographic erasure (destroy the tenant DEK) → purge.
* Logs and analytics exclude Restricted data.

---

## 6. Audit Trail

### 6.1 What Is Audited

| Category | Examples | Detail |
| --- | --- | --- |
| Data changes | Any insert/update/delete on audited entities | Before/after JSON diff (changed fields only), entity, id |
| Business actions | Confirm order, approve payment, finalize payroll, post journal, **submit smart attendance, complete task, request maintenance** | Action code + parameters |
| Security events | Login success/fail, MFA, password change, token reuse, lockout, role/permission change | IP, user-agent, device |
| Licensing | Module toggle, tenant suspend | Old/new value, **reason** |
| Sensitive reads | Salary/ID reveal, exports, Support Mode access | Who, what, why |
| Integration | Dead-lettered/replayed events | Event id, handler |

### 6.2 Schema

```sql
CREATE TABLE audit.audit_logs (
    id           uuid        NOT NULL,
    tenant_id    uuid,                       -- NULL for platform-level events
    occurred_at  timestamptz NOT NULL DEFAULT now(),
    actor_id     uuid,
    actor_type   varchar(20) NOT NULL,       -- User | System | Superadmin | Support
    impersonated_by uuid,
    action       varchar(100) NOT NULL,      -- e.g. sales.order.confirmed
    module_code  varchar(50),
    entity_type  varchar(100),
    entity_id    varchar(100),
    before_data  jsonb,
    after_data   jsonb,
    ip_address   inet,
    user_agent   text,
    trace_id     varchar(64),
    prev_hash    bytea,                      -- tamper-evidence chain (per tenant)
    row_hash     bytea,
    PRIMARY KEY (id, occurred_at)
) PARTITION BY RANGE (occurred_at);

```

* **Append-only:** `omni_app` has `INSERT, SELECT` only on `audit.audit_logs` (no `UPDATE/DELETE`). A trigger rejects mutations. Monthly partitions. Archive to immutable (WORM) storage after N months.
* **Tamper-evidence:** `row_hash = SHA256(prev_hash || canonical_json(row))` per tenant chain. A scheduled job verifies chain integrity and alerts on a break.
* Redact before storage: passwords, tokens, and Restricted fields are stored as `"[REDACTED]"` or hashed values.

### 6.3 Capture Mechanism

```csharp
public sealed class AuditSaveChangesInterceptor(IAuditContext ctx, IAuditWriter writer) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData e, InterceptionResult<int> result, CancellationToken ct = default)
    {
        var entries = e.Context!.ChangeTracker.Entries()
            .Where(x => x.Entity is IAuditable && x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
        foreach (var entry in entries)
            writer.Enqueue(AuditEntryFactory.From(entry, ctx));   // diff only changed props; redact [Sensitive]
        return result;
    }
    // After successful commit (SavedChangesAsync) the pending audit entries are persisted in the SAME transaction
    // via the outbox-style writer, so no audit row exists without its change, and no change exists without its audit row.
}

```

* Business-action audit entries are emitted explicitly through `IAuditLogger.LogAsync(...)` in handlers for key operations.
* Audit logging failures **must fail the operation** for financial/security-critical actions (fail closed).
* Viewing: Tenant Admin UI with filters (user, entity, date, action) and export. Each export is itself audited.

---

## 7. API & Application Security Controls

| Area | Requirement |
| --- | --- |
| Input validation | FluentValidation on every command. Reject unknown JSON fields on critical endpoints. Enforce max lengths, ranges, enums, decimal scale. Never trust client-computed totals |
| SQL injection | EF Core/parameterized queries only. Raw SQL via `FromSqlInterpolated`/Dapper parameters. String-concatenated SQL is forbidden |
| XSS | React escaping by default. `dangerouslySetInnerHTML` is forbidden (sanitize with DOMPurify if unavoidable). Strict **CSP** (no inline scripts, nonce-based) |
| CSRF | `SameSite` cookies + Origin/Referer verification + anti-forgery token for cookie-auth mutating calls |
| CORS | Explicit allow-list of origins. Never `*` with credentials |
| Security headers | `Strict-Transport-Security`, `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`, `Permissions-Policy`, `frame-ancestors 'none'` |
| Rate limiting | .NET `RateLimiter`: auth endpoints 5/min/IP+account. API 600/min/user. Per-tenant global quota. Stricter on password reset/MFA |
| Idempotency | `Idempotency-Key` required on mobile POSTs that create transactions; stored 24 h |
| File uploads | Allow-list MIME + extension, magic-byte sniffing, size limits, AV scan, random storage keys, served via signed URLs from a separate domain, `Content-Disposition: attachment`, **EXIF data stripped (especially for attendance selfies) to prevent location metadata leakage or tampering.** |
| **Anti-Spoofing (Phase 6)** | **Mobile Smart Attendance payloads (GPS coordinates, timestamps) must be signed at the device level. API rejects impossible travel or time-travel heuristics.** |
| IDOR/BOLA | Every lookup by id goes through tenant filter **and** data-scope check. Return `404` (not `403`) for other tenants' resources |
| Mass assignment | Never bind request bodies to entities. Use explicit command DTOs |
| Error leakage | No stack traces or SQL errors to clients. Return `traceId` only |
| Dependency security | Dependabot/Renovate, `dotnet list package --vulnerable`, `npm audit`, SBOM (CycloneDX), pinned versions |
| SSRF | Outbound calls (webhooks) restricted to allow-listed schemes/ports with private-IP blocking |
| Mobile | No sensitive data in logs/screenshots (secure flag), encrypted local DB (SQLCipher) for the offline queue, wipe on logout |

## 8. Infrastructure Security

* Network: private subnets for DB/Redis. WAF in front of the API. DDoS protection. Only 443 exposed.
* Least-privilege service accounts. Separate environments (dev/stage/prod) with separate secrets and data. **No production data in non-prod.**
* Containers: non-root user, read-only FS, minimal base images, image scanning (Trivy), signed images.
* Backups: encrypted, tested restore monthly, PITR enabled.

## 9. Security in the SDLC

* PR checks: SAST (CodeQL/Semgrep), secret scan, dependency scan, architecture tests, tenant-isolation tests, license-matrix tests.
* DAST (OWASP ZAP) on staging nightly. External penetration test before GA and annually.
* Threat model review for each new module and for every new integration event that carries PII.
* Security-relevant changes require CODEOWNERS review by the security owner.

## 10. Incident Response & Monitoring

* Alerts: token-reuse detection, spike in 401/403, `license_denied_total` anomalies, RLS violation errors, audit-chain break, dead-letter spikes, impossible-travel logins.
* Severity runbooks (SEV1–SEV4), 72-hour breach-notification workflow (per applicable regulation), post-incident review within 5 business days.
* Security contact and vulnerability disclosure policy published (`/.well-known/security.txt`).

## 11. Compliance Alignment

Design baseline: **OWASP ASVS Level 2**, aligned to **ISO/IEC 27001** controls and **SOC 2** trust criteria. Data-protection obligations (GDPR and local equivalents) are handled via classification, consent records, DSAR tooling and retention policies. Region-specific payroll/tax data handling is documented per localization pack.

```

```