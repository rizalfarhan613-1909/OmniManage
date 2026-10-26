# OmniManage — Product Requirements Document (PRD)

| Field | Value |
|---|---|
| Product | OmniManage — Multi-Tenant ERP & HRIS |
| Document status | Extended Phase 6 Baseline v1.1 (source of truth for scope & enterprise expansion) |
| Architecture style | Modular Monolith, API-first |
| Related docs | `DESIGN.md`, `Architecture.md`, `Security.md`, `Rules.md` |

> **AI agent directive:** This document defines WHAT to build. If a requirement is not in this PRD, do not invent it. If a requirement is ambiguous, state your assumption explicitly in the PR description. Never expand scope without an explicit instruction.

---

## 1. Executive Summary

OmniManage is a cloud-hosted, multi-tenant ERP and HRIS platform that unifies Finance, Procurement, Sales, Inventory, Manufacturing, HRIS & Payroll, CRM, Project Management, and Asset Maintenance on one data model. Its defining promises are:

1. **Zero double-data-entry.** A business fact is entered once, in the module that owns it. Every other module reacts through domain/integration events.
2. **Plug-and-play licensing.** Each tenant (customer company) runs only the modules it has licensed. Modules switch ON/OFF dynamically, without redeploying and without data loss.
3. **Real-time visibility.** Management dashboards reflect operational events within seconds.
4. **Role-appropriate experience.** Nine personas get tailored web and mobile experiences (RBAC).

### 1.1 Business Goals
| ID | Goal | Measure |
|---|---|---|
| G1 | Eliminate duplicate entry across modules | 0 user-facing screens requiring re-keying of data owned by another module |
| G2 | Enable modular monetization | Superadmin can toggle any module for any tenant in < 30 s, effective in < 60 s |
| G3 | Real-time operational insight | Dashboard KPI latency (event → UI) p95 < 5 s |
| G4 | Enterprise-grade trust | Full audit trail on 100% of state-changing operations |
| G5 | Fast tenant onboarding | New tenant provisioned and usable in < 10 min |
| G6 | Automated HR & Payroll Execution | 100% automated salary calculations & GL auto-postings without manual recalculation |

### 1.2 Non-Goals (global)
- Not a public marketplace, e-commerce storefront, or POS system.
- Not a full BI/ML platform in MVP (Analytics is Phase 3, Python/FastAPI).
- No tenant-specific code forks. Customization is through configuration, custom fields and workflows only.

---

## 2. Target Audience

### 2.1 Customer Segments
| Segment | Profile | Needs |
|---|---|---|
| Primary | Mid-market companies, 50–1,000 employees, distribution / light manufacturing / services | Integrated operations without buying 8 separate systems |
| Secondary | Growing SMEs, 10–50 employees | Start with 2–3 modules, add more as they grow (dynamic licensing) |
| Tertiary | Multi-entity groups | Multiple legal entities/branches under one tenant |

### 2.2 Buyer vs. User
- **Buyer:** CEO / Owner, CFO, COO, HR Director.
- **Daily users:** Finance staff, sales reps, warehouse operators, HR staff, project managers, production planners, all employees (self-service).
- **Platform operator:** OmniManage company staff (System Superadmin).

---

## 3. User Roles & Personas (RBAC)

| # | Role | Scope | Primary Platform | Core Responsibilities |
|---|---|---|---|---|
| R1 | **System Superadmin** | Global (platform) | Web (Admin Console) | Create/suspend tenants, billing & plans, enable/disable modules per tenant, platform audit. **Cannot read tenant business data** unless in audited, time-boxed Support Mode. |
| R2 | **Tenant Admin / CEO** | Tenant | Web | Full access to the tenant's data, user & role management, approvals, real-time BI dashboards. |
| R3 | **HR Manager** | Tenant / HRIS | Web | Employee master data, smart attendance, leave policy, kasbon, payroll runs, PPh21. |
| R4 | **Finance Manager** | Tenant / Finance | Web | A/P, A/R, invoicing, journals, approvals, period close. |
| R5 | **Sales Representative** | Tenant / Sales + CRM | Web (+ mobile read) | Leads, opportunities, quotations, Sales Orders. |
| R6 | **Warehouse Operator** | Tenant / Inventory | Mobile (Flutter) + Web | Goods receipt, put-away, picking, stock count, barcode scanning. |
| R7 | **Production Manager** | Tenant / Manufacturing | Web | BOMs, MRP, production scheduling, work orders. |
| R8 | **Project Manager** | Tenant / Projects | Web | Client management, project timelines, task assignments, KPI generation. |
| R9 | **General Staff** | Self only | Mobile + Web | Employee Self-Service: clock-in/out (GPS + photo), leave/WFH requests, kasbon/expense, payslips, task checklist. |

