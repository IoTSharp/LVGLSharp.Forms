---
title: 关于
description: 介绍 LVGLSharp 的简介、初衷、核心价值观，以及它背后的 IoTSharp 团队背景。
lang: zh-CN
template: structured
hero:
  eyebrow: "LVGLSharp"
  title: "关于"
  lead: "`LVGLSharp` 是一个以 `LVGL` 为渲染底座、面向 .NET 开发者的跨平台 GUI 运行时项目，目标是在保留 WinForms 风格开发体验的同时，把界面能力带到更轻量、更可裁剪、也更适合设备端与多宿主环境的运行时中。"
  actions:
    - label: "项目仓库"
      url: "https://github.com/IoTSharp/LVGLSharp"
      style: primary
    - label: "GitHub 组织"
      url: "https://github.com/IoTSharp"
      style: secondary
  tags:
    - "LVGLSharp"
    - "LVGL"
    - "WinForms-style"
    - "Cross-platform Runtime"
  note_title: "项目说明"
  note_text: >-
    `LVGLSharp` 关注的不是把传统桌面控件逐个搬运到另一套技术栈上，而是建立一条从熟悉的 .NET 桌面开发方式走向跨平台 GUI 运行时的自然路径。
    当前阶段，项目重点放在 WinForms 风格开发体验、多宿主运行时能力、NativeAOT 与轻量部署支持，以及设备端、边缘侧和远程交互场景中的工程化落地。
    它既服务于希望降低迁移成本的桌面开发者，也服务于需要在真实设备、现场系统和跨平台宿主中构建界面的工程团队。
stats:
  - label: "当前贡献者"
    value: "5"
  - label: "组织主线"
    value: "IoT + Runtime"
  - label: "镜像入口"
    value: "GitHub + Gitee"
  - label: "当前项目"
    value: "LVGLSharp"
