# Build-And-Deploy.ps1
# Compiles the ExtendedWhips mod and copies output DLL to Construction Simulator plugins

param(
    [string]$Configuration = "Release",
    [string]$GamePluginsDir = "D:\SteamLibrary\steamapps\common\Construction Simulator\BepInEx\plugins\ExtendedWhips"
)

$ErrorActionPreference = "Stop"

$ProjectDir = Join-Path $PSScriptRoot "..\src\ExtendedWhips"
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " Building ExtendedWhips ($Configuration)" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

dotnet build "$ProjectDir\ExtendedWhips.csproj" -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
    exit 1
}

$OutputDir = Join-Path $ProjectDir "bin\$Configuration\net6.0"
$OutputDll = Join-Path $OutputDir "ExtendedWhips.dll"

if (-not (Test-Path $OutputDll)) {
    # Check netstandard2.1 fallback
    $OutputDir = Join-Path $ProjectDir "bin\$Configuration\netstandard2.1"
    $OutputDll = Join-Path $OutputDir "ExtendedWhips.dll"
}

if (-not (Test-Path $OutputDll)) {
    Write-Error "Could not locate compiled ExtendedWhips.dll!"
    exit 1
}

Write-Host "`nDeploying to: $GamePluginsDir" -ForegroundColor Yellow
if (-not (Test-Path $GamePluginsDir)) {
    New-Item -ItemType Directory -Path $GamePluginsDir -Force | Out-Null
}

Copy-Item $OutputDll -Destination $GamePluginsDir -Force
Write-Host "Deployed successfully!" -ForegroundColor Green
Write-Host "Ready to run Construction Simulator with the mod."

