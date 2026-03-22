# 运行时路线图

本文统一整理 `LVGLSharp.Forms` 相关运行时的当前状态、后续宿主扩展方向，以及建议实施顺序。

## 目标

保持 `LVGLSharp.Forms` 外部 API 稳定，将平台差异尽量收敛到各自的运行时项目中：

- `LVGLSharp.Runtime.Windows`
- `LVGLSharp.Runtime.Linux`
- `LVGLSharp.Runtime.MacOs`（规划）
- 独立远程运行时（规划，用于 `VNC`、`RDP` 等远程方案）

总体原则：

- `Forms` 层不直接感知具体平台宿主差异
- 统一通过运行时注册机制接入 `IView`
- 平台探测与具体宿主实现解耦
- 新增宿主时尽量不影响现有外部调用方式

## 当前状态

### 已有运行时

- `LVGLSharp.Runtime.Windows`
- `LVGLSharp.Runtime.Linux`

### Linux 当前已支持宿主

当前 `LinuxView` 统一作为 Linux 运行时入口，并在内部按环境路由到不同 `IView` 实现：

- `WslgView`
- `X11View`
- `FrameBufferView`

### 当前开发进展

- `WaylandView`
  - 已完成第一阶段的初步交付
  - 当前已具备基础窗口/渲染面、`lv_display` 初始化、基础输入接线与主循环骨架
  - 仍应继续围绕稳定性、输入细节、缓冲提交与环境覆盖做增强
- `SdlView`
  - 作为下一步实施重点开始推进
  - 首版目标仍按“最小可交付版本”落地，优先满足独立窗口、基础渲染、基础输入与 demo 跑通

### `LVGLSharp.Forms` 兼容层收尾事项

- 键盘消息兼容链已完成第一阶段打通：
  - `PreProcessControlMessage`
  - `PreviewKeyDown`
  - `ProcessKeyMessage`
  - `ProcessDialogKey` 第一版
- 后续仍需继续收尾：
  - `ProcessCmdKey` 第一版（常用快捷键与命令键消费语义）
  - `ProcessDialogKey` 增强（`Enter` / `Escape` / 默认按钮 / 取消按钮 / 更完整焦点导航）
  - `Focus` / `Focused` / `ContainsFocus` / `SelectNextControl` 与 LVGL `group/focus` 模型正式对齐
  - `TextBox` 与 `Control` 键盘处理职责继续收敛，减少重复消费
  - `Paint` / `Invalidate` / `Refresh` / `OnPrint` 的 LVGL 兼容策略评估与分层实现
  - `Message` / `KeyEventArgs` / `KeyPressEventArgs` / `PreviewKeyDownEventArgs` 继续向 WinForms 语义补齐

当前环境探测由 `LinuxEnvironmentDetector` 负责，`LinuxView` 只负责路由。

### 已支持 Linux 宿主说明

#### `WslgView`
- 目标场景：`WSL2 + WSLg`
- 当前实现：继承 `X11View`
- 当前价值：
  - 为 `WSLg` 提供单独扩展点
  - 提供更明确的标题与诊断信息

#### `X11View`
- 目标场景：传统 Linux 桌面环境、`XWayland` 路径
- 当前价值：
  - 当前桌面 Linux 兼容路径最成熟
  - 适合 demo、开发调试与过渡兼容

#### `FrameBufferView`
- 目标场景：`fbdev` 设备、极简环境、旧设备
- 当前价值：
  - 提供无桌面环境下的基础兼容路径
  - 可继续保留作为历史兼容方案

## 统一实施顺序

后续规划按下面顺序逐个推进：

1. `WaylandView`
2. `SdlView`
3. `DrmView`
4. `KmsView`
5. `OffscreenView`
6. `DirectFB`
7. `Mir`
8. `LVGLSharp.Runtime.MacOs`
9. 独立远程运行时（`VNC`、`RDP` 等）

> 当前执行状态：`WaylandView` 已进入“首版已落地、继续增强”阶段；实现重点切换到 `SdlView`。

## Linux 路线

### 分阶段最小可交付版本

