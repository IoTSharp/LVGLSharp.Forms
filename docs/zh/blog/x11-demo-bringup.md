---
title: X11 Demo 带起记录：把 PictureBox、MusicDemo 和 SmartWatchDemo 跑起来
description: 如果你也在 Linux X11 下带 Demo，这篇记录会直接带你看我们怎么处理显示选择、字体样式、字形位图和稳定回退路径问题。
lang: zh-CN
---

# X11 Demo 带起记录：把 PictureBox、MusicDemo 和 SmartWatchDemo 跑起来

> 如果你也在排 Linux 宿主调试、X11 首帧验证、字体渲染链路和 Demo 稳定性问题，这篇文章会直接带你看我们是怎么一路排下来的。

我们这轮的目标很直接：

- 在 `X11` 下跑起 `PictureBoxDemo`
- 在 `X11` 下跑起 `MusicDemo`
- 在 `X11` 下跑起 `SmartWatchDemo`
- 给这三个 Demo 留下可复用的截图资产

但我们真正做下去以后，很快就发现问题并不只是“窗口能不能创建出来”，而是整条链路里有好几层都在互相影响：

- 运行时到底连到了哪个 `DISPLAY`
- 根对象字体是怎么挂到 LVGL 样式树上的
- 自定义字形位图回调是不是遵守了 LVGL 的内存契约
- 控件创建阶段读取到的字体指针是不是稳定值
- 截图阶段到底该以哪条稳定路径为准

## 第一层经验：先确认你真的跑在想要的 X11 上

表面上我们是用 X11 方式启动 Demo，但最开始的一个误判是：以为运行时会优先走我们准备好的 `Xvfb :99`。

实际检查后才发现，当前 Linux 运行时在选 `X11` 显示时，并不单纯依赖进程内的 `DISPLAY` 变量，而是会先扫描系统里可连接的 X11 socket。结果是，最终真正连上的显示是桌面环境里的 `:1`，不是我们以为的 `:99`。

这件事带来的直接教训是：

- 不要只看启动命令里的 `DISPLAY`
- 要用真实窗口树或窗口信息确认最终连上的显示
- 截图工具也必须对准真实显示，否则你会在错误环境里排错

这也是为什么我们最后保留下来的有效截图，都是围绕 `DISPLAY=:1` 完成的。

## 第二层经验：托管侧手动分配 `lv_style_t` 很脆弱

`MusicDemo` 和 `SmartWatchDemo` 最早的崩溃栈，都集中在字体相关路径上：

- `lv_font_get_line_height`
- `lv_text_get_size`
- `lv_label_refr_text`
- `lv_obj_refresh_style`

第一眼看像是“某个 Label 自己坏了”，但继续看实现后发现，更可疑的是默认字体样式的安装方式。

原来的 Linux 默认字体路径里，会在托管侧手动分配一个 `lv_style_t`，然后再调用：

- `lv_style_init`
- `lv_style_set_text_font`
- `lv_obj_add_style`

这条路的问题不在于 API 名字不对，而在于 `lv_style_t` 是一个完全由原生侧定义和演进的数据结构。只要托管侧的布局、对齐、生命周期管理里有一点点偏差，这种“自己 malloc 一个 native style 再交回去”的方式就会非常脆弱。

我们在 `[LvglHostDefaults.cs](/home/admin/openclaw/workspace/LVGLSharp.Forms_1/src/LVGLSharp.Runtime.Linux/LvglHostDefaults.cs)` 里改成了更保守的做法：不再手动创建 `lv_style_t`，而是直接把字体挂到 root 对象上：

```csharp
lv_obj_set_style_text_font(root, font, 0);
```

这个改动本身不华丽，但非常值钱。因为它把“托管代码模拟 native style 生命周期”这件高风险的事拿掉了。

## 第三层经验：字形位图回调必须严格遵守 LVGL 的缓冲区契约

继续往下看，另一处高概率问题出现在自定义字体管理器里。

`[SixLaborsFontManager.cs](/home/admin/openclaw/workspace/LVGLSharp.Forms_1/src/LVGLSharp.Core/SixLaborsFontManager.cs)` 的 `GetGlyphBitmap` 之前有两个很危险的点：

1. 它返回的是 `draw_buf` 指针，而不是 `draw_buf->data`
2. 它把自己的缓存生命周期和 LVGL 传进来的绘制缓冲区混在了一起

这会带来一个很典型的问题：LVGL 以为自己拿到的是字形像素数据地址，实际却拿到了别的结构体地址，或者拿到了一份和当前绘制缓冲生命周期不一致的缓存。

我们这轮主要做了两件事：

- 直接把字形渲染到 `draw_buf->data`
- 返回值改成 `draw_buf->data`

也就是说，回调只负责把这次字形的像素填进 LVGL 给的缓冲区，而不再擅自把额外分配的缓存塞回绘制链路。

这个修复之后，`MusicDemo` 的稳定性明显提升，已经能在 X11 下真实起窗并截图。

## 第四层经验：样式查询拿回来的字体指针不一定是可信来源

`SmartWatchDemo` 比 `MusicDemo` 更难一点。前两轮修复之后，它不再一开始就死在窗口创建前，但仍然会在非常早的 Label 创建阶段崩掉。

创建轨迹显示，它在首页标题 `Smart Watch Demo` 刚创建完附近就出问题了。继续用 `gdb` 看寄存器后，最关键的证据出现了：传进 `lv_font_get_line_height` 的“字体指针”居然是 `0xff`。

