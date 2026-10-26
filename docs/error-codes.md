# Error Codes

Error codes use the format `MODULE.ENTITY.REASON` with `UPPER_SNAKE` segments.
Each domain code is declared once in its module's `Domain/Errors` folder.
Transport, validation and infrastructure outcomes that can occur on any endpoint
are declared in `BuildingBlocks.Domain/CommonErrors.cs` and listed under
Cross-cutting below.

## Cross-cutting

| Code | Type | Meaning |
|---|---|---|
| `VALIDATION.FAILED` | Validation | Request binding or contract validation failed; see `details[]`. |
| `SYSTEM.UNEXPECTED` | Failure | An unhandled server exception occurred; correlate with `meta.traceId`. |
| `SYSTEM.UNAVAILABLE` | Failure | A dependency (API, database, license store) is unreachable. |
| `SYSTEM.NOT_FOUND` | Not Found | No endpoint matches the requested path. |
| `AUTH.TOKEN_EXPIRED` | Unauthorized | The access token expired; the client should refresh once. |

The frontend additionally synthesizes `SYSTEM.NETWORK_UNAVAILABLE` when the
browser cannot reach the BFF. `CONCURRENCY_CONFLICT` and `RATE_LIMIT.EXCEEDED`
are reserved by `Rules.md §5.5` for the concurrency and rate-limiting layers.

## Platform

| Code | Type | Meaning |
|---|---|---|
| `PLATFORM.TENANT.INVALID_ID` | Validation | Tenant identifier is missing. |
| `PLATFORM.TENANT.INVALID_CODE` | Validation | Tenant code is blank, too short/long or uses invalid characters. |
| `PLATFORM.TENANT.INVALID_NAME` | Validation | Tenant name is blank or longer than 200 characters. |
| `PLATFORM.TENANT.INVALID_PLAN_CODE` | Validation | Plan code is blank, too short/long or uses invalid characters. |
| `PLATFORM.TENANT.INVALID_CREATED_AT` | Validation | Tenant creation time is missing. |
| `PLATFORM.TENANT.INVALID_STATUS_TRANSITION` | Conflict | The requested tenant lifecycle transition is not allowed from the current status. |
| `PLATFORM.TENANT_MODULE.INVALID_TENANT_ID` | Validation | Tenant identifier is missing. |
| `PLATFORM.TENANT_MODULE.INVALID_MODULE_CODE` | Validation | Module code is blank, too short/long or uses invalid characters. |
| `PLATFORM.TENANT_MODULE.INVALID_CHANGED_BY` | Validation | The actor changing the license is missing. |
| `PLATFORM.TENANT_MODULE.CHANGE_REASON_REQUIRED` | Validation | A license change reason is required. |
| `PLATFORM.TENANT_MODULE.CHANGE_REASON_TOO_LONG` | Validation | The license change reason exceeds 500 characters. |
| `PLATFORM.TENANT_MODULE.INVALID_ACTIVATION_TIME` | Validation | The activation time is missing. |
| `PLATFORM.TENANT_MODULE.INVALID_EXPIRY` | Validation | Expiry is not later than the activation/reference time. |
| `PLATFORM.TENANT_MODULE.ALREADY_ACTIVE` | Conflict | The module is already effective for the tenant. |
| `PLATFORM.TENANT_MODULE.ALREADY_INACTIVE` | Conflict | The module is already inactive. |
| `PLATFORM.TENANT_MODULE.NOT_ACTIVE` | Conflict | Expiry cannot be changed while the module is inactive. |
| `PLATFORM.USER.INVALID_ID` | Validation | User identifier is missing. |
| `PLATFORM.USER.INVALID_TENANT_ID` | Validation | Tenant identifier is missing. |
| `PLATFORM.USER.INVALID_EMAIL` | Validation | Email is blank, malformed or longer than 320 characters. |
| `PLATFORM.USER.INVALID_PASSWORD_HASH` | Validation | Password hash is blank or too long. |
| `PLATFORM.USER.ROLE_TENANT_MISMATCH` | Conflict | The role belongs to another tenant. |
| `PLATFORM.USER.ROLE_NOT_ASSIGNED` | Not Found | The role is not assigned to the user. |
| `PLATFORM.ROLE.INVALID_ID` | Validation | Role identifier is missing. |
| `PLATFORM.ROLE.INVALID_TENANT_ID` | Validation | Tenant identifier is missing. |
| `PLATFORM.ROLE.INVALID_NAME` | Validation | Role name is blank or longer than 100 characters. |
| `PLATFORM.ROLE.PERMISSION_NOT_GRANTED` | Not Found | The permission is not granted to the role. |
| `PLATFORM.PERMISSION.INVALID_CODE` | Validation | Permission code is not in lowercase `module.resource.action` format. |
| `PLATFORM.PLATFORM_USER.INVALID_ID` | Validation | A platform user identifier is required. |
| `PLATFORM.PLATFORM_USER.INVALID_EMAIL` | Validation | A valid platform user email address is required. |
| `PLATFORM.PLATFORM_USER.INVALID_PASSWORD_HASH` | Validation | A platform user password hash is required. |
| `PLATFORM.PLATFORM_USER.INVALID_CREATED_AT` | Validation | A platform user creation timestamp is required. |
| `PLATFORM.REFRESH_TOKEN.INVALID_ID` | Validation | A refresh token identifier is required. |
| `PLATFORM.REFRESH_TOKEN.INVALID_TENANT_ID` | Validation | A refresh token tenant identifier is required. |
| `PLATFORM.REFRESH_TOKEN.INVALID_USER_ID` | Validation | A refresh token user identifier is required. |
| `PLATFORM.REFRESH_TOKEN.INVALID_SESSION_ID` | Validation | A refresh token session identifier is required. |
| `PLATFORM.REFRESH_TOKEN.INVALID_TOKEN_HASH` | Validation | The refresh token hash is not a valid SHA-256 value. |
| `PLATFORM.REFRESH_TOKEN.INVALID_EXPIRY` | Validation | Refresh token expiry is not later than its issue time. |
| `PLATFORM.REFRESH_TOKEN.EXPIRED` | Unauthorized | The refresh token has expired. |
| `PLATFORM.REFRESH_TOKEN.ALREADY_REVOKED` | Unauthorized | The refresh token has already been revoked or rotated. |
| `PLATFORM.MODULE_NOT_LICENSED` | Forbidden | The requested module is not active for the tenant. |
| `PLATFORM.LICENSE_UNAVAILABLE` | Failure | The license store could not be reached; the request was not authorized. |

