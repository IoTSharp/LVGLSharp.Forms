---
title: LVGLSharp 首页
description: 用 WinForms 的开发体验，连接 LVGL 的跨平台渲染能力。
lang: zh-CN
template: structured
hero:
  eyebrow: "跨平台 GUI · WinForms over LVGL · NativeAOT Ready"
  title: "让熟悉的 WinForms 思维，跑在更广阔的运行时上。"
  lead: "LVGLSharp 以 **LVGL** 为渲染内核，提供面向 .NET 的 WinForms 风格开发体验，目标覆盖 Windows、Linux 与设备侧场景，同时持续强化 **NativeAOT**、轻量部署与跨平台宿主能力。"
  actions:
    - label: "开始阅读文档"
      url: "/zh/navigation.html"
      style: primary
    - label: "查看设计博客"
      url: "/zh/blog/"
      style: secondary
    - label: "GitHub 仓库"
      url: "https://github.com/IoTSharp/LVGLSharp"
      style: secondary
  tags:
    - "Windows / Linux"
    - "LVGL Rendering"
    - "WinForms-style API"
    - "NativeAOT Friendly"
  code_title: "Project Snapshot"
  code: |
    dotnet add package LVGLSharp.Forms

    Application.Run(new MainForm());

    // Familiar form model
    // LVGL-backed rendering
    // Cross-platform runtime layering
    // Device-oriented deployment path
  note_title: "一句话理解"
  note_text: "不是把 LVGL 简单包一层 .NET API，而是尽量保留 WinForms 的开发心智，同时把渲染与宿主能力迁移到跨平台、轻量、可裁剪的运行时体系上。"
stats:
  - label: "核心定位"
    value: "WinForms over LVGL"
  - label: "主要平台"
    value: "Windows + Linux"
  - label: "发布方向"
    value: "NativeAOT"
  - label: "工程目标"
    value: "跨平台 UI Runtime"
sections:
  - title: "为什么值得关注"
    description: "它连接了传统桌面开发效率与设备侧图形运行时能力，让熟悉的控件、事件、窗体模型延伸到更轻量的宿主与部署场景。"
    variant: cards
    columns: 3
    items:
      - title: "熟悉的开发体验"
        description: "延续窗体、控件、事件和布局思维，降低从 WinForms 迁移到跨平台图形栈的认知门槛。"
      - title: "LVGL 渲染底座"
        description: "利用 LVGL 的轻量、高性能与设备适配能力，支撑更广泛的图形宿主与显示环境。"
      - title: "AOT 与轻量部署"
        description: "围绕 NativeAOT、自包含发布和设备端部署持续优化，减少运行时依赖和部署复杂度。"
  - title: "快速入口"
    description: "如果你想快速判断这个项目是否适合自己，可以先从下面几个入口开始。"
    variant: quick-links
    columns: 3
    items:
      - title: "文档导航"
        description: "按主题、角色和语言快速找到合适的阅读顺序。"
        url: "/zh/navigation.html"
      - title: "工程与 CI"
        description: "了解当前仓库的构建、打包、发布与自动化流程。"
        url: "/zh/ci-workflows.html"
      - title: "WSL / Linux 开发"
        description: "如果你关注 Linux 图形宿主和开发体验，从这里进入最合适。"
        url: "/zh/WSL-Developer-Guide.html"
  - title: "推荐阅读路径"
    description: "按你的关注点进入，而不是从所有文档里盲目翻找。"
    variant: link-lists
    columns: 4
    items:
      - title: "第一次了解项目"
        description: "适合先理解“为什么存在”和“整体目标是什么”。"
        links:
          - label: "为什么要做 WinForms over LVGL"
            url: "/zh/blog/why-winforms-over-lvgl.html"
          - label: "English Home"
            url: "/en/"
      - title: "关注架构与工程化"
        description: "适合想快速理解模块边界、运行时分层与工程结构的读者。"
        links:
          - label: "项目架构拆解"
            url: "/zh/blog/architecture.html"
          - label: "CI 工作流说明"
            url: "/zh/ci-workflows.html"
      - title: "关注 Linux / 宿主路线"
        description: "适合关心 X11、WSLg、FrameBuffer、Wayland 和后续显示宿主方向的读者。"
        links:
          - label: "Linux 图形宿主路线"
            url: "/zh/blog/linux-hosts.html"
          - label: "WSL 开发指南"
            url: "/zh/WSL-Developer-Guide.html"
      - title: "关注 AOT / 发布"
        description: "适合关注裁剪、自包含发布、运行时体积和部署方式的读者。"
        links:
          - label: "NativeAOT 与 GUI"
            url: "/zh/blog/nativeaot-gui.html"
          - label: "更新记录"
            url: "https://github.com/IoTSharp/LVGLSharp/blob/main/CHANGELOG.md"
  - title: "项目能力地图"
    description: "这个项目的价值，不只在于“能画界面”，更在于一整套可扩展的运行时与工程组织方式。"
    variant: cards
    columns: 3
    items:
      - title: "窗体与控件模型"
        description: "保持传统 WinForms 的编程习惯，包括生命周期、控件树、事件和布局组织方式。"
      - title: "运行时分层"
        description: "通过 Core、Interop、Runtime.Windows、Runtime.Linux 等层次组织平台能力，避免把渲染与宿主耦死在一起。"
      - title: "设备化部署路径"
        description: "为 x64、ARM、ARM64 等平台提供更现实的设备端与边缘端 UI 运行可能性。"
  - title: "Demo 与使用场景"
    description: "这个项目不是抽象概念验证，而是面向真实界面、真实运行时和真实发布链路持续推进。"
    variant: cards
    columns: 3
    items:
      - title: "桌面验证"
        description: "通过 Windows 与 Linux 宿主持续验证 UI 生命周期、控件模型和跨平台运行一致性。"
      - title: "设备方向探索"
        description: "围绕 FrameBuffer、Wayland、SDL、DRM/KMS 等路线逐步扩展适合设备端的运行方式。"
      - title: "示例驱动演进"
        description: "通过 MusicDemo、SmartWatchDemo、PictureBoxDemo、WinFormsDemo 等示例沉淀真实能力。"
  - title: "快速开始"
    description: "如果你想先感受整体开发方式，可以从下面的最小路径开始。"
    variant: list
    surface: true
    items:
      - label: "查看 GitHub 仓库，了解项目结构"
        url: "https://github.com/IoTSharp/LVGLSharp"
      - label: "阅读文档导航，选择你的阅读路线"
        url: "/zh/navigation.html"
      - label: "进入博客索引，理解设计背景与架构取舍"
        url: "/zh/blog/"
      - label: "结合 src/Demos 下的示例项目理解实际使用场景"
  - title: "当前重点阅读"
    description: "如果只看三篇，建议先从下面开始。"
    variant: list
    surface: true
    items:
      - label: "为什么要做 WinForms over LVGL"
        url: "/zh/blog/why-winforms-over-lvgl.html"
      - label: "项目架构拆解"
        url: "/zh/blog/architecture.html"
      - label: "NativeAOT 与 GUI"
        url: "/zh/blog/nativeaot-gui.html"
---

