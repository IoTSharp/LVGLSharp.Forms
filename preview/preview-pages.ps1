param(
    [int]$Port = 4000,
    [switch]$NoServe
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$pagesSrc = Join-Path $repoRoot 'pages-src'
$siteDir = Join-Path $repoRoot '_site'

Write-Host 'Preparing local Pages preview source...' -ForegroundColor Cyan

if (Test-Path $pagesSrc) {
    Remove-Item $pagesSrc -Recurse -Force
}

if (Test-Path $siteDir) {
    Remove-Item $siteDir -Recurse -Force
}

New-Item -ItemType Directory -Path $pagesSrc -Force | Out-Null

Get-ChildItem (Join-Path $repoRoot 'docs') -Force | ForEach-Object {
    Copy-Item $_.FullName $pagesSrc -Recurse -Force
}

Copy-Item (Join-Path $repoRoot 'ROADMAP.md') (Join-Path $pagesSrc 'ROADMAP.md')
Copy-Item (Join-Path $repoRoot 'CHANGELOG.md') (Join-Path $pagesSrc 'CHANGELOG.md')

Push-Location $repoRoot
try {
    $bundle = Get-Command bundle -ErrorAction SilentlyContinue
    if (-not $bundle) {
        throw "未找到 Bundler。请先安装 Ruby 和 Bundler，然后在仓库目录执行：bundle install"
    }

    Write-Host 'Checking Jekyll/GitHub Pages dependencies...' -ForegroundColor Cyan
    & bundle exec jekyll --version | Out-Host

    if ($LASTEXITCODE -ne 0) {
        throw "Jekyll 不可用。请先在仓库目录执行：bundle install"
    }

    Write-Host 'Building site with Jekyll (bundle exec jekyll build)...' -ForegroundColor Cyan
    & bundle exec jekyll build --source $pagesSrc --destination $siteDir

    if ($LASTEXITCODE -ne 0) {
        throw "Jekyll 构建失败，请先执行 bundle install 并检查站点配置。"
    }

    if ($NoServe) {
        Write-Host "Site prepared at: $siteDir" -ForegroundColor Green
        exit 0
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
        Write-Warning '未找到 python，无法自动启动本地 HTTP 预览。HTML 已生成到 _site，可以用其他静态服务器打开。'
    }
}
finally {
    Pop-Location
}

