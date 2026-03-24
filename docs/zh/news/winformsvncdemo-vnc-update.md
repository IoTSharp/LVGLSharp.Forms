---
title: 我把 WinFormsVncDemo 真正带到了 VNC 链路上
description: 用第一人称记录这轮 VncView、远程运行时重组、Windows 发布修复与文档补位的结果。
lang: zh-CN
---

# 我把 WinFormsVncDemo 真正带到了 VNC 链路上

> 2026 年 3 月 24 日，我想用一篇短新闻汇报一个我自己也很满意的阶段结果：`WinFormsVncDemo` 已经不只是本地窗口示例，它现在可以直接跑在 `VncView` 上，并通过桌面 VNC 客户端访问。

![WinFormsVncDemo 通过 Windows 桌面 VNC Viewer 访问的案例截图](/images/winformsvncdemo-vnc-case.png)

## 这次我推进了哪些关键变化

- 我把 `VncView` 和 `RdpView` 保持为对外可见的入口类型，没有把它们埋进内部子目录里。
- 我把远程运行时内部实现重新按职责归位到了 `Frames`、`Input`、`Transport`、`Session` 和 `Views` 这些目录，让传输、输入和帧处理边界更清楚。
- 我把 `WinFormsVncDemo` 里的 VNC 地址、端口和窗口尺寸细节收回到运行时注册层，`Program` 重新只负责启动应用，Demo 本身也不再承担远程会话配置职责。
- 我给 `VncView` 补上了默认构造路径，这让 Demo 可以用默认值直接起服务，后面专门设计 `ViewOptions` 时也不会把当前体验拖住。
- 我让 Demo 启动时把 `5900` 端口和本机可访问地址直接打印到控制台，打开后就知道该连哪一个地址。
- 我修正了 `LVGLSharp.Native.targets` 对原生运行时文件的发布行为，让 Windows 自包含 / AOT 发布产物能把必需的 `lvgl.dll` 一起带上，而不是启动时再抛 `DllNotFoundException`。
- 我把这次 Windows 侧通过 VNC Viewer 访问 `WinFormsVncDemo` 的截图正式放进文档，作为远程宿主链路已经跑通的案例留档。

## 为什么我觉得这轮值得写进新闻

过去这条链路更像“能拼起来”，现在它开始更像“能拿给别人试”。我最看重的不是把一个 Demo 临时跑起来，而是把职责边界理顺：对外入口简单，内部传输实现分层，Demo 启动代码干净，发布产物也能真正落地。

这意味着我们现在已经有了一个更像产品能力的基线：一个普通的 `WinForms` 风格界面，可以被 `VncView` 托管，对外暴露成 VNC 服务，再被桌面客户端直接访问。这对后面的远程宿主能力、自动化验证，以及更正式的 `ViewOptions` 设计，都是很实在的一步。

## 我接下来会继续盯什么

- 我会继续把远程运行时的配置面整理成更清晰的 `ViewOptions` 体系。
- 我会继续补更多真实案例截图，而不是只靠口头描述功能。
- 我也会继续把发布链路打磨到“拿到产物就能测”的程度，减少 Demo 与真实部署之间的落差。