Permission naming convention: `<module>.<resource>.<action>` (e.g., `sales.order.confirm`, `hris.payroll.process`). The full matrix lives in `Security.md`.

---

## 4. Module Catalogue & Licensing Codes

Each licensable capability has a `module_code` stored in `TenantModules`. `PLATFORM_*` and `CORE_*` codes are **always ON** and are never licensable.

| Domain | `module_code` | Description | Hard Dependencies | Soft Dependencies (graceful degradation) |
|---|---|---|---|---|
| Platform | `PLATFORM_CORE` | Identity, tenancy, licensing, audit, notifications | — | — |
| Master Data | `CORE_MASTERDATA` | Business partners, items, UoM, currency, tax, org structure | `PLATFORM_CORE` | — |
| Finance | `FIN_GL` | General ledger, chart of accounts, journals, periods | — | — |
| Finance | `FIN_AR` | Accounts receivable, invoices, receipts | `FIN_GL` | `SALES_CORE` |
| Finance | `FIN_AP` | Accounts payable, vendor bills, payments | `FIN_GL` | `PROC_CORE` |
| Procurement | `PROC_CORE` | Requisition, PO, goods-receipt matching | — | `INV_CORE`, `FIN_AP` |
| Sales | `SALES_CORE` | Quotation, Sales Order, delivery, pricing | — | `INV_CORE`, `FIN_AR`, `CRM_CORE`, `MFG_CORE` |
| Inventory | `INV_CORE` | Warehouses, stock ledger, reservations, scanning | — | `PROC_CORE`, `SALES_CORE` |
| Manufacturing | `MFG_CORE` | BOM, MRP, work orders, scheduling | `INV_CORE` | `ASSET_CORE` |
| HRIS | `HRIS_CORE` | Employee master, org chart, contracts | — | — |
| HRIS | `HRIS_ATTENDANCE` | Shifts, smart clock-in/out (GPS + photo proof), geofence, WFH, leave | `HRIS_CORE` | — |
| HRIS | `HRIS_PAYROLL` | Payroll runs (Monthly/Weekly/Daily), Kasbon, PPh21 & Coretax, payslips | `HRIS_CORE` | `HRIS_ATTENDANCE`, `FIN_GL` |
| HRIS | `HRIS_ESS` | Employee Self-Service (web + mobile) | `HRIS_CORE` | `HRIS_ATTENDANCE`, `HRIS_PAYROLL` |
| CRM | `CRM_CORE` | Leads, opportunities, activities, pipeline | — | `SALES_CORE` |
| Assets | `ASSET_CORE` | Asset register, QR code scanning, depreciation, preventive/corrective maintenance | — | `FIN_GL`, `MFG_CORE` |
| Projects | `PROJECT_CORE` | Client, projects, tasks, work logs, KPI Generator | — | `HRIS_CORE`, `SALES_CORE` |

**Licensing rules**
- LR-1: A module cannot be enabled if a hard dependency is OFF (API returns `409 MODULE_DEPENDENCY_UNMET`).
- LR-2: A module cannot be disabled if another ACTIVE module has it as a hard dependency.
- LR-3: Disabling a module is a **soft disable**: data is retained read-protected, never deleted. Re-enabling restores full access.
- LR-4: Soft-dependency integration events for a disabled consumer are recorded as `SKIPPED_MODULE_DISABLED`. On re-enable, the Superadmin may trigger an optional **backfill** job.

---

## 5. Functional Requirements

Priority: **M** = Must (MVP), **S** = Should (Phase 2 / Phase 6 Expansion), **C** = Could (Phase 3+).

