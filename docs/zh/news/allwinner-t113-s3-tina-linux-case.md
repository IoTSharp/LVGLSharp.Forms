---
title: LVGLSharp 全志 T113-S3 成功移植案例（.NET 10 + Tina Linux）
description: 记录 LVGLSharp 在全志 T113-S3、128MB 内存与 Tina Linux 5.0 环境下完成 .NET 10 非 AOT 部署的真实运行结果。
lang: zh-CN
---

# LVGLSharp 全志 T113-S3 成功移植案例（.NET 10 + Tina Linux）

> 这是一个面向嵌入式 Linux 场景的真实移植案例：`LVGLSharp` 已在 `全志 T113-S3` + `Tina Linux 5.0` 环境中完成运行验证，并在 `1024x600` 显示屏上稳定渲染与交互。

## 案例图片

> 电路板图片放置路径：`docs/images/cases/allwinner-t113-s3-board.jpg`  
> 界面截图放置路径：`docs/images/cases/allwinner-t113-s3-ui.jpg`

![全志 T113-S3 电路板实物图](/images/cases/allwinner-t113-s3-board.jpg)

![LVGLSharp 在全志 T113-S3 上的界面截图](/images/cases/allwinner-t113-s3-ui.jpg)

## 硬件平台

- 主控：全志 `T113-S3`
- 内存：`128MB RAM`
- 存储：`128MB FLASH`
- 显示屏：`1024x600` 分辨率
- 系统环境：`Tina Linux 5.0` / `GCC 12.3` / `Glibc 2.35`

## 运行环境

- 运行框架：`.NET 10`
- 部署方式：非 `AOT`，采用普通框架依赖部署
- UI 框架：`LVGLSharp` + `LVGL 9.5.0`
- 输入设备：触摸屏正常适配

## 运行状态

- 界面正常渲染，已在 `1024x600` 高分辨率下稳定显示
- 触摸输入正常
- 中文显示与字体渲染正常
- `128MB` 内存环境稳定运行
- `Tina Linux` 环境无依赖问题、无报错

## 这个案例说明了什么

- `LVGLSharp` 已经不只是桌面或通用 Linux 环境验证，也可以进一步落到资源受限的嵌入式 Linux 平台。
- 在 `全志 T113-S3` 这类常见 SoC 上，`.NET 10` 与 `LVGL 9.5.0` 的组合已经具备实际运行基础。
- 对需要中文界面、触摸输入和中等分辨率显示的设备项目来说，这条技术路线已经有了可复用的落地参考。
