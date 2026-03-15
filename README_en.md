# LVGLSharp.Forms

[中文](./README.md) | English

**LVGLSharp.Forms** is a cross-platform WinForms API compatibility layer that uses [LVGL](https://github.com/lvgl/lvgl) as the underlying rendering engine. The goal is to achieve **WYSIWYG (What You See Is What You Get)** — UI designed with the Visual Studio Windows Forms Designer on Windows will render with high fidelity on Linux (arm / arm64 / x64) and other embedded platforms.

> ⚠️ This project is currently in the experimental phase and is not yet suitable for production use.

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

![Preview 1](./preview/1.png)

![Preview 2](./preview/2.png)

---

## 📦 NuGet Packages

| Package | Description |
|---------|-------------|
| `LVGLSharp.Forms` | WinForms API compatibility layer (core package) |
| `LVGLSharp.Interop` | LVGL P/Invoke bindings (auto-generated) |
| `LVGLSharp.Native` | Platform-native LVGL libraries (win-x64, linux-arm, etc.) |

---

## 🚀 Quick Start

### 1. Create a Project

Create a Windows Forms App (.NET) in Visual Studio, then replace the `System.Windows.Forms` reference with `LVGLSharp.Forms`.

### 2. Entry Point

```csharp
using LVGLSharp.Forms;

Application.SetHighDpiMode(HighDpiMode.SystemAware);
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.Run(new frmMain());
```

### 3. Run on Linux

Publish using NativeAOT:

```bash
dotnet publish -r linux-arm64 -c Release
```

---

## 🏗️ Project Structure

```
src/
├── LVGLSharp.WinForms/     # WinForms API compatibility layer (core)
│   ├── Forms/              # Control implementations (Control, Form, Button, etc.)
│   ├── Darwing/            # Cross-platform drawing types (Size, Point, Color, etc.)
│   └── Runtime/            # Platform runtime (Windows / Linux)
├── LVGLSharp.Interop/      # LVGL P/Invoke auto-generated bindings
├── LVGLSharp.Native/       # Platform-native libraries
├── LVGLSharp.Core/         # Shared core library
└── Demos/
    └── WinFormsDemo/       # Demo application
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

## 📄 License

This project is licensed under the [MIT License](./LICENSE.txt).