### 5.1 Platform
| ID | Requirement | Pri |
|---|---|---|
| PLT-001 | Tenant lifecycle: create, suspend, reactivate, archive | M |
| PLT-002 | Module Management dashboard: per-tenant toggle switches with dependency validation | M |
| PLT-003 | Login payload returns `activeModules[]`, `permissions[]`, tenant branding | M |
| PLT-004 | Custom roles per tenant composed of permissions | M |
| PLT-005 | Immutable audit trail with before/after diff, viewable and exportable | M |
| PLT-006 | Approval workflow engine (configurable multi-step, amount thresholds) | M |
| PLT-007 | In-app + email notifications; real-time push via SignalR | M |
| PLT-008 | Billing/plan management, usage metering | S |
| PLT-009 | Custom fields & tenant-level configuration | S |
| PLT-010 | Superadmin Support Mode (time-boxed, audited impersonation) | S |
| PLT-011 | S3/MinIO Object Storage Integration per tenant for photos, receipts, documents | M |
| PLT-012 | Universal Export Engine: PDF & Excel generation for reports, invoices, and payslips | M |

### 5.2 Finance & Accounting
| ID | Requirement | Pri |
|---|---|---|
| FIN-001 | Chart of accounts, fiscal periods, period locking | M |
| FIN-002 | Double-entry journal engine; **all subledger postings are produced by automatic posting rules, never by manual re-entry** | M |
| FIN-003 | A/R invoices auto-created from Sales Orders (policy: on confirmation or on delivery) | M |
| FIN-004 | A/P vendor bills (manual + auto-created from PO/GRN three-way match) | M |
| FIN-005 | Customer receipts, vendor payments, allocation to open items | M |
| FIN-006 | Approval workflow for invoices/payments above a threshold | M |
| FIN-007 | Aging reports, trial balance, P&L, balance sheet | M |
| FIN-008 | Multi-currency with exchange-rate tables and revaluation | S |
| FIN-009 | Bank reconciliation; tax reporting packs | S |

### 5.3 Procurement
| ID | Requirement | Pri |
|---|---|---|
| PRC-001 | Purchase requisition → RFQ → Purchase Order, with approvals | M |
| PRC-002 | Auto-generated requisition from MRP shortages (`MaterialRequirementRaised`) | M |
| PRC-003 | PO receipt (GRN) via Inventory; three-way match with vendor bill | M |
| PRC-004 | Vendor performance scorecards | C |

### 5.4 Sales
| ID | Requirement | Pri |
|---|---|---|
| SAL-001 | Quotation → Sales Order conversion | M |
| SAL-002 | Sales Order lifecycle: Draft → Confirmed → Partially Delivered → Delivered → Invoiced → Closed / Cancelled | M |
| SAL-003 | Real-time available-to-promise check during order entry (if `INV_CORE` ON) | M |
| SAL-004 | Customer credit-limit check with approval escalation (if `FIN_AR` ON) | M |
| SAL-005 | Price lists, discounts, tax calculation | M |
| SAL-006 | Delivery orders and returns | S |

### 5.5 Inventory (Warehouse)
| ID | Requirement | Pri |
|---|---|---|
| INV-001 | Multi-warehouse, bin locations, lot/serial tracking | M |
| INV-002 | Immutable stock ledger; on-hand, reserved and available quantities are derived | M |
| INV-003 | Reservation/deduction on `SalesOrderConfirmed` (policy-driven) | M |
| INV-004 | Mobile barcode/QR scanning: receive, put-away, pick, transfer, stock count | M |
| INV-005 | Reorder points; auto-raise requisitions | S |
| INV-006 | Valuation: FIFO and weighted average (per tenant) | M |

### 5.6 Manufacturing
| ID | Requirement | Pri |
|---|---|---|
| MFG-001 | BOM (multi-level) and routing | M (Lite) |
| MFG-002 | **MRP-Lite:** consume `StockShortageDetected` / `SalesOrderConfirmed`, compute raw-material requirements, notify Production Manager | M |
| MFG-003 | Work orders, material issue, finished-goods receipt | S |
| MFG-004 | Finite-capacity scheduling (Gantt) | S |
| MFG-005 | Production costing posted to Finance | S |

### 5.7 HRIS & Payroll
| ID | Requirement | Pri |
|---|---|---|
| HR-001 | Employee master, org structure, contracts, documents | M |
| HR-002 | **Smart Attendance:** Shifts, clock-in/out with GPS location geofencing, photo proof upload, WiFi validation, overtime, and WFH requests | M |
| HR-003 | Leave & Permit management: Leave types, accrual rules, expiration dates, multi-level approval workflow | M |
| HR-004 | Reimbursement & Expense management: Expense submission with photo receipts, real-time approval | M |
| HR-005 | Salary Advance (Kasbon / Simpan Pinjam): Application, approval, auto-deduction from monthly payroll | M |
| HR-006 | **Payroll Calculation Engine:** Monthly, weekly, and daily payroll runs; support earning/deduction components, PPh21 & Coretax compliance, PDF payslip generation | M |
| HR-007 | **Finance GL Auto-Posting:** Automated posting of salary expense and cash liability journals to `FIN_GL` on payroll finalization | M |
| HR-008 | Statutory payroll packs (tax, BPJS, social security) per country | S |
| HR-009 | Recruitment, performance appraisal | C |