## Master Data

| Code | Type | Meaning |
|---|---|---|
| `MASTERDATA.BUSINESS_PARTNER.INVALID_ID` | Validation | A business partner identifier is missing. |
| `MASTERDATA.BUSINESS_PARTNER.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `MASTERDATA.BUSINESS_PARTNER.INVALID_CODE` | Validation | Business partner code is blank, out of range or uses invalid characters. |
| `MASTERDATA.BUSINESS_PARTNER.INVALID_NAME` | Validation | Business partner name is blank or longer than 200 characters. |
| `MASTERDATA.BUSINESS_PARTNER.INVALID_PARTNER_TYPE` | Validation | Partner type is not one of Customer, Vendor or Both. |
| `MASTERDATA.BUSINESS_PARTNER.INVALID_TAX_ID` | Validation | Tax identifier is longer than 50 characters. |
| `MASTERDATA.BUSINESS_PARTNER.INVALID_CREDIT_LIMIT` | Validation | Credit limit is negative or exceeds the maximum supported amount. |
| `MASTERDATA.BUSINESS_PARTNER.INVALID_CREATED_AT` | Validation | A valid creation time is required. |
| `MASTERDATA.BUSINESS_PARTNER.INVALID_CREATED_BY` | Validation | The user creating the business partner is required. |
| `MASTERDATA.BUSINESS_PARTNER.CODE_ALREADY_EXISTS` | Conflict | A business partner with this code already exists in the tenant. |
| `MASTERDATA.ITEM.INVALID_ID` | Validation | An item identifier is missing. |
| `MASTERDATA.ITEM.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `MASTERDATA.ITEM.INVALID_SKU` | Validation | SKU is blank, out of range or uses invalid characters. |
| `MASTERDATA.ITEM.INVALID_NAME` | Validation | Item name is blank or longer than 200 characters. |
| `MASTERDATA.ITEM.INVALID_UOM_CODE` | Validation | Unit of measure code is not 2 to 10 letters or digits. |
| `MASTERDATA.ITEM.INVALID_CATEGORY` | Validation | Item category is longer than 100 characters. |
| `MASTERDATA.ITEM.INVALID_BASE_PRICE` | Validation | Base price is negative or exceeds the maximum supported amount. |
| `MASTERDATA.ITEM.INVALID_COST_PRICE` | Validation | Cost price is negative or exceeds the maximum supported amount. |
| `MASTERDATA.ITEM.INVALID_CREATED_AT` | Validation | A valid creation time is required. |
| `MASTERDATA.ITEM.INVALID_CREATED_BY` | Validation | The user creating the item is required. |
| `MASTERDATA.ITEM.SKU_ALREADY_EXISTS` | Conflict | An item with this SKU already exists in the tenant. |

