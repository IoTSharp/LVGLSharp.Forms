---
title: "X11 Demo Bring-up Notes: Making PictureBox, MusicDemo, and SmartWatchDemo Run"
description: If you are bringing demos up on Linux X11, this write-up walks you through how we handled display selection, font styling, glyph callbacks, and the fallback path that made capture stable.
lang: en
---

# X11 Demo Bring-up Notes: Making PictureBox, MusicDemo, and SmartWatchDemo Run

> If you are debugging Linux hosts, validating the first X11 frame, or trying to make real demos stable enough to capture, this write-up shows you exactly how we worked through it.

Our goal in this round was straightforward:

- bring `PictureBoxDemo` up under `X11`
- bring `MusicDemo` up under `X11`
- bring `SmartWatchDemo` up under `X11`
- preserve reusable screenshot assets for all three

Once we got into it, this turned out to be much more than “does the window open or not?”. Several layers had to line up at the same time:

- which `DISPLAY` the runtime actually connected to
- how the root font was attached into the LVGL style tree
- whether the custom glyph bitmap callback respected LVGL's buffer contract
- whether font pointers read during control creation were stable
- which path was good enough to treat as the screenshot baseline

## Lesson 1: Verify the real X11 target first

At first we assumed the demos would use the `Xvfb :99` display we prepared for validation.

That assumption was wrong.

The current Linux runtime does not simply trust the process-local `DISPLAY` value. It also scans candidate X11 sockets and can end up preferring a real desktop display. In this bring-up round, the actual working target was `:1`, not `:99`.

That sounds like an operational detail, but it matters a lot:

- the real window inspection tools must point at the actual display
- screenshot capture must target the same display
- otherwise you can end up debugging the wrong environment

That is why every screenshot we kept from this round came from `DISPLAY=:1`.

## Lesson 2: Manually allocating `lv_style_t` from managed code is fragile

The earliest crash stacks from `MusicDemo` and `SmartWatchDemo` both converged on font-related LVGL paths:

- `lv_font_get_line_height`
- `lv_text_get_size`
- `lv_label_refr_text`
- `lv_obj_refresh_style`

At first glance that can look like a label-specific bug. But after following the initialization path more carefully, the more suspicious part was how the default font style was installed.

The old Linux default-font path manually allocated an `lv_style_t` from managed code, then called:

- `lv_style_init`
- `lv_style_set_text_font`
- `lv_obj_add_style`

That approach is risky because `lv_style_t` is a native-owned structure. If managed layout, alignment, or lifecycle assumptions drift even slightly from what the native library expects, the whole path becomes brittle.

We changed `[LvglHostDefaults.cs](/home/admin/openclaw/workspace/LVGLSharp.Forms_1/src/LVGLSharp.Runtime.Linux/LvglHostDefaults.cs)` to use a simpler and safer path:

```csharp
lv_obj_set_style_text_font(root, font, 0);
```

That change removed a whole class of “managed code trying to simulate native style ownership” problems.

## Lesson 3: The glyph bitmap callback has to obey LVGL's memory contract exactly

The next high-probability issue lived inside the custom font manager.

In `[SixLaborsFontManager.cs](/home/admin/openclaw/workspace/LVGLSharp.Forms_1/src/LVGLSharp.Core/SixLaborsFontManager.cs)`, `GetGlyphBitmap` previously had two dangerous behaviors:

1. it returned `draw_buf` instead of `draw_buf->data`
2. it mixed its own bitmap caching lifetime with the draw buffer that LVGL provided for the current render

That is the kind of bug that often survives the first few steps and only explodes deeper in the render stack. LVGL expects a pointer to glyph pixel data. If it instead receives a struct pointer or a buffer with the wrong ownership model, later blending code becomes vulnerable.

We fixed that by doing less:

- render directly into `draw_buf->data`
- return `draw_buf->data`

In other words, the callback now fills the buffer LVGL asked for and returns the exact address LVGL expects.

After that change, `MusicDemo` became much more stable and was able to open as a real X11 window and produce a usable screenshot.

## Lesson 4: A font pointer returned from style lookup is not always a trusted source

`SmartWatchDemo` was trickier.

After the first wave of fixes, it no longer failed before window creation, but it still crashed very early during label creation. A lightweight creation trace narrowed the problem down to the first title label. Then `gdb` showed the critical clue: the “font pointer” passed into `lv_font_get_line_height` was `0xff`.

At that point the issue was no longer “the font file is bad”. It was “something in the style path is feeding LVGL a bogus value as if it were a font pointer”.