### 5.8 CRM
| ID | Requirement | Pri |
|---|---|---|
| CRM-001 | Leads, contacts, accounts, opportunities, Kanban pipeline | M |
| CRM-002 | Activity log (calls, meetings, tasks) | M |
| CRM-003 | Opportunity "Won" → one-click Quotation/Sales Order (no re-keying) | M |
| CRM-004 | Email integration, campaign tracking | C |

### 5.9 Asset Maintenance & Registry
| ID | Requirement | Pri |
|---|---|---|
| AST-001 | Asset register with hierarchy, QR Code label generation, depreciation hooks to Finance GL | M |
| AST-002 | Maintenance requests, preventive schedules, corrective work orders | S |
| AST-003 | Spare parts consumption from Inventory | S |

### 5.10 Project & Task Management
| ID | Requirement | Pri |
|---|---|---|
| PRJ-001 | Client directory, project timelines, budget & expense tracking | M |
| PRJ-002 | Task management, checklists, work logs, task assignment notifications | M |
| PRJ-003 | **Automated KPI Generator:** Calculate employee/team performance metrics based on task completion and SLAs | M |

---

## 6. User Stories

Format: **As a** `<role>`, **I want** `<capability>` **so that** `<value>`. Acceptance Criteria (AC) use Given/When/Then.

### Platform / Superadmin
**US-001 — Toggle module per tenant** *(R1)*
As a Superadmin, I want to switch modules ON/OFF per tenant so that I can license features dynamically.
- AC1: Given tenant T has `FIN_GL` OFF, when I enable `FIN_AR`, then the API rejects with `409 MODULE_DEPENDENCY_UNMET`.
- AC2: Given I toggle `HRIS_PAYROLL` OFF, then any request by tenant T to a `[RequireModule("HRIS_PAYROLL")]` endpoint returns `403 MODULE_NOT_LICENSED` within 60 s.
- AC3: Every toggle writes an audit record (actor, tenant, module, old/new value).

**US-002 — Onboard tenant** *(R1)*
As a Superadmin, I want to create a tenant with a plan template so that its admin can log in immediately.
- AC: Creating a tenant seeds roles, default chart of accounts template, numbering sequences and a Tenant Admin invitation.

### Tenant Admin / CEO
**US-003 — Real-time dashboard** *(R2)*
As a CEO, I want a live dashboard (revenue, A/R, stock value, production status, headcount, attendance) so that I can decide quickly.
- AC1: When a Sales Order is confirmed, the "Sales Today" and "Open A/R" widgets update without page refresh within 5 s.
- AC2: Widgets for modules that are OFF are not rendered and not queried.

**US-004 — Manage roles** *(R2)*
As a Tenant Admin, I want to create custom roles from permission sets so that access matches my org chart.

### HR Manager
**US-005 — Manage employees** *(R3)* — create, edit, terminate employees; sensitive fields (salary, national ID, bank account) visible only with `hris.employee.view_sensitive`.

**US-006 — Run payroll** *(R3)*
As an HR Manager, I want to run monthly/weekly/daily payroll from attendance, leave, and kasbon data so that I do not re-enter hours.
- AC1: Payroll pulls approved attendance/overtime/unpaid leave/kasbon auto-deductions automatically (if `HRIS_ATTENDANCE` ON).
- AC2: On "Finalize", payslips are generated (PDF) and a GL journal is posted to Finance (if `FIN_GL` ON).
- AC3: A finalized payroll run is immutable. Corrections are done through an adjustment run.

**US-007 — Approve leave & reimburse** *(R3, line managers)* — approve/reject leave, WFH, and reimbursement requests with comments; leave balances update and attendance calendar blocks those days.

### Finance Manager
**US-008 — Review auto-created invoices** *(R4)*
As a Finance Manager, I want A/R invoices auto-created from confirmed Sales Orders so that sales data is never re-keyed.
- AC: Each auto invoice links to its SO and its journal entry (Dr A/R, Cr Revenue, Cr Tax Payable).

