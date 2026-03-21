#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [switch]$Clean,
    [string]$Configuration = "Release",
    [ValidateSet("win-x64", "win-x86", "win-arm64")]
    [string]$Rid = "win-x64",
    [int]$Jobs = [Environment]::ProcessorCount,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Demo
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Fail {
    param([string]$Message)
    throw $Message
}

function Require-Command {
    param([string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        Fail "missing required command: $Name"
    }
}

function Get-DemoTargetFramework {
    param([string]$DemoName)

    switch ($DemoName) {
        "SerialPort" { return "net10.0-windows" }
        "WinFormsDemo" { return "net10.0-windows" }
        "PictureBoxDemo" { return "net10.0-windows" }
        "MusicDemo" { return "net10.0-windows" }
        "SmartWatchDemo" { return "net10.0-windows" }
        default { return "net10.0" }
    }
}

function Get-DemoProjectPath {
    param([string]$DemoName)

    switch ($DemoName) {
        "MusicDemo" { return (Join-Path $rootDir "src/Demos/MusicWinFromsDemo/MusicDemo.csproj") }
        default { return (Join-Path $rootDir "src/Demos/$DemoName/$DemoName.csproj") }
    }
}

function Normalize-Demo {
    param([string]$Name)

    switch ($Name.ToLowerInvariant()) {
        "serialport" { return "SerialPort" }
        "winformsdemo" { return "WinFormsDemo" }
        "pictureboxdemo" { return "PictureBoxDemo" }
        "musicdemo" { return "MusicDemo" }
        "smartwatchdemo" { return "SmartWatchDemo" }
        default { return $null }
    }
}

$allDemos = @(
    "SerialPort",
    "WinFormsDemo",
    "PictureBoxDemo",
    "MusicDemo",
    "SmartWatchDemo"
)

$demoNames = @()
foreach ($item in $Demo) {
    $normalized = Normalize-Demo $item
    if (-not $normalized) {
        Fail "unknown demo: $item"
    }

    if ($demoNames -notcontains $normalized) {
        $demoNames += $normalized
    }
}

if ($demoNames.Count -eq 0) {
    $demoNames = $allDemos
}

switch ($Rid) {
    "win-x64" { $cmakeArch = "x64" }
    "win-x86" { $cmakeArch = "Win32" }
    "win-arm64" { $cmakeArch = "ARM64" }
    default { Fail "unsupported RID: $Rid" }
}

Require-Command "cmake"
Require-Command "dotnet"

$rootDir = $PSScriptRoot
$lvglSourceDir = Join-Path $rootDir "libs/lvgl"
$lvglBuildDir = Join-Path $rootDir "libs/build/lvgl-$Rid"
$distDir = Join-Path $rootDir "dist/$Rid"
$lvBuildConfDir = [System.IO.Path]::GetFullPath((Join-Path $rootDir "libs"))

if ($Clean) {
    Remove-Item -Recurse -Force $distDir -ErrorAction SilentlyContinue
    Remove-Item -Recurse -Force $lvglBuildDir -ErrorAction SilentlyContinue
}

New-Item -ItemType Directory -Force -Path $distDir | Out-Null

Write-Host "==> Building LVGL shared library ($Rid)"
& cmake `
    -S $lvglSourceDir `
    -B $lvglBuildDir `
    -G "Visual Studio 17 2022" `
    -A $cmakeArch `
    -DBUILD_SHARED_LIBS=ON `
    -DCONFIG_LV_BUILD_EXAMPLES=OFF `
    -DCONFIG_LV_BUILD_DEMOS=OFF `
    -DCONFIG_LV_USE_THORVG_INTERNAL=OFF `
    -DCONFIG_LV_USE_PRIVATE_API=ON `
    "-DLV_BUILD_CONF_DIR=$lvBuildConfDir"
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

& cmake --build $lvglBuildDir --config $Configuration --parallel $Jobs
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$lvglDll = $null
$preferredDllDir = Join-Path $lvglBuildDir $Configuration
if (Test-Path $preferredDllDir) {
    $lvglDll = Get-ChildItem -Path $preferredDllDir -Filter "lvgl.dll" -Recurse -ErrorAction SilentlyContinue |
        Sort-Object FullName |
        Select-Object -First 1
}

if (-not $lvglDll) {
    $lvglDll = Get-ChildItem -Path $lvglBuildDir -Filter "lvgl.dll" -Recurse -ErrorAction SilentlyContinue |
        Sort-Object FullName |
        Select-Object -First 1
}

if (-not $lvglDll) {
    Fail "missing built LVGL shared library under $lvglBuildDir"
}

function Publish-Demo {
    param([string]$DemoName)

    $projectPath = Get-DemoProjectPath $DemoName
    $publishDir = Join-Path $distDir $DemoName
    $executablePath = Join-Path $publishDir "$DemoName.exe"
    $targetFramework = Get-DemoTargetFramework $DemoName

    if (-not (Test-Path $projectPath)) {
        Fail "missing demo project: $projectPath"
    }

    Write-Host "==> Publishing $DemoName ($targetFramework)"
    Remove-Item -Recurse -Force $publishDir -ErrorAction SilentlyContinue

    & dotnet publish $projectPath `
        -f $targetFramework `
        -c $Configuration `
        -r $Rid `
        -o $publishDir `
        -p:EnableWindowsTargeting=true
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    if (-not (Test-Path $executablePath)) {
        Fail "missing published executable: $executablePath"
    }

    Copy-Item -LiteralPath $lvglDll.FullName -Destination (Join-Path $publishDir "lvgl.dll") -Force

    Get-ChildItem -Path $publishDir -File -Filter "*.pdb" -ErrorAction SilentlyContinue |
        Remove-Item -Force -ErrorAction SilentlyContinue
    Get-ChildItem -Path $publishDir -File -Filter "*.dbg" -ErrorAction SilentlyContinue |
        Remove-Item -Force -ErrorAction SilentlyContinue

    Write-Host "    output: $publishDir"
}

foreach ($demoName in $demoNames) {
    Publish-Demo $demoName
}

Write-Host "==> Done"
Write-Host "Published demos under $distDir"
