---
title: 我把 WPF 布局兼容做到了可落地：Grid 权重、StackPanel 流式与启动稳定性
description: 记录 WPF 侧对 LVGL 布局适配的最新进展，包括 Grid `*` 权重分配、StackPanel 流式布局、图片资源加载与启动崩溃修复。
lang: zh-CN
---

# 我把 WPF 布局兼容做到了可落地：Grid 权重、StackPanel 流式与启动稳定性

> 2026 年 3 月 30 日，这一轮我集中做了 WPF 与 LVGL 布局语义对齐，目标是同一套 XAML 在微软 WPF 和我们的 WPF 中都能稳定显示。

![WPF 布局兼容与运行效果截图](/images/wpf-layout-parity-stability-20260330.png)

## 这轮我完成了什么

- 把 `Grid` 中 `*` 从“降级为 Auto”改成“按权重分配空间”，让星号行列不再失真。
- 把 `StackPanel` 调整为更接近 WinForms `FlowLayout` 的排布方式，降低无显式尺寸子控件在同一位置重叠的风险。
- 补齐图片资源路径处理，保证 `Image` 控件在 WPF 示例中可正常加载显示。
- 联动修复启动稳定性问题，处理了组合控件和托管字体路径上的崩溃触发点。

## 为什么这个变化重要

- 对 XAML 使用者来说，布局语义更接近微软 WPF，迁移成本更低。
- 对 LVGL 映射层来说，`Grid` 与 `StackPanel` 的行为更可预测，避免“能编译但布局错位”的隐性问题。
- 对示例和回归来说，WPF Demo 可以更稳定地作为验证样本，而不只是一次性演示。

## 验证结果

- `WpfApp` 可成功构建并启动，不再出现本轮阻塞的启动崩溃。
- 示例页面中 `Button`、`CheckBox`、`ComboBox`、`Label`、`RadioButton`、`TextBlock`、`TextBox`、`Image` 可同时显示。
- 关键截图已纳入 docs，作为这轮兼容落地的基线记录。

## 接下来我会继续

- 继续收敛 `Grid` 与 `StackPanel` 的边界行为，包括最小尺寸、间距与换行策略。
- 补更多控件组合回归用例，确保布局兼容不是单点通过。