这说明问题已经不是“字体文件坏了”，而是“某个环节把错误的值当成了字体指针”。

根因落在 WinForms 样式辅助逻辑上：控件创建时会从 root 样式里再查一次当前字体，然后把这个值再设置给新控件。理论上这听起来合理，但在当前 X11 带起阶段，这个查询返回值并不稳定。

我们这轮对应的修复是：

- 新增 `[LvglRuntimeFontRegistry.cs](/home/admin/openclaw/workspace/LVGLSharp.Forms_1/src/LVGLSharp.Core/LvglRuntimeFontRegistry.cs)`
- 在安装默认字体时，把“当前活动字体指针”缓存到托管侧
- 在 `[ApplicationStyleSet.cs](/home/admin/openclaw/workspace/LVGLSharp.Forms_1/src/LVGLSharp.WinForms/Forms/ApplicationStyleSet.cs)` 里优先使用这份托管缓存，而不是在控件创建期重新向样式系统查询

这个思路的价值在于：

- 字体是谁安装的，就由谁保留可信指针
- 控件创建期不再依赖一个时机敏感的样式查询结果
- 避免把 `0xff` 这类哨兵值或错误值继续往 LVGL 深处传

## 第五层经验：稳定带起和彻底修完，不一定是同一件事

到这一步，`PictureBoxDemo`、`MusicDemo`、`SmartWatchDemo` 都已经向前推进了不少，但还有一个现实问题必须正视：

自定义字体路径在更深的绘制阶段，仍然可能在部分 X11 文本绘制场景下崩到 `lv_draw_sw_blend_color_to_rgb565` 这类软件混色路径里。

这意味着：

- “能创建窗口”不等于“整条字体绘制链路已经完全正确”
- “第一屏能出内容”不等于“所有 Label 都已经稳定”

所以我们没有把“彻底修完自定义字体渲染”伪装成已经完成，而是补了一条明确的稳定回退路径：

- 在 `[X11View.cs](/home/admin/openclaw/workspace/LVGLSharp.Forms_1/src/LVGLSharp.Runtime.Linux/X11View.cs)` 增加 `LVGLSHARP_DISABLE_CUSTOM_FONT=1`

打开这个开关后，X11 运行时会跳过当前的自定义字体管理器，退回到 LVGL 自带字体链路。这样做的代价很明确：

- 复杂 Demo 能稳定起窗、截图
- 但 CJK 文本可能暂时退化成缺字或方框

这是一个典型的工程取舍：

- 先拿到稳定可展示的 X11 案例
- 再继续深修自定义字体路径

对 Demo 带起和文档落地来说，这个取舍是合理的。

## 当前推荐的 X11 启动方式

如果你现在的目标是“先稳定跑起来并截图”，我们建议你先用下面这组环境变量：

```bash
DISPLAY=:1 \
LVGLSHARP_LINUX_HOST=x11 \
XDG_SESSION_TYPE=x11 \
WAYLAND_DISPLAY= \
LVGLSHARP_DISABLE_CUSTOM_FONT=1 \
dotnet run -f net10.0 --project src/Demos/SmartWatchDemo/SmartWatchDemo.csproj -p:EnableWindowsTargeting=true
```

如果你要跑 `MusicDemo`，也是同样思路，只需要把项目路径换成对应的 `csproj`。

如果你现在在做复杂页面、截图沉淀或回归验证，建议先走这条稳定路径。等我们把自定义字体绘制链路完全修稳，再把这个开关逐步收回。

## 这次我们已经拿到的结果

我们已经产出了三张可归档的 X11 截图：

- `PictureBoxDemo`: `/images/x11-pictureboxdemo.png`
- `MusicDemo`: `/images/x11-musicdemo.png`
- `SmartWatchDemo`: `/images/x11-smartwatchdemo.png`

对应的案例页也已经补到中文截图页和英文截图页里：

- [中文界面截图页](/zh/preview-local.html)
- [English Screenshot Gallery](/en/preview-local.html)

## 如果你也要带 X11，可以先记住这几条

- X11 带起不是单点问题，而是显示选择、样式安装、字体查询、字形位图和截图链路一起工作的结果。
- 只要 native 结构体的 ABI 不是你自己完全控制的，就要谨慎对待“托管侧手动分配再交回原生侧”的做法。
- 字形回调里，返回值和缓冲区所有权一定要完全匹配 LVGL 的约定，否则问题会在更深的绘制阶段才爆出来。
- 当样式系统查询结果在生命周期早期不稳定时，保存一份自己可信的活动字体指针，通常比重复查询更可靠。
- 给 X11 带起过程留一个“稳定回退开关”并不丢人，它能让案例、截图、验证和后续修复并行推进。

## 下一步还要做什么

我们这次把 X11 路线从“有的 Demo 黑屏、有的 Demo 崩溃”推进到了“复杂 Demo 可以稳定起窗并沉淀截图”。

但这还不是终点。如果你后面也要继续跟进，这几项最值得继续收尾：

- 把自定义字体路径在 X11 下的软件绘制崩溃彻底修掉
- 让 `MusicDemo` 和 `SmartWatchDemo` 在不关闭自定义字体时也能稳定运行
- 恢复 `PictureBoxDemo` 这类界面的 CJK 文本显示质量
- 最终把 `LVGLSHARP_DISABLE_CUSTOM_FONT=1` 从“稳定回退路径”收敛成“排障开关”

如果我们把这一步走通，X11 路线就不再只是“文档里有截图”，而是会真正成为 Linux 图形宿主验证的一条稳定工程路径。
