-- Local development bootstrap for OmniManage.
--
-- The database itself and the owner role (omni_migrator) are created by the
-- PostgreSQL container. This script adds the non-owner runtime role from
-- Architecture.md §7.5 and makes sure the module schemas, privileges and
-- default privileges exist for future migration-created objects.
--
-- Runtime connects as omni_app: NOSUPERUSER, NOBYPASSRLS, not a table owner.
-- With no tenant GUC set, current_setting('app.tenant_id', true) is NULL and
-- every RLS policy fails closed.

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'omni_app') THEN
        CREATE ROLE omni_app LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;
    END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'omni_readonly') THEN
        CREATE ROLE omni_readonly LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;
    END IF;
END
$$;

CREATE SCHEMA IF NOT EXISTS platform AUTHORIZATION omni_migrator;
CREATE SCHEMA IF NOT EXISTS masterdata AUTHORIZATION omni_migrator;
CREATE SCHEMA IF NOT EXISTS inventory AUTHORIZATION omni_migrator;
CREATE SCHEMA IF NOT EXISTS sales AUTHORIZATION omni_migrator;
CREATE SCHEMA IF NOT EXISTS finance AUTHORIZATION omni_migrator;
CREATE SCHEMA IF NOT EXISTS hris AUTHORIZATION omni_migrator;
CREATE SCHEMA IF NOT EXISTS projects AUTHORIZATION omni_migrator;
CREATE SCHEMA IF NOT EXISTS asset_maintenance AUTHORIZATION omni_migrator;
CREATE SCHEMA IF NOT EXISTS audit AUTHORIZATION omni_migrator;

GRANT CONNECT ON DATABASE omnimanage_db TO omni_app, omni_readonly;

GRANT USAGE ON SCHEMA platform, masterdata, inventory, sales, finance,
    hris, projects, asset_maintenance, audit
    TO omni_app, omni_readonly;

-- Existing objects (relevant when the init scripts are re-run on an existing
-- database). audit_logs is append-only for the runtime role (Security.md §5):
-- INSERT and SELECT only, never UPDATE or DELETE.
GRANT SELECT, INSERT, UPDATE, DELETE
    ON ALL TABLES IN SCHEMA platform, masterdata, inventory, sales, finance,
        hris, projects, asset_maintenance
    TO omni_app;
GRANT SELECT, INSERT
    ON ALL TABLES IN SCHEMA audit
    TO omni_app;
GRANT USAGE, SELECT
    ON ALL SEQUENCES IN SCHEMA platform, masterdata, inventory, sales, finance,
        hris, projects, asset_maintenance, audit
    TO omni_app;
GRANT SELECT
    ON ALL TABLES IN SCHEMA platform, masterdata, inventory, sales, finance,
        hris, projects, asset_maintenance, audit
    TO omni_readonly;

-- Objects created later by EF Core migrations run as omni_migrator.
ALTER DEFAULT PRIVILEGES FOR ROLE omni_migrator
    IN SCHEMA platform, masterdata, inventory, sales, finance,
        hris, projects, asset_maintenance
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO omni_app;
ALTER DEFAULT PRIVILEGES FOR ROLE omni_migrator
    IN SCHEMA audit
    GRANT SELECT, INSERT ON TABLES TO omni_app;
ALTER DEFAULT PRIVILEGES FOR ROLE omni_migrator
    IN SCHEMA platform, masterdata, inventory, sales, finance,
        hris, projects, asset_maintenance, audit
    GRANT USAGE, SELECT ON SEQUENCES TO omni_app;
ALTER DEFAULT PRIVILEGES FOR ROLE omni_migrator
    IN SCHEMA platform, masterdata, inventory, sales, finance,
        hris, projects, asset_maintenance, audit
    GRANT SELECT ON TABLES TO omni_readonly;