## Inventory

| Code | Type | Meaning |
|---|---|---|
| `INVENTORY.STOCK.INVALID_ID` | Validation | A stock ledger entry identifier is required. |
| `INVENTORY.STOCK.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `INVENTORY.STOCK.INVALID_WAREHOUSE_ID` | Validation | A warehouse identifier is required. |
| `INVENTORY.STOCK.INVALID_ITEM_ID` | Validation | An item identifier is required. |
| `INVENTORY.STOCK.INVALID_QUANTITY_DELTA` | Validation | Quantity delta is zero or outside the supported range. |
| `INVENTORY.STOCK.INVALID_MOVEMENT_TYPE` | Validation | Movement type is not Receive, Issue, Transfer or Adjustment. |
| `INVENTORY.STOCK.INVALID_REFERENCE_NUMBER` | Validation | Reference number is blank or longer than 100 characters. |
| `INVENTORY.STOCK.INVALID_OCCURRED_AT_UTC` | Validation | A valid occurrence time is required. |
| `INVENTORY.STOCK.LEDGER_IS_APPEND_ONLY` | Conflict | Stock ledger entries are append-only and cannot be modified or deleted. |
| `INVENTORY.WAREHOUSE.INVALID_ID` | Validation | A warehouse identifier is required. |
| `INVENTORY.WAREHOUSE.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `INVENTORY.WAREHOUSE.INVALID_CODE` | Validation | Warehouse code is out of range or uses unsupported characters. |
| `INVENTORY.WAREHOUSE.INVALID_NAME` | Validation | Warehouse name is blank or longer than 200 characters. |
| `INVENTORY.WAREHOUSE.INVALID_ADDRESS` | Validation | Warehouse address is longer than 500 characters. |
| `INVENTORY.WAREHOUSE.INVALID_CREATED_AT` | Validation | A valid creation time is required. |
| `INVENTORY.WAREHOUSE.INVALID_CREATED_BY` | Validation | The user creating the warehouse is required. |
| `INVENTORY.WAREHOUSE.INVALID_UPDATED_AT` | Validation | A valid update time is required. |
| `INVENTORY.WAREHOUSE.INVALID_UPDATED_BY` | Validation | The user updating the warehouse is required. |
| `INVENTORY.WAREHOUSE.CODE_ALREADY_EXISTS` | Conflict | A warehouse with this code already exists in the tenant. |
| `INVENTORY.WAREHOUSE.NOT_FOUND` | Not Found | The warehouse does not exist in the current tenant. |
| `INVENTORY.WAREHOUSE.INACTIVE` | Validation | Stock movements cannot be recorded against an inactive warehouse. |

## Auth

| Code | Type | Meaning |
|---|---|---|
| `AUTH.INVALID_CREDENTIALS` | Unauthorized | Tenant code, email or password is missing or does not match an active user. |
| `AUTH.UNAUTHENTICATED` | Unauthorized | No valid bearer token was supplied. |
| `AUTH.TOKEN_EXPIRED` | Unauthorized | The access token expired; the client should refresh once. |
| `AUTH.FORBIDDEN` | Forbidden | The authenticated principal lacks the required permission. |
| `AUTH.INVALID_REFRESH_TOKEN` | Unauthorized | The refresh token is unknown, expired, revoked or already rotated. |