**US-009 — Approve payments** *(R4)* — payments above the threshold require a second approver.

**US-010 — Three-way match** *(R4)* — the system matches PO, GRN and vendor bill and flags variances beyond tolerance.

**US-011 — Close period** *(R4)* — lock a fiscal period. Postings to a locked period are rejected.

### Sales Representative
**US-012 — Create Sales Order** *(R5)*
As a Sales Rep, I want to create and confirm a Sales Order using the CRM account and catalogue so that downstream modules act automatically.
- AC1: Customer, price and tax auto-populate from master data.
- AC2: On Confirm, the response returns within 500 ms. Downstream effects are asynchronous but visible on the order's "Fulfilment Timeline".
- AC3: If stock is insufficient, the order is confirmed with status `Backordered` and Manufacturing/Procurement are notified. It is not silently rejected.

**US-013 — Pipeline management** *(R5)* — drag opportunities across Kanban stages; a "Won" opportunity converts to a Quotation with lines pre-filled.

### Warehouse Operator
**US-014 — Receive goods by scan** *(R6)* — scan the PO barcode, then scan item barcodes and confirm quantities. This works offline and syncs later.
- AC: Scans are idempotent (same `Idempotency-Key` never double-posts).

**US-015 — Pick for delivery** *(R6)* — a guided pick list sorted by bin; short-picks trigger a stock-discrepancy event.

**US-016 — Stock count** *(R6)* — blind count by bin; variances go to a Finance-approved adjustment.

### Production Manager
**US-017 — See material requirements** *(R7)* — receive an in-app notification and an MRP worksheet listing raw-material shortages caused by confirmed orders.
- AC: The worksheet lists SO reference, item, required qty, on-hand, on-order, shortage, suggested action (Produce / Purchase).

**US-018 — Schedule production** *(R7)* — create work orders and view the schedule on a Gantt chart (Phase 2).

### Project Manager
**US-019 — Track projects & generate KPIs** *(R8)*
As a Project Manager, I want to assign tasks with checklists and generate automated KPI reports so that I can track team productivity without manual spreadsheets.
- AC1: Tasks update in real-time, and completed work logs automatically feed into the employee KPI Generator engine.

### General Staff
**US-020 — Clock in/out with Smart Attendance** *(R9)* — one-tap clock-in on web/mobile with GPS geofence validation and photo proof upload. Offline events are queued with device timestamp and flagged `OFFLINE_SYNCED`.

**US-021 — Request leave & Kasbon** *(R9)* — see balance, submit leave/WFH/kasbon requests, and track approval status. Push notifications are sent on decision.

**US-022 — View payslips & QR Assets** *(R9)* — view and download payslips after biometric re-authentication on mobile; scan Asset QR codes for maintenance reporting.

---

## 7. Use Cases

### UC-01 — Sales-to-Finance Automation Flow (Flagship)

| Attribute | Value |
|---|---|
| Actors | Sales Rep (primary); Inventory, Manufacturing, Finance, Dashboard (system) |
| Required modules | `SALES_CORE` (mandatory); `INV_CORE`, `MFG_CORE`, `FIN_AR`, `FIN_GL` (optional, degrade gracefully) |
| Trigger | Sales Rep clicks **Confirm** on a Draft Sales Order |
| Preconditions | User holds `sales.order.confirm`. Customer is active. Lines reference active items. |
| Postconditions | Order `Confirmed`. Stock reserved or deducted. Material requirements raised if short. A/R invoice posted. Dashboard updated. Full audit trail. |

