---
title: WSL Developer Guide
description: Explains how to run, publish, and debug Linux-path demos inside WSL2 and WSLg.
lang: en
---

# WSL Developer Guide

This document explains how developers in this repository can run and debug Linux-path demos inside `WSL2/WSLg`.

If you also want the current completion status and the recommended next engineering step, see the [ROADMAP](https://github.com/IoTSharp/LVGLSharp/blob/main/ROADMAP.md) and [`navigation.md`](./navigation.md).

## Short Conclusion First

The most practical workflow for this repository today is:

1. keep editing code on Windows using `Visual Studio`
2. use the repository's Linux publish script or `dotnet publish` to generate Linux executables
3. run the published outputs inside `WSL2`
4. if Linux-side breakpoint debugging is required, prefer `VS Code + Remote WSL`

## Why this is the recommended path

According to the current Microsoft documentation:

- the Windows Subsystem for Linux developer guidance clearly recommends:
  - `Visual Studio Code` for WSL remote development and debugging
  - `Visual Studio` native WSL workflows mainly for `C++` cross-platform development
- for a repository like this one, using `.NET + custom Linux runtime`, the most reliable approach remains:
  - edit on Windows
  - run in WSL
  - switch to `VS Code Remote WSL` when Linux-side breakpoints are needed

That is why this guide does not treat "Visual Studio directly F5-debugging a WSL .NET demo" as the primary path. Instead, it documents the more reliable workflow.

## Environment Preparation

### 1. Install WSL2

Install and initialize WSL from Windows:

```powershell
wsl --install
```

After installation, it is recommended to prepare at least one Ubuntu distribution.

### 2. Confirm WSLg is available

Inside WSL, run:

```bash
echo $DISPLAY
echo $WAYLAND_DISPLAY
ls /mnt/wslg
```

If any of the following are true, `WSLg` is usually available:

- `DISPLAY` has a value
- `WAYLAND_DISPLAY` has a value
- `/mnt/wslg` exists

### 3. Install the .NET SDK inside WSL

The repository currently targets `.NET 10`, so make sure the matching SDK exists inside WSL:

```bash
dotnet --info
```

If not, install the corresponding SDK in WSL using the official .NET installation instructions.

### 3.1 NuGet fallback path note for WSL

If your global NuGet configuration on Windows contains fallback package folders that only exist on Windows, direct `restore/publish` calls inside WSL may fail because of paths such as `C:\Program Files\...`.

This repository now provides `NuGet.Wsl.Config`, and `build-linux-demos.sh` automatically uses it to avoid Windows-only fallback path issues.

If you run `dotnet restore` or `dotnet publish` manually inside WSL, it is also recommended to pass:

```bash
--configfile /mnt/d/source/LVGLSharp/NuGet.Wsl.Config
```

### 4. Install Linux native dependencies

The Linux demo publish script depends on tools such as `cmake`. On Ubuntu / Debian:

```bash
sudo apt-get update
sudo apt-get install -y cmake ninja-build
```

### 4.1 If Chinese text appears broken in WSL

If Chinese labels, buttons, or titles appear garbled under `WSLg / Wayland`, do not immediately assume an encoding issue. In this repository, a more common cause is that WSL does not actually have usable `CJK` fonts installed.

Recommended fix:

```bash
sudo apt-get update
sudo apt-get install -y fonts-noto-cjk fonts-wqy-zenhei
fc-list :lang=zh | head
```

If `fc-list :lang=zh` is still empty, then WSL still has no proper Chinese font recognized by `fontconfig`, and demos such as `PictureBoxDemo` may still fall back to `DejaVu Sans` or another non-CJK font.

## Recommended Ways to Run Demos

### Option A: Publish on Windows, run inside WSL

This is the most recommended workflow today.

#### Step 1: Run the Linux publish script from the repository root

Windows PowerShell:

```powershell
bash ./build-linux-demos.sh --clean --rid linux-x64
```

If successful, outputs are usually placed under:

- `dist/linux-x64/WinFormsDemo/`
- `dist/linux-x64/PictureBoxDemo/`
- `dist/linux-x64/SmartWatchDemo/`
- other demo directories

#### Step 2: Enter the repository path inside WSL

If the repository is located at `D:\source\LVGLSharp` on Windows, the WSL path is usually:

```bash
cd /mnt/d/source/LVGLSharp
```

#### Step 3: Run a specific demo

For `WinFormsDemo`:

```bash
cd /mnt/d/source/LVGLSharp/dist/linux-x64/WinFormsDemo
./WinFormsDemo
```

For `PictureBoxDemo`:

```bash
cd /mnt/d/source/LVGLSharp/dist/linux-x64/PictureBoxDemo
./PictureBoxDemo
```

## Demo-by-Demo Run Guide

The following sections summarize publishing and launching instructions for common demos. The examples assume:

- repository root: `/mnt/d/source/LVGLSharp`
- target RID: `linux-x64`
- publish method: preferably the repository script `build-linux-demos.sh`

### `WinFormsDemo`

Good for validating basic controls, form lifecycle, the `LVGLSharp Layout` pattern, and common interaction behavior.

#### Publish only this demo

Windows PowerShell:

```powershell
bash ./build-linux-demos.sh --clean --rid linux-x64 WinFormsDemo
```

#### Launch inside WSL

```bash
cd /mnt/d/source/LVGLSharp/dist/linux-x64/WinFormsDemo
./WinFormsDemo
```

### `PictureBoxDemo`

Good for validating image loading, scaling, rotation, anti-aliasing, and Linux image-path behavior.

#### Publish only this demo

Windows PowerShell:

```powershell
bash ./build-linux-demos.sh --clean --rid linux-x64 PictureBoxDemo
```

#### Launch inside WSL

```bash
cd /mnt/d/source/LVGLSharp/dist/linux-x64/PictureBoxDemo
./PictureBoxDemo
```

#### Additional checks recommended

- verify whether `Assets/` has been copied to the output directory
- if images do not show, first verify current paths and file existence

#### Wayland / WSLg validation suggestions

`PictureBoxDemo` is especially suitable for verifying:

- whether the first image frame is actually submitted to the `Wayland` surface
- whether the image region still renders after the first resize
- whether pointer focus entering the window keeps interaction working

If a black screen or blank area appears, do not first blame the image assets. Check in this order:

1. whether the first frame was submitted
2. whether the first frame was retriggered after resize
3. whether input focus has correctly entered the window
4. whether `root invalidate` has been retriggered when needed

#### Wayland / WSLg observed progress

The current local `WSLg` validation has already confirmed the following for `PictureBoxDemo`:

- `build-linux-demos.sh PictureBoxDemo` can successfully publish under WSL
- the `PictureBoxDemo` process can start under `WSLg`
- a real window title has been observed from the Windows side, indicating that the `Wayland/WSLg` path has already entered the actual window creation stage
- Linux system font fallback has been improved in `WaylandView`, so Chinese control text is no longer blocked purely by missing CJK font handling when validating screenshots

If you still suspect a font path issue, inspect the additional diagnostics now emitted by `WaylandView.ToString()`:

- `FontPath=...`
- `FontDiag=...`

These indicate the actual selected font file and whether the runtime had to fall back to a generic font such as `DejaVu Sans`.

#### Wayland / WSLg screenshots

The following image has already been committed into the repository and can be used as a concrete screenshot example for `PictureBoxDemo` under `WSLg / Wayland`:

![PictureBoxDemo under WSLg / Wayland](./images/wslg-pictureboxdemo-wayland.png)

The following image shows the same demo after a window resize and can be used as an additional example that the first frame remains visible after resizing:

![PictureBoxDemo under WSLg / Wayland after resize](./images/wslg-pictureboxdemo-wayland-resized.png)

At this stage, `PictureBoxDemo` should be treated as an image-path validation demo:

- verify whether the first image frame is submitted
- verify whether the image region recovers immediately after resize
- only return to image asset assumptions after checking frame submission, focus, and invalidation

#### Current conclusions about rendering behavior

Based on current validation, the following can already be concluded:

- first-frame visibility: the demo is already renderable enough to capture screenshots, so this is not a total-black-screen path anymore
- resize behavior: screenshots taken after enlarging the window still show valid output, and no full-window blank state has been observed
- focus handling: after bringing the window to the foreground, button interactions still produce visible updates, so there is currently no evidence that focus entry causes a black screen
- scaling / rotation: real button interaction tests have already been performed, and screenshot hashes changed after scaling and rotation, meaning button response and repaint paths are working
- button refresh stability: after foreground/background switches, further button interactions still changed the screenshot result, so there is currently no evidence that focus return breaks repainting
- `root invalidate`: based on first frame, resize, and interaction visibility, there is currently no evidence that `PictureBoxDemo` still needs another dedicated `root invalidate` fix

In other words, `PictureBoxDemo` has already moved beyond "can the path start at all?" and into real rendering validation.

### `SmartWatchDemo`

This demo is useful for verifying multi-page UI structure, larger compositional layouts, and more complete interaction flows. If you investigate Linux rendering issues, `SmartWatchDemo` is a good stress case because it exposes layout, rendering order, focus handling, and hidden-page initialization more quickly than a minimal sample.

---

If you are looking for the shortest practical guidance: publish from Windows, run inside WSL, and use `VS Code Remote WSL` when you need Linux-side breakpoints. That remains the most stable workflow for this repository today.