sections:
  - title: "LVGLSharp 核心价值观"
    description: "`LVGLSharp` 想解决的问题，不只是“能不能画界面”，而是“能不能以更长期、可迁移、可工程化的方式做界面”。"
    variant: cards
    columns: 3
    items:
      - title: "低迁移成本"
        description: "尽量保留 WinForms 风格的开发心智，让熟悉桌面开发的 .NET 开发者可以更自然地进入跨平台 GUI 运行时。"
      - title: "面向长期演进"
        description: "不把项目做成一次性的 API 包装，而是持续围绕分层运行时、宿主扩展、AOT 和设备端场景打基础。"
      - title: "工程可落地"
        description: "强调真实运行时、真实宿主、真实构建发布链路，而不是停留在概念 Demo 或单平台演示上。"
  - title: "如果你准备这样使用它"
    description: "`LVGLSharp` 不是所有 GUI 项目的通用答案，但如果你的场景接近下面这些方向，它会更值得你继续往下看。"
    variant: cards
    columns: 4
    items:
      - title: ".NET 桌面开发者"
        description: "如果你熟悉 WinForms，又想把现有开发经验延伸到跨平台和设备侧运行环境，这一类场景会很契合。"
      - title: "设备与边缘端项目"
        description: "如果你需要本地交互界面、状态显示面板、控制台界面或轻量部署 UI，这一类设备端场景会很合适。"
      - title: "跨平台运行时探索"
        description: "如果你正在关注 Windows、Linux、Headless、Remote 等多宿主运行时路线，这个方向会更值得你投入。"
      - title: "AOT 与轻量部署"
        description: "如果你在意 NativeAOT、自包含发布、裁剪、启动速度和运行时依赖控制，这里会更贴近你的工程需求。"
  - title: "为什么 IoTSharp 会做 GUI / LVGLSharp"
    description: "从团队已有项目结构看，`LVGLSharp` 并不是孤立出现的，它更像是 IoTSharp 在设备端工程能力上的自然延伸。"
    variant: cards
    columns: 3
    items:
      - title: "设备需要界面"
        description: "当团队长期面向设备管理、边缘接入和现场系统时，最终就会遇到本地交互界面、状态显示与操作面板的问题。"
      - title: "桌面经验需要迁移"
        description: "很多 .NET 开发者熟悉 WinForms，但设备端和跨平台环境并不天然适合传统桌面技术，于是需要一个新的运行时承载层。"
      - title: "IoT 与 GUI 会汇合"
        description: "一旦团队同时掌握设备连接、跨平台运行、打包发布和工程化能力，继续往 GUI 运行时推进，其实是顺势而为。"
  - title: "LVGLSharp 在团队中的位置"
    description: "`LVGLSharp` 更偏向 IoTSharp 技术版图里的界面运行时探索。它把团队在设备侧、跨平台和工程化方面的积累，延伸到了 GUI 与运行时宿主层。"
    variant: cards
    columns: 3
    items:
      - title: "WinForms over LVGL"
        description: "保留熟悉的 WinForms 开发心智，同时把渲染能力迁移到更轻量、可跨平台的 LVGL 栈上。"
      - title: "多宿主运行时"
        description: "继续推进 Windows、Linux、Headless、Remote 和后续设备侧宿主能力，而不把项目局限在单一桌面场景。"
      - title: "设备端工程方向"
        description: "结合组织原有的 IoT 背景，LVGLSharp 更适合被理解成面向设备与边缘端的一条 GUI 运行时路线，而不是单纯的桌面控件封装。"
  - title: "IoTSharp 团队画像"
    description: "从组织主页可以看到，IoTSharp 并不只做单一项目，而是在一个相对完整的技术带宽上持续积累开源资产。"
    variant: cards
    columns: 3
    items:
      - title: "物联网平台能力"
        description: "组织核心项目 `IoTSharp` 聚焦设备管理、数据采集、处理、状态检测和远程控制，说明团队长期关注真实设备连接与平台化能力。"
      - title: "基础组件沉淀"
        description: "像 `mqttclient`、`TaosConnector`、`AspNetCore.HealthChecks` 这类项目，反映出团队不仅做业务平台，也会持续沉淀通信、数据接入和服务治理层组件。"
      - title: "工程与工具链扩展"
        description: "像 `SilkierQuartz`、`iotsharp.github.io`、以及现在的 `LVGLSharp`，说明团队也在不断补齐调度、站点、开发体验与跨平台界面相关能力。"
  - title: "团队能力地图"
    description: "如果把 IoTSharp 组织里的项目放在一起看，大致可以归纳出下面这几类能力。"
    variant: cards
    columns: 4
    items:
      - title: "设备与协议"
        description: "设备管理、远程控制、MQTT 接入、嵌入式与跨平台连接能力。"
      - title: "数据与存储"
        description: "数据采集、处理、可视化、数据库接入与工业数据链路。"
      - title: "服务与调度"
        description: "健康检查、任务调度、后台运行、服务治理与运维支撑。"
      - title: "界面与运行时"
        description: "站点、开发体验、跨平台 GUI 运行时，以及 `LVGLSharp` 这一条界面工程路线。"
  - title: "IoTSharp 代表项目列表"
    description: "以下项目来自当前 `IoTSharp` GitHub 组织页公开展示的代表仓库，可以帮助理解团队的长期技术重心。"
    variant: quick-links
    columns: 3
    items:
      - title: "IoTSharp"
        description: "面向设备管理、数据采集、处理、可视化和远程控制的开源 IoT 平台。"
        url: "https://github.com/IoTSharp/IoTSharp"
      - title: "mqttclient"
        description: "高性能、跨平台 MQTT 客户端，覆盖嵌入式、Linux、Windows、Mac 等环境。"
        url: "https://github.com/IoTSharp/mqttclient"
      - title: "SilkierQuartz"
        description: "围绕 Quartz 的任务托管与 Web 管理工具，体现团队在调度与运维侧的积累。"
        url: "https://github.com/IoTSharp/SilkierQuartz"
      - title: "TaosConnector"
        description: "面向 TDengine 的 ADO.NET / ORM / Stmt 访问组件，反映出数据接入方向。"
        url: "https://github.com/IoTSharp/TaosConnector"
      - title: "AspNetCore.HealthChecks"
        description: "围绕 .NET 健康检查扩展的组件集合，体现服务治理与基础设施方向。"
        url: "https://github.com/IoTSharp/AspNetCore.HealthChecks"
      - title: "LVGLSharp"
        description: "当前面向跨平台 GUI 运行时、WinForms 风格体验与设备侧界面的延伸项目。"
        url: "https://github.com/IoTSharp/LVGLSharp"
  - title: "当前贡献者"
    description: "以下名字来自当前仓库 `git shortlog -sn HEAD`，已排除机器人账号。"
    variant: link-lists
    columns: 2
    items:
      - title: "主要提交者"
        description: "当前历史里提交记录较多、对整体演进影响更明显的成员。"
        links:
          - label: "maikebing"
            url: "https://github.com/IoTSharp/LVGLSharp"
          - label: "OpenClaw"
            url: "https://github.com/IoTSharp/LVGLSharp"
          - label: "xcssa"
            url: "https://github.com/IoTSharp/LVGLSharp"
      - title: "共同贡献者"
        description: "包括早期基础和后续补充提交的真实贡献者。"
        links:
          - label: "imxcstar"
            url: "https://github.com/imxcstar/LVGLSharp"
          - label: "麦壳饼"
            url: "https://github.com/IoTSharp/LVGLSharp"
  - title: "组织与源码入口"
    description: "如果你想了解 IoTSharp 的项目版图或继续跟踪 `LVGLSharp` 源码，可以从这里进入。"
    variant: quick-links
    columns: 3
    items:
      - title: "IoTSharp 组织"
        description: "查看组织主页、项目列表和整体技术方向。"
        url: "https://github.com/IoTSharp"
      - title: "LVGLSharp 仓库"
        description: "查看当前项目源码、Issue、PR 与发布记录。"
        url: "https://github.com/IoTSharp/LVGLSharp"
      - title: "Gitee 镜像"
        description: "国内访问更方便的镜像与协作入口。"
        url: "https://gitee.com/IoTSharp/LVGLSharp"
---
