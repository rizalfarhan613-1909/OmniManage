'use client';

import { useI18n } from '@/components/providers/LanguageProvider';

import { useState } from 'react';

import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { Input } from '@/components/ui/Input';

import type { ClockInRequest } from '@/features/hris/types';

type Props = {
  isOpen: boolean;
  isPending: boolean;
  error: unknown;
  onConfirm: (request: ClockInRequest) => void;
  onCancel: () => void;
};

/** Mirrors `SmartAttendanceLog` bounds; the backend repeats both checks. */
const LATITUDE_RANGE = { min: -90, max: 90 } as const;
const LONGITUDE_RANGE = { min: -180, max: 180 } as const;

const DEFAULT_ATTENDANCE_MODE = 'GPS_PHOTO';

/**
 * Coordinates are held as text so an untouched field stays empty instead of
 * collapsing to `0` (a valid coordinate) and so `NaN` can never be serialized
 * into the request body.
 */
type ClockInForm = {
  employeeId: string;
  latitude: string;
  longitude: string;
  photoUrl: string;
  attendanceMode: string;
  wifiSsid: string;
};

const INITIAL_FORM: ClockInForm = {
  employeeId: '',
  latitude: '',
  longitude: '',
  photoUrl: '',
  attendanceMode: DEFAULT_ATTENDANCE_MODE,
  wifiSsid: '',
};

function toOptionalString(value: string): string | null {
  const trimmed = value.trim();

  return trimmed.length > 0 ? trimmed : null;
}

function toCoordinate(value: string, min: number, max: number): number | null {
  const trimmed = value.trim();

  if (trimmed.length === 0) {
    return null;
  }

  const parsed = Number(trimmed);

  return Number.isFinite(parsed) && parsed >= min && parsed <= max ? parsed : null;
}

/** Returns `null` while the form is incomplete or out of range. */
function buildRequest(form: ClockInForm): ClockInRequest | null {
  const employeeId = form.employeeId.trim();
  const latitude = toCoordinate(form.latitude, LATITUDE_RANGE.min, LATITUDE_RANGE.max);
  const longitude = toCoordinate(form.longitude, LONGITUDE_RANGE.min, LONGITUDE_RANGE.max);

  if (employeeId.length === 0 || latitude === null || longitude === null) {
    return null;
  }

  return {
    employeeId,
    latitude,
    longitude,
    photoUrl: toOptionalString(form.photoUrl),
    attendanceMode: toOptionalString(form.attendanceMode),
    wifiSsid: toOptionalString(form.wifiSsid),
  };
}

export function ClockInDialog({ isOpen, isPending, error, onConfirm, onCancel }: Props) {
  const { t } = useI18n();
  const [formData, setFormData] = useState<ClockInForm>(INITIAL_FORM);

  /**
   * The dialog footer renders outside the `<form>`, so its confirm button never
   * triggers native validation. Both paths funnel through this guard, matching
   * the sibling dialogs that return early on an incomplete form.
   */
  const submit = () => {
    const request = buildRequest(formData);

    if (!request) {
      return;
    }

    onConfirm(request);
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    submit();
  };

  return (
    <ConfirmDialog
      isOpen={isOpen}
      title={t('hris.attendance.clockIn.title')}
      description={t('hris.attendance.clockIn.description')}
      confirmLabel={
        isPending ? t('hris.attendance.clockIn.submitting') : t('hris.attendance.clockIn.submit')
      }
      pendingLabel={t('hris.attendance.clockIn.submitting')}
      cancelLabel={t('common.cancel')}
      isPending={isPending}
      error={error}
      onConfirm={submit}
      onCancel={onCancel}
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="employeeId" className="mb-1 block text-sm font-medium text-charcoal">
            {t('hris.attendance.clockIn.employeeId')}
          </label>
          <Input
            id="employeeId"
            type="text"
            placeholder="EMP-001"
            value={formData.employeeId}
            onChange={(e) => setFormData({ ...formData, employeeId: e.target.value })}
            required
          />
        </div>
        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <label htmlFor="latitude" className="mb-1 block text-sm font-medium text-charcoal">
              {t('hris.attendance.clockIn.latitude')}
            </label>
            <Input
              id="latitude"
              type="number"
              step="any"
              min={LATITUDE_RANGE.min}
              max={LATITUDE_RANGE.max}
              inputMode="decimal"
              placeholder="-6.2088"
              value={formData.latitude}
              onChange={(e) => setFormData({ ...formData, latitude: e.target.value })}
              required
            />
          </div>
          <div>
            <label htmlFor="longitude" className="mb-1 block text-sm font-medium text-charcoal">
              {t('hris.attendance.clockIn.longitude')}
            </label>
            <Input
              id="longitude"
              type="number"
              step="any"
              min={LONGITUDE_RANGE.min}
              max={LONGITUDE_RANGE.max}
              inputMode="decimal"
              placeholder="106.8456"
              value={formData.longitude}
              onChange={(e) => setFormData({ ...formData, longitude: e.target.value })}
              required
            />
          </div>
        </div>
        <div>
          <label htmlFor="photoUrl" className="mb-1 block text-sm font-medium text-charcoal">
            {t('hris.attendance.clockIn.photoUrl')}
          </label>
          <Input
            id="photoUrl"
            type="url"
            placeholder="https://example.com/photo.jpg"
            value={formData.photoUrl}
            onChange={(e) => setFormData({ ...formData, photoUrl: e.target.value })}
          />
        </div>
        <div>
          <label htmlFor="attendanceMode" className="mb-1 block text-sm font-medium text-charcoal">
            {t('hris.attendance.clockIn.attendanceMode')}
          </label>
          <Input
            id="attendanceMode"
            type="text"
            placeholder={DEFAULT_ATTENDANCE_MODE}
            value={formData.attendanceMode}
            onChange={(e) => setFormData({ ...formData, attendanceMode: e.target.value })}
          />
        </div>
        <div>
          <label htmlFor="wifiSsid" className="mb-1 block text-sm font-medium text-charcoal">
            {t('hris.attendance.clockIn.wifiSsid')}
          </label>
          <Input
            id="wifiSsid"
            type="text"
            placeholder="Office-WiFi"
            value={formData.wifiSsid}
            onChange={(e) => setFormData({ ...formData, wifiSsid: e.target.value })}
          />
        </div>
      </form>
    </ConfirmDialog>
  );
}
