# Install-BepInEx.ps1
# Automates the installation of BepInEx 6 (IL2CPP 64-bit) for Construction Simulator

param(
    [string]$GameDir = "D:\SteamLibrary\steamapps\common\Construction Simulator"
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " ConSim BepInEx 6 (IL2CPP x64) Installer" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

if (-not (Test-Path $GameDir)) {
    Write-Error "Could not find Construction Simulator at: $GameDir"
    exit 1
}

$ExePath = Join-Path $GameDir "ConSim.exe"
if (-not (Test-Path $ExePath)) {
    Write-Error "ConSim.exe not found in $GameDir!"
    exit 1
}

$DownloadUrl = "https://github.com/BepInEx/BepInEx/releases/download/v6.0.0-pre.2/BepInEx-Unity.IL2CPP-win-x64-6.0.0-pre.2.zip"
$ZipDest = Join-Path $env:TEMP "BepInEx_ConSim.zip"

Write-Host "`n[1/3] Downloading BepInEx 6 (IL2CPP x64)..." -ForegroundColor Yellow
Invoke-WebRequest -Uri $DownloadUrl -OutFile $ZipDest -UserAgent "PowerShell"
Write-Host "Download complete: $ZipDest" -ForegroundColor Green

Write-Host "`n[2/3] Extracting BepInEx into game folder..." -ForegroundColor Yellow
Expand-Archive -Path $ZipDest -DestinationPath $GameDir -Force
Remove-Item $ZipDest -Force -ErrorAction SilentlyContinue
Write-Host "Extracted successfully into: $GameDir" -ForegroundColor Green

Write-Host "`n[3/3] Ensuring plugins directory exists..." -ForegroundColor Yellow
$PluginsDir = Join-Path $GameDir "BepInEx\plugins"
if (-not (Test-Path $PluginsDir)) {
    New-Item -ItemType Directory -Path $PluginsDir -Force | Out-Null
}

Write-Host "`nInstallation finished!" -ForegroundColor Green
Write-Host "IMPORTANT NEXT STEP:" -ForegroundColor Cyan
Write-Host "Launch Construction Simulator once and exit it."
Write-Host "BepInEx will unhollow and generate all IL2CPP interop assemblies in BepInEx\interop."

