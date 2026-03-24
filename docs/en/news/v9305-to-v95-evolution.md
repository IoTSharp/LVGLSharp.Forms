---
title: From v9.3.0.5 to the current 9.5 line
description: A formal summary of how LVGLSharp evolved from v9.3.0.5 on March 17, 2026 to the current documented 9.5 baseline, across runtimes, demos, validation, docs, and release engineering.
lang: en
---

# From v9.3.0.5 to the current 9.5 line

> This note records the main project changes from `v9.3.0.5` on March 17, 2026 to the current documented `9.5` baseline. What changed in this period was not only the version line itself, but the project’s internal boundaries, runtime coverage, validation paths, documentation structure, and release discipline.

## The scale of the change

Measured at the repository level, this phase includes roughly:

- `298` changed files
- about `27,011` inserted lines
- about `6,080` deleted or rewritten lines

This was not a narrow patch cycle. It was a structural expansion from a compatibility-focused codebase into a more complete runtime and release-oriented system.

## What v9.3.0.5 represented

`v9.3.0.5` already established an important early baseline:

- the core package split was present
- `LVGLSharp.Forms`, `Core`, `Interop`, `Native`, `Runtime.Windows`, and `Runtime.Linux` were already recognizable
- common controls, `Application.Run(Form)`, NativeAOT direction, and event bridging were already part of the public story
- demos such as `WinFormsDemo`, `PictureBoxDemo`, and `SerialPort` already existed
- documentation still centered mainly on the repository README

That made the `9.3` line the stage where the project proved its foundation. The `9.5` line is where that foundation became broader, clearer, and more organized.

## What changed between 9.3 and 9.5

## 1. The LVGL and runtime abstraction base became stronger

The first major shift in the `9.5` line was at the platform foundation:

- the project moved onto the `LVGL 9.5` baseline
- the generated P/Invoke surface grew with new enums, structs, and drawing-related bindings
- earlier abstractions continued to converge toward a clearer `IView` / `ViewLifetimeBase` direction
- runtime registration became more standardized, with `ApplicationConfiguration.Initialize()` taking a clearer central role

This matters because it makes the relationship between the Forms layer and the lower runtime layer more explicit and easier to extend.

## 2. The runtime model moved from “can run” to “can be layered”

Compared with `v9.3.0.5`, the current `9.5` line has a much more explicit runtime matrix:

- the Windows side has been further normalized in naming and runtime boundaries
- the Linux side now covers `WSLg`, `X11`, `Wayland`, `SDL`, and `FrameBuffer`
- `DrmView` is now in the repository as the first clear entry point for `DRM/KMS`
- `Offscreen` was split out of the Linux host branch into `LVGLSharp.Runtime.Headless`
- `LVGLSharp.Runtime.MacOs` and `LVGLSharp.Runtime.Remote` now exist as dedicated skeletons for future `macOS`, `VNC`, and `RDP` work

In other words, the project is no longer only “cross-platform enough to run.” It is becoming a runtime system whose host boundaries can be explained, extended, and validated more deliberately.

## 3. Demo and validation coverage expanded significantly

At the `9.3` stage, demos mainly served as proof-of-life paths. In the `9.5` line, they became a major part of how capability is expressed and checked:

- `MusicDemo` and `SmartWatchDemo` expanded the repository into richer UI validation paths
- `OffscreenDemo` added a dedicated entry point for headless rendering and PNG output
- `MacOsAotDemo` provided the first AOT-oriented verification path for the macOS skeleton
- `WinFormsVncDemo`, `WinFormsRdpDemo`, and `RdpDemo` gave the remote runtime story concrete demo surfaces
- `tests/LVGLSharp.Headless.Tests` introduced the first real automated checks around snapshots, background color handling, and frame-source reuse

That means the project now relies less on ad hoc manual validation alone and more on a combination of demos, snapshots, and release-output checks.

## 4. Documentation moved from README-first to site-first

One of the clearest visible changes from `9.3` to `9.5` is the documentation system itself.

What was once mostly README-driven now includes a structured site with:

- home pages
- navigation pages
- blog articles
- project news
- NuGet guidance
- screenshot galleries
- bilingual page organization and language switching
- roadmap and changelog alignment
- local preview tooling and GitHub Pages publication flow

The documentation is no longer just attached to the repository. It has become part of the project’s actual engineering and release surface.

## 5. Packaging and release engineering became more deliberate

The `9.5` line also pushed release engineering forward:

- native assets are organized under `runtimes/{rid}/native`
- workflows are now decomposed into prepare, native build, NuGet pack, demo build, and release publish stages
- version normalization and asset aggregation are more explicit
- the dependency relationship between source builds, Native packaging, and downstream package consumers is better controlled

This is a major shift from “the project can build” toward “the project can build, package, and publish with clearer rules.”

## What the 9.5 line really means

Taken together, the project achieved three especially important transitions between `v9.3.0.5` and the current `9.5` line:

1. from a compatibility-layer proof point to a multi-runtime structure
2. from basic demos to layered validation through demos, tests, and screenshots
3. from repository-centered notes to a fuller site, roadmap, changelog, and release flow

That is why the `9.3` line can be understood as the stage where the project established its base, while the `9.5` line is the stage where it began to take on real engineering shape.

## What continues next

Even with that progress, several paths are still actively moving:

- `DRM/KMS` still needs a fuller native backend
- Headless validation should continue to grow
- `macOS` and Remote are still in skeleton and early-validation form
- `LVGLSharp.Runtime.Headless`, `LVGLSharp.Runtime.MacOs`, and `LVGLSharp.Runtime.Remote` have now been added to the packaging workflow and are expected to move into formal NuGet publication later

The direction remains consistent: preserve the WinForms-style top-level developer experience, while continuing to strengthen runtime boundaries, automated verification, and release discipline underneath it.

## Continue reading

- [Roadmap](https://github.com/IoTSharp/LVGLSharp/blob/main/ROADMAP.md)
- [Changelog](https://github.com/IoTSharp/LVGLSharp/blob/main/CHANGELOG.md)
