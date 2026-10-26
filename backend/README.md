# OmniManage Backend

Phase 5B modular monolith backend for OmniManage.

## Prerequisites

- .NET SDK 8.0.4xx
- PostgreSQL 16

## Commands

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build
dotnet run --project src/Host/OmniManage.Api --no-launch-profile
```

For the full local stack (Docker, migrations, seed data), see the repository
root `README.md` and `run.ps1`.

## Configuration

`PostgreSql:ConnectionString` in `appsettings.json` points at the local
`omnimanage_db` database with the non-owner runtime role `omni_app`. Local
Docker uses trust authentication, so no password is committed. Supply a real
password from the environment (or a secret manager) for any shared database:

```powershell
$env:PostgreSql__ConnectionString = "Host=localhost;Port=5433;Database=omnimanage_db;Username=omni_app;Password=<from-secret-store>;SslMode=VerifyFull"
```

The host fails fast at startup if `PostgreSql:ConnectionString` is empty or if
`PostgreSql:MajorVersion` is not `16`.

### JWT signing key

Access tokens are signed with RS256. `Jwt:PrivateKeyPem` must contain a
PEM-encoded RSA private key (PKCS#8 `BEGIN PRIVATE KEY` or PKCS#1
`BEGIN RSA PRIVATE KEY`) and must never be committed. Outside Development the
host fails fast if the value is empty or cannot be imported. In Development an
ephemeral key is generated in memory at startup when the value is missing, and
a warning is logged; tokens stop validating after a restart. The matching
public key is distributed to token consumers; `Jwt:KeyId` must identify it.

```powershell
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out jwt-private.pem
$env:Jwt__PrivateKeyPem = Get-Content -Raw jwt-private.pem
```

`Jwt:Issuer`, `Jwt:Audience` and `Jwt:AccessTokenMinutes` (default `15`) are
non-secret and live in `appsettings.json`. Tokens carry the tenant in the
`tenant_id` claim and the user in `sub`; both are required by the license
middleware and the session endpoint.

### Password hashing

Passwords are hashed with Argon2id (64 MiB memory, 3 iterations, parallelism 2)
using the PHC string format. Legacy ASP.NET Core Identity PBKDF2 hashes remain
verifiable and are transparently rehashed with Argon2id on the next successful
login (`Security.md §2.2`).

## File storage and exports

`IStorageService` and `IExportEngine` are registered by
`AddOmniManageInfrastructure`. `Storage:Provider` defaults to `Local`
(`Storage:LocalRootPath`, ignored by git) for development. Set it to `S3` to use
AWS S3 or an S3-compatible endpoint such as MinIO or Cloudflare R2 with
`Storage:BucketName`, `Storage:Region`, `Storage:ServiceUrl`,
`Storage:AccessKey`/`Storage:SecretKey` and `Storage:ForcePathStyle`. When
credentials are omitted the AWS default credential chain is used. Bucket-level
public access must stay disabled; files are only reachable through short-lived
pre-signed URLs and are keyed by
`{tenant_id}/{module_code}/{year}/{month}/{file_id}.ext`. The local fallback
returns `file://` URIs and is intended for development only.

`IExportEngine` renders PDF documents from HTML templates with QuestPDF and
writes Excel workbooks with MiniExcel's streaming writer. Generated files are
buffered in temporary files that are deleted when the returned stream is
disposed.

## Database structure

Each module owns one `DbContext`, one PostgreSQL schema and one
`__EFMigrationsHistory` table in that schema. Runtime connections must use the
non-owner role `omni_app`; migrations run as `omni_migrator` in CI/CD. RLS is
enforced by setting `app.tenant_id` on every opened connection via
`TenantConnectionInterceptor`, and EF Core global query filters provide the
application-level layer. Module contexts must register
`AddTenantIsolation(tenantContext)` and apply the shared outbox/inbox mappings
with `modelBuilder.ApplyOutboxConfiguration(schema)` and
`modelBuilder.ApplyInboxConfiguration(schema)`.

### Migrations

Each Infrastructure project contains an `IDesignTimeDbContextFactory` that reads
`OMNIMANAGE_MIGRATION_CONNECTION` (or falls back to the local `omni_migrator`
connection). From the repository root:

```powershell
$env:OMNIMANAGE_MIGRATION_CONNECTION = "Host=localhost;Port=5433;Database=omnimanage_db;Username=omni_migrator;SslMode=Prefer"
dotnet ef database update `
    --context PlatformDbContext `
    --project backend/src/Modules/Platform/OmniManage.Modules.Platform.Infrastructure/OmniManage.Modules.Platform.Infrastructure.csproj `
    --startup-project backend/src/Host/OmniManage.Api/OmniManage.Api.csproj
```

Repeat for `MasterDataDbContext`, `InventoryDbContext`, `SalesDbContext`,
`FinanceDbContext`, `HrisDbContext`, `ProjectsDbContext` and
`AssetMaintenanceDbContext`, or run `.\run.ps1 migrate` from the repository root.
The initial migrations are forward-only and every `Down` throws. Tenant tables
get RLS from `migrationBuilder.EnableTenantRls(schema, table)` in the module
migration; never hand-write the policy.

### Development seeding

`Seeding:Enabled` defaults to `false`; `appsettings.Development.json` sets it to
`true`. When enabled, `DbSeeder` runs after host build and before the server
starts. It is idempotent and creates the demo tenant `TEN-8492`, tenant admin
`sarah.jenkins@acme.com`, platform superadmin, RBAC, active modules, master data,
warehouse and GL accounts. It also creates 13 role-specific demo users, 120 HRIS
employees with attendance and payroll history, 24 projects with tasks and work
logs, and 120 assets with maintenance orders. The demo passwords are local
developer fixtures and must not be reused outside Development. Integration tests
run outside Development, so they do not seed.