为避免一次性把所有宿主做重，建议每个宿主先落一个最小可交付版本，再逐步增强。

#### `WaylandView` 最小可交付版本
- 能创建原生窗口或可见渲染面
- 能完成 `lv_display` 初始化与基础 flush
- 能接入鼠标、键盘基础输入
- 能跑通 `Form.Show()`、`Application.Run()`、基础 demo
- 首版不强求复杂输入法、剪贴板、窗口高级特性

#### `SdlView` 最小可交付版本
- 能基于 `SDL` 提供独立窗口
- 能映射 LVGL 渲染缓冲到窗口表面
- 能接入基础鼠标、键盘、滚轮事件
- 能作为跨平台调试宿主运行基础 demo

当前实施补充说明：

- 第一版优先在 `LVGLSharp.Runtime.Linux` 中提供 `SdlView`
- 第一版以单宿主闭环为主，先保证 `Form.Show()`、`Application.Run()`、基础交互可用
- 在首版验证稳定前，不强行替换现有默认 Linux 宿主优先级
- 当前可通过环境变量显式启用 SDL 宿主：`LVGLSHARP_LINUX_HOST=sdl` 或 `LVGLSHARP_USE_SDL=1`

#### `DrmView` 最小可交付版本
- 能在无桌面环境下完成显示初始化
- 能完成全屏渲染输出
- 能接入至少一种基础输入设备
- 能作为 `FrameBufferView` 之后的现代设备侧替代路径

#### `KmsView` 最小可交付版本
- 能完成模式设置或输出面选择
- 能与显示缓冲提交链路打通
- 能独立承担基础显示输出职责
- 首版先聚焦显示链路，不强求复杂热插拔能力

#### `OffscreenView` 最小可交付版本
- 能完成离屏 `lv_display` 初始化
- 能输出静态图像、截图或测试缓冲
- 能支撑基础快照测试或回归验证
- 首版不强求实时远程交互

#### `DirectFB` 最小可交付版本
- 能在目标旧环境成功创建显示面
- 能完成基础渲染与最小输入接入
- 能明确标注为兼容宿主，而不是主推宿主

#### `Mir` 最小可交付版本
- 能在目标环境完成基础窗口显示
- 能接入最小鼠标、键盘输入
- 能跑通基础 demo 验证宿主可用性

### 第一阶段：现代 Linux 桌面与调试宿主

#### 1. `WaylandView`
- 场景：现代 Linux 桌面主流路径
- 价值：
  - 面向 `GNOME`、`KDE` 与现代发行版
  - 减少长期只依赖 `X11/XWayland` 的风险
- 说明：
  - 这是 Linux 路线中的首要新增宿主

#### 2. `SdlView`
- 场景：开发调试、跨平台快速验证
- 价值：
  - 更利于开发阶段快速验证
  - 适合作为调试宿主、示例宿主与实验宿主
- 说明：
  - 优先级按统一顺序排在 `WaylandView` 之后
  - 当前已进入实现阶段

### 第二阶段：设备侧原生显示宿主

#### 3. `DrmView`
- 场景：嵌入式设备、全屏 kiosk、无桌面环境
- 价值：
  - 比 `FrameBufferView` 更现代
  - 更贴近设备端部署目标

#### 4. `KmsView`
- 场景：底层显示模式设置、设备侧显示输出控制
- 价值：
  - 作为 `DRM/KMS` 路线中的独立能力补齐
  - 便于后续按需分别演进 `DRM` 与 `KMS`

### 第三阶段：测试与补充兼容宿主

#### 5. `OffscreenView`
- 场景：自动化测试、截图、回归验证、离屏渲染
- 价值：
  - 不依赖真实桌面环境
  - 适合 `CI`、快照测试与渲染验证

#### 6. `DirectFB`
- 场景：旧图形栈兼容
- 价值：
  - 主要用于特定历史环境兼容
- 说明：
  - 技术较旧，但按当前统一顺序仍列入后续实现清单

#### 7. `Mir`
- 场景：特定 Linux 图形环境
- 价值：
  - 适用于较窄的特定场景
