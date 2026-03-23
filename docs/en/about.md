---
title: About
description: Introduces LVGLSharp itself first, then the IoTSharp team context, project landscape, and current human contributors.
lang: en
template: structured
hero:
  eyebrow: "LVGLSharp"
  title: "About"
  lead: "`LVGLSharp` is a cross-platform GUI runtime for .NET developers built on top of `LVGL`, with the goal of preserving a WinForms-style development experience while bringing UI work into lighter, more trimmable, and more host-flexible runtime environments."
  actions:
    - label: "Project repository"
      url: "https://github.com/IoTSharp/LVGLSharp"
      style: primary
    - label: "GitHub organization"
      url: "https://github.com/IoTSharp"
      style: secondary
  tags:
    - "LVGLSharp"
    - "LVGL"
    - "WinForms-style"
    - "Cross-platform Runtime"
  note_title: "Project note"
  note_text: >-
    `LVGLSharp` is not trying to reproduce a traditional desktop control stack one widget at a time, but to create a natural transition path from familiar .NET desktop development into cross-platform GUI runtime work.
    At this stage, the project is focused on a WinForms-style developer experience, multi-host runtime support, NativeAOT and lightweight deployment, and engineering-ready UI paths for device-side, edge, and remote interaction scenarios.
    It is meant both for desktop developers who want lower migration cost and for engineering teams that need UI on real devices, field systems, and cross-platform hosts.
stats:
  - label: "Current contributors"
    value: "5"
  - label: "Core direction"
    value: "IoT + Runtime"
  - label: "Source mirrors"
    value: "GitHub + Gitee"
  - label: "Current project"
    value: "LVGLSharp"
