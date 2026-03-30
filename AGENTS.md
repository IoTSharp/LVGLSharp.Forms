# LVGLSharp Agent Guidance

This file is the canonical cross-agent memory for this repository.

Use this file as the shared source of truth for repo-specific AI guidance.
- Codex-compatible agents should read this file directly.
- Claude-facing entrypoints should delegate to this file.
- When adding or updating repo-specific AI memory, update this file first.

## Scope
- These rules apply to work in this repository only.
- If generic framework advice conflicts with a rule here, follow this file.
- Keep agent-facing repo memory here instead of user-global preferences.

## Project Rules
- Prefer standard WinForms lifecycle and methods such as Show, CreateHandle, and Application.Run; avoid exposing non-standard APIs such as Form.Init.
- New implementation work in this repository must be based on LVGL and must not introduce Windows or Win32-specific implementations or APIs.
- LVGLSharp.Forms is not expected to remain designer-openable; remove System.Drawing dependencies there and keep it cross-platform. Only WinFormsDemo should support the designer. Do not add a net10.0-windows target to LVGLSharp.Forms.
- This repository uses the term LVGLSharp 布局 for the layout shape where an outer TableLayoutPanel only does vertical partitioning and each row contains a FlowLayoutPanel for the actual controls. Do not describe this as WinFormsDemo style or Demo layout.
- When implementing LVGLSharp 布局, use fixed absolute row heights on the main TableLayoutPanel, never percentage row heights, and do not place business controls directly on the main TableLayoutPanel.

## AOT And Loading
- Strict AOT compatibility is required.
- Do not suppress trimming or AOT issues by comments alone.
- Replace reflection-based activation and runtime type discovery with explicit AOT-safe code paths such as static mappings, explicit factories, or generated code.
- Avoid Activator, CreateInstance, GetTypes, or similar dynamic activation paths in LVGLSharp code.
- For XAML loading, prefer embedded-resource based loading and avoid runtime directory scanning.

## Font Handling
- Shared fallback font handling should live in a core-level solution so multiple runtime views can reuse one strategy.

## Headless And Snapshot Testing
- In headless LVGL interop tests, lv_obj_align is the four-parameter overload. If you need a base object, use lv_obj_align_to.
- Test projects do not automatically inherit the interop project's global using setup. When using lv_align_t, lv_label_long_mode_t, lv_flex_flow_t, and similar enums directly, add explicit using LVGLSharp.Interop or the required using static imports.
- For Chinese snapshot coverage, a lightweight image-signature plus minimum ink-pixel assertion is an acceptable alternative to introducing a separate external snapshot framework.

## Runtime Debugging Learnings
- When diagnosing a Linux/X11 black screen, first distinguish between a created-but-not-painted window and a control-tree creation stall. If Load never runs, inspect control creation first instead of blaming font parsing.
- For multi-page full-screen UIs on X11/LVGL, do not mount all hidden full pages into the same viewport during startup. Mount the first page first and lazy-load or gradually warm the others.
- Passive controls such as Label and display-only Panel should not bridge the full LVGL event set by default. Only enable event bridging on controls that really need input or gestures.
- Prefer driving dynamic UI refresh from the main message-loop tick. Do not continuously push LVGL or Forms control state from background threads.
- When validating X11 rendering, do not rely on the window title bar alone. Verify actual viewport pixels.

## Repository Roadmap Preference
- Preferred host implementation order is: Wayland, SdlView, DRM, KMS, Offscreen, DirectFB, Mir.
- After Linux host work, add LVGLSharp.Runtime.MacOs.
- After that, add a dedicated remote runtime for VNC, RDP, and similar remote hosts.

## Maintenance Rule
- CLAUDE.md should stay as a thin pointer to this file instead of becoming a second source of truth.
- If repo-specific AI instructions are updated elsewhere, mirror them here so Claude- and Codex-style agents can see the same guidance.