## Sales

| Code | Type | Meaning |
|---|---|---|
| `SALES.ORDER.INVALID_ID` | Validation | A sales order identifier is required. |
| `SALES.ORDER.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `SALES.ORDER.INVALID_ORDER_NUMBER` | Validation | Order number is blank, starts with an invalid character, exceeds 50 characters or uses unsupported characters. |
| `SALES.ORDER.ORDER_NUMBER_ALREADY_EXISTS` | Conflict | A sales order with this number already exists in the tenant. |
| `SALES.ORDER.INVALID_CUSTOMER_ID` | Validation | A customer identifier is required. |
| `SALES.ORDER.INVALID_LINE_ID` | Validation | A sales order line identifier is required. |
| `SALES.ORDER.INVALID_ITEM_ID` | Validation | An item identifier is required on every line. |
| `SALES.ORDER.INVALID_QUANTITY` | Validation | Line quantity is not greater than zero or exceeds the supported range. |
| `SALES.ORDER.INVALID_UNIT_PRICE` | Validation | Unit price is negative or exceeds the supported monetary range. |
| `SALES.ORDER.INVALID_LINE_TOTAL` | Validation | Line total exceeds the supported monetary range. |
| `SALES.ORDER.NO_LINES` | Validation | A sales order must contain at least one line. |
| `SALES.ORDER.INVALID_TOTAL_AMOUNT` | Validation | The order total exceeds the supported monetary range. |
| `SALES.ORDER.INVALID_CREATED_AT` | Validation | A valid creation time is required. |
| `SALES.ORDER.INVALID_CREATED_BY` | Validation | The user creating the sales order is required. |
| `SALES.ORDER.INVALID_UPDATED_AT` | Validation | A valid update time is required. |
| `SALES.ORDER.INVALID_UPDATED_BY` | Validation | The user updating the sales order is required. |
| `SALES.ORDER.INVALID_EVENT_ID` | Validation | A domain event identifier is required. |
| `SALES.ORDER.INVALID_STATUS_FILTER` | Validation | The status filter is not Draft, Confirmed or Cancelled. |
| `SALES.ORDER.INVALID_STATE` | Conflict | Only draft sales orders can be confirmed. |
| `SALES.ORDER.NOT_FOUND` | Not Found | The sales order does not exist in the current tenant. |

## Procurement

| Code | Type | Meaning |
|---|---|---|
| `PROCUREMENT.SUPPLIER.INVALID_ID` | Validation | A supplier identifier is required. |
| `PROCUREMENT.SUPPLIER.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `PROCUREMENT.SUPPLIER.INVALID_CODE` | Validation | Supplier code is out of range or uses unsupported characters. |
| `PROCUREMENT.SUPPLIER.INVALID_NAME` | Validation | Supplier name is blank or longer than 200 characters. |
| `PROCUREMENT.SUPPLIER.INVALID_CONTACT_NAME` | Validation | Supplier contact name is longer than 100 characters. |
| `PROCUREMENT.SUPPLIER.INVALID_EMAIL` | Validation | Supplier email is malformed or longer than 320 characters. |
| `PROCUREMENT.SUPPLIER.INVALID_PHONE` | Validation | Supplier phone number is longer than 50 characters. |
| `PROCUREMENT.SUPPLIER.INVALID_PAYMENT_TERMS_DAYS` | Validation | Payment terms are outside the 0 to 365 day range. |
| `PROCUREMENT.SUPPLIER.INVALID_CREATED_AT` | Validation | A valid creation time is required. |
| `PROCUREMENT.SUPPLIER.INVALID_CREATED_BY` | Validation | The user creating the supplier is required. |
| `PROCUREMENT.SUPPLIER.CODE_ALREADY_EXISTS` | Conflict | A supplier with this code already exists in the tenant. |
| `PROCUREMENT.SUPPLIER.INACTIVE` | Conflict | Purchase orders cannot be placed with an inactive supplier. |
| `PROCUREMENT.SUPPLIER.NOT_FOUND` | Not Found | The supplier does not exist in the current tenant. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_ID` | Validation | A purchase order identifier is required. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_ORDER_NUMBER` | Validation | Order number is blank, starts with an invalid character, exceeds 50 characters or uses unsupported characters. |
| `PROCUREMENT.PURCHASE_ORDER.ORDER_NUMBER_ALREADY_EXISTS` | Conflict | A purchase order with this number already exists in the tenant. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_SUPPLIER_ID` | Validation | A supplier identifier is required. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_SUPPLIER_NAME` | Validation | The supplier name snapshot is missing or longer than 200 characters. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_LINE_ID` | Validation | A purchase order line identifier is required. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_ITEM_ID` | Validation | An item identifier is required on every line. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_QUANTITY` | Validation | Line quantity is not greater than zero or exceeds the supported range. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_UNIT_PRICE` | Validation | Unit price is negative or exceeds the supported monetary range. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_LINE_TOTAL` | Validation | Line total exceeds the supported monetary range. |
| `PROCUREMENT.PURCHASE_ORDER.NO_LINES` | Validation | A purchase order must contain at least one line. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_TOTAL_AMOUNT` | Validation | The purchase order total exceeds the supported monetary range. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_EXPECTED_DELIVERY_DATE` | Validation | Expected delivery date is earlier than the order date. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_NOTES` | Validation | Purchase order notes are longer than 1000 characters. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_CREATED_AT` | Validation | A valid creation time is required. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_CREATED_BY` | Validation | The user creating the purchase order is required. |
| `PROCUREMENT.PURCHASE_ORDER.INVALID_STATUS_FILTER` | Validation | The status filter is not Draft, Approved, PartiallyReceived, Received or Cancelled. |
| `PROCUREMENT.PURCHASE_ORDER.SUPPLIER_NOT_FOUND` | Not Found | The selected supplier does not exist in the current tenant. |
| `PROCUREMENT.PURCHASE_ORDER.NOT_FOUND` | Not Found | The purchase order does not exist in the current tenant. |

