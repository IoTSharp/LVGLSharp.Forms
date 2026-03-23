---
title: LVGLSharp CI Workflow Guide
description: Explains how the current GitHub Actions CI/CD pipeline is structured and how the workflows relate to each other.
lang: en
---

# LVGLSharp CI Workflow Guide

This document explains how the current GitHub Actions CI/CD pipeline in the `LVGLSharp` repository is split, how the workflows depend on each other, what triggers them, and what each workflow is responsible for.

## 1. Design Goals

The current CI structure follows these goals:

- split “prepare version”, “build native libraries”, “build demos”, “pack NuGet”, and “publish assets” into independent stages
- make workflows reusable and easier to maintain
- ensure branch and PR runs validate the pipeline without accidentally publishing
- allow tag and manual release scenarios to reuse the same build outputs

## 2. Workflow Overview

### 2.1 `.github/workflows/nuget-publish.yml`

The main orchestration workflow.

Responsibilities:

- receive external triggers
- call `prepare-release.yml`
- call `build-native.yml`
- call `pack-nuget.yml`
- call `build-demos.yml` on tag or manual release scenarios
- call `publish-release.yml` on tag or manual release scenarios

Triggers:

- `push` to `main` / `develop` / `master`
- `push` tag `v*`
- `pull_request` to `main` / `develop` / `master`
- `workflow_dispatch`

### 2.2 `.github/workflows/prepare-release.yml`

Reusable workflow for release metadata preparation.

Responsibilities:

- validate the incoming version
- normalize `v9.5.0.5` into `9.5.0.5`
- produce a standard `release_tag`

Inputs:

- `package_version`

Outputs:

- `package_version`
- `release_tag`

### 2.3 `.github/workflows/build-native.yml`

Reusable workflow for multi-platform native library assets.

Responsibilities:

- build Windows native DLLs
- build Linux native shared libraries
- validate exported symbols
- generate `sha256`
- upload per-RID native artifacts

Artifacts:

- `native-win-x64`
- `native-win-x86`
- `native-win-arm64`
- `native-linux-x64`
- `native-linux-arm`
- `native-linux-arm64`

### 2.4 `.github/workflows/build-demos.yml`

Reusable workflow for demo assets.

Responsibilities:

- build and publish Linux demo packages
- build and publish Windows demo packages
- upload demo archives

Inputs:

- `package_version` (optional)

Artifacts:

- `demo-release-linux-x64`
- `demo-release-win-x64`

### 2.5 `.github/workflows/pack-nuget.yml`

Reusable workflow for NuGet package assets.

Responsibilities:

- download native build artifacts
- assemble `src/LVGLSharp.Native/runtimes/{rid}/native`
- pack all NuGet packages
- upload `nuget-packages`

Inputs:

- `package_version`

Artifacts:

- `nuget-packages`

### 2.6 `.github/workflows/publish-release.yml`

Reusable workflow for release publishing.

Responsibilities:

- download `nuget-packages`
- download `demo-release-*`
- collect `release_assets`
- publish to GitHub Release
- publish to NuGet.org
- publish to GitHub Packages

Inputs:

- `release_tag`
- `publish_nuget`
- `publish_github`

## 3. Dependency Graph

The overall dependency structure is:

```text
nuget-publish.yml
├─ prepare-release.yml
├─ build-native.yml
├─ pack-nuget.yml
├─ build-demos.yml         (tag / manual release only)
└─ publish-release.yml     (tag / manual release only)
```

Execution order:

```text
prepare
  └─ build-native
       └─ pack

prepare
  └─ build-demos          (tag / workflow_dispatch only)

prepare + pack + build-demos
  └─ publish              (tag / manual release only)
```

## 4. Behavior by Trigger Type

### 4.1 Branch Push

Runs:

- `prepare`
- `build-native`
- `pack`

Does not run:

- `build-demos`
- `publish`

Purpose:

- validate version normalization, native library build, and NuGet packaging pipeline

### 4.2 Pull Request

Runs:

- `prepare`
- `build-native`
- `pack`

Does not run:

- `build-demos`
- `publish`

Purpose:

- validate core build and packaging steps before merge

### 4.3 Tag Release

Runs:

- `prepare`
- `build-native`
- `pack`
- `build-demos`
- `publish`

Purpose:

- generate and publish the complete release asset set

### 4.4 Manual `workflow_dispatch`

By default, manual runs allow you to:

- choose a version number
- choose whether to publish to NuGet.org
- choose whether to publish to GitHub Packages

If publishing is not enabled:

- the pipeline stops at the build stage

If publishing is enabled:

- the full release flow is executed

## 5. Version Rules

Current version rules:

- only four-part numeric versions are accepted
- valid examples:
  - `9.5.0.5`
  - `v9.5.0.5`

After normalization:

- `package_version = 9.5.0.5`
- `release_tag = v9.5.0.5`

## 6. Artifact Types

### Native Library Artifacts

Produced by `build-native.yml`:

- multi-platform `.dll` / `.so`
- corresponding `.sha256` files

### Demo Artifacts

Produced by `build-demos.yml`:

- Windows zip package
- Linux tar.gz package

### NuGet Artifacts

Produced by `pack-nuget.yml`:

- `LVGLSharp.Native`
- `LVGLSharp.Interop`
- `LVGLSharp.Core`
- `LVGLSharp.Runtime.Windows`
- `LVGLSharp.Runtime.Linux`
- `LVGLSharp.Forms`

## 7. Why the Pipeline Is Split This Way

This decomposition brings several benefits:

- failures are easier to localize
- stages are reusable
- future expansion toward macOS, Wayland, and more runtimes becomes easier
- the main orchestration file stays smaller and clearer
- release engineering and documentation workflows can evolve independently

## 8. Recommended Maintenance Rules

Recommended conventions going forward:

- orchestration workflows should only coordinate, not embed large build logic blocks
- reusable workflows should document inputs and outputs clearly
- artifact names should stay stable to avoid downstream churn
- manual input descriptions should follow a consistent format
- branch and PR runs should stay validation-focused, while publishing remains limited to tag or explicit manual release scenarios

---

If the repository later adds a GitHub Pages documentation publishing workflow, it can continue to build on this same decomposed CI structure.

