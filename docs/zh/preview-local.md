---
title: 界面截图
description: 如果你想先看当前已经跑出来的界面效果，这个页面会集中展示可直接预览的截图。
lang: zh-CN
template: structured
intro:
  eyebrow: "Screenshots"
  title: "界面截图展示"
  description: "如果你想先看 `LVGLSharp` 当前已经跑出来的界面效果，可以先从这个页面开始。后面我们补新的 Demo、宿主环境或 UI 效果图时，也会继续收在这里。"
sections:
  - title: "当前截图"
    description: "如果你想快速浏览当前的运行时、宿主环境和界面完整度，可以先看这一组。点击图片可以查看原图。"
    variant: showcase
    columns: 2
    items:
      - title: "桌面界面预览 A"
        badge: "Desktop"
        image: "/images/showcase/desktop-preview-1.png"
        alt: "LVGLSharp desktop preview screenshot A"
        description: "如果你想先看桌面布局、控件风格和整体视觉结构，这张图最直观。"
      - title: "桌面界面预览 B"
        badge: "Desktop"
        image: "/images/showcase/desktop-preview-2.png"
        alt: "LVGLSharp desktop preview screenshot B"
        description: "更完整地展示窗口内容密度、控件组合以及当前 Demo 的界面形态。"
      - title: "WSLg / Wayland PictureBox 检查"
        badge: "WSLg"
        image: "/images/wslg-pictureboxdemo-wayland-embedded-font-check.png"
        alt: "WSLg Wayland PictureBox embedded font check"
        description: "如果你在看 Linux 图形宿主下的字体和渲染验证，这张图能快速给你上下文。"
  - title: "这次我们补上的 X11 截图"
    description: "如果你想配合 [X11 Demo 带起记录](/zh/blog/x11-demo-bringup.html) 一起看，这一组截图就是对应的实际结果。"
    variant: showcase
    columns: 3
    items:
      - title: "PictureBoxDemo on X11"
        badge: "X11"
        image: "/images/x11-pictureboxdemo.png"
        alt: "PictureBoxDemo running on X11"
        description: "我们已经让 `PictureBoxDemo` 能在 X11 下起窗并形成案例截图。当前稳定截图走的是内置字体回退路径，因此中文控件文案仍待后续恢复。"
      - title: "MusicDemo on X11"
        badge: "X11"
        image: "/images/x11-musicdemo.png"
        alt: "MusicDemo running on X11"
        description: "在修正字体样式安装和字形位图回调后，我们已经让 `MusicDemo` 能在 X11 下稳定起窗并沉淀截图。"
      - title: "SmartWatchDemo on X11"
        badge: "X11"
        image: "/images/x11-smartwatchdemo.png"
        alt: "SmartWatchDemo running on X11"
        description: "我们也已经把 `SmartWatchDemo` 的 X11 首屏与截图链路走通了，它现在就是复杂多页界面的代表性案例。"
  - title: "如果你还想继续看更多案例"
    description: "后面如果我们继续补场景，最适合优先往这个方向扩。"
    variant: cards
    columns: 3
    items:
      - title: "Demo 截图"
        description: "例如 `MusicDemo`、`SmartWatchDemo`、`PictureBoxDemo`、`WinFormsDemo` 的关键界面。"
      - title: "多宿主截图"
        description: "例如 Windows、WSLg、X11、Wayland、FrameBuffer、Offscreen 等不同运行环境。"
      - title: "功能前后对比"
        description: "如果你想看控件增强、渲染改进、主题更新和交互优化前后的变化，这类对比图最有价值。"
  - title: "维护方式"
    description: "如果你后面也要继续补截图，沿着这套组织方式往下加就好。"
    variant: list
    surface: true
    items:
      - label: "把截图文件放到 `docs/images/` 下；如果是通用展陈图，也可以继续放到 `docs/images/showcase/`"
      - label: "在当前页面 front matter 的 `sections.items` 里追加一条截图记录"
      - label: "优先补能体现平台宿主差异、真实运行效果和代表性 Demo 的画面"
---
