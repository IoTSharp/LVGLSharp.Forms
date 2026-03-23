---
title: LVGLSharp Home
description: Bring a WinForms-style development experience to LVGL-powered cross-platform UI.
lang: en
template: structured
hero:
  eyebrow: "Cross-platform GUI · WinForms over LVGL · NativeAOT Ready"
  title: "Keep the familiar WinForms mindset, target much broader runtimes."
  lead: "LVGLSharp uses **LVGL** as the rendering core while exposing a .NET-friendly, WinForms-style development model. The goal is to bridge desktop productivity with Windows, Linux, and device-oriented runtime environments, while continuing to improve **NativeAOT**, lightweight deployment, and host portability."
  actions:
    - label: "Start with docs"
      url: "/en/navigation.html"
      style: primary
    - label: "Read the blog"
      url: "/en/blog/"
      style: secondary
    - label: "GitHub repository"
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
  note_title: "In one sentence"
  note_text: "This is not just a thin .NET wrapper over LVGL. It is an attempt to preserve the WinForms development model while moving rendering and runtime hosting toward a lighter, more portable, and AOT-friendly architecture."
stats:
  - label: "Core identity"
    value: "WinForms over LVGL"
  - label: "Main platforms"
    value: "Windows + Linux"
  - label: "Publishing direction"
    value: "NativeAOT"
  - label: "Engineering target"
    value: "Cross-platform UI Runtime"
sections:
  - title: "Why it matters"
    description: "It connects familiar desktop development productivity with lightweight runtime capabilities for broader display hosts and deployment models."
    variant: cards
    columns: 3
    items:
      - title: "Familiar development flow"
        description: "Preserves forms, controls, events, and layout thinking so developers can reuse the WinForms mental model instead of starting from a lower-level graphics stack."
      - title: "LVGL rendering foundation"
        description: "Builds on LVGL for lightweight, high-performance, and device-adaptive rendering across broader platform and host combinations."
      - title: "AOT and lightweight deployment"
        description: "Moves toward NativeAOT, self-contained publishing, and device-friendly packaging with lower runtime overhead."
  - title: "Quick entry points"
    description: "If you want to evaluate the project quickly, these are the best places to start."
    variant: quick-links
    columns: 3
    items:
      - title: "Documentation map"
        description: "Jump into the right reading path by topic, audience, and language."
        url: "/en/navigation.html"
      - title: "Engineering and CI"
        description: "Understand the build, packaging, publishing, and automation structure."
        url: "/en/ci-workflows.html"
      - title: "WSL / Linux development"
        description: "Start here if your main interest is Linux graphics hosts and development workflow."
        url: "/en/WSL-Developer-Guide.html"
  - title: "Recommended reading paths"
    description: "Choose a path based on your interest instead of browsing everything manually."
    variant: link-lists
    columns: 4
    items:
      - title: "First-time readers"
        description: "Best if you want to understand why the project exists and what it is trying to achieve."
        links:
          - label: "Why WinForms over LVGL"
            url: "/en/blog/why-winforms-over-lvgl.html"
          - label: "中文首页"
            url: "/zh/"
      - title: "Architecture and engineering"
        description: "Best if you care about module boundaries, runtime layering, and repository organization."
        links:
          - label: "Architecture Breakdown"
            url: "/en/blog/architecture.html"
          - label: "CI Workflow Guide"
            url: "/en/ci-workflows.html"
      - title: "Linux and host strategy"
        description: "Best if you care about X11, WSLg, FrameBuffer, Wayland, and future display-host paths."
        links:
          - label: "Linux Host Strategy"
            url: "/en/blog/linux-hosts.html"
          - label: "WSL Developer Guide"
            url: "/en/WSL-Developer-Guide.html"
      - title: "AOT and publishing"
        description: "Best if you care about trimming, self-contained deployments, runtime size, and publishing shape."
        links:
          - label: "NativeAOT and GUI"
            url: "/en/blog/nativeaot-gui.html"
          - label: "Changelog"
            url: "https://github.com/IoTSharp/LVGLSharp/blob/main/CHANGELOG.md"
  - title: "Capability map"
    description: "The project value is not only about drawing UI, but about the runtime structure and engineering model behind it."
    variant: cards
    columns: 3
    items:
      - title: "Forms and controls model"
        description: "Keeps the traditional WinForms programming style, including lifecycle, control tree, events, and layout organization."
      - title: "Layered runtime structure"
        description: "Organizes capabilities across Core, Interop, Runtime.Windows, and Runtime.Linux so rendering and hosting stay decoupled."
      - title: "Device deployment path"
        description: "Opens a realistic path for x64, ARM, and ARM64 device-side UI scenarios in the .NET ecosystem."
  - title: "Demos and usage scenarios"
    description: "This project is not only a concept exploration. It is being shaped against real UI scenarios, real runtime hosts, and real publishing paths."
    variant: cards
    columns: 3
    items:
      - title: "Desktop validation"
        description: "Windows and Linux hosts are used to continuously validate lifecycle behavior, control models, and cross-platform runtime consistency."
      - title: "Device-oriented exploration"
        description: "The runtime roadmap keeps expanding around FrameBuffer, Wayland, SDL, DRM/KMS, and other device-side host strategies."
      - title: "Demo-driven evolution"
        description: "Examples such as MusicDemo, SmartWatchDemo, PictureBoxDemo, and WinFormsDemo help anchor the project in practical usage."
  - title: "Quick start path"
    description: "If you want to get a fast feel for the project, this is the shortest useful path."
    variant: list
    surface: true
    items:
      - label: "Inspect the GitHub repository for project structure"
        url: "https://github.com/IoTSharp/LVGLSharp"
      - label: "Use the documentation map to choose a reading path"
        url: "/en/navigation.html"
      - label: "Read the blog index for rationale and architecture tradeoffs"
        url: "/en/blog/"
      - label: "Study the demo projects under src/Demos for practical usage patterns"
  - title: "Start with these"
    description: "If you only read three pieces first, use this shortlist."
    variant: list
    surface: true
    items:
      - label: "Why WinForms over LVGL"
        url: "/en/blog/why-winforms-over-lvgl.html"
      - label: "Architecture Breakdown"
        url: "/en/blog/architecture.html"
      - label: "NativeAOT and GUI"
        url: "/en/blog/nativeaot-gui.html"
---

