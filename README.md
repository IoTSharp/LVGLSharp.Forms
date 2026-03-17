# LVGLSharp.Forms

中文 | [English](./README_en.md)

**LVGLSharp.Forms** 是一个跨平台的 WinForms API 兼容层，以 [LVGL](https://github.com/lvgl/lvgl) 作为底层渲染引擎。目标是实现**所见即所得**——在 Visual Studio Windows Forms 设计器中设计的界面，可以在 Linux（arm / arm64 / x64）等嵌入式平台上以高度一致的效果运行。

> ⚠️ 项目目前处于试验阶段，尚不可用于生产环境。

---

## ✨ 特性

- 🖥️ **WinForms API 兼容**：使用与 `System.Windows.Forms` 高度相似的 API，轻松迁移现有代码。
- 🔤 **LVGL 全 API 互操作**：基于 ClangSharpPInvokeGenerator 自动生成的 P/Invoke 绑定，覆盖 LVGL 全部 C API。
- 🚀 **NativeAOT 支持**：支持发布为无依赖的原生可执行文件（已验证 win-x64 / linux-arm）。
- 🌍 **跨平台**：支持 Windows（x86 / x64 / arm64）、Linux（x64 / arm / arm64）。
- 🧩 **内置常用控件**：Button、Label、TextBox、CheckBox、RadioButton、ComboBox、ListBox、ProgressBar、TrackBar、NumericUpDown、PictureBox、Panel、GroupBox、FlowLayoutPanel、TableLayoutPanel、RichTextBox 等。
- 🎨 **自定义绘图类型**：提供 `LVGLSharp.Drawing` 命名空间下的 `Size`、`Point`、`Color` 等类型，无需依赖 `System.Drawing`，天然跨平台。

---

## 📷 预览

以下为经过 NativeAOT 发布的 win-x64 / linux-arm 应用程序预览（无任何额外依赖）：

<iframe src="//player.bilibili.com/player.html?isOutside=true&aid=116237182962589&bvid=BV12Fwjz5EuZ&cid=36733586714&p=1" scrolling="no" border="0" frameborder="no" framespacing="0" allowfullscreen="true"></iframe>

---

## 📦 NuGet 包

| 包名 | 说明 |
|------|------|
| `LVGLSharp.Forms` | WinForms API 兼容层 |
| `LVGLSharp.Core` | 运行时共享抽象与公共字体/宿主辅助能力 |
| `LVGLSharp.Runtime.Windows` | Windows 平台运行时；引用后自动注册 Windows 宿主 |
| `LVGLSharp.Runtime.Linux` | Linux 平台运行时；引用后自动注册 Linux 宿主 |
| `LVGLSharp.Interop` | LVGL P/Invoke 绑定（自动生成） |
| `LVGLSharp.Native` | 各平台 LVGL 原生库（win-x86 / win-x64 / win-arm64、linux-arm 等） |

---

## 🚀 快速开始

### 1. 创建项目

 推荐按仓库中的示例采用多目标框架的方式：使用 Visual Studio 创建 Windows Forms 应用程序（.NET），以 `net10.0-windows` 目标启用设计器（`UseWindowsForms=true`），再增加一个纯 `net10.0` 目标用于跨平台运行。

 在跨平台运行目标下：

 - 始终引用 `LVGLSharp.Forms`
 - Windows 项目再引用 `LVGLSharp.Runtime.Windows`
 - Linux 项目再引用 `LVGLSharp.Runtime.Linux`

 对应平台运行时包被引用后，会自动完成宿主注册；如果没有引用任何平台运行时包，编译期会收到提示。可参考示例工程的配置：[`src/Demos/WinFormsDemo/WinFormsDemo.csproj`](./src/Demos/WinFormsDemo/WinFormsDemo.csproj)。

### 2. 入口程序

 引用了对应平台运行时包后，入口程序无需手动注册运行时：

```csharp
using LVGLSharp.Forms;

Application.SetHighDpiMode(HighDpiMode.SystemAware);
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.Run(new frmMain());
```

### 3. `LVGLSharp 布局`

`LVGLSharp 布局` 是本仓库推荐的界面组织方式，用于让 LVGL 后端在不同平台上获得更稳定、一致的控件排布效果。

其核心约束如下：

- 外层使用一个 `TableLayoutPanel` 只做纵向分区。
- 每一行放一个 `FlowLayoutPanel`，由该行的 `FlowLayoutPanel` 承载实际业务控件。
- 主 `TableLayoutPanel` 使用固定绝对行高，不使用百分比行高。
- 不要把 `Button`、`TextBox`、`ComboBox` 等业务控件直接挂到主 `TableLayoutPanel` 上。

它与 WinForms 中常见布局方式的区别在于：

- 在传统 WinForms 中，控件通常可以直接放进 `TableLayoutPanel` 单元格，设计器也经常使用百分比行高或列宽。
- 在 `LVGLSharp 布局` 中，主 `TableLayoutPanel` 只负责分区，不直接承载业务控件；实际控件应放入每行的 `FlowLayoutPanel`。
- 在传统 WinForms 中，布局更依赖设计器自动生成的网格参数；而在 `LVGLSharp 布局` 中，强调可预测的固定行高与行内容器，以降低不同平台和不同字体度量下的排布偏差。

如果你的窗体在 Windows 设计器中看起来正常，但迁移到 LVGL 运行时后出现控件重叠、裁剪或行高不稳定，优先检查是否遵循了 `LVGLSharp 布局`。

### 4. 在 Linux 上运行

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
│   ├── Drawing/            # 跨平台绘图类型（Size、Point、Color 等）
│   └── Runtime/            # 公共运行时注册入口与共享胶水代码
├── LVGLSharp.Core/         # 公共核心库
├── LVGLSharp.Windows/      # Windows 平台运行时
├── LVGLSharp.Runtime.Linux/# Linux 平台运行时
├── LVGLSharp.Interop/      # LVGL P/Invoke 自动生成绑定
├── LVGLSharp.Native/       # 各平台原生库
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

## 💬 交流

欢迎加入微信群，与我们交流项目使用、跨平台适配、控件实现与问题排查经验。

如果你对 LVGLSharp.Forms 感兴趣，欢迎扫码加入微信群交流。

![LVGLSharp 微信交流群](./preview/wechat-group.png)

---

## 📄 许可证

本项目基于 [MIT License](./LICENSE.txt) 开源。
