# build-integration-host.ps1

param (
    [string]$baseWebApiSource = "..\..\BaseWebApi\BaseWebApi"
)

$ErrorActionPreference = "Stop"
$scriptRoot = $PSScriptRoot

# === INPUTS ===
$integrationSourceDir = $scriptRoot                       # current folder = IntegrationTestHost
$buildFolder          = "..\..\..\Build"
$csprojName           = "BaseWebApi.csproj"
$solutionName         = "BaseWebApi.sln"
$targetDir            = Join-Path $buildFolder "BaseWebApi"
$packagesInstaller    = "install-domainservices-packages.ps1"

# === CLEAN BUILD ===
Write-Host "Cleaning $buildFolder..." -ForegroundColor Cyan
if (Test-Path $buildFolder) {
    Remove-Item -Force -Recurse $buildFolder
}
New-Item -ItemType Directory -Path $buildFolder | Out-Null

# === COPY TEMPLATE ===
Write-Host "Copying BaseWebApi template..." -ForegroundColor Cyan
if (-Not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
}
Get-ChildItem -Path $baseWebApiSource | ForEach-Object {
    $destinationPath = Join-Path $targetDir $_.Name
    Copy-Item -Recurse -Force -Path $_.FullName -Destination $destinationPath
}

# === PATCH .csproj for RuntimeIdentifier ===
Write-Host "Patching .csproj to use RuntimeIdentifier instead of PlatformTarget..." -ForegroundColor Cyan
$csprojPath = Join-Path $targetDir $csprojName

if (-Not (Test-Path $csprojPath)) {
    Write-Error ".csproj file not found at $csprojPath"
    exit 1
}

[xml]$csprojXml = Get-Content $csprojPath
$propertyGroup = $csprojXml.Project.PropertyGroup | Where-Object { $_.TargetFramework -ne $null }

if ($propertyGroup) {
    $platformNode = $propertyGroup.SelectSingleNode("PlatformTarget")
    if ($platformNode -ne $null) {
        $propertyGroup.RemoveChild($platformNode) | Out-Null
    }

    if (-not $propertyGroup.RuntimeIdentifier) {
        $runtimeNode = $csprojXml.CreateElement("RuntimeIdentifier")
        $runtimeNode.InnerText = "win-x64"
        $propertyGroup.AppendChild($runtimeNode) | Out-Null
    }

    $csprojXml.Save($csprojPath)
    Write-Host "Patched $csprojName to use RuntimeIdentifier=win-x64" -ForegroundColor Green
} else {
    Write-Error "Could not find <TargetFramework> in $csprojName to patch"
    exit 1
}

# === COPY App_Data ===
Write-Host "Copying App_Data..." -ForegroundColor Cyan
$appDataSrc  = Join-Path $integrationSourceDir 'App_Data'
$appDataDest = Join-Path $targetDir            'App_Data'
if (-not (Test-Path $appDataDest)) { New-Item -ItemType Directory -Path $appDataDest -Force | Out-Null }
Copy-Item -Recurse -Force -Path (Join-Path $appDataSrc '*') -Destination $appDataDest

# === CREATE SCENARIOS BASELINE COPY ===
Write-Host "Creating scenarios-baseline.json from scenarios.json..." -ForegroundColor Cyan
$scenariosJson     = Join-Path $appDataDest 'scenarios.json'
$scenariosBaseline = Join-Path $appDataDest 'scenarios-baseline.json'

if (Test-Path $scenariosJson) {
    Copy-Item -Force -Path $scenariosJson -Destination $scenariosBaseline
    Write-Host 'Created scenarios-baseline.json' -ForegroundColor Green
} else {
    Write-Warning "scenarios.json not found at $scenariosJson, cannot create baseline"
}

# === COPY RadarImagesController.cs ===
Write-Host "Copying RadarImagesController.cs..." -ForegroundColor Cyan
$controllerSource = Join-Path $integrationSourceDir "RadarImagesController.cs"
$controllerDest   = Join-Path $targetDir "RadarImagesController.cs"
Copy-Item -Force -Path $controllerSource -Destination $controllerDest

# === COPY FakeModelDataReader.cs ===
Write-Host "Copying FakeModelDataReader.cs..." -ForegroundColor Cyan
$controllerSource = Join-Path $integrationSourceDir "FakeModelDataReader.cs"
$controllerDest   = Join-Path $targetDir "FakeModelDataReader.cs"
Copy-Item -Force -Path $controllerSource -Destination $controllerDest

# === COPY FakeScenarioWorker.cs ===
Write-Host "Copying FakeModelDataReader.cs..." -ForegroundColor Cyan
$controllerSource = Join-Path $integrationSourceDir "FakeScenarioWorker.cs"
$controllerDest   = Join-Path $targetDir "FakeScenarioWorker.cs"
Copy-Item -Force -Path $controllerSource -Destination $controllerDest

# === COPY ScenarioRepositoryWithFakeFactory.cs ===
Write-Host "Copying FakeModelDataReader.cs..." -ForegroundColor Cyan
$controllerSource = Join-Path $integrationSourceDir "ScenarioRepositoryWithFakeFactory.cs"
$controllerDest   = Join-Path $targetDir "ScenarioRepositoryWithFakeFactory.cs"
Copy-Item -Force -Path $controllerSource -Destination $controllerDest

# === OVERWRITE Program.cs (safely) ===
Write-Host "Overwriting Program.cs..." -ForegroundColor Cyan
$programSource = Join-Path $integrationSourceDir "Program.cs"
$programDest   = Join-Path $targetDir "Program.cs"

