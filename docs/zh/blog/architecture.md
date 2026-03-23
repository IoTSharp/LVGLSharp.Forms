---
title: 架构拆解：从 Forms 到 Runtime，再到底层 LVGL
description: 理解 LVGLSharp 如何通过 Forms、Core、Interop、Native 与 Runtime 分层组织跨平台 GUI 能力。
lang: zh-CN
---

# LVGLSharp.Forms 架构拆解：从 Forms 到 Runtime，再到底层 LVGL

> 这篇文章适合想快速建立整体工程认知的读者，重点关注分层边界、职责拆分与长期可维护性。

## 为什么这个项目需要分层

如果要同时满足：

- WinForms 风格 API
- 跨平台运行
- 多宿主图形环境
- NativeAOT 友好
- 可打包、可发布、可维护

那么项目不可能只靠一个大程序集解决问题。

`LVGLSharp.Forms` 当前最有价值的地方之一，就是它已经逐步形成了比较清楚的分层结构。

## 第一层：Forms 兼容层

`LVGLSharp.WinForms` 面向应用开发者，负责暴露：

- `Form`
- `Control`
- 各类控件
- 事件与生命周期语义

这是最接近 WinForms 使用体验的一层。

## 第二层：Core 共享抽象

`LVGLSharp.Core` 负责：

- 公共抽象
- 视图生命周期基础设施
- 字体与图片辅助能力
- 平台无关的共享逻辑

这层存在的意义，是让上层 Forms 和下层 Runtime 不直接耦死。

## 第三层：Interop 绑定层

`LVGLSharp.Interop` 让 .NET 可以完整访问 LVGL C API。

这一层的存在说明项目并不想把自己锁死在“高级封装”里，而是保留向下直达 LVGL 的能力。

## 第四层：Native 分发层

`LVGLSharp.Native` 负责原生库分发。

这是一个很关键但常被忽视的工程层：

- 没有它，多平台原生库分发会很混乱
- 有了它，NuGet 包结构和 CI 打包阶段都能更清晰

## 第五层：平台运行时层

平台运行时目前包括：

- `LVGLSharp.Runtime.Windows`
- `LVGLSharp.Runtime.Linux`

它们负责把平台宿主差异隔离出去，让应用代码不用自己处理底层平台逻辑。

## 第六层：CI / Packaging / Docs

一个成熟方向的项目，不只是代码能跑，还要：

- 能打包
- 能发布
- 能解释自己

当前仓库里的 CI 拆分和 docs 体系，已经开始承担这一层职责。

## 结语

`LVGLSharp.Forms` 目前最值得肯定的一点，不只是它连接了 WinForms 和 LVGL，而是它正在形成一套完整的工程结构：

- API 层
- 抽象层
- 绑定层
- 原生分发层

- 平台运行时层
- 文档与发布工程层
- 平台运行时层
- 发布与文档层

这才是一个项目能长期演进的基础。