- 说明：
  - 生态覆盖有限，但仍纳入统一实施顺序

## macOS 路线

### 8. `LVGLSharp.Runtime.MacOs`

建议单独新建 `LVGLSharp.Runtime.MacOs` 项目，而不是把 macOS 逻辑并入现有 Windows 或 Linux 运行时。

建议目标：

- 提供 macOS 专属 `IView` 宿主实现
- 保持 `LVGLSharp.Forms` 调用方式不变
- 将 macOS 平台初始化、窗口管理、输入桥接收敛到独立运行时中

结构建议：

- `LVGLSharp.Runtime.MacOs`
  - `MacOsView`
  - macOS 平台初始化与注册入口
  - macOS 输入与渲染桥接实现

最小可交付版本建议：

- 能创建 macOS 原生窗口与基础渲染面
- 能跑通 `ApplicationConfiguration.Initialize()` 与 `Application.Run()`
- 能支持基础鼠标、键盘输入
- 能运行一个最小 demo 窗口
- 首版不强求高级文本输入、复杂窗口管理与系统集成能力

## 远程运行时路线

### 9. 独立远程运行时

在本地平台运行时之外，建议新增一个独立运行时项目，专门处理远程图形方案，而不是把远程能力直接耦合到 `LinuxView`、`X11View` 或后续 `MacOsView` 中。

建议覆盖方向：

- `VNC`
- `RDP`
- 后续其他远程显示协议

建议目标：

- 抽离远程显示、远程输入回传与会话管理能力
- 与本地窗口宿主分层，避免混淆本地宿主与远程宿主职责
- 为后续云端、容器化、无人值守环境提供扩展空间

建议结构：

- `LVGLSharp.Runtime.Remote`（命名可后续再定）
  - `VncView` 或对应远程宿主实现
  - `RdpView` 或对应远程宿主实现
  - 公共远程传输与输入桥接组件

最小可交付版本建议：

- 能把 LVGL 渲染结果编码并传到远端
- 能从远端接收最基础的鼠标、键盘输入
- 能以独立运行时形式接入，而不改 `Forms` 层公开 API
- 首版先验证单会话可行性，不强求多会话与高性能编解码优化

## 目录结构草案

建议后续运行时按统一模式组织目录，降低新增宿主时的理解成本。

### `LVGLSharp.Runtime.Linux`

- `LinuxView.cs`
- `LinuxEnvironmentDetector.cs`
- `Hosts/`
  - `WslgView.cs`
  - `X11View.cs`
  - `FrameBufferView.cs`
  - `WaylandView.cs`
  - `SdlView.cs`
  - `DrmView.cs`
  - `KmsView.cs`
  - `OffscreenView.cs`
  - `DirectFbView.cs`
  - `MirView.cs`
- `Interop/`
  - Wayland、SDL、DRM、KMS、Mir 等各自 `P/Invoke` 封装
- `Input/`
  - 鼠标、键盘、滚轮、文本输入桥接
- `Rendering/`
  - flush、缓冲、像素格式转换
- `Diagnostics/`
  - 环境探测、错误信息、诊断输出
- `Registration/`
  - Linux 运行时自动注册入口

### `LVGLSharp.Runtime.MacOs`

- `MacOsView.cs`
- `Interop/`
  - macOS 原生 API 封装
- `Input/`
  - 鼠标、键盘、文本输入桥接
- `Rendering/`
  - 窗口表面、缓冲提交、像素转换
- `Registration/`
  - macOS 运行时注册入口

### `LVGLSharp.Runtime.Remote`

- `RemoteViewBase.cs`
- `VncView.cs`
- `RdpView.cs`
- `Transport/`
  - 图像传输、会话管理、协议适配
- `Input/`
  - 远端输入事件回传与本地映射
- `Encoding/`
  - 图像编码、压缩、分块策略
- `Registration/`
  - 远程运行时注册入口

## `WaylandView` 第一阶段拆分清单

建议先把 `WaylandView` 分成几个明确层次，避免首版把窗口、输入、渲染、协议细节混在一个文件里。

### 核心类型

- `WaylandView`
  - 实现 `IView`
  - 负责生命周期、主循环、资源释放
