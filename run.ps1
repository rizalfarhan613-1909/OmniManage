<#
.SYNOPSIS
    Local developer entry point for OmniManage.

.DESCRIPTION
    Starts the PostgreSQL container, applies the EF Core migrations, and runs
    the API and web app. Seeding is enabled only by
    backend/src/Host/OmniManage.Api/appsettings.Development.json.

.PARAMETER Task
    infra     Start PostgreSQL and wait until it is healthy.
    migrate   Build the solution and apply every module migration.
    backend   Run the API on http://localhost:5000.
    frontend  Run the Next.js app on http://localhost:3000.
    all       infra + migrate, then backend in a new window and frontend here.
    reset     Delete the local database volume, then run infra + migrate.
    status    Show container status.

.EXAMPLE
    .\run.ps1 all
#>

[CmdletBinding()]
param(
    [ValidateSet('infra', 'migrate', 'backend', 'frontend', 'all', 'reset', 'status')]
    [string]$Task = 'all'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$solution = Join-Path $root 'backend/OmniManage.sln'
$apiProject = Join-Path $root 'backend/src/Host/OmniManage.Api/OmniManage.Api.csproj'
$frontendDirectory = Join-Path $root 'frontend'
$migrationConnection = 'Host=localhost;Port=5433;Database=omnimanage_db;Username=omni_migrator;Password=Password123!;SslMode=Prefer;Include Error Detail=false'

$migrations = @(
    @{
        Context = 'PlatformDbContext'
        Project = 'backend/src/Modules/Platform/OmniManage.Modules.Platform.Infrastructure/OmniManage.Modules.Platform.Infrastructure.csproj'
    },
    @{
        Context = 'MasterDataDbContext'
        Project = 'backend/src/Modules/MasterData/OmniManage.Modules.MasterData.Infrastructure/OmniManage.Modules.MasterData.Infrastructure.csproj'
    },
    @{
        Context = 'InventoryDbContext'
        Project = 'backend/src/Modules/Inventory/OmniManage.Modules.Inventory.Infrastructure/OmniManage.Modules.Inventory.Infrastructure.csproj'
    },
    @{
        Context = 'ManufacturingDbContext'
        Project = 'backend/src/Modules/Manufacturing/OmniManage.Modules.Manufacturing.Infrastructure/OmniManage.Modules.Manufacturing.Infrastructure.csproj'
    },
    @{
        Context = 'ProcurementDbContext'
        Project = 'backend/src/Modules/Procurement/OmniManage.Modules.Procurement.Infrastructure/OmniManage.Modules.Procurement.Infrastructure.csproj'
    },
    @{
        Context = 'SalesDbContext'
        Project = 'backend/src/Modules/Sales/OmniManage.Modules.Sales.Infrastructure/OmniManage.Modules.Sales.Infrastructure.csproj'
    },
    @{
        Context = 'FinanceDbContext'
        Project = 'backend/src/Modules/Finance/OmniManage.Modules.Finance.Infrastructure/OmniManage.Modules.Finance.Infrastructure.csproj'
    },
    @{
        Context = 'HrisDbContext'
        Project = 'backend/src/Modules/Hris/OmniManage.Modules.Hris.Infrastructure/OmniManage.Modules.Hris.Infrastructure.csproj'
    },
    @{
        Context = 'ProjectsDbContext'
        Project = 'backend/src/Modules/Projects/OmniManage.Modules.Projects.Infrastructure/OmniManage.Modules.Projects.Infrastructure.csproj'
    },
    @{
        Context = 'AssetMaintenanceDbContext'
        Project = 'backend/src/Modules/AssetMaintenance/OmniManage.Modules.AssetMaintenance.Infrastructure/OmniManage.Modules.AssetMaintenance.Infrastructure.csproj'
    }
)

function Write-Step {
    param([Parameter(Mandatory = $true)][string]$Message)

    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Assert-Command {
    param([Parameter(Mandatory = $true)][string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found on PATH."
    }
}

function Invoke-Infra {
    Assert-Command -Name 'docker'

    Write-Step 'Starting PostgreSQL 16 (docker compose)'
    docker compose up -d postgres
    if ($LASTEXITCODE -ne 0) {
        throw 'docker compose up failed. Check that the Docker engine is running and port 5433 is free.'
    }

    Write-Step 'Waiting for PostgreSQL to become healthy'
    $deadline = (Get-Date).AddSeconds(90)

    while ((Get-Date) -lt $deadline) {
        $status = docker inspect --format '{{.State.Health.Status}}' omnimanage-postgres 2>$null

        if ($status -eq 'healthy') {
            Write-Host 'PostgreSQL is healthy.' -ForegroundColor Green
            return
        }

        if ($status -eq 'unhealthy') {
            docker compose logs postgres
            throw 'PostgreSQL reported an unhealthy state.'
        }

        Start-Sleep -Seconds 2
    }

    docker compose logs postgres
    throw 'Timed out waiting for PostgreSQL. Check "docker compose logs postgres".'
}

function Invoke-Migrate {
    Assert-Command -Name 'dotnet'

    dotnet ef --version *> $null
    if ($LASTEXITCODE -ne 0) {
        throw 'The dotnet-ef tool is required. Install it with: dotnet tool install --global dotnet-ef --version 8.0.0'
    }

    $env:OMNIMANAGE_MIGRATION_CONNECTION = $migrationConnection

    Write-Step 'Building the solution'
    dotnet build $solution
    if ($LASTEXITCODE -ne 0) {
        throw 'The solution build failed.'
    }

    foreach ($migration in $migrations) {
        Write-Step "Applying migrations for $($migration.Context)"
        dotnet ef database update `
            --context $migration.Context `
            --project (Join-Path $root $migration.Project) `
            --startup-project $apiProject `
            --no-build `
            --connection $migrationConnection

        if ($LASTEXITCODE -ne 0) {
            throw "Migrations failed for $($migration.Context)."
        }
    }

    Write-Host 'All migrations applied.' -ForegroundColor Green
}

function Invoke-Backend {
    Assert-Command -Name 'dotnet'

    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ASPNETCORE_URLS = 'http://localhost:5000'

    Write-Step 'Starting the API on http://localhost:5000'
    dotnet run --project $apiProject --no-launch-profile
}

function Invoke-Frontend {
    Assert-Command -Name 'npm'

    Push-Location $frontendDirectory
    try {
        if (-not (Test-Path (Join-Path $frontendDirectory 'node_modules'))) {
            Write-Step 'Installing frontend dependencies'
            npm install
        }

        Write-Step 'Starting the web app on http://localhost:3000'
        npm run dev
    }
    finally {
        Pop-Location
    }
}

function Start-BackendWindow {
    Assert-Command -Name 'dotnet'

    Write-Step 'Starting the API in a new window on http://localhost:5000'
    $backend = Start-Process `
        -FilePath 'dotnet' `
        -ArgumentList @('run', '--project', $apiProject, '--no-launch-profile') `
        -WorkingDirectory $root `
        -PassThru

    Write-Step 'Waiting for the API health endpoint'
    $deadline = (Get-Date).AddSeconds(90)

    while ((Get-Date) -lt $deadline) {
        if ($backend.HasExited) {
            throw "The API exited with code $($backend.ExitCode). Review the backend window output."
        }

        try {
            $response = Invoke-WebRequest -Uri 'http://localhost:5000/health/live' -UseBasicParsing -TimeoutSec 2
            if ($response.StatusCode -eq 200) {
                Write-Host 'API is healthy.' -ForegroundColor Green
                return
            }
        }
        catch {
            # The API is still starting.
        }

        Start-Sleep -Seconds 2
    }

    throw 'Timed out waiting for the API health endpoint.'
}

Push-Location $root
try {
    switch ($Task) {
        'infra' {
            Invoke-Infra
        }
        'migrate' {
            Invoke-Infra
            Invoke-Migrate
        }
        'backend' {
            Invoke-Backend
        }
        'frontend' {
            Invoke-Frontend
        }
        'status' {
            docker compose ps
        }
        'reset' {
            $answer = Read-Host "This deletes the local OmniManage database volume. Type 'reset' to continue"
            if ($answer -ne 'reset') {
                Write-Host 'Reset cancelled.' -ForegroundColor Yellow
                return
            }

            docker compose down -v
            Invoke-Infra
            Invoke-Migrate
        }
        'all' {
            $env:ASPNETCORE_ENVIRONMENT = 'Development'
            $env:ASPNETCORE_URLS = 'http://localhost:5000'

            Invoke-Infra
            Invoke-Migrate
            Start-BackendWindow
            Invoke-Frontend
        }
    }
}
finally {
    Pop-Location
}
