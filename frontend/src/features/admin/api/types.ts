/** Mirrors the Platform `TenantStatus` enum (serialized as a string). */
export type TenantStatus = 'Active' | 'Suspended' | 'Archived';

/**
 * Tenant row in the Superadmin master directory.
 *
 * ASSUMPTION: the platform directory projection carries `licensedModules`,
 * active user count and quota alongside the tenant header so the console can
 * render the matrix without an N+1 module request per row. The backend
 * `TenantModule` aggregate remains the authority; the PATCH response and the
 * query invalidation reconcile this projection.
 */
export interface TenantSummary {
  id: string;
  code: string;
  name: string;
  status: TenantStatus;
  planCode: string;
  activeUserCount: number;
  activeUserQuota: number;
  licensedModules: string[];
  createdAt: string;
}

/** Mirrors the Platform `TenantModule` projection. */
export interface TenantModuleLicense {
  tenantId: string;
  moduleCode: string;
  isActive: boolean;
  activatedAt: string | null;
  expiresAt: string | null;
  changedBy: string;
  changeReason: string;
  version: number;
}

export interface ToggleTenantModuleInput {
  tenantId: string;
  moduleCode: string;
  isActive: boolean;
  /** Required by the backend on every license change (Security.md §6.1). */
  reason: string;
}

export type AuditActorType = 'User' | 'System' | 'Superadmin' | 'Support';

export interface AuditActor {
  id: string | null;
  name: string | null;
  type: AuditActorType;
}

/**
 * Immutable audit row (Security.md §6.2). `beforeData` / `afterData` are the
 * changed fields only, already redacted by the backend.
 */
export interface AuditLogEntry {
  id: string;
  occurredAt: string;
  actor: AuditActor;
  action: string;
  moduleCode: string | null;
  entityType: string | null;
  entityId: string | null;
  ipAddress: string | null;
  userAgent: string | null;
  traceId: string;
  beforeData: Record<string, unknown> | null;
  afterData: Record<string, unknown> | null;
  rowHash: string;
  prevHash: string | null;
}

export type ActiveSessionStatus = 'Active' | 'Idle';

/** Live session row for the instant-termination control (DESIGN.md view 10). */
export interface ActiveUserSession {
  id: string;
  userId: string;
  userName: string;
  email: string;
  roles: string[];
  ipAddress: string;
  userAgent: string | null;
  startedAt: string;
  lastSeenAt: string;
  status: ActiveSessionStatus;
}

export interface TerminateSessionResult {
  terminated: boolean;
}
