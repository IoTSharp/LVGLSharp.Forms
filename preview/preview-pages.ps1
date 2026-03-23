param(
    [int]$Port = 4000,
    [switch]$NoServe
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$docsDir = Join-Path $repoRoot 'docs'
$siteDir = Join-Path $repoRoot '_site'

$machinePath = [Environment]::GetEnvironmentVariable('Path', 'Machine')
$userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
if ($machinePath -or $userPath) {
    $env:Path = @($machinePath, $userPath) -join ';'
}

Write-Host 'Preparing local Pages preview...' -ForegroundColor Cyan

if (Test-Path $siteDir) {
    Remove-Item $siteDir -Recurse -Force
}

Push-Location $repoRoot
try {
    $bundle = Get-Command bundle -ErrorAction SilentlyContinue
    if (-not $bundle) {
        throw "Bundler was not found. Install Ruby and Bundler, then run: bundle install"
    }

    Write-Host 'Checking Jekyll/GitHub Pages dependencies...' -ForegroundColor Cyan
    & bundle exec jekyll --version | Out-Host

    if ($LASTEXITCODE -ne 0) {
        throw "Jekyll is not available. Run: bundle install"
    }

    Write-Host 'Building site with Jekyll (bundle exec jekyll build)...' -ForegroundColor Cyan
    & bundle exec jekyll build --source $docsDir --destination $siteDir

    if ($LASTEXITCODE -ne 0) {
        throw "Jekyll build failed. Run bundle install and check the site configuration."
    }

    if ($NoServe) {
        Write-Host "Site prepared at: $siteDir" -ForegroundColor Green
        return
    }

    Write-Host "Starting local preview at http://127.0.0.1:$Port/" -ForegroundColor Green
    $python = Get-Command python -ErrorAction SilentlyContinue
    if ($python) {
        Push-Location $siteDir
        try {
            & python -m http.server $Port
        }
        finally {
            Pop-Location
        }
    }
    else {
        Write-Warning "Python was not found. HTML was generated to $siteDir and can be served by any static server."
    }
}
finally {
    Pop-Location
}
