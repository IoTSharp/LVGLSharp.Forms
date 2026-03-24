---
title: Screenshot Gallery
description: If you want a quick look at the UI we already have running, this page collects the screenshot previews in one place.
lang: en
template: structured
intro:
  eyebrow: "Screenshots"
  title: "Screenshot Gallery"
  description: "If you want a quick look at the `LVGLSharp` UI we already have running, start here. As we capture more demos, runtime hosts, and UI states, we will keep adding them to this page."
sections:
  - title: "Current screenshots"
    description: "Use this area for representative runtime, host, and UI screenshots. Click an image to open the original file."
    variant: showcase
    columns: 2
    items:
      - title: "Desktop Preview A"
        badge: "Desktop"
        image: "/images/showcase/desktop-preview-1.png"
        alt: "LVGLSharp desktop preview screenshot A"
        description: "A compact screenshot for showing layout direction, control style, and the overall UI structure."
      - title: "Desktop Preview B"
        badge: "Desktop"
        image: "/images/showcase/desktop-preview-2.png"
        alt: "LVGLSharp desktop preview screenshot B"
        description: "A fuller desktop capture that shows denser content, control composition, and the current demo surface."
      - title: "WSLg / Wayland PictureBox Check"
        badge: "WSLg"
        image: "/images/wslg-pictureboxdemo-wayland-embedded-font-check.png"
        alt: "WSLg Wayland PictureBox embedded font check"
        description: "Tracks Linux-host rendering and embedded-font validation in a real WSLg / Wayland scenario."
  - title: "The X11 screenshots we added in this round"
    description: "If you want to read the [X11 Demo Bring-up Notes](/en/blog/x11-demo-bringup.html) side by side with the actual results, these are the matching screenshots."
    variant: showcase
    columns: 3
    items:
      - title: "PictureBoxDemo on X11"
        badge: "X11"
        image: "/images/x11-pictureboxdemo.png"
        alt: "PictureBoxDemo running on X11"
        description: "We can now launch `PictureBoxDemo` on X11 and keep a reusable screenshot asset for it. The current stable capture uses the built-in font fallback path, so CJK labels still need a later recovery pass."
      - title: "MusicDemo on X11"
        badge: "X11"
        image: "/images/x11-musicdemo.png"
        alt: "MusicDemo running on X11"
        description: "After fixing default font installation and the glyph bitmap callback contract, we can now open `MusicDemo` stably on X11 and capture it."
      - title: "SmartWatchDemo on X11"
        badge: "X11"
        image: "/images/x11-smartwatchdemo.png"
        alt: "SmartWatchDemo running on X11"
        description: "We also now have a working X11 first-screen capture path for `SmartWatchDemo`, and it has become the representative multi-page UI case from this round."
  - title: "If you want to see this gallery grow"
    description: "As we keep extending the gallery, these are the most useful directions to add next."
    variant: cards
    columns: 3
    items:
      - title: "Demo screenshots"
        description: "Add key views from `MusicDemo`, `SmartWatchDemo`, `PictureBoxDemo`, `WinFormsDemo`, and future samples."
      - title: "Multi-host comparisons"
        description: "Capture the same UI across Windows, WSLg, X11, Wayland, FrameBuffer, and Offscreen environments."
      - title: "Before / after UI changes"
        description: "Use side-by-side progress shots if you want to show rendering improvements, theme updates, and control refinements more clearly."
  - title: "How to maintain it"
    description: "If you add more screenshots later, keep them simple and consistent with this structure."
    variant: list
    surface: true
    items:
      - label: "Place new screenshots under `docs/images/`; keep using `docs/images/showcase/` for generic curated gallery assets when helpful"
      - label: "Append a new item under the page front matter `sections.items` collection"
      - label: "Prefer screenshots that highlight real runtime hosts, representative demos, and meaningful visual progress"
---
