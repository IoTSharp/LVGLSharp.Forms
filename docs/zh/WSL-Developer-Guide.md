---
title: WSL 开发者手册
description: 说明如何在 WSL2 和 WSLg 中运行、发布和调试仓库里的 Linux 路径 demo。
lang: zh-CN
---

# WSL 开发者手册

本文面向本仓库开发者，说明如何在 `WSL2/WSLg` 中运行和调试 Linux 路径下的 demo。

如果你想同时了解当前已完成的工程范围和下一阶段优先事项，可以同时参考：[ROADMAP](https://github.com/IoTSharp/LVGLSharp/blob/main/ROADMAP.md) 和 [`navigation.md`](./navigation.md)。

## 先说结论

对本仓库当前最实用的工作流：

1. 在 Windows 上继续使用 `Visual Studio` 编辑代码
2. 使用仓库里的 Linux 发布脚本或 `dotnet publish` 产出 Linux 可执行文件
3. 在 `WSL2` 里运行发布结果
4. 如果需要 Linux 侧断点调试，优先使用 `VS Code + Remote WSL`

## 为什么这样建议

根据当前 Microsoft 官方文档：

- `Windows Subsystem for Linux` 的开发环境文档明确推荐：
  - `Visual Studio Code` 用于 WSL 远程开发与调试
  - `Visual Studio` 的原生 WSL 工作流主要聚焦在 `C++` 跨平台开发
- 对本仓库这种 `.NET + 自定义 Linux runtime` 的路径，最稳的方式仍然是：
  - Windows 侧编辑
  - WSL 侧运行
  - 需要 Linux 断点时切到 `VS Code Remote WSL`

所以，本手册不把“Visual Studio 直接 F5 调试 WSL 中的 .NET demo”作为主路径，而是提供更稳妥、可落地的开发方式。

## 环境准备

### 1. 安装 WSL2

先在 Windows 安装并初始化 WSL：

```powershell
wsl --install
```

安装完成后，建议至少准备一个 Ubuntu 发行版。

### 2. 确认 WSLg 可用

在 WSL 里执行：

```bash
echo $DISPLAY
echo $WAYLAND_DISPLAY
ls /mnt/wslg
```

如果下面任一项成立，通常表示 `WSLg` 可用：

- `DISPLAY` 有值
- `WAYLAND_DISPLAY` 有值
- `/mnt/wslg` 存在

### 3. 在 WSL 中安装 .NET SDK

仓库当前目标包含 `.NET 10`，请确保 WSL 里有对应 SDK：

```bash
dotnet --info
```

如果没有，请在 WSL 内按 .NET 官方安装方式安装对应 SDK。

### 3.1 WSL 下的 NuGet fallback 路径说明

如果你的 Windows 侧全局 NuGet 配置里带有仅 Windows 可见的 fallback package folder，WSL 中直接 `restore/publish` 可能会因为类似 `C:\Program Files\...` 的路径而失败。

本仓库现在提供了 `NuGet.Wsl.Config`，并且 `build-linux-demos.sh` 会自动使用它来清理这类 Windows-only fallback 配置。

如果你手工在 WSL 中执行 `dotnet restore` 或 `dotnet publish`，也建议显式带上：

```bash
--configfile /mnt/d/source/LVGLSharp/NuGet.Wsl.Config
```

### 4. 安装 Linux 原生依赖

本仓库的 Linux demo 发布脚本依赖 `cmake` 等工具。在 Ubuntu / Debian 下：

```bash
sudo apt-get update
sudo apt-get install -y cmake ninja-build
```

### 4.1 如果 WSL 下中文乱码

如果 `WSLg / Wayland` 路径下中文标签、按钮或标题出现乱码，不要先怀疑字符串编码。对当前仓库来说，更常见的原因是 `WSL` 里没有真正可用的 `CJK` 字体。

建议先在 `WSL` 中安装中文字体包：

```bash
sudo apt-get update
sudo apt-get install -y fonts-noto-cjk fonts-wqy-zenhei
fc-list :lang=zh | head
```

如果 `fc-list :lang=zh` 仍为空，就说明当前 `WSL` 里依然没有被 `fontconfig` 正确识别的中文字体，`PictureBoxDemo` 这类界面仍可能退回到 `DejaVu Sans` 等非中文字体，继续表现为乱码或缺字。

## 运行 demo 的推荐方式

### 方式 A：在 Windows 侧执行发布脚本，在 WSL 中运行产物

这是当前最推荐的方式。

#### 第一步：在仓库根目录执行 Linux 发布脚本

Windows PowerShell：

```powershell
bash ./build-linux-demos.sh --clean --rid linux-x64
```

执行成功后，产物通常在：

- `dist/linux-x64/WinFormsDemo/`
- `dist/linux-x64/PictureBoxDemo/`
- `dist/linux-x64/SmartWatchDemo/`
- 其他 demo 目录

#### 第二步：在 WSL 中进入仓库目录

假设仓库在 Windows 的 `D:\source\LVGLSharp`，则在 WSL 中路径通常是：

```bash
cd /mnt/d/source/LVGLSharp
```

#### 第三步：运行某个 demo

例如运行 `WinFormsDemo`：

```bash
cd /mnt/d/source/LVGLSharp/dist/linux-x64/WinFormsDemo
./WinFormsDemo
```

例如运行 `PictureBoxDemo`：

```bash
cd /mnt/d/source/LVGLSharp/dist/linux-x64/PictureBoxDemo
./PictureBoxDemo
```

## 按 demo 分类的启动手册

下面按当前仓库里常用 demo 分别列出“发布 + 启动”步骤。默认示例均使用：

- 仓库根目录：`/mnt/d/source/LVGLSharp`
- 目标 RID：`linux-x64`
- 发布方式：优先复用仓库脚本 `build-linux-demos.sh`

### `WinFormsDemo`

适合验证基础控件、窗体生命周期、`LVGLSharp 布局`、通用交互行为。

#### 仅发布这个 demo

Windows PowerShell：

```powershell
bash ./build-linux-demos.sh --clean --rid linux-x64 WinFormsDemo
```

#### 在 WSL 中启动

```bash
cd /mnt/d/source/LVGLSharp/dist/linux-x64/WinFormsDemo
./WinFormsDemo
```

### `PictureBoxDemo`

适合验证图片加载、缩放、旋转、抗锯齿，以及 Linux 图像路径行为。

#### 仅发布这个 demo

Windows PowerShell：

```powershell
bash ./build-linux-demos.sh --clean --rid linux-x64 PictureBoxDemo
```

#### 在 WSL 中启动

```bash
cd /mnt/d/source/LVGLSharp/dist/linux-x64/PictureBoxDemo
./PictureBoxDemo
```

#### 建议额外检查

- `Assets/` 是否已复制到输出目录
- 如果图像未显示，优先检查当前路径和图片资源是否存在

#### Wayland / WSLg 验证建议

`PictureBoxDemo` 更适合拿来验证：

- 图片首帧是否真正提交到 `Wayland` surface
- 窗口首次 resize 后图片区域是否还能继续出图
- 鼠标焦点进入后交互是否正常

如果出现黑屏或空白，不要先把原因归到图片资源本身，优先按下面顺序排查：

1. 首帧提交是否发生
2. resize 后首帧是否重新触发
3. 输入焦点是否已正确进入窗口
4. `root invalidate` 是否在需要时补发

#### Wayland / WSLg 实测进展

当前已经在本机 `WSLg` 环境下继续跑过 `PictureBoxDemo` 的发布与启动链路：

- `build-linux-demos.sh PictureBoxDemo` 已可在 `WSL` 下完成发布
- `PictureBoxDemo` 进程已能在 `WSLg` 下启动
- 在 Windows 侧进程窗口标题观测中，已看到 `PictureBox 演示程序 - LVGLSharp [WSLg::0]` 窗口标题，说明 `Wayland/WSLg` 路径已进入真实窗口创建阶段
- `WaylandView` 现已补上 Linux 系统字体回退，`PictureBoxDemo` 的中文控件文本不再因缺少 CJK 字体而影响截图验证

如果仍然怀疑是字体路径命中错误，可以直接看 `WaylandView.ToString()` 中新增的两段诊断：

- `FontPath=...`
- `FontDiag=...`

它们会告诉你当前真实选中的字体文件，以及是否因为找不到真正的 `CJK` 字体而退回到通用字体，例如 `DejaVu Sans`。

#### Wayland / WSLg 截图演示

下面这张图片已经作为仓库内的截图资源落地，可直接作为当前 `PictureBoxDemo` 在 `WSLg / Wayland` 路径下的实测样例：

![PictureBoxDemo 在 WSLg / Wayland 下的实测截图](./images/wslg-pictureboxdemo-wayland.png)

下面这张图片对应窗口放大后的再次抓图，可作为 `resize` 后首帧仍然可见的辅助样例：

![PictureBoxDemo 在 WSLg / Wayland 下 resize 后的实测截图](./images/wslg-pictureboxdemo-wayland-resized.png)

现阶段建议把 `PictureBoxDemo` 当作图片链路专项验证 demo：

- 重点看图片首帧是否提交
- 重点看 resize 后图片区域是否立即恢复
- 如果内容仍异常，再回到首帧提交、输入焦点和 `root invalidate` 继续排查

#### 当前针对画面问题的判断

基于本轮实机验证，当前可以先得到下面的结论：

- 首帧显示：已经进入可截图状态，说明首帧并非完全黑屏
- resize 后首帧：在放大窗口后再次抓图仍能得到有效画面，当前未见 resize 后整窗空白
- 焦点推进：窗口切到前台后，继续执行按钮交互仍能得到有效画面，当前未观察到焦点进入即黑屏的问题
- 缩放 / 旋转：本轮已经对 `PictureBoxDemo` 做过真实按钮交互验证，缩放和旋转操作后的窗口截图哈希均发生变化，说明按钮响应和画面刷新链路当前是工作的
- 按钮刷新稳定性：在前后台切换后继续触发按钮操作，截图结果仍持续变化，当前没有看到“焦点回来后按钮失效或不刷新”的证据
- `root invalidate`：结合首帧、resize 后以及交互后的持续可见结果，当前没有证据表明还需要为 `PictureBoxDemo` 额外补一轮新的 `root invalidate` 修复

也就是说，`PictureBoxDemo` 现阶段已经从“链路可启动”推进到了“可以做真实画面验证”。后续如果还发现局部区域不刷新、首图偶发缺失或 resize 后局部残影，再优先回到这四项继续深挖。

### `SmartWatchDemo`

适合验证多页整屏界面、首屏加载、动态 UI 刷新、复杂布局切换。

#### 仅发布这个 demo

Windows PowerShell：

```powershell
bash ./build-linux-demos.sh --clean --rid linux-x64 SmartWatchDemo
```

#### 在 WSL 中启动

```bash
cd /mnt/d/source/LVGLSharp/dist/linux-x64/SmartWatchDemo
./SmartWatchDemo
```

#### 建议额外检查

- 如果出现黑屏，不要只看标题栏，优先确认实际 viewport 是否有像素内容
- 优先确认是否是控件树创建阶段卡住，而不是先怀疑字体问题

#### Wayland / WSLg 实测结果

本仓库已经在 `WSLg/Wayland` 下对 `SmartWatchDemo` 做过一轮真实启动验证，当前结论是：

- 进程可以在 `WSLg` 下正常启动并持续存活
- 当前窗口不是“只有标题栏出来”，而是已经能看到实际像素内容
- 首页内容已经进入真实渲染阶段，可见中部手表区域、四周按钮与底部状态文本

#### Wayland / WSLg 截图演示

下面这张图片已经作为仓库内的截图资源落地，可直接作为当前 `Wayland/WSLg` 路径的实测演示样例：

![SmartWatchDemo 在 WSLg / Wayland 下的实测截图](./images/wslg-smartwatchdemo-wayland.png)

结合本次实测截图，可以把当前状态理解为：

- `Wayland` 原生窗口创建链路已打通
- `wl_shm` 渲染链路已进入可见输出阶段
- `SmartWatchDemo [WSLg::0]` 已能作为 Wayland 路径的截图演示样例

也就是说，当前 `SmartWatchDemo` 更像是“内容已显示，但还要继续打磨首帧/resize/焦点细节”，而不是“窗口完全黑屏”。

如果后续在 `WSLg` 下再次看到空白或局部异常，优先继续检查：

1. 首帧提交
2. resize 后首帧
3. 输入焦点
4. `root invalidate`

推荐把 `SmartWatchDemo` 作为当前 Wayland 路径的首选截图演示，把 `PictureBoxDemo` 作为图片链路和 resize 行为的补充验证 demo。

### `MusicDemo`

适合验证较复杂界面、列表/详情切换、图片与动画感较强的页面效果。

> 仓库中的项目目录当前仍是 `src/Demos/MusicWinFromsDemo/`，但 Linux 发布产物目录名是 `MusicDemo`。

#### 仅发布这个 demo

Windows PowerShell：

```powershell
bash ./build-linux-demos.sh --clean --rid linux-x64 MusicDemo
```

#### 在 WSL 中启动

```bash
cd /mnt/d/source/LVGLSharp/dist/linux-x64/MusicDemo
./MusicDemo
```

### `SerialPort`

适合验证串口界面与输入交互，但在 `WSL` 中运行时要特别注意设备可见性。

#### 仅发布这个 demo

Windows PowerShell：

```powershell
bash ./build-linux-demos.sh --clean --rid linux-x64 SerialPort
```

#### 在 WSL 中启动

```bash
cd /mnt/d/source/LVGLSharp/dist/linux-x64/SerialPort
./SerialPort
```

#### 特别说明

- 如果只是验证界面行为，可以先不接真实串口设备
- 如果要验证真实串口，需先确认对应设备已经正确映射到 `WSL`
- 如果串口列表为空，先排查设备映射问题，不要先怀疑 UI 逻辑

## 按用途选 demo

如果你不确定先跑哪个 demo，可以按目的选择：

- 验证基础窗体/控件：`WinFormsDemo`
- 验证图片链路：`PictureBoxDemo`
- 验证复杂多页界面：`SmartWatchDemo`
- 验证复杂视觉效果：`MusicDemo`
- 验证串口相关交互：`SerialPort`

## 快速命令对照

### 发布全部 demo

```powershell
bash ./build-linux-demos.sh --clean --rid linux-x64
```

### 发布单个 demo

```powershell
bash ./build-linux-demos.sh --clean --rid linux-x64 WinFormsDemo
bash ./build-linux-demos.sh --clean --rid linux-x64 PictureBoxDemo
bash ./build-linux-demos.sh --clean --rid linux-x64 SmartWatchDemo
bash ./build-linux-demos.sh --clean --rid linux-x64 MusicDemo
bash ./build-linux-demos.sh --clean --rid linux-x64 SerialPort
```

### 在 WSL 中启动单个 demo

```bash
cd /mnt/d/source/LVGLSharp/dist/linux-x64/WinFormsDemo && ./WinFormsDemo
cd /mnt/d/source/LVGLSharp/dist/linux-x64/PictureBoxDemo && ./PictureBoxDemo
cd /mnt/d/source/LVGLSharp/dist/linux-x64/SmartWatchDemo && ./SmartWatchDemo
cd /mnt/d/source/LVGLSharp/dist/linux-x64/MusicDemo && ./MusicDemo
cd /mnt/d/source/LVGLSharp/dist/linux-x64/SerialPort && ./SerialPort
```

### 方式 B：在 WSL 内直接 publish 再运行

如果你希望所有 Linux 步骤都在 WSL 中完成，可直接在 WSL 中执行：

```bash
cd /mnt/d/source/LVGLSharp
dotnet publish src/Demos/WinFormsDemo/WinFormsDemo.csproj -f net10.0 -r linux-x64 -c Release -o ./artifacts/wsl/WinFormsDemo
```

然后运行：

```bash
cd /mnt/d/source/LVGLSharp/artifacts/wsl/WinFormsDemo
./WinFormsDemo
```

> 注意：如果你不使用仓库脚本，而是自己 `publish`，请确认 Linux 侧所需的原生库与发布输出完整可用。

## 如何判断当前走的是 WSLg 路径

当前仓库的 Linux runtime 已经会自动探测环境：

- `LinuxView` 是统一入口
- `LinuxEnvironmentDetector` 负责探测
- 检测到 `WSLg` 时会路由到 `WslgView`

当前 `WslgView` 有两个可观察点：

1. 窗口标题会带 `WSLg` 标识
2. `WslgView.ToString()` 会返回简要诊断摘要

WSLg 判断依据包括：

- `WSL_DISTRO_NAME`
- `WSL_INTEROP`
- `WAYLAND_DISPLAY`
- `WSLG_RUNTIME_DIR`
- `/mnt/wslg`
- `DISPLAY`

## 调试建议

## 方案 1：Visual Studio 继续负责编辑，WSL 负责运行

适合日常开发验证。

推荐流程：

1. 在 `Visual Studio` 中改代码
2. 用 PowerShell 执行 Linux publish
3. 到 WSL 中运行 demo
4. 看窗口效果、日志、异常输出

优点：
- 与当前仓库结构最匹配
- 不需要额外改工程配置
- 最稳

缺点：
- 不是单击 `F5` 的 Linux 断点调试体验

## 方案 2：VS Code + Remote WSL 调试 Linux demo

如果你需要 Linux 侧断点调试，推荐这条路径。

### 第一步：在 Windows 安装 VS Code 和 Remote WSL 扩展

在 WSL 中进入仓库目录后执行：

```bash
cd /mnt/d/source/LVGLSharp
code .
```

这会用 `Remote WSL` 方式打开当前仓库。

### 第二步：在 WSL 终端中恢复和发布

```bash
dotnet restore
dotnet build
```

然后按需要运行或调试对应 demo 的 `net10.0` 目标。

### 第三步：按 Linux 目标调试

建议优先针对具体 demo 做启动，而不是一次调整个解决方案。

例如：
- `src/Demos/WinFormsDemo/WinFormsDemo.csproj`
- 目标框架：`net10.0`

## 方案 3：Visual Studio 直接调 Windows 目标，WSL 只做 Linux 行为验证

如果当前主要目的是开发控件逻辑，而不是定位 Linux 宿主问题，可以：

1. 先在 Windows 目标下用 `Visual Studio` 正常调试
2. 功能稳定后再切到 WSL 验证 Linux 行为

这种方式适合：
- 控件逻辑调试
- 事件链调试
- 基础布局验证

不适合：
- X11 / WSLg 宿主问题
- Linux 字体、输入、显示协议问题

## 调试前建议检查项

每次在 WSL 中启动 Linux demo 前，建议先检查：

```bash
echo $DISPLAY
echo $WAYLAND_DISPLAY
echo $WSL_DISTRO_NAME
echo $WSL_INTEROP
ls /mnt/wslg
```

再检查发布结果：

```bash
ls
```

确认：
- 可执行文件存在
- 原生库存在
- 当前目录正确

## 常见问题

### 1. 窗口没出来

优先检查：

- `DISPLAY` 是否存在
- `/mnt/wslg` 是否存在
- 是否真的运行了 Linux 发布产物
- 是否在 `WSL` 中运行，而不是在 Windows shell 中直接执行 Linux ELF

### 2. 标题栏出来了，但内容黑屏

优先排查：

- 是否是控件树创建阶段卡住
- 是否首页挂载内容过多
- 是否隐藏页或大布局在启动阶段一次性挂载

如果当前是在 `Wayland / WSLg` 路径下，还应额外确认：

- 是否已经发生首帧 `flush`
- `wl_buffer release` 之后是否触发了补发重绘
- resize 后是否重新设置了 `lv_display` 分辨率与 buffers
- 焦点进入后是否有后续输入与重绘推进

### 2.1 中文乱码或缺字

优先排查：

- `fc-list :lang=zh` 是否能列出中文字体
- 是否已经安装 `fonts-noto-cjk` 或 `fonts-wqy-zenhei`
- `WaylandView.ToString()` 里的 `FontPath` 是否落回了 `DejaVu Sans` 一类的通用字体
- `FontDiag` 是否提示 `NoCjkFontFound`

### 3. Visual Studio 里不会配 WSL 启动调试

对本仓库当前阶段，建议不要先追求这条路径。

更建议：
- `Visual Studio` 负责编辑和 Windows 调试
- `WSL` 负责运行 Linux 产物
- 需要 Linux 断点时使用 `VS Code Remote WSL`

这是当前投入最小、稳定性最高的做法。

## 建议的团队工作流

推荐团队内部统一为下面流程：

1. Windows 上使用 `Visual Studio` 开发
2. 功能修改后先保证 Windows / 基础构建通过
3. 用 Linux publish 脚本产出 demo
4. 在 `WSL2/WSLg` 中运行验证
5. 只有遇到 Linux 宿主专属问题时，再切 `VS Code Remote WSL` 做断点调试

这样可以兼顾：
- Windows 侧开发效率
- Linux 侧真实运行环境验证
- 最少的工程与 IDE 配置复杂度

