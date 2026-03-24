---
title: I finally brought WinFormsVncDemo onto a real VNC path
description: A first-person update on the VncView path, remote runtime cleanup, Windows publishing fix, and the documentation case built around it.
lang: en
---

# I finally brought WinFormsVncDemo onto a real VNC path

> March 24, 2026. I wanted to capture one result from this round in a news-style format: `WinFormsVncDemo` is no longer just a local window sample. It can now run directly on `VncView` and be reached from a desktop VNC client.

![WinFormsVncDemo shown through a Windows desktop VNC viewer](/images/winformsvncdemo-vnc-case.png)

## The key changes I pushed forward in this round

- I kept `VncView` and `RdpView` as the public entry points instead of hiding them under internal subdirectories.
- I reorganized the internal remote runtime implementation into `Frames`, `Input`, `Transport`, `Session`, and `Views`, so the boundaries around transport, input, and frame handling are much clearer.
- I pulled VNC host, port, and window-size details back into runtime registration, so `Program` once again only starts the app and the demo itself no longer owns remote session wiring.
- I added a default construction path for `VncView`, which lets the demo come up with sensible defaults now without blocking a future `ViewOptions` design.
- I made the demo print port `5900` and the locally reachable addresses to the console at startup, so the first-run experience immediately tells you where to connect.
- I fixed the native-runtime publishing behavior in `LVGLSharp.Native.targets`, so Windows self-contained / AOT output carries the required `lvgl.dll` instead of failing later with `DllNotFoundException`.
- I also moved the Windows-side VNC Viewer screenshot for `WinFormsVncDemo` into the docs, so this work is represented by a real, shareable runtime case instead of words alone.

## Why I think this deserved a news entry

Before this round, the path felt more like something that could be pieced together. Now it feels much closer to something I can hand to someone else to try. The biggest win for me is not just that a demo launches, but that the responsibilities are cleaner: the public entry is simple, the internal runtime layers are easier to read, the demo startup path stays clean, and the published output is much more believable.

That gives us a stronger baseline for the next stage: a normal WinForms-style UI can now be hosted by `VncView`, exposed as a VNC service, and opened from a desktop client. That is a practical step forward for remote-host scenarios, automation, and a more formal configuration story later.

## What I want to keep improving next

- I want to keep shaping the remote runtime configuration into a clearer `ViewOptions` model.
- I want to keep adding real-case screenshots instead of describing progress only in text.
- I want the publish pipeline to keep moving toward “take the output and test it directly,” with less gap between demo code and real deployment.
