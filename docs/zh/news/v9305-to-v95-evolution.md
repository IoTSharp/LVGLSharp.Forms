---
title: 自 v9.3.0.5 至当前 9.5 线：项目演进纪要
description: 述自 2026 年 3 月 17 日之 v9.3.0.5 至当前 9.5 文档基线 9.5.0.5，LVGLSharp 在运行时、示例、验证、文档与发布体系上的主要演进。
lang: zh-CN
---

# 自 v9.3.0.5 至当前 9.5 线：项目演进纪要

> 兹记项目自 2026 年 3 月 17 日之 `v9.3.0.5`，迄当前 `9.5` 文档基线 `9.5.0.5` 之主要演进。此一阶段，所变者不独在版本号，更在工程边界、运行时矩阵、验证路径与对外文档之体例，皆由雏形渐次整饬，遂成今日之规模。

## 先言其势

若以仓库差异观之，自 `v9.3.0.5` 至今，项目已有如下变化：

- 变更文件约 `298` 项。
- 新增代码与文档约 `27,011` 行。
- 调整或移除内容约 `6,080` 行。

此非一二示例之修补，而是自“单体兼容层”迈向“多运行时、多示例、多文档入口”的一次系统推进。

## v9.3.0.5 时之基线

`v9.3.0.5` 所代表者，乃项目早期较完整之第一次文档化发布。当时项目已有如下基础：

- `LVGLSharp.Forms`、`LVGLSharp.Core`、`LVGLSharp.Interop`、`LVGLSharp.Native`、`LVGLSharp.Runtime.Windows`、`LVGLSharp.Runtime.Linux` 等核心包分层。
- `Application.Run(Form)`、常用控件、NativeAOT 与基础事件桥接已经可对外说明。
- 示例以 `WinFormsDemo`、`PictureBoxDemo`、`SerialPort` 为主。
- 文档仍以 `README` 为主要入口，站点化结构尚未成形。

是以 `9.3` 一线之价值，在于立其本；而 `9.5` 一线之价值，在于扩其体、明其界、充其实。

## 自 9.3 至 9.5，其要有五

## 一曰：LVGL 互操作层与运行时抽象更趋完整

`9.5` 一线首先完成者，是底座之抬升：

- 项目正式转入 `LVGL 9.5` 基线。
- 自动生成的 P/Invoke 绑定进一步扩展，相关枚举、结构与绘制类型大幅更新。
- `IWindow` 等较早抽象逐步收敛为更清晰的 `IView` / `ViewLifetimeBase` 体系。
- 运行时注册入口继续标准化，`ApplicationConfiguration.Initialize()` 成为更明确的统一入口。

此一步之意义，在于使上层 `Forms` API 与下层平台宿主之间的关系愈加清晰，为后续扩展宿主矩阵奠定基础。

## 二曰：运行时矩阵由“可跑”进于“分层”

与 `v9.3.0.5` 相比，当前 `9.5` 线最显著之变化，在于运行时已不复止于单一 Windows / Linux 路径，而是开始形成分层矩阵：

- Windows 路径继续整理，命名与运行时边界更趋统一。
- Linux 路径由单一宿主入口，扩展为 `WSLg`、`X11`、`Wayland`、`SDL`、`FrameBuffer` 等多宿主并存。
- `DrmView` 已入仓，`DRM/KMS` 路线开始具备明确入口。
- `Offscreen` 自 Linux 宿主分支中独立出来，形成 `LVGLSharp.Runtime.Headless`。
- `LVGLSharp.Runtime.MacOs` 与 `LVGLSharp.Runtime.Remote` 已具骨架，为 `macOS`、`VNC`、`RDP` 等后续路线预留独立边界。

换言之，项目已由“跨平台可运行”进一步走向“跨平台运行时可组织、可区分、可扩展”。

## 三曰：示例与验证入口显著增多

`v9.3.0.5` 时，示例仍以基础验证为主；至 `9.5` 一线，示例已成为推动能力边界的重要载体：

- 新增 `MusicDemo` 与 `SmartWatchDemo`，使复杂界面与控件协作路径得到更真实之检验。
- 新增 `OffscreenDemo`，使无头渲染、PNG 导出与快照验证具备独立入口。
- 新增 `MacOsAotDemo`，作为 `macOS` 路线之首个 AOT 验证入口。
- 新增 `WinFormsVncDemo`、`WinFormsRdpDemo` 与 `RdpDemo`，使 Remote 路线不止停于抽象。
- 新增 `tests/LVGLSharp.Headless.Tests`，将快照回归、自定义背景色、远程帧源适配等纳入自动化验证。

至此，项目之验证方式已不再单赖人工演示，而是开始同时具备示例验证、快照验证与发布产物验证三层路径。

## 四曰：文档由 README 进于站点

此阶段另一大变化，在于文档体系之建立。

自 `9.3` 线之 README 中心结构，至当前 `9.5` 线，项目已形成完整文档站点：

- 首页、导航页、博客、新闻页、NuGet 安装页、截图页皆已成形。
- 中英文内容已分层整理，并具语言切换入口。
- `ROADMAP.md`、`CHANGELOG.md` 与首页叙述开始统一口径。
- 本地 Pages 预览与 GitHub Pages 发布流程亦已接入。

是故今日之文档，已不惟“附于仓库”，而是成为项目对外表达、对内梳理与阶段归档之一部分。

## 五曰：发布与打包体系渐成章法

`9.5` 一线中，CI/CD 与打包流程亦有显著推进：

- Native 资产已按多 RID 形式组织至 `runtimes/{rid}/native`。
- 工作流拆分为 `prepare`、`build-native`、`pack-nuget`、`build-demos`、`publish-release` 等阶段。
- 版本标准化、打包产物聚合与 NuGet 发布入口皆较 `9.3` 时更为清楚。
- 本地开发、源码仓库构建与 NuGet 包依赖之间的关系，也通过 `LVGLSharpNativePackageVersion` 等属性进一步收拢。

此举之益，在于项目已不只是“能编译”，而是开始具备较稳定之发布秩序。

## 由此观之，9.5 一线之实义

若概而言之，自 `v9.3.0.5` 至当前 `9.5` 线，项目完成了三件尤为关键之事：

1. 自单体能力验证，进于多运行时分层。
2. 自基础示例演示，进于示例、测试与截图并行验证。
3. 自 README 说明，进于站点、路线图、变更日志与发布流程并举。

故 `9.3` 可视为“立本之阶段”，而 `9.5` 则更近于“成势之阶段”。

## 后续所向

当前 `9.5` 线虽已较 `9.3` 时大为拓展，然尚有数事仍在续进：

- `DRM/KMS` 尚待原生后端补齐。
- `Headless` 路线尚宜继续扩大快照回归覆盖。
- `macOS` 与 Remote 仍多属骨架与首轮验证形态，尚待进一步实装。
- 已补入打包流程的 `LVGLSharp.Runtime.Headless`、`LVGLSharp.Runtime.MacOs` 与 `LVGLSharp.Runtime.Remote`，后续亦将进入正式 NuGet 发布节奏。

项目后续仍将沿此方向推进：既不轻弃 `Forms` 上层开发体验，亦不缓于运行时边界、自动化验证与发布秩序之建设。

## 参阅

- [项目路线图](https://github.com/IoTSharp/LVGLSharp/blob/main/ROADMAP.md)
- [项目变更日志](https://github.com/IoTSharp/LVGLSharp/blob/main/CHANGELOG.md)
