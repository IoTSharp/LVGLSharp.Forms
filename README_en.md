# LVGLSharp.Forms

[中文](./README.md) | English

**LVGLSharp.Forms** is a cross-platform WinForms API compatibility layer that uses [LVGL](https://github.com/lvgl/lvgl) as the underlying rendering engine. The goal is to achieve **WYSIWYG (What You See Is What You Get)** — UI designed with the Visual Studio Windows Forms Designer on Windows will render with high fidelity on Linux (arm / arm64 / x64) and other embedded platforms.

> ⚠️ This project is currently in the experimental phase and is not yet suitable for production use.

---

## 📢 Current Release

- **Version**: `9.3.0.5`
- **Release Tag**: `v9.3.0.5`
- **Release Positioning**: the first fully documented release derived from the initial project baseline, consolidating the current capabilities, package layout, and release notes.

### 9.3.0.5 Highlights

- Continues the initial LVGL-backed WinForms compatibility direction and documents the now-available controls, runtime hosts, and packaging layout.
- Clarifies the responsibilities of `LVGLSharp.Forms`, `LVGLSharp.Core`, the platform runtime packages, and `LVGLSharp.Native`.
- Documents `Application.Run(Form)` lifecycle support, the LVGL event bridge, and NativeAOT publishing readiness.
- Synchronizes the release summary with [`CHANGELOG.md`](./CHANGELOG.md) so future releases can build on top of tag-based release records.

For the complete release history starting from the initial version, see [`CHANGELOG.md`](./CHANGELOG.md).

---

## ✨ Features

- 🖥️ **WinForms API Compatibility**: An API surface closely mirroring `System.Windows.Forms`, making it easy to migrate existing code.
- 🔤 **Full LVGL API Interop**: Auto-generated P/Invoke bindings via ClangSharpPInvokeGenerator covering the entire LVGL C API.
- 🚀 **NativeAOT Support**: Publish as self-contained native executables with no managed runtime dependency (validated on win-x64 and linux-arm).
- 🌍 **Cross-Platform**: Supports Windows (x64) and Linux (x64 / arm / arm64).
- 🧩 **Built-in Common Controls**: Button, Label, TextBox, CheckBox, RadioButton, ComboBox, ListBox, ProgressBar, TrackBar, NumericUpDown, PictureBox, Panel, GroupBox, FlowLayoutPanel, TableLayoutPanel, RichTextBox, and more.
- 🎨 **Custom Drawing Types**: The `LVGLSharp.Darwing` namespace provides `Size`, `Point`, `Color`, and other types without any dependency on `System.Drawing`, ensuring true cross-platform portability.

---

## 📷 Preview

The following screenshots show an application published with NativeAOT for win-x64 / linux-arm (no external dependencies required):

<iframe src="//player.bilibili.com/player.html?isOutside=true&aid=116237182962589&bvid=BV12Fwjz5EuZ&cid=36733586714&p=1" scrolling="no" border="0" frameborder="no" framespacing="0" allowfullscreen="true"></iframe>
---

## 📦 NuGet Packages

| Package | Description |
|---------|-------------|
| `LVGLSharp.Forms` | WinForms API compatibility layer |
| `LVGLSharp.Core` | Shared runtime abstractions and common font/host helper capabilities |
| `LVGLSharp.Runtime.Windows` | Windows platform runtime |
| `LVGLSharp.Runtime.Linux` | Linux platform runtime |
| `LVGLSharp.Interop` | LVGL P/Invoke bindings (auto-generated) |
| `LVGLSharp.Native` | Platform-native LVGL libraries (win-x86 / win-x64 / win-arm64, linux-arm, etc.) |

---

## 🚀 Quick Start

### 1. Create a Project

Use the same multi-target pattern as the demos in this repository:

- use `UseWindowsForms=true` on the `net10.0-windows` target so it stays on the standard WinForms path
- use `UseLVGLSharpForms=true` on the `net10.0` target so it uses the `LVGLSharp.Forms` path

Whether a target uses WinForms or `LVGLSharp.Forms` must be decided only by `UseWindowsForms` and `UseLVGLSharpForms`, not by OS symbols such as `WINDOWS`.

Typical project configuration:

```xml
<PropertyGroup>
  <TargetFrameworks>net10.0-windows;net10.0</TargetFrameworks>
</PropertyGroup>

<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0-windows'">
  <UseWindowsForms>true</UseWindowsForms>
</PropertyGroup>

<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <UseLVGLSharpForms>true</UseLVGLSharpForms>
  <PublishAot>true</PublishAot>
</PropertyGroup>
```

On the `UseLVGLSharpForms=true` target:

- reference `LVGLSharp.Forms`
- reference `LVGLSharp.Runtime.Windows`
- reference `LVGLSharp.Runtime.Linux`
- register the runtime explicitly before `Application.Run(...)` by calling a helper that wraps `Application.UseRuntime(...)` and `Image.RegisterFactory(...)` (the demos use `DemoRuntimeConfiguration.Configure()`)

See [`src/Demos/PictureBoxDemo/PictureBoxDemo.csproj`](./src/Demos/PictureBoxDemo/PictureBoxDemo.csproj).

### 2. Entry Point

The `UseWindowsForms=true` target does not require any `LVGLSharp` runtime registration.

The `UseLVGLSharpForms=true` target must register the runtime before `Application.Run(...)`. The recommended pattern is to wrap that in a shared helper, like the demos do:

```csharp
ApplicationConfiguration.Initialize();

#if LVGLSHARP_FORMS
DemoRuntimeConfiguration.Configure();
#endif

Application.Run(new frmMain());
```

Inside `DemoRuntimeConfiguration.Configure()`, the `LVGLSharp.Forms` path detects the current runtime platform (Windows or Linux) and selects the matching LVGL host and image implementation.

### 3. Run on Linux

Publish using NativeAOT:

```bash
dotnet publish -r linux-arm64 -c Release
```

Windows publish example:

```powershell
dotnet publish -f net10.0-windows -r win-x64 -c Release
```

Linux / `LVGLSharp.Forms` publish example:

```bash
dotnet publish -f net10.0 -r linux-x64 -c Release
```

---

## 🏗️ Project Structure

```
src/
├── LVGLSharp.WinForms/     # WinForms API compatibility layer (core)
│   ├── Forms/              # Control implementations (Control, Form, Button, etc.)
│   ├── Drawing/            # Cross-platform drawing types (Size, Point, Color, etc.)
│   └── Runtime/            # Shared runtime registration glue and host integration
├── LVGLSharp.Interop/      # LVGL P/Invoke auto-generated bindings
├── LVGLSharp.Native/       # Platform-native libraries
├── LVGLSharp.Core/         # Shared core library
├── LVGLSharp.Windows/      # Windows platform runtime
├── LVGLSharp.Runtime.Linux/# Linux platform runtime
└── Demos/
    ├── WinFormsDemo/       # Baseline WinForms / LVGLSharp.Forms comparison demo
    ├── PictureBoxDemo/     # PictureBox demo
    ├── MusicWinFromsDemo/  # MusicDemo demo application
    ├── SmartWatchDemo/     # SmartWatch UI demo
    └── SerialPort/         # SerialPort demo application
libs/
└── lvgl/                   # LVGL source code (submodule)
```

---

## 🙏 Acknowledgements

- **[imxcstar / LVGLSharp](https://github.com/imxcstar/LVGLSharp)**: Provides the foundational low-level LVGL .NET wrapper that this project is built upon.
- **[LVGL](https://github.com/lvgl/lvgl)**: A lightweight, high-performance embedded GUI library.
- **[ClangSharpPInvokeGenerator](https://github.com/dotnet/ClangSharp)**: Used to auto-generate the complete LVGL P/Invoke bindings.
- **[SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)**: Cross-platform image processing library.
- **[SixLabors.Fonts](https://github.com/SixLabors/Fonts)**: Cross-platform font parsing library.

---

## 💬 Community

You are welcome to join our WeChat group to discuss project usage, cross-platform adaptation, control implementation, and troubleshooting.

If you are interested in LVGLSharp.Forms, scan the QR code below to join the WeChat group.

![LVGLSharp WeChat Group](./preview/wechat-group.png)

---

## 📄 License

This project is licensed under the [MIT License](./LICENSE.txt).