## Finance

| Code | Type | Meaning |
|---|---|---|
| `FINANCE.GL_ACCOUNT.INVALID_ID` | Validation | A general ledger account identifier is required. |
| `FINANCE.GL_ACCOUNT.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `FINANCE.GL_ACCOUNT.INVALID_CODE` | Validation | Account code is blank, exceeds 20 characters or starts with an invalid character. |
| `FINANCE.GL_ACCOUNT.INVALID_NAME` | Validation | Account name is blank or longer than 200 characters. |
| `FINANCE.GL_ACCOUNT.INVALID_TYPE` | Validation | Account type is not Asset, Liability, Equity, Revenue or Expense. |
| `FINANCE.GL_ACCOUNT.INVALID_CREATED_AT` | Validation | A valid creation time is required. |
| `FINANCE.GL_ACCOUNT.INVALID_CREATED_BY` | Validation | The user creating the account is required. |
| `FINANCE.JOURNAL_ENTRY.INVALID_ID` | Validation | A journal entry identifier is required. |
| `FINANCE.JOURNAL_ENTRY.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `FINANCE.JOURNAL_ENTRY.INVALID_ENTRY_NUMBER` | Validation | Entry number is blank, exceeds 50 characters or starts with an invalid character. |
| `FINANCE.JOURNAL_ENTRY.INVALID_POSTED_DATE` | Validation | A valid posting date is required. |
| `FINANCE.JOURNAL_ENTRY.INVALID_DESCRIPTION` | Validation | Description is blank or longer than 500 characters. |
| `FINANCE.JOURNAL_ENTRY.NO_LINES` | Validation | A journal entry must contain at least two lines. |
| `FINANCE.JOURNAL_ENTRY.INVALID_LINE_ID` | Validation | A journal entry line identifier is required. |
| `FINANCE.JOURNAL_ENTRY.INVALID_GL_ACCOUNT_ID` | Validation | A general ledger account is required on every line. |
| `FINANCE.JOURNAL_ENTRY.INVALID_LINE_AMOUNT` | Validation | A line must carry exactly one positive debit or credit within the supported monetary range. |
| `FINANCE.JOURNAL_ENTRY.UNBALANCED` | Conflict | Total debits and credits are not equal or are zero. |
| `FINANCE.JOURNAL_ENTRY.INVALID_CREATED_AT` | Validation | A valid creation time is required. |
| `FINANCE.JOURNAL_ENTRY.INVALID_CREATED_BY` | Validation | The user creating the journal entry is required. |
| `FINANCE.JOURNAL_ENTRY.INVALID_DATE_RANGE` | Validation | The posting date range is inverted. |
| `FINANCE.JOURNAL_ENTRY.POSTED_ENTRIES_ARE_IMMUTABLE` | Conflict | Posted journal entries are append-only; post a reversing entry instead. |
| `FINANCE.JOURNAL_ENTRY.NOT_FOUND` | Not Found | The journal entry does not exist in the current tenant. |
| `FINANCE.AR_INVOICE.INVALID_ID` | Validation | An A/R invoice identifier is required. |
| `FINANCE.AR_INVOICE.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `FINANCE.AR_INVOICE.INVALID_INVOICE_NUMBER` | Validation | Invoice number is blank, exceeds 50 characters or starts with an invalid character. |
| `FINANCE.AR_INVOICE.INVALID_SALES_ORDER_ID` | Validation | A sales order identifier is required. |
| `FINANCE.AR_INVOICE.INVALID_CUSTOMER_ID` | Validation | A customer identifier is required. |
| `FINANCE.AR_INVOICE.INVALID_TOTAL_AMOUNT` | Validation | Invoice total is not positive or exceeds the supported monetary range. |
| `FINANCE.AR_INVOICE.INVALID_CREATED_AT` | Validation | A valid creation time is required. |
| `FINANCE.AR_INVOICE.INVALID_CREATED_BY` | Validation | The user creating the invoice is required. |
| `FINANCE.AR_INVOICE.INVALID_STATUS_FILTER` | Validation | The status filter is not Unpaid, Paid or Cancelled. |
| `FINANCE.AR_INVOICE.NOT_FOUND` | Not Found | The A/R invoice does not exist in the current tenant. |

## HRIS

| Code | Type | Meaning |
|---|---|---|
| `HRIS.EMPLOYEE.INVALID_ID` | Validation | An employee identifier is required. |
| `HRIS.EMPLOYEE.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `HRIS.EMPLOYEE.INVALID_EMPLOYEE_CODE` | Validation | Employee code is blank, out of range, starts with an invalid character or uses unsupported characters. |
| `HRIS.EMPLOYEE.INVALID_FULL_NAME` | Validation | Employee full name is blank or longer than 200 characters. |
| `HRIS.EMPLOYEE.INVALID_EMAIL` | Validation | Employee email is blank, malformed or longer than 320 characters. |
| `HRIS.EMPLOYEE.INVALID_JOB_TITLE` | Validation | Employee job title is blank or longer than 100 characters. |
| `HRIS.EMPLOYEE.INVALID_DEPARTMENT_ID` | Validation | A department identifier is required. |
| `HRIS.EMPLOYEE.INVALID_JOINED_DATE` | Validation | A valid joining date is required. |
| `HRIS.EMPLOYEE.ALREADY_ACTIVE` | Conflict | The employee is already active. |
| `HRIS.EMPLOYEE.ALREADY_INACTIVE` | Conflict | The employee is already inactive. |
| `HRIS.EMPLOYEE.NOT_FOUND` | Not Found | The employee does not exist in the current tenant. |
| `HRIS.ATTENDANCE.INVALID_ID` | Validation | An attendance log identifier is required. |
| `HRIS.ATTENDANCE.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `HRIS.ATTENDANCE.INVALID_EMPLOYEE_ID` | Validation | An employee identifier is required. |
| `HRIS.ATTENDANCE.INVALID_CLOCK_IN_UTC` | Validation | A valid clock-in time is required. |
| `HRIS.ATTENDANCE.INVALID_CLOCK_OUT_UTC` | Validation | Clock-out time is missing or not later than the clock-in time. |
| `HRIS.ATTENDANCE.INVALID_LATITUDE` | Validation | Latitude is outside the -90 to 90 degree range. |
| `HRIS.ATTENDANCE.INVALID_LONGITUDE` | Validation | Longitude is outside the -180 to 180 degree range. |
| `HRIS.ATTENDANCE.INVALID_PHOTO_URL` | Validation | Photo URL is longer than 1000 characters. |
| `HRIS.ATTENDANCE.INVALID_STATUS_TRANSITION` | Conflict | Only a clocked-in attendance log can be clocked out. |
| `HRIS.PAYROLL.INVALID_ID` | Validation | A payroll run identifier is required. |
| `HRIS.PAYROLL.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `HRIS.PAYROLL.INVALID_PERIOD_CODE` | Validation | Payroll period code is blank, out of range, starts with an invalid character or uses unsupported characters. |
| `HRIS.PAYROLL.INVALID_TOTAL_BASIC_SALARY` | Validation | Total basic salary is negative or exceeds the supported monetary range. |
| `HRIS.PAYROLL.INVALID_TOTAL_ALLOWANCES` | Validation | Total allowances are negative or exceed the supported monetary range. |
| `HRIS.PAYROLL.INVALID_TOTAL_DEDUCTIONS` | Validation | Total deductions are negative or exceed the supported monetary range. |
| `HRIS.PAYROLL.INVALID_NET_PAY_TOTAL` | Validation | Net pay total is negative or exceeds the supported monetary range. |
| `HRIS.PAYROLL.INVALID_PROCESSED_AT_UTC` | Validation | A valid processing time is required to finalize a payroll run. |
| `HRIS.PAYROLL.INVALID_STATUS_TRANSITION` | Conflict | The requested payroll status transition is not allowed from the current status. |
| `HRIS.PAYROLL.NOT_FOUND` | Not Found | The payroll run does not exist in the current tenant. |

