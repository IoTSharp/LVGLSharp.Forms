# LVGLSharp

中文 | [English](./README_en.md)

**LVGLSharp** 是一个基于 [LVGL](https://github.com/lvgl/lvgl) 的跨平台 WinForms API 兼容层，核心包为 `LVGLSharp.Forms`。目标是尽量保留熟悉的 Windows Forms 开发方式，让同一套界面继续运行在 Windows、Linux 与嵌入式场景中。

当前稳定基线：`9.5.0.5`

- 文档：<https://lvglsharp.net/>
- 路线图：<https://github.com/IoTSharp/LVGLSharp/blob/main/ROADMAP.md>
- 变更日志：<https://github.com/IoTSharp/LVGLSharp/blob/main/CHANGELOG.md>

> 项目仍在快速演进中，暂不建议直接用于生产环境。

## 特性

- WinForms API 兼容：`Control`、`Form`、`Button`、`Label`、`TextBox`、`CheckBox`、`RadioButton`、`ComboBox`、`ListBox`、`PictureBox`、`Panel`、`GroupBox`、`FlowLayoutPanel`、`TableLayoutPanel`、`ProgressBar`、`TrackBar`、`NumericUpDown`、`RichTextBox` 等常用控件已具备可用基础。
- LVGL 全量互操作：`LVGLSharp.Interop` 通过 ClangSharpPInvokeGenerator 自动生成 P/Invoke 绑定，可直接访问 LVGL C API。
- 跨平台运行时：当前提供 Windows、Linux 与 Headless 路径，Linux 侧已包含 `WSLg`、`X11`、`Wayland`、`SDL`、`FrameBuffer` 宿主；`DRM/KMS`、`macOS` 与 Remote 路径正在继续补齐。
- NativeAOT 与原生库分发：支持 NativeAOT 发布，并通过 `LVGLSharp.Native` 提供多 RID 原生库包。
- 自动运行时注册：引用运行时包后，`ApplicationConfiguration.Initialize()` 会通过 `buildTransitive` 完成平台注册。
- 无头渲染与快照：`LVGLSharp.Runtime.Headless` 提供 `OffscreenView`、PNG 导出与快照回归测试入口，适合截图、自动化验证和远程帧源复用。
- 跨平台绘图抽象：`LVGLSharp.Drawing` 提供 `Size`、`Point`、`Color` 等类型，不依赖 `System.Drawing`。

## 预览

以下预览图来自 `docs/images` 中已收录的部分运行效果截图。

<p align="center">
  <img src="./docs/images/x11-pictureboxdemo.png" alt="LVGLSharp X11 PictureBoxDemo" width="48%" />
  <img src="./docs/images/x11-musicdemo.png" alt="LVGLSharp X11 MusicDemo" width="48%" />
</p>

<p align="center">
  <img src="./docs/images/x11-smartwatchdemo.png" alt="LVGLSharp X11 SmartWatchDemo" width="48%" />
  <img src="./docs/images/wslg-pictureboxdemo-wayland-embedded-font-check.png" alt="LVGLSharp WSLg Wayland Embedded Font Check" width="48%" />
</p>

<p align="center">
  <img src="./docs/images/winformsvncdemo-vnc-case.png" alt="LVGLSharp WinFormsVncDemo over VNC" width="48%" />
</p>

## NuGet 包

| NuGet名称 | 版本 | 下载量 | 说明 |
|---|---|---|---|
| `LVGLSharp.Forms` | [![LVGLSharp.Forms](https://img.shields.io/nuget/v/LVGLSharp.Forms.svg)](https://www.nuget.org/packages/LVGLSharp.Forms/) | ![NuGet](https://img.shields.io/nuget/dt/LVGLSharp.Forms) | WinForms API 兼容层与 `buildTransitive` 集成入口。 |
| `LVGLSharp.Core` | [![LVGLSharp.Core](https://img.shields.io/nuget/v/LVGLSharp.Core.svg)](https://www.nuget.org/packages/LVGLSharp.Core/) | ![NuGet](https://img.shields.io/nuget/dt/LVGLSharp.Core) | 共享运行时抽象、字体与公共辅助能力。 |
| `LVGLSharp.Interop` | [![LVGLSharp.Interop](https://img.shields.io/nuget/v/LVGLSharp.Interop.svg)](https://www.nuget.org/packages/LVGLSharp.Interop/) | ![NuGet](https://img.shields.io/nuget/dt/LVGLSharp.Interop) | LVGL P/Invoke 绑定。 |
| `LVGLSharp.Native` | [![LVGLSharp.Native](https://img.shields.io/nuget/v/LVGLSharp.Native.svg)](https://www.nuget.org/packages/LVGLSharp.Native/) | ![NuGet](https://img.shields.io/nuget/dt/LVGLSharp.Native) | 多 RID 原生库分发包。 |
| `LVGLSharp.Runtime.Windows` | [![LVGLSharp.Runtime.Windows](https://img.shields.io/nuget/v/LVGLSharp.Runtime.Windows.svg)](https://www.nuget.org/packages/LVGLSharp.Runtime.Windows/) | ![NuGet](https://img.shields.io/nuget/dt/LVGLSharp.Runtime.Windows) | Windows 运行时。 |
| `LVGLSharp.Runtime.Linux` | [![LVGLSharp.Runtime.Linux](https://img.shields.io/nuget/v/LVGLSharp.Runtime.Linux.svg)](https://www.nuget.org/packages/LVGLSharp.Runtime.Linux/) | ![NuGet](https://img.shields.io/nuget/dt/LVGLSharp.Runtime.Linux) | Linux 运行时。 |
| `LVGLSharp.Runtime.Headless` | 待发布 | - | 无头渲染、截图与自动化验证运行时。 |
| `LVGLSharp.Runtime.MacOs` | 待发布 | - | macOS 运行时骨架与诊断结构。 |
| `LVGLSharp.Runtime.Remote` | 待发布 | - | Remote 抽象层与 `VNC` / `RDP` 骨架。 |

当前已发布到 NuGet 的是前 6 个包；`LVGLSharp.Runtime.Headless`、`LVGLSharp.Runtime.MacOs` 与 `LVGLSharp.Runtime.Remote` 目前仍以仓库内工程为主，尚未单独发布。

`LVGLSharp.Forms` 包内已携带分析器。一般应用只需要选择 `LVGLSharp.Forms` 和目标运行时包；`LVGLSharp.Native` 通常会由依赖链自动带入。

## 快速开始

### 1. 项目文件

推荐使用多目标框架，把 WinForms 设计器路径与 `LVGLSharp.Forms` 路径放在同一个工程里：

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

<ItemGroup Condition="'$(TargetFramework)' == 'net10.0'">
  <PackageReference Include="LVGLSharp.Forms" Version="9.5.0.5" />
  <PackageReference Include="LVGLSharp.Runtime.Windows" Version="9.5.0.5" />
  <PackageReference Include="LVGLSharp.Runtime.Linux" Version="9.5.0.5" />
</ItemGroup>
```

如果需要无头渲染、截图或自动化验证，再额外引用 `LVGLSharp.Runtime.Headless`。

### 2. 入口程序

`UseLVGLSharpForms=true` 的目标只需要正常调用 `ApplicationConfiguration.Initialize()`：

```csharp
ApplicationConfiguration.Initialize();
Application.Run(new frmMain());
```

### 3. 发布示例

```bash
dotnet publish -f net10.0 -r linux-arm64 -c Release
dotnet publish -f net10.0 -r linux-x64 -c Release
dotnet publish -f net10.0-windows -r win-x64 -c Release
```

## 当前状态

| 方向 | 状态 | 说明 |
|---|---|---|
| WinForms API 兼容层 | 可用 | 核心控件、`Form` 生命周期与基础布局模式已经可用 |
| Windows 运行时 | 可用 | 当前稳定路径之一 |
| Linux `WSLg` / `X11` | 可用 | 当前桌面侧主路径 |
| Linux `FrameBuffer` | 可用 | 当前设备侧主路径 |
| Linux `Wayland` / `SDL` | 实验性 | 已实现，仍需更多验证与发布纪律 |
| Headless `Offscreen` | 可用 | 已支持 PNG 快照、截图与回归测试入口 |
| Linux `DRM/KMS` | 骨架中 | `DrmView` 已预留，原生后端待补齐 |
| macOS 运行时 | 骨架中 | 诊断、上下文与 surface 骨架已在仓库内 |
| Remote 运行时 | 骨架中 | `VNC` / `RDP` 抽象与 skeleton 已落仓 |

更完整的状态和下一阶段优先级见路线图。

## 交流

欢迎通过文档站、Issue 或微信群交流项目使用、跨平台适配与问题排查经验。

![LVGLSharp 微信交流群](./preview/wechat-group.png)




## 许可证

本项目基于 [MIT License](./LICENSE.txt) 开源。