**Main flow**
1. Sales Rep creates a Sales Order (customer from CRM/MasterData; items and prices auto-filled) and clicks Confirm.
2. **Sales** validates the order (credit limit via the Finance contract if `FIN_AR` ON). It sets status `Confirmed` and writes `SalesOrderConfirmed` to the **Outbox** in the **same DB transaction**.
3. The Outbox dispatcher publishes the event. Each subscriber runs in its own transaction with its own idempotency record.
4. **Inventory** (if ON) reserves/deducts stock per `InventoryPolicy`, emits `StockReserved` per line (or `StockShortageDetected` for the shortfall), and updates the stock ledger.
5. **Manufacturing** (if ON) consumes `StockShortageDetected`. It explodes the BOM of shortage items into raw-material requirements, creates an `MrpRequirement`, notifies the Production Manager (`MaterialRequirementRaised`). **Procurement** may auto-create requisition drafts from the raw-material shortage.
6. **Finance** (if ON) consumes `SalesOrderConfirmed` (or `DeliveryPosted`, per `ArPostingPolicy`). It creates the A/R invoice, runs the posting rules, and posts the journal (Dr Accounts Receivable, Cr Sales Revenue, Cr Tax Payable). It emits `ArInvoicePosted`.
7. The **Dashboard projection** consumes all resulting events, updates read models, and pushes deltas to the web client over SignalR.
8. The Sales Rep sees the **Fulfilment Timeline** on the order, updated in real time.

### UC-02 — Superadmin Enables a Module
1. Superadmin opens **Module Management → Tenant → Modules**.
2. Toggles `HRIS_PAYROLL` ON. The UI performs a dependency pre-check.
3. `PATCH /api/v1/platform/tenants/{id}/modules/HRIS_PAYROLL` → validated, persisted, audited.
4. The tenant module cache is invalidated (distributed cache + pub/sub).
5. On the tenant's next navigation or `GET /auth/session`, the sidebar shows "Payroll". Backend access is enforced immediately.

### UC-03 — Warehouse Receiving by Mobile
1. Operator logs in to the Flutter app and selects Warehouse → Receive.
2. Scans the PO QR code. The system loads expected lines.
3. Scans item barcodes and confirms quantity per line (lot/serial prompts if tracked).
4. The app stores scans locally (SQLite). On connectivity, it syncs a `ReceiveGoods` command with an `Idempotency-Key`.
5. Inventory posts the stock ledger and emits `GoodsReceived`. Procurement updates PO status. Finance (if ON) accrues a GRNI liability.

### UC-04 — Employee Clock-In (Smart Attendance)
1. General Staff opens the app and taps Clock In.
2. The app captures timestamp, GPS location, selfie photo proof, and device ID.
3. Server validates shift, geofence, and duplicate punches. Uploads photo to tenant's S3/MinIO bucket.
4. Records the attendance event. Late or out-of-fence events are flagged for HR exception queue.

### UC-05 — Payroll Finalization to Finance GL Flow
1. HR Manager reviews attendance, approved leave, overtime, and active Kasbon deductions.
2. Executes Payroll Calculation Engine (`HRIS_PAYROLL`).
3. Clicks **Finalize Payroll Run**.
4. System generates individual PDF payslips stored in Object Storage.
5. Emits `PayrollFinalizedIntegrationEvent` to the Outbox.
6. **Finance** (`FIN_GL`) consumes the event and auto-posts the journal entry:
   - **Debit:** Salary & Wages Expense Account
   - **Credit:** Kasbon Receivable Account (deduction portion)
   - **Credit:** Tax Payable (PPh21 portion)
   - **Credit:** Accrued Payroll Cash Liability Account
7. Payslips become immediately available on Employee Self-Service (`HRIS_ESS`).

---

## 8. Non-Functional Requirements

| Category | Requirement |
|---|---|
| Performance | API p95 < 300 ms (reads), < 600 ms (writes). Event propagation p95 < 5 s. |
| Scalability | Launch target: 500 tenants, 10,000 concurrent users. Horizontally scalable stateless API with Redis L2 cache. |
| Availability | 99.9% monthly SLA. RPO ≤ 15 min, RTO ≤ 1 h. |
| Security | See `Security.md`. OWASP ASVS L2 baseline. |
| Data isolation | Strict tenant isolation enforced at app layer AND database layer (RLS). Object storage isolated by tenant buckets. |
| Localization | i18n-ready from day 1 (EN + ID). Multi-currency, locale-aware dates/numbers, PPh21 tax rules. Timezone stored UTC. |
| Accessibility | WCAG 2.2 AA for web. |
| Auditability | Every state change recorded: who, what, when, before/after, trace ID. |
| Observability | Structured logs (Serilog), metrics, distributed traces (OpenTelemetry). |
| Data retention | Configurable per tenant. Default financial and HR records 10 years. |
| Browser/Device | Latest 2 versions of Chrome/Edge/Safari/Firefox. Android 8+, iOS 14+. Responsive Web UI. |

---

## 9. MVP & Expansion Scope