The root cause landed in the WinForms-side style helper. During control creation, the helper was querying the current font from the root style and then reapplying that value to newly created controls. That sounds reasonable in theory, but during the current X11 bring-up lifecycle, that query result was not stable enough to trust.

We fixed that by introducing a managed registry for the active runtime font:

- add `[LvglRuntimeFontRegistry.cs](/home/admin/openclaw/workspace/LVGLSharp.Forms_1/src/LVGLSharp.Core/LvglRuntimeFontRegistry.cs)`
- cache the active font pointer at the moment the Linux runtime installs it
- teach `[ApplicationStyleSet.cs](/home/admin/openclaw/workspace/LVGLSharp.Forms_1/src/LVGLSharp.WinForms/Forms/ApplicationStyleSet.cs)` to use that managed pointer instead of querying the root style again during control creation

The key idea is simple:

- the code that installs the font should keep the trusted pointer
- control creation should not depend on a style lookup that can return transient or invalid values
- obvious bad sentinels such as `0xff` should never be allowed to flow deeper into LVGL

## Lesson 5: “Stable enough to bring up” and “fully fixed” are not the same thing

By this point we had moved all three demos forward substantially, but one more reality had to be acknowledged:

the custom font path could still crash later in the draw pipeline, especially inside software blending paths such as `lv_draw_sw_blend_color_to_rgb565`.

That means:

- “the window opens” is not the same as “the full font-rendering pipeline is correct”
- “the first screen appears” is not the same as “every label is stable”

So instead of pretending the custom font renderer was completely solved, this round added an explicit stability fallback:

- `[X11View.cs](/home/admin/openclaw/workspace/LVGLSharp.Forms_1/src/LVGLSharp.Runtime.Linux/X11View.cs)` now supports `LVGLSHARP_DISABLE_CUSTOM_FONT=1`

With that switch enabled, the X11 runtime skips the current custom font manager and falls back to LVGL's built-in font path.

The tradeoff is very clear:

- complex demos become stable enough to launch and capture
- but CJK text can temporarily degrade into missing glyphs or square placeholders

That is not the final state we want, but it is a valid engineering move for bring-up work because it allows screenshots, regression checks, and documentation to keep moving while the deeper renderer issue is still under investigation.

## The currently recommended X11 launch command

If your goal right now is “make it run stably first and capture the UI”, the current recommended X11 command is:

```bash
DISPLAY=:1 \
LVGLSHARP_LINUX_HOST=x11 \
XDG_SESSION_TYPE=x11 \
WAYLAND_DISPLAY= \
LVGLSHARP_DISABLE_CUSTOM_FONT=1 \
dotnet run -f net10.0 --project src/Demos/SmartWatchDemo/SmartWatchDemo.csproj -p:EnableWindowsTargeting=true
```

`MusicDemo` follows the same pattern with its own `csproj`.

If you are doing layout validation, screenshot collection, or UI regression work, that stable path is the best current baseline. We can keep improving the deeper custom-font path in parallel.

## What we have now

We now have three X11 screenshot assets:

- `PictureBoxDemo`: `/images/x11-pictureboxdemo.png`
- `MusicDemo`: `/images/x11-musicdemo.png`
- `SmartWatchDemo`: `/images/x11-smartwatchdemo.png`

They have also been added to the curated screenshot pages:

- [Chinese Screenshot Page](/zh/preview-local.html)
- [English Screenshot Gallery](/en/preview-local.html)

## What you should keep in mind if you are bringing up X11

- X11 bring-up is not a single bug. It is the combined result of display selection, style installation, font ownership, glyph callback correctness, and capture tooling.
- If a native structure ABI is not fully under your control, managed-side manual allocation should be treated with great caution.
- In glyph callbacks, return values and buffer ownership must match LVGL's contract exactly, or the crash may surface much later in the render stack.
- If style lookups are unstable during early control creation, keeping a trusted active font pointer on the managed side is safer than repeatedly querying the style tree.
- A deliberate stability fallback switch is not a failure. It is often what lets demos, screenshots, validation, and deeper debugging move forward at the same time.

## What we still need to finish

We moved the X11 path from “some demos are black or crashing” to “complex demos can launch and be captured”.

But we are not at the finish line yet. If you keep following this path, these are the next items we still need to close:

- fully fix the deeper custom-font rendering crash on X11
- make `MusicDemo` and `SmartWatchDemo` stable without disabling the custom font path
- restore CJK text quality for views such as `PictureBoxDemo`
- eventually reduce `LVGLSHARP_DISABLE_CUSTOM_FONT=1` from a stability path to a debugging-only switch

If we land that next step, X11 stops being just “something we managed to screenshot once” and becomes a solid engineering-grade validation path for Linux host work.
