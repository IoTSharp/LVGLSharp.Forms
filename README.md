# LVGLSharp.Forms

中文 | [English](./README_en.md)

**LVGLSharp.Forms** 是一个跨平台的 WinForms API 兼容层，以 [LVGL](https://github.com/lvgl/lvgl) 作为底层渲染引擎。目标是实现**所见即所得**——在 Visual Studio Windows Forms 设计器中设计的界面，可以在 Linux（arm / arm64 / x64）等嵌入式平台上以高度一致的效果运行。

> ⚠️ 项目目前处于试验阶段，尚不可用于生产环境。

---

## ✨ 特性

- 🖥️ **WinForms API 兼容**：使用与 `System.Windows.Forms` 高度相似的 API，轻松迁移现有代码。
- 🔤 **LVGL 全 API 互操作**：基于 ClangSharpPInvokeGenerator 自动生成的 P/Invoke 绑定，覆盖 LVGL 全部 C API。
- 🚀 **NativeAOT 支持**：支持发布为无依赖的原生可执行文件（已验证 win-x64 / linux-arm）。
- 🌍 **跨平台**：支持 Windows（x64）、Linux（x64 / arm / arm64）。
- 🧩 **内置常用控件**：Button、Label、TextBox、CheckBox、RadioButton、ComboBox、ListBox、ProgressBar、TrackBar、NumericUpDown、PictureBox、Panel、GroupBox、FlowLayoutPanel、TableLayoutPanel、RichTextBox 等。
- 🎨 **自定义绘图类型**：提供 `LVGLSharp.Darwing` 命名空间下的 `Size`、`Point`、`Color` 等类型，无需依赖 `System.Drawing`，天然跨平台。

---

## 📷 预览

以下为经过 NativeAOT 发布的 win-x64 / linux-arm 应用程序预览（无任何额外依赖）：

![预览图 1](./preview/1.png)

![预览图 2](./preview/2.png)

---

## 📦 NuGet 包

| 包名 | 说明 |
|------|------|
| `LVGLSharp.Forms` | WinForms API 兼容层（核心包） |
| `LVGLSharp.Interop` | LVGL P/Invoke 绑定（自动生成） |
| `LVGLSharp.Native` | 各平台 LVGL 原生库（win-x64、linux-arm 等） |

---

## 🚀 快速开始

### 1. 创建项目

使用 Visual Studio 创建 Windows Forms 应用程序（.NET），然后将 `System.Windows.Forms` 的引用替换为 `LVGLSharp.Forms`。

### 2. 入口程序

```csharp
using LVGLSharp.Forms;

Application.SetHighDpiMode(HighDpiMode.SystemAware);
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.Run(new frmMain());
```

### 3. 在 Linux 上运行

使用 NativeAOT 发布：

```bash
dotnet publish -r linux-arm64 -c Release
```

---

## 🏗️ 项目结构

```
src/
├── LVGLSharp.WinForms/     # WinForms API 兼容层（核心）
│   ├── Forms/              # 控件实现（Control、Form、Button 等）
│   ├── Darwing/            # 跨平台绘图类型（Size、Point、Color 等）
│   └── Runtime/            # 平台运行时（Windows / Linux）
├── LVGLSharp.Interop/      # LVGL P/Invoke 自动生成绑定
├── LVGLSharp.Native/       # 各平台原生库
├── LVGLSharp.Core/         # 公共核心库
└── Demos/
    └── WinFormsDemo/       # 演示项目
libs/
└── lvgl/                   # LVGL 源码（submodule）
```

---

## 🙏 致谢

- **[imxcstar / LVGLSharp](https://github.com/imxcstar/LVGLSharp)**：提供了最底层的 LVGL .NET 封装支撑，本项目基于此构建。
- **[LVGL](https://github.com/lvgl/lvgl)**：轻量级、高性能的嵌入式 GUI 库。
- **[ClangSharpPInvokeGenerator](https://github.com/dotnet/ClangSharp)**：用于自动生成 LVGL 全量 P/Invoke 绑定。
- **[SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)**：跨平台图像处理库。
- **[SixLabors.Fonts](https://github.com/SixLabors/Fonts)**：跨平台字体解析库。

---

## 📄 许可证

本项目基于 [MIT License](./LICENSE.txt) 开源。