### 9.1 In Scope (Phase 1 MVP — Complete & Verified)
| Area | MVP Depth |
|---|---|
| Platform | Tenants, Auth (JWT + refresh), RBAC with custom roles, **Dynamic Module Licensing end-to-end**, Module Management dashboard, audit trail, notifications, approval engine |
| Master Data | Partners, items, UoM, currency, tax codes, org structure |
| Finance | GL, A/R, A/P, auto posting rules, period lock, core reports |
| Procurement | Requisition → PO → GRN → vendor bill link |
| Sales | Quotation, Sales Order, pricing, credit check, delivery order |
| Inventory | Warehouses, stock ledger, reservations, mobile scanning |
| Manufacturing | **Lite:** BOM + MRP-Lite notifications |
| HRIS | Employee master, basic attendance, leave, basic payroll, ESS |
| CRM | Leads, opportunities, activities, pipeline, convert to quotation/order |
| Dashboards | CEO real-time dashboard, role-specific home dashboards |

### 9.2 Enterprise Expansion Scope (Phase 6 Expansion)
| Area | Phase 6 Depth |
|---|---|
| Smart Attendance | GPS geofencing, photo proof upload, WiFi validation, WFH requests |
| Advanced Payroll | Monthly/weekly/daily runs, Kasbon deductions, PPh21 & Coretax compliance, GL auto-posting |
| Project & Tasks | Client directory, project timelines, task checklists, work logs, automated KPI Generator |
| Asset Register | Asset hierarchy, QR code scanning, depreciation schedule hooks to GL, preventive maintenance |
| Storage & Export | S3/MinIO tenant object storage, PDF/Excel universal export engine |

### 9.3 MVP & Expansion Exit Criteria
- UC-01 and UC-05 pass end-to-end in automated integration tests.
- Module toggle test matrix passes (every module ON/OFF combination within dependency rules).
- Cross-tenant isolation penetration test: 0 leaks.
- p95 targets met in load test at 100 concurrent tenants with Redis L2 cache enabled.

---

## 10. Roadmap
| Phase | Theme | Highlights |
|---|---|---|
| 1 (MVP) | Integrated core | Fully delivered and verified (Sales, Stock, Finance, Auth, Licensing) |
| 2-5 | Validation & Infrastructure | PostgreSQL RLS, Docker Compose, Seeding, BFF Proxy, 489+ total tests |
| 6 | Enterprise Expansion | Smart Attendance (GPS/Photo), Kasbon, PPh21 Payroll Auto-GL, Projects/Tasks, KPI Generator, Asset QR Code, S3/MinIO Storage |
| 7 | Intelligence | Python FastAPI analytics, forecasting, anomaly detection, data warehouse via CDC |
| 8 | Ecosystem | Public API/webhooks marketplace, tenant-defined workflows, white-label |

---

## 11. Success Metrics (KPIs)
- Time-to-first-invoice for a new tenant ≤ 1 day.
- ≥ 90% of Sales Orders flow to A/R with zero manual intervention.
- ≥ 95% of Payroll runs auto-post to GL with zero manual ledger re-entry.
- Event dead-letter rate < 0.1%.
- Mobile/Web clock-in success rate ≥ 99.5% (including offline-sync).
- NPS ≥ 40 within 2 quarters of GA.

## 12. Risks & Mitigations
| Risk | Impact | Mitigation |
|---|---|---|
| Module coupling erodes boundaries | Cannot license modules independently | Architecture tests (NetArchTest) in CI. Contracts-only inter-module communication. |
| Cross-tenant data leak | Catastrophic | App-level filters + PostgreSQL RLS + isolation integration tests + pen test |
| Event loss / duplication | Financial inconsistency | Transactional Outbox + idempotent inbox + replay tooling |
| Stale license cache | Access after disable | Short TTL + Redis L2 pub/sub invalidation + DB fallback |
| Scope creep in expansion | Delay | Strict Phase 6 boundaries defined in §9.2 |

## 13. Glossary
**Tenant** — a customer company. **Module** — a licensable capability identified by `module_code`. **Outbox** — table persisting events atomically with business data. **RLS** — PostgreSQL Row-Level Security. **ATP** — Available-to-Promise. **MRP** — Material Requirements Planning. **GRN/GRNI** — Goods Receipt Note / Goods Received Not Invoiced. **ESS** — Employee Self-Service. **Kasbon** — Salary advance. **PPh21** — Indonesian income tax on employment earnings.