if ((Resolve-Path $programSource).Path -ne (Resolve-Path $programDest).Path) {
    Copy-Item -Force -Path $programSource -Destination $programDest
} else {
    Write-Host "⚠ Skipped: Source and destination Program.cs are the same." -ForegroundColor Yellow
}

# === OVERWRITE appsettings.json (safely) ===
Write-Host "Overwriting appsettings.json..." -ForegroundColor Cyan
$appsettingsSource = Join-Path $integrationSourceDir "appsettings.json"
$appsettingsDest   = Join-Path $targetDir "appsettings.json"

if ((Resolve-Path $appsettingsSource).Path -ne (Resolve-Path $appsettingsDest).Path) {
    Copy-Item -Force -Path $appsettingsSource -Destination $appsettingsDest
} else {
    Write-Host "Skipped: Source and destination appsettings.json are the same." -ForegroundColor Yellow
}

# === COPY NuGet.config ===
Write-Host "Copying NuGet.config..." -ForegroundColor Cyan
$nugetConfigSrc = Join-Path $integrationSourceDir "NuGet.config"
$nugetConfigDest = Join-Path $targetDir "NuGet.config"
Copy-Item -Force -Path $nugetConfigSrc -Destination $nugetConfigDest

# === INSTALL NuGet Packages ===
Write-Host "Installing NuGet packages to generated .csproj..." -ForegroundColor Cyan
& (Join-Path $integrationSourceDir $packagesInstaller) `
    -csprojPath (Join-Path $targetDir $csprojName)

# === INSTALL NuGet Packages into Test Project ===
$testProjectInstaller = Join-Path $scriptRoot "..\IntegrationTestHost.Tests\install-domainservices-packages.ps1"
$testProjectPath = Join-Path $scriptRoot "..\IntegrationTestHost.Tests\IntegrationTestHost.Tests.csproj"

Write-Host "Installing Domain Services packages into test project..." -ForegroundColor Cyan
& $testProjectInstaller -csprojPath $testProjectPath

# === CHECK IF RUNNING INSIDE GITHUB ACTIONS ===
$inGitHubActions = $env:GITHUB_ACTIONS -eq "true"

if ($inGitHubActions) {
    Write-Host "Detected GitHub Actions environment. Skipping docker-compose and setting up PostgreSQL directly." -ForegroundColor Cyan
} else {
    # === START DOCKER COMPOSE ===
    Write-Host "Starting PostgreSQL container..." -ForegroundColor Cyan
    $composeFile = Join-Path $integrationSourceDir "docker-compose.yml"
    docker-compose -f $composeFile up -d

    # === WAIT FOR POSTGRES TO BE READY ===
    Write-Host "Waiting for PostgreSQL to be ready..." -ForegroundColor Cyan
    $maxAttempts = 20
    $attempt = 0
    $ready = $false

    while ($attempt -lt $maxAttempts) {
        $exitCode = docker exec integrationtest-postgres pg_isready -U postgres
        if ($LASTEXITCODE -eq 0) {
            Write-Host "PostgreSQL is ready." -ForegroundColor Green
            $ready = $true
            break
        } else {
            Write-Host "Waiting for PostgreSQL... ($attempt/$maxAttempts)"
            Start-Sleep -Seconds 1
            $attempt++
        }
    }

    if (-not $ready) {
        Write-Error "PostgreSQL did not become ready in time."
        docker-compose -f $composeFile logs
        exit 1
    }
}

# === BUILD PROJECT ===
Write-Host "Building generated WebApi..." -ForegroundColor Cyan
dotnet build (Join-Path $targetDir $csprojName)
Pop-Location

# === RUN PROJECT IN BACKGROUND ===
Write-Host "Running generated Web API in background..." -ForegroundColor Cyan
Push-Location $targetDir
$dotnetRun = Start-Process "dotnet" -ArgumentList "run" -NoNewWindow -PassThru
Start-Sleep -Seconds 2

# === WAIT FOR PORT 5000 TO BE OPEN ===
function Test-PortOpen {
    param ([string]$hostname, [int]$port)
    try {
        $client = New-Object System.Net.Sockets.TcpClient
        $client.Connect($hostname, $port)
        $client.Close()
        return $true
    } catch {
        return $false
    }
}

$maxAttempts = 60
$attempt = 0
$ready = $false
Write-Host "Waiting for port 5000 to be open..." -ForegroundColor Cyan
while ($attempt -lt $maxAttempts) {
    if (Test-PortOpen -hostname "localhost" -port 5000) {
        Write-Host "TCP port 5000 is open." -ForegroundColor Green
        $ready = $true
        break
    } else {
        Write-Host "Waiting for port 5000... ($attempt/$maxAttempts)"
        Start-Sleep -Seconds 1
        $attempt++
    }
}
if (-not $ready) {
    Write-Error "TCP port 5000 never opened."
    $dotnetRun.Kill()
    exit 1
}

Start-Sleep -Seconds 5

# === RUN TESTS ===
Write-Host "Running integration tests..." -ForegroundColor Cyan
dotnet test $testProjectPath

# === CLEAN UP: STOP API PROCESS ===
Write-Host "Stopping Web API process..." -ForegroundColor Cyan
$dotnetRun.Kill()

Pop-Location
Write-Host "Build completed. Output ready in: $buildFolder" -ForegroundColor Green