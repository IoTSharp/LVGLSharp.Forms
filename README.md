# LVGLSharp

中文 | [English](./README_en.md)

**LVGLSharp** 是一个跨平台的 WinForms API 兼容层项目，以 [LVGL](https://github.com/lvgl/lvgl) 作为底层渲染引擎。当前核心 UI 兼容层仍然是 `LVGLSharp.Forms`，目标是实现**所见即所得**——在 Visual Studio Windows Forms 设计器中设计的界面，可以在 Linux（arm / arm64 / x64）等嵌入式平台上以高度一致的效果运行。

📘 文档站点（GitHub Pages）：

- GitHub 仓库：`https://github.com/IoTSharp/LVGLSharp`
- 正式站点：`https://lvglsharp.net/`
- 备用镜像：`https://gtiee.com/IoTSharp/LVGLSharp`

- 中文首页：`docs/index.md`
- 英文首页：`docs/index.en.md`
- CI 文档：`docs/ci-workflows.md`
- 英文 CI 文档：`docs/ci-workflows.en.md`
- 中文导航：`docs/navigation.md`
- 英文导航：`docs/navigation.en.md`
- 中文博客索引：`docs/blog/index.md`
- 英文博客索引：`docs/blog/index.en.md`

站点访问建议：

- 中文首页：<https://lvglsharp.net/>
- 英文首页：<https://lvglsharp.net/index.en.html>

本地预览建议：

- 本地预览基于 Jekyll，与 GitHub Pages 保持一致。
- 首次使用前，请先安装 Ruby 与 Bundler，并在仓库根目录执行 `bundle install`。
- 可运行 `preview/preview-pages.ps1` 在本地生成并预览 Pages HTML。
- 默认地址为 `http://127.0.0.1:4000/`。
- 如只想生成 `_site` 而不启动预览，可使用 `preview/preview-pages.ps1 -NoServe`。
- 在 VS Code 中可直接运行任务：`Preview Pages HTML` 或 `Build Pages HTML Only`。

> ⚠️ 项目目前处于试验阶段，尚不可用于生产环境。

---

## 📢 当前发布版本

- **版本号**：`9.5.0.5`
- **发布 Tag**：`v9.5.0.5`
- **发布定位**：自初始版本演进而来的首个完整文档化发布，统一整理当前能力、包结构与发布说明。

### 9.5.0.5 发布摘要

- 延续初始版本中基于 LVGL 的 WinForms API 兼容层方向，补齐核心控件、运行时宿主与打包说明，并统一切换到 `LVGLSharp` 仓库名。
- 明确 `LVGLSharp.Forms`、`LVGLSharp.Core`、平台运行时包与 `LVGLSharp.Native` 的职责划分。
- 补充 `Application.Run(Form)` 生命周期支持、LVGL 事件桥接与 NativeAOT 发布能力说明。
- 将当前稳定可对外说明的能力同步到 [`CHANGELOG.md`](./CHANGELOG.md)，便于后续基于 tag 自动生成发布记录。

如需查看从初始版本开始的完整发布记录，请参考 [`CHANGELOG.md`](./CHANGELOG.md)。

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
| `LVGLSharp.Runtime.Windows` | Windows 平台运行时 |
| `LVGLSharp.Runtime.Linux` | Linux 平台运行时 |
| `LVGLSharp.Interop` | LVGL P/Invoke 绑定（自动生成） |
| `LVGLSharp.Native` | 各平台 LVGL 原生库（win-x86 / win-x64 / win-arm64、linux-arm 等） |

---

## 🚀 快速开始

### 1. 创建项目

 推荐按仓库中的示例采用多目标框架方式：

- `net10.0-windows` 目标使用 `UseWindowsForms=true`，只走标准 WinForms 路径
- `net10.0` 目标使用 `UseLVGLSharpForms=true`，走 `LVGLSharp.Forms` 路径

是否使用 WinForms 还是 `LVGLSharp.Forms`，只由 `UseWindowsForms` 与 `UseLVGLSharpForms` 决定，不通过 `WINDOWS` 这类 OS 符号判断。

典型工程文件配置如下：

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

在 `UseLVGLSharpForms=true` 的目标下：

- 引用 `LVGLSharp.Forms`
- 引用 `LVGLSharp.Runtime.Windows`
- 引用 `LVGLSharp.Runtime.Linux`
- `buildTransitive` 会根据已引用的运行时包自动生成对应平台的注册代码，并在 `ApplicationConfiguration.Initialize()` 中完成运行时初始化
- Linux 侧当前默认宿主仍以 `WSLg`、`X11`、`Wayland`、`SDL`、`FrameBuffer` 为主；同时已加入 `DRM/KMS` 与 `Offscreen` 的首批骨架入口，供后续逐步实现与验证

可参考示例工程：[`src/Demos/PictureBoxDemo/PictureBoxDemo.csproj`](./src/Demos/PictureBoxDemo/PictureBoxDemo.csproj)。

### 2. 入口程序

 `UseWindowsForms=true` 的目标无需任何 `LVGLSharp` 运行时注册。

 `UseLVGLSharpForms=true` 的目标只需要正常调用 `ApplicationConfiguration.Initialize()`；如果已引用 `LVGLSharp.Runtime.Windows`、`LVGLSharp.Runtime.Linux`，运行时会自动注册：

```csharp
ApplicationConfiguration.Initialize();

Application.Run(new frmMain());
```

如果只引用 Windows 运行时，则只生成 Windows 注册；如果只引用 Linux 运行时，则只生成 Linux 注册；如果两者都引用，则会在启动时按当前平台自动选择对应的 `LVGL` 宿主与图片加载实现。

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

Windows 目标发布示例：

```powershell
dotnet publish -f net10.0-windows -r win-x64 -c Release
```

Linux / `LVGLSharp.Forms` 目标发布示例：

```bash
dotnet publish -f net10.0 -r linux-x64 -c Release
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
├── LVGLSharp.Runtime.Windows/ # Windows 平台运行时
├── LVGLSharp.Runtime.Linux/# Linux 平台运行时
├── LVGLSharp.Runtime.MacOs/# MacOs 平台运行时骨架
├── LVGLSharp.Runtime.Remote/# 跨平台远程运行时抽象骨架
├── LVGLSharp.Interop/      # LVGL P/Invoke 自动生成绑定
├── LVGLSharp.Native/       # 各平台原生库
└── Demos/
    ├── WinFormsDemo/       # 基础 WinForms / LVGLSharp.Forms 对照演示
    ├── PictureBoxDemo/     # PictureBox 控件演示
    ├── MusicDemo/          # MusicDemo 演示项目
  ├── OffscreenDemo/      # Offscreen 无头渲染与 PNG 输出演示
    ├── SmartWatchDemo/     # SmartWatch 界面演示
    └── SerialPort/         # SerialPort 演示项目
libs/
└── lvgl/                   # LVGL 源码（submodule）
```

---

## 📚 开发文档

- [`ROADMAP.md`](./ROADMAP.md)：汇总当前已完成的里程碑、各运行时路径状态与下一阶段建议优先项。
- [`docs/WSL-Developer-Guide.md`](./docs/WSL-Developer-Guide.md)：`WSL2/WSLg` 下运行、验证与调试 demo 的开发者手册。
- [`docs/navigation.md`](./docs/navigation.md)：文档站点导航页，可快速跳转到首页、专题文档、博客和更新记录。

当前 Linux 宿主选择补充说明：

- 当前 `LinuxView` 会自动探测并选择宿主；后续会进一步收敛到统一的显式选项对象。
- 目前已识别值包括：`wslg`、`wayland`、`x11`、`sdl`、`framebuffer`、`drm` / `kms`、`offscreen`。
- 其中 `drm` / `kms` 与 `offscreen` 当前为骨架入口，尚未完成原生后端实现。

当前新增独立示例：

- `src/Demos/OffscreenDemo/OffscreenDemo.csproj`：演示 `OffscreenView` 的无头渲染与 PNG 导出。
- 可通过命令行参数显式指定输出文件路径、宽高与 DPI。

当前新增骨架能力：

- `src/LVGLSharp.Runtime.MacOs/`：macOS 运行时骨架。
- `src/LVGLSharp.Runtime.Remote/`：跨平台远程运行时抽象骨架，用于后续 `VNC` / `RDP`。

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
