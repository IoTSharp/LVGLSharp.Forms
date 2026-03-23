---
title: LVGLSharp CI 工作流说明
description: 说明仓库当前 GitHub Actions CI/CD 的拆分方式、依赖关系和发布职责。
lang: zh-CN
---

# LVGLSharp CI 工作流说明

本文档用于说明 `LVGLSharp` 仓库当前 GitHub Actions CI/CD 的拆分方式、依赖关系、触发条件，以及各工作流承担的职责。

## 1. 设计目标

当前 CI 拆分遵循以下目标：

- 将“准备版本”“构建原生库”“构建 Demo”“打包 NuGet”“发布资产”分解为独立阶段
- 让工作流可以被复用，避免单个 YAML 文件过大
- 让普通分支与 PR 只执行验证型任务，不误触发发布
- 让 tag 与手动发布场景可以完整复用同一套构建产物

## 2. 当前工作流一览

### 2.1 `.github/workflows/nuget-publish.yml`

主协调工作流，负责串联整个流程。

职责：

- 接收外部触发
- 调用 `prepare-release.yml`
- 调用 `build-native.yml`
- 调用 `pack-nuget.yml`
- 在 tag 或手动发布时调用 `build-demos.yml`
- 在 tag 或手动发布时调用 `publish-release.yml`

触发条件：

- `push` 到 `main` / `develop` / `master`
- `push` tag `v*`
- `pull_request` 到 `main` / `develop` / `master`
- `workflow_dispatch`

### 2.2 `.github/workflows/prepare-release.yml`

准备发布元数据的可复用工作流。

职责：

- 校验输入版本号
- 将 `v9.5.0.5` 规范化为 `9.5.0.5`
- 生成标准 `release_tag`

输入：

- `package_version`

输出：

- `package_version`
- `release_tag`

### 2.3 `.github/workflows/build-native.yml`

构建多平台原生库资产的可复用工作流。

职责：

- 构建 Windows 原生 DLL
- 构建 Linux 原生 SO
- 校验导出符号
- 生成 `sha256`
- 上传各 RID 的原生制品

产物：

- `native-win-x64`
- `native-win-x86`
- `native-win-arm64`
- `native-linux-x64`
- `native-linux-arm`
- `native-linux-arm64`

### 2.4 `.github/workflows/build-demos.yml`

构建 Demo 资产的可复用工作流。

职责：

- 构建并发布 Linux demo 包
- 构建并发布 Windows demo 包
- 上传 demo 压缩产物

输入：

- `package_version`（可选）

产物：

- `demo-release-linux-x64`
- `demo-release-win-x64`

### 2.5 `.github/workflows/pack-nuget.yml`

打包 NuGet 资产的可复用工作流。

职责：

- 下载原生库产物
- 组装 `src/LVGLSharp.Native/runtimes/{rid}/native`
- 打包所有 NuGet 包
- 上传 `nuget-packages`

输入：

- `package_version`

产物：

- `nuget-packages`

### 2.6 `.github/workflows/publish-release.yml`

发布 Release 资产的可复用工作流。

职责：

- 下载 `nuget-packages`
- 下载 `demo-release-*`
- 汇总 `release_assets`
- 发布到 GitHub Release
- 发布到 NuGet.org
- 发布到 GitHub Packages

输入：

- `release_tag`
- `publish_nuget`
- `publish_github`

## 3. 依赖关系

完整依赖关系如下：

```text
nuget-publish.yml
├─ prepare-release.yml
├─ build-native.yml
├─ pack-nuget.yml
├─ build-demos.yml   (仅 tag / 手动发布时)
└─ publish-release.yml (仅 tag / 手动发布时)
```

更精确的执行顺序：

```text
prepare
  └─ build-native
       └─ pack

prepare
  └─ build-demos        (仅 tag / workflow_dispatch)

prepare + pack + build-demos
  └─ publish            (仅 tag / 手动且要求发布)
```

## 4. 各触发场景的行为

### 4.1 普通分支 push

执行：

- `prepare`
- `build-native`
- `pack`

不执行：

- `build-demos`
- `publish`

用途：

- 验证版本规范化、原生库构建、NuGet 打包流程是否健康

### 4.2 Pull Request

执行：

- `prepare`
- `build-native`
- `pack`

不执行：

- `build-demos`
- `publish`

用途：

- 在合并前验证核心构建与打包链路

### 4.3 Tag 发布

执行：

- `prepare`
- `build-native`
- `pack`
- `build-demos`
- `publish`

用途：

- 生成并发布完整 Release 资产

### 4.4 手动触发 `workflow_dispatch`

默认可以：

- 自定义版本号
- 选择是否发布到 NuGet.org
- 选择是否发布到 GitHub Packages

如果不勾选发布：

- 只执行到构建阶段

如果勾选发布：

- 执行完整发布链路

## 5. 版本号规则

当前版本号规则：

- 只接受四段数字版本号
- 允许输入：
  - `9.5.0.5`
  - `v9.5.0.5`

规范化后：

- `package_version = 9.5.0.5`
- `release_tag = v9.5.0.5`

## 6. 产物说明

### 原生库产物

由 `build-native.yml` 输出：

- 多平台 `.dll` / `.so`
- 对应的 `.sha256`

### Demo 产物

由 `build-demos.yml` 输出：

- Windows zip 包
- Linux tar.gz 包

### NuGet 产物

由 `pack-nuget.yml` 输出：

- `LVGLSharp.Native`
- `LVGLSharp.Interop`
- `LVGLSharp.Core`
- `LVGLSharp.Runtime.Windows`
- `LVGLSharp.Runtime.Linux`
- `LVGLSharp.Forms`

## 7. 为什么这样拆分

拆分后的好处包括：

- 更容易定位失败阶段
- 更容易按阶段复用
- 更适合后续扩展 macOS、Wayland、更多运行时
- 更便于维护 GitHub Pages、Release、NuGet 等不同发布目标
- 主协调文件更薄，阅读成本更低

## 8. 后续可扩展方向

未来可以继续扩展：

- 增加 `docs` / `GitHub Pages` 专用发布工作流
- 增加 macOS 或其他宿主运行时构建工作流
- 增加 smoke test / demo run 验证工作流
- 增加工作流缓存与并行优化

## 9. 建议维护约定

建议后续持续遵守以下约定：

- 协调型工作流只负责编排，不写大段构建脚本
- 可复用工作流尽量声明清楚 `inputs` / `outputs`
- artifact 名称保持稳定，避免下游下载步骤频繁修改
- 手动触发输入统一采用“字段名：用途说明”的描述格式
- 分支 / PR 只执行验证型任务，发布动作只在 tag 或显式手动发布时执行

---

如果后续还要补 GitHub Pages 文档站点工作流，可以在这套拆分基础上继续增加 `docs-pages.yml`，直接复用当前构建与发布体系。

