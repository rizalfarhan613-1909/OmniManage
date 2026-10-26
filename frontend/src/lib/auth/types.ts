/**
 * Session payload returned by `POST /api/v1/auth/login`,
 * `POST /api/v1/auth/refresh` and `GET /api/v1/auth/session`.
 * Mirrors `AuthSessionDto` + `AuthUserDto` + `AuthTenantDto` on the backend.
 */
export interface SessionUser {
  id: string;
  email: string;
  roles: string[];
}

export interface SessionTenant {
  id: string;
  code: string;
  name: string;
}

export interface Session {
  user: SessionUser;
  tenant: SessionTenant;
  activeModules: string[];
  permissions: string[];
}
