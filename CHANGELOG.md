# Changelog

本文件记录 LVGLSharp.Forms 的所有重要变更，遵循 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/) 规范，版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。

All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

---

## [未发布 / Unreleased]

### 新增 / Added
- WinForms API 兼容层核心框架，基于 LVGL 渲染引擎。
- 支持控件：`Button`、`Label`、`TextBox`、`CheckBox`、`RadioButton`、`ComboBox`、`ListBox`、`ProgressBar`、`TrackBar`、`NumericUpDown`、`PictureBox`、`Panel`、`GroupBox`、`FlowLayoutPanel`、`TableLayoutPanel`、`RichTextBox`。
- `LVGLSharp.Darwing` 命名空间：跨平台绘图类型 `Size`、`Point`、`Color` 等，不依赖 `System.Drawing`。
- NativeAOT 支持（win-x64、linux-arm、linux-arm64、linux-x64）。
- 基于 ClangSharpPInvokeGenerator 自动生成的 LVGL 全量 P/Invoke 绑定（`LVGLSharp.Interop`）。
- 平台原生库分发包 `LVGLSharp.Native`，支持 win-x64、win-x86、win-arm64、linux-x64、linux-arm、linux-arm64。
- LVGL GCHandle 事件桥接机制：通过 `[UnmanagedCallersOnly]` 静态回调将 LVGL 事件路由至托管控件事件。
- `Application.Run(Form)` 生命周期支持。
- WinFormsDemo 演示项目。

### 变更 / Changed
- 无。

### 修复 / Fixed
- 无。

---

## [9.3.0] - 初始版本 / Initial Release

### 新增 / Added
- 项目初始化，基于 [imxcstar/LVGLSharp](https://github.com/imxcstar/LVGLSharp) 构建底层 LVGL .NET 封装。
- 引入 LVGL 9.3 作为 git submodule。
- 基础 `Control` 与 `Form` 类实现，支持 `Controls` 层级管理及 LVGL 对象创建。
- 初步验证 NativeAOT 发布流程（win-x64、linux-arm）。

---

## 致谢 / Acknowledgements

- **[imxcstar / LVGLSharp](https://github.com/imxcstar/LVGLSharp)**：提供了最底层的 LVGL .NET 封装支撑。
- **[LVGL](https://github.com/lvgl/lvgl)**：轻量级、高性能的嵌入式 GUI 库。
- **[ClangSharpPInvokeGenerator](https://github.com/dotnet/ClangSharp)**：用于自动生成 LVGL P/Invoke 绑定。
- **[SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)**：跨平台图像处理库。
- **[SixLabors.Fonts](https://github.com/SixLabors/Fonts)**：跨平台字体解析库。
