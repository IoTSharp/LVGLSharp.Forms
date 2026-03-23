---
title: 界面截图
description: 集中展示当前文档站可用的界面截图，后续可以持续补充更多 Demo 与宿主环境画面。
lang: zh-CN
template: structured
intro:
  eyebrow: "Screenshots"
  title: "界面截图展示"
  description: "这里集中展示 `LVGLSharp` 当前可对外展示的运行截图。后续如果有新的 Demo、宿主环境或 UI 效果图，可以继续放到这个页面里，作为项目视觉进展记录。"
sections:
  - title: "当前截图"
    description: "优先收录能体现运行时、宿主环境和界面完整度的截图。点击图片可以查看原图。"
    variant: showcase
    columns: 2
    items:
      - title: "桌面界面预览 A"
        badge: "Desktop"
        image: "/images/showcase/desktop-preview-1.png"
        alt: "LVGLSharp desktop preview screenshot A"
        description: "适合作为桌面布局、控件风格和整体视觉结构的预览图。"
      - title: "桌面界面预览 B"
        badge: "Desktop"
        image: "/images/showcase/desktop-preview-2.png"
        alt: "LVGLSharp desktop preview screenshot B"
        description: "更完整地展示窗口内容密度、控件组合以及当前 Demo 的界面形态。"
      - title: "WSLg / Wayland PictureBox 检查"
        badge: "WSLg"
        image: "/images/wslg-pictureboxdemo-wayland-embedded-font-check.png"
        alt: "WSLg Wayland PictureBox embedded font check"
        description: "用于记录 Linux 图形宿主下的字体和渲染验证情况。"
  - title: "后续建议补充"
    description: "这个页面后面可以继续按场景扩展，不需要混到文章正文里。"
    variant: cards
    columns: 3
    items:
      - title: "Demo 截图"
        description: "例如 `MusicDemo`、`SmartWatchDemo`、`PictureBoxDemo`、`WinFormsDemo` 的关键界面。"
      - title: "多宿主截图"
        description: "例如 Windows、WSLg、X11、Wayland、FrameBuffer、Offscreen 等不同运行环境。"
      - title: "功能前后对比"
        description: "适合记录控件增强、渲染改进、主题更新和交互优化前后的效果变化。"
  - title: "维护方式"
    description: "后续新增截图时，尽量保持同一套组织方式。"
    variant: list
    surface: true
    items:
      - label: "把截图文件放到 `docs/images/showcase/` 下"
      - label: "在当前页面 front matter 的 `sections.items` 里追加一条截图记录"
      - label: "优先补能体现平台宿主差异、真实运行效果和代表性 Demo 的画面"
---