## Projects

| Code | Type | Meaning |
|---|---|---|
| `PROJECTS.PROJECT.INVALID_ID` | Validation | A project identifier is required. |
| `PROJECTS.PROJECT.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `PROJECTS.PROJECT.INVALID_CODE` | Validation | Project code is blank, out of range, starts with an invalid character or uses unsupported characters. |
| `PROJECTS.PROJECT.INVALID_NAME` | Validation | Project name is blank or longer than 200 characters. |
| `PROJECTS.PROJECT.INVALID_DESCRIPTION` | Validation | Project description is longer than 1000 characters. |
| `PROJECTS.PROJECT.INVALID_CUSTOMER_ID` | Validation | A customer identifier is required. |
| `PROJECTS.PROJECT.INVALID_START_DATE` | Validation | A valid project start date is required. |
| `PROJECTS.PROJECT.INVALID_END_DATE` | Validation | Project end date is earlier than the start date. |
| `PROJECTS.PROJECT.INVALID_STATUS_TRANSITION` | Conflict | The requested project status transition is not allowed from the current status. |
| `PROJECTS.PROJECT.NOT_FOUND` | Not Found | The project does not exist in the current tenant. |
| `PROJECTS.TASK.INVALID_ID` | Validation | A project task identifier is required. |
| `PROJECTS.TASK.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `PROJECTS.TASK.INVALID_PROJECT_ID` | Validation | A project identifier is required. |
| `PROJECTS.TASK.INVALID_TITLE` | Validation | Task title is blank or longer than 200 characters. |
| `PROJECTS.TASK.INVALID_ASSIGNEE_EMPLOYEE_ID` | Validation | The assignee is not a valid employee identifier. |
| `PROJECTS.TASK.INVALID_ESTIMATED_HOURS` | Validation | Estimated hours are negative or exceed the supported range. |
| `PROJECTS.TASK.INVALID_HOURS` | Validation | Logged hours are not greater than zero or exceed the supported range. |
| `PROJECTS.TASK.INVALID_COMPLETED_HOURS` | Validation | Completed hours exceed the supported range. |
| `PROJECTS.TASK.INVALID_STATUS_TRANSITION` | Conflict | The requested task status transition is not allowed from the current status. |
| `PROJECTS.TASK.NOT_FOUND` | Not Found | The project task does not exist in the current tenant. |
| `PROJECTS.WORK_LOG.INVALID_ID` | Validation | A work log identifier is required. |
| `PROJECTS.WORK_LOG.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `PROJECTS.WORK_LOG.INVALID_PROJECT_TASK_ID` | Validation | A project task identifier is required. |
| `PROJECTS.WORK_LOG.INVALID_EMPLOYEE_ID` | Validation | An employee identifier is required. |
| `PROJECTS.WORK_LOG.INVALID_LOGGED_DATE` | Validation | A valid work log date is required. |
| `PROJECTS.WORK_LOG.INVALID_HOURS_SPENT` | Validation | Hours spent are not greater than zero or exceed 24 in a single entry. |
| `PROJECTS.WORK_LOG.INVALID_DESCRIPTION` | Validation | Work log description is blank or longer than 500 characters. |

## Asset Maintenance

| Code | Type | Meaning |
|---|---|---|
| `ASSETS.ASSET.INVALID_ID` | Validation | An asset identifier is required. |
| `ASSETS.ASSET.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `ASSETS.ASSET.INVALID_ASSET_CODE` | Validation | Asset code is blank, out of range, starts with an invalid character or uses unsupported characters. |
| `ASSETS.ASSET.INVALID_NAME` | Validation | Asset name is blank or longer than 200 characters. |
| `ASSETS.ASSET.INVALID_CATEGORY` | Validation | Asset category is blank or longer than 100 characters. |
| `ASSETS.ASSET.INVALID_SERIAL_NUMBER` | Validation | Asset serial number is longer than 100 characters. |
| `ASSETS.ASSET.INVALID_QR_CODE_URL` | Validation | Asset QR code URL is longer than 1000 characters. |
| `ASSETS.ASSET.INVALID_PURCHASE_VALUE` | Validation | Purchase value is negative or exceeds the supported monetary range. |
| `ASSETS.ASSET.INVALID_ACQUISITION_DATE` | Validation | A valid acquisition date is required. |
| `ASSETS.ASSET.INVALID_STATUS_TRANSITION` | Conflict | The requested asset status transition is not allowed from the current status. |
| `ASSETS.ASSET.CODE_ALREADY_EXISTS` | Conflict | An asset with this code already exists in the tenant. |
| `ASSETS.ASSET.NOT_FOUND` | Not Found | The asset does not exist in the current tenant. |
| `ASSETS.MAINTENANCE_ORDER.INVALID_ID` | Validation | A maintenance order identifier is required. |
| `ASSETS.MAINTENANCE_ORDER.INVALID_TENANT_ID` | Validation | A tenant identifier is required. |
| `ASSETS.MAINTENANCE_ORDER.INVALID_ASSET_ID` | Validation | An asset identifier is required. |
| `ASSETS.MAINTENANCE_ORDER.INVALID_REQUEST_NUMBER` | Validation | Maintenance request number is blank, out of range, starts with an invalid character or uses unsupported characters. |
| `ASSETS.MAINTENANCE_ORDER.INVALID_SCHEDULED_DATE` | Validation | The scheduled date is not valid. |
| `ASSETS.MAINTENANCE_ORDER.INVALID_COMPLETED_DATE` | Validation | A valid completion date is required. |
| `ASSETS.MAINTENANCE_ORDER.INVALID_COST` | Validation | Maintenance cost is negative or exceeds the supported monetary range. |
| `ASSETS.MAINTENANCE_ORDER.INVALID_DESCRIPTION` | Validation | Maintenance description is blank or longer than 1000 characters. |
| `ASSETS.MAINTENANCE_ORDER.INVALID_STATUS_TRANSITION` | Conflict | The requested maintenance order status transition is not allowed from the current status. |
| `ASSETS.MAINTENANCE_ORDER.REQUEST_NUMBER_ALREADY_EXISTS` | Conflict | A maintenance order with this request number already exists in the tenant. |