- `WaylandDisplayConnection`
  - 负责 display、registry、global 对象绑定
- `WaylandWindow`
  - 负责 surface、toplevel、configure/close 等窗口职责
- `WaylandInputSource`
  - 负责 pointer、keyboard 基础事件桥接
- `WaylandBufferPresenter`
  - 负责渲染缓冲提交与 flush 对接

### 第一阶段接口边界

#### `WaylandView`
- 对外暴露 `IView` 所需成员
- 组合连接、窗口、输入、渲染组件
- 协调 `Init()`、`ProcessEvents()`、`StartLoop()`、`Stop()`

#### `WaylandDisplayConnection`
- 初始化 Wayland 连接
- 发现并绑定必要全局对象
- 提供事件轮询或调度能力

#### `WaylandWindow`
- 创建基础 surface
- 接收窗口配置事件
- 维护窗口尺寸与关闭状态

#### `WaylandInputSource`
- 映射 pointer 位置、按键状态、滚轮
- 映射 keyboard 键值到 LVGL 所需输入
- 首版先不处理复杂输入法

#### `WaylandBufferPresenter`
- 持有 LVGL 渲染缓冲
- 执行像素格式转换
- 把缓冲提交到 Wayland surface

### 第一阶段验收标准

- 能在支持 Wayland 的 Linux 环境显示窗口
- 能绘制基础 LVGL 界面
- 能响应鼠标与键盘基础输入
- 能关闭窗口并正确释放资源
- 能运行至少一个现有 demo 完成手工验证

## `SdlView` 第一阶段拆分清单

建议 `SdlView` 先以最小实现闭环为目标，首版允许先集中在较少文件中，验证通过后再按职责拆分。

### 核心类型

- `SdlView`
  - 实现 `IView`
  - 负责 SDL 生命周期、窗口、渲染提交、事件循环与资源释放
- 后续可按需要拆分为：
  - `SdlNative`
  - `SdlInputSource`
  - `SdlBufferPresenter`

### 第一阶段接口边界

#### `SdlView`
- 对外暴露 `IView` 所需成员
- 完成 `SDL` 初始化、窗口创建、纹理/渲染器准备
- 对接 `lv_display` flush 回调，把 LVGL 缓冲提交到 SDL 窗口
- 接收鼠标、键盘、滚轮与窗口关闭事件
- 协调 `Init()`、`ProcessEvents()`、`StartLoop()`、`Stop()`

### 第一阶段验收标准

- 能在安装 `SDL2` 的 Linux 环境创建独立窗口
- 能绘制基础 LVGL 界面
- 能响应鼠标、键盘、滚轮基础输入
- 能关闭窗口并正确释放 SDL / LVGL 资源
- 能运行至少一个现有 demo 完成手工验证

## 推荐结构

建议长期保持下面的总体结构：

- `LVGLSharp.Forms`：稳定的 WinForms API 兼容层
- `LVGLSharp.Runtime.Windows`：Windows 本地运行时
- `LVGLSharp.Runtime.Linux`：Linux 本地运行时与 Linux 宿主路由
- `LVGLSharp.Runtime.MacOs`：macOS 本地运行时
- `LVGLSharp.Runtime.Remote`：远程图形运行时

其中 Linux 运行时内部建议继续保持：

- `LinuxView`：统一入口与路由
- `LinuxEnvironmentDetector`：环境探测与诊断
- 具体宿主实现：
  - `WslgView`
  - `X11View`
  - `FrameBufferView`
  - `WaylandView`
  - `SdlView`
  - `DrmView`
  - `KmsView`
  - `OffscreenView`
  - `DirectFB`
  - `Mir`

## 规划原则

- 先补齐 Linux 宿主覆盖，再进入 macOS 运行时
- macOS 完成后，再单独推进远程运行时
- 当前已支持的 `WslgView`、`X11View`、`FrameBufferView` 继续保留
- 新宿主尽量通过新增运行时实现，而不是侵入 `Forms` 层
- 所有新增实现继续遵循统一注册、统一入口、统一路由的总体思路
