---
title: Blog Index
description: If you want one place to start with LVGLSharp architecture, runtime hosts, X11 bring-up work, and NativeAOT, this is the index to use.
lang: en
template: structured
intro:
  eyebrow: "Design Notes"
  title: "Blog Index"
  description: "If you want one place to explore the project rationale, architecture, Linux host strategy, X11 bring-up work, and NativeAOT direction, start here."
sections:
  - title: "Article entry points"
    description: "If you want the technical story first, these are the best entry points."
    variant: quick-links
    columns: 2
    items:
      - title: "Why WinForms over LVGL"
        description: "Start here if you want to understand why the project should exist at all and what gap it is trying to close."
        url: "/en/blog/why-winforms-over-lvgl.html"
      - title: "Architecture Breakdown"
        description: "Start here if you want a quick mental model of the repository structure, runtime layers, and module boundaries."
        url: "/en/blog/architecture.html"
      - title: "NativeAOT and GUI"
        description: "Start here if you care about deployment size, runtime trimming, AOT constraints, and GUI tradeoffs."
        url: "/en/blog/nativeaot-gui.html"
      - title: "Linux Host Strategy"
        description: "Start here if you are following Linux desktop, device-side hosts, and the longer runtime roadmap."
        url: "/en/blog/linux-hosts.html"
      - title: "X11 Demo Bring-up Notes"
        description: "Start here if you want the practical debugging story behind getting `PictureBoxDemo`, `MusicDemo`, and `SmartWatchDemo` running on X11."
        url: "/en/blog/x11-demo-bringup.html"
  - title: "Suggested reading order"
    description: "If this is your first pass through the blog, this order builds context most effectively."
    variant: list
    surface: true
    ordered: true
    items:
      - label: "Why WinForms over LVGL"
        url: "/en/blog/why-winforms-over-lvgl.html"
      - label: "Architecture Breakdown"
        url: "/en/blog/architecture.html"
      - label: "Linux Host Strategy"
        url: "/en/blog/linux-hosts.html"
      - label: "X11 Demo Bring-up Notes"
        url: "/en/blog/x11-demo-bringup.html"
      - label: "NativeAOT and GUI"
        url: "/en/blog/nativeaot-gui.html"
  - title: "Continue reading"
    description: "Once you finish the core articles, these pages are the next best places to continue."
    variant: quick-links
    columns: 3
    items:
      - title: "Project News"
        description: "Use the news section if you want the strongest recent changes in a faster, update-style format."
        url: "/en/news/"
      - title: "Documentation Navigation"
        description: "Return to the English docs map and choose the next reading path by topic."
        url: "/en/navigation.html"
      - title: "Screenshot Gallery"
        description: "Review the currently curated demo and runtime-host screenshots."
        url: "/en/preview-local.html"
      - title: "NuGet Guide"
        description: "Start here if you are ready to install packages and try a minimal setup."
        url: "/en/nuget.html"
---
