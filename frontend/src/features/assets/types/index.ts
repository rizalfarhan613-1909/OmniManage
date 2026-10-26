/**
 * Wire types for the Asset Maintenance module (Phase 6).
 *
 * Mirrors the presentation contracts and domain aggregates on the backend.
 * Following Rules.md §7, decimal values travel as fixed-scale strings while
 * dates use ISO-8601 (`DateOnly` values are `yyyy-MM-dd`).
 */

/** Mirrors `AssetStatus`. */
export type AssetStatus = 'Active' | 'UnderMaintenance' | 'Retired';

/** Mirrors `MaintenanceStatus`. */
export type MaintenanceStatus = 'Scheduled' | 'InProgress' | 'Completed' | 'Cancelled';

/** `POST /api/v1/assets` request body. */
export interface CreateAssetRequest {
  assetCode: string;
  name: string;
  category: string;
  serialNumber?: string | null;
  qrCodeUrl?: string | null;
  purchaseValue: number;
  /** `DateOnly` in `yyyy-MM-dd` format. */
  acquisitionDate: string;
}

/** `POST /api/v1/assets/{assetId}/maintenance-orders` request body. */
export interface RequestMaintenanceRequest {
  requestNumber: string;
  /** `DateOnly` in `yyyy-MM-dd` format. */
  scheduledDate?: string | null;
  description: string;
  /** Defaults to `CORRECTIVE` on the backend when omitted. */
  maintenanceType?: string | null;
  /** Defaults to `NORMAL` on the backend when omitted. */
  priority?: string | null;
}

/** Mirrors `Asset`. */
export interface AssetDto {
  id: string;
  assetCode: string;
  name: string;
  category: string;
  serialNumber: string | null;
  qrCodeUrl: string | null;
  purchaseValue: string;
  /** `DateOnly` in `yyyy-MM-dd` format. */
  acquisitionDate: string;
  status: AssetStatus;
}

/** Mirrors `MaintenanceOrder`. */
export interface MaintenanceOrderDto {
  id: string;
  assetId: string;
  requestNumber: string;
  /** `DateOnly` in `yyyy-MM-dd` format. */
  scheduledDate: string | null;
  /** `DateOnly` in `yyyy-MM-dd` format. */
  completedDate: string | null;
  cost: string;
  status: MaintenanceStatus;
  description: string;
}