sections:
  - title: "LVGLSharp core values"
    description: "`LVGLSharp` is not only trying to answer whether a UI can be rendered, but whether it can be built in a way that is sustainable, portable, and engineering-friendly over time."
    variant: cards
    columns: 3
    items:
      - title: "Low migration cost"
        description: "Preserve a WinForms-style mental model so .NET desktop developers can move toward cross-platform GUI runtime work more naturally."
      - title: "Built for long-term evolution"
        description: "Avoid becoming a one-off API wrapper and instead keep investing in runtime layering, host expansion, AOT readiness, and device-side paths."
      - title: "Engineering that can ship"
        description: "Focus on real runtimes, real hosts, and real build-and-release workflows instead of stopping at concept demos or single-platform experiments."
  - title: "Who it is for / target scenarios"
    description: "`LVGLSharp` is not a universal answer for every GUI problem, but it is especially relevant for the following users and environments."
    variant: cards
    columns: 4
    items:
      - title: ".NET desktop developers"
        description: "Useful for developers with WinForms experience who want to carry that knowledge into cross-platform and device-oriented runtime environments."
      - title: "Device and edge-side projects"
        description: "Useful when a project needs local interaction, status dashboards, operator panels, or lightweight deployed UI on the device side."
      - title: "Cross-platform runtime exploration"
        description: "Useful for teams and individuals exploring Windows, Linux, Headless, Remote, and other multi-host runtime paths."
      - title: "AOT and lightweight deployment"
        description: "Useful for engineering scenarios that care about NativeAOT, self-contained publishing, trimming, startup speed, and runtime dependency control."
  - title: "Why IoTSharp would build GUI / LVGLSharp"
    description: "Given the rest of the portfolio, `LVGLSharp` looks like a natural extension rather than a disconnected side project."
    variant: cards
    columns: 3
    items:
      - title: "Devices eventually need UI"
        description: "A team that spends years around device management, edge systems, and field-side tooling will eventually run into local interfaces, status displays, and operator panels."
      - title: "Desktop developer habits still matter"
        description: "Many .NET developers understand WinForms well, but device and cross-platform environments do not map cleanly onto classic desktop stacks, which creates space for a new runtime layer."
      - title: "IoT and GUI naturally converge"
        description: "Once a team already understands connectivity, cross-platform runtime concerns, packaging, and engineering workflows, pushing further into GUI runtime work is a logical next step."
  - title: "Where LVGLSharp fits"
    description: "`LVGLSharp` is best understood as the UI-runtime branch of the broader IoTSharp engineering direction. It carries the team's cross-platform, device-side, and engineering-oriented experience into the GUI layer."
    variant: cards
    columns: 3
    items:
      - title: "WinForms over LVGL"
        description: "Preserve the familiar WinForms mental model while moving rendering onto the lighter and more portable LVGL stack."
      - title: "Multi-host runtime work"
        description: "Keep expanding across Windows, Linux, Headless, Remote, and future device-side host implementations instead of stopping at a single desktop runtime."
      - title: "Device-oriented UI path"
        description: "Given the wider IoTSharp background, LVGLSharp makes more sense as a GUI runtime path for devices and edge environments than as a simple desktop control wrapper."
  - title: "How the IoTSharp team presents itself"
    description: "The organization page suggests that IoTSharp is building a wider open-source portfolio rather than maintaining a single isolated product."
    variant: cards
    columns: 3
    items:
      - title: "IoT platform capabilities"
        description: "The core `IoTSharp` project focuses on device management, data collection, processing, status detection, and remote control, which signals a long-term interest in real device connectivity and platform-level workflows."
      - title: "Infrastructure components"
        description: "Projects such as `mqttclient`, `TaosConnector`, and `AspNetCore.HealthChecks` show that the team also invests in communication, data access, and service-governance building blocks."
      - title: "Engineering and tooling extensions"
        description: "Projects like `SilkierQuartz`, `iotsharp.github.io`, and now `LVGLSharp` suggest that the team keeps extending into scheduling, websites, developer experience, and cross-platform UI/runtime tooling."
  - title: "Team capability map"
    description: "Taken together, the IoTSharp repositories suggest a capability map that spans several adjacent layers."
    variant: cards
    columns: 4
    items:
      - title: "Devices and protocols"
        description: "Device management, remote control, MQTT access, embedded integration, and cross-platform connectivity."
      - title: "Data and storage"
        description: "Data collection, processing, visualization, database access, and industrial data pipelines."
      - title: "Services and scheduling"
        description: "Health checks, job scheduling, background execution, service governance, and operations support."
      - title: "UI and runtime"
        description: "Websites, developer tooling, cross-platform GUI runtime work, and the `LVGLSharp` UI engineering path."
  - title: "Representative IoTSharp projects"
    description: "The repositories below are visible on the current public `IoTSharp` organization page and help illustrate the team's long-running technical priorities."
    variant: quick-links
    columns: 3
    items:
      - title: "IoTSharp"
        description: "An open-source IoT platform focused on device management, data collection, processing, visualization, and remote control."
        url: "https://github.com/IoTSharp/IoTSharp"
      - title: "mqttclient"
        description: "A high-performance cross-platform MQTT client spanning embedded systems, Linux, Windows, and Mac."
        url: "https://github.com/IoTSharp/mqttclient"
      - title: "SilkierQuartz"
        description: "Quartz hosting and web management tooling that reflects the team's scheduling and operations-side experience."
        url: "https://github.com/IoTSharp/SilkierQuartz"
      - title: "TaosConnector"
        description: "An ADO.NET / ORM / statement access component for TDengine, showing the team's data access direction."
        url: "https://github.com/IoTSharp/TaosConnector"
      - title: "AspNetCore.HealthChecks"
        description: "A set of health-check related components that reflects service-governance and infrastructure interests."
        url: "https://github.com/IoTSharp/AspNetCore.HealthChecks"
      - title: "LVGLSharp"
        description: "The team's current extension into cross-platform GUI runtime work, WinForms-style developer experience, and device-side UI."
        url: "https://github.com/IoTSharp/LVGLSharp"
  - title: "Current contributors"
    description: "The names below come from the current repository history via `git shortlog -sn HEAD`, with bot accounts excluded."
    variant: link-lists
    columns: 2
    items:
      - title: "Primary committers"
        description: "The contributors currently appearing most often in the visible history."
        links:
          - label: "maikebing"
            url: "https://github.com/IoTSharp/LVGLSharp"
          - label: "OpenClaw"
            url: "https://github.com/IoTSharp/LVGLSharp"
          - label: "xcssa"
            url: "https://github.com/IoTSharp/LVGLSharp"
      - title: "Additional contributors"
        description: "Human contributors covering early foundations and later supporting work."
        links:
          - label: "imxcstar"
            url: "https://github.com/imxcstar/LVGLSharp"
          - label: "麦壳饼"
            url: "https://github.com/IoTSharp/LVGLSharp"
  - title: "Organization and source entry points"
    description: "Use these links if you want to understand the broader IoTSharp project landscape or continue into the LVGLSharp source."
    variant: quick-links
    columns: 3
    items:
      - title: "IoTSharp organization"
        description: "Open the organization page, repository list, and broader technical landscape."
        url: "https://github.com/IoTSharp"
      - title: "LVGLSharp repository"
        description: "Follow source code, issues, pull requests, and release history for this project."
        url: "https://github.com/IoTSharp/LVGLSharp"
      - title: "Gitee mirror"
        description: "A mirror and collaboration entry point that can be easier to access in some regions."
        url: "https://gitee.com/IoTSharp/LVGLSharp"
---
