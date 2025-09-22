param(
    [string]$csprojPath = "IntegrationTestHost.Tests.csproj"
)

$ErrorActionPreference = "Stop"

# Validate file
if (-not (Test-Path $csprojPath)) {
    Write-Error "Could not find $csprojPath. Run this script from the test directory or pass the path explicitly."
    exit 1
}

# Domain Services packages
$packages = @(
    "DHI.Services",
    "DHI.Services.WebApiCore",
    "DHI.Services.Security.WebApi",
    "DHI.Services.TimeSeries.WebApi",
    "DHI.Services.GIS.WebApi",
    "DHI.Services.Jobs.WebApi",
    "DHI.Services.Connections.WebApi",
    "DHI.Services.Logging.WebApi",
    "DHI.Services.Spreadsheets.WebApi",
    "DHI.Services.Places.WebApi",
    "DHI.Services.Documents.WebApi",
    "DHI.Services.JsonDocuments.WebApi",
    "DHI.Services.WebApi",
    "DHI.Services.Meshes.WebApi",
    "DHI.Services.Scalars.WebApi",
    "DHI.Services.Tables.WebApi",
    "DHI.Services.TimeSteps.WebApi",
    "DHI.Services.Rasters.WebApi",
    "DHI.Services.Models.WebApi",
    "DHI.Services.PostgreSQL",
    "DHI.Services.MIKECore-NoVCRedist",
    "DHI.Services.MCLite",
    "DHI.Services.DS",
    "DHI.Services.MIKE1D",
    "DHI.Services.MIKECloud",
    "DHI.Services.OpenXML",
    "DHI.Services.ShapeFile",
    "DHI.Services.USGS"
)

Write-Host "Installing latest Domain Services packages into test project..." -ForegroundColor Cyan

foreach ($package in $packages) {
    Write-Host "→ $package"
    dotnet add "$csprojPath" package $package --prerelease
}

Write-Host "`n✅ All packages installed into $csprojPath" -ForegroundColor Green