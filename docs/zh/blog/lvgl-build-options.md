---
title: LVGL 编译参数说明：从 lv_conf.h 到 CMake 开关
description: 结合当前仓库的 lv_conf.h 与构建脚本，说明 LVGL 编译参数的组织方式、优先级和常见取舍。
lang: zh-CN
---

# LVGL 编译参数说明：从 `lv_conf.h` 到 CMake 开关

> 如果你正在调整 LVGL 的体积、渲染能力、Linux 后端或调试开关，这篇文章会先把参数放在哪里、谁覆盖谁、哪些参数最值得先看讲清楚。

## 先分清两层：`lv_conf.h` 和构建系统参数

LVGL 的大多数编译期开关都定义在 `lv_conf.h` 里。

它们决定的事情包括：

- 颜色深度
- 内存池大小
- 软件渲染能力
- 日志与断言
- 布局、主题、字体、图片、文件系统等功能模块
- SDL、X11、Wayland、FrameBuffer、DRM 之类的宿主或驱动支持

但如果你是通过 CMake 集成 LVGL，那么事情会多一层：

- `LV_*` 宏通常写在 `lv_conf.h`
- `CONFIG_LV_*` 和 `CONFIG_LV_BUILD_*` 由 CMake 传入
- 一部分纯构建选项并不在 `lv_conf.h`，例如 `BUILD_SHARED_LIBS`

在当前仓库里，这两层分别落在：

- `libs/lv_conf.h`
- `libs/build_lvgl.bat`
- `libs/build_lvgl.sh`

也就是说，真正生效的“编译参数”并不一定只看一个头文件。

## 参数到底谁说了算

这是最容易踩坑的地方。

如果只是普通集成，LVGL 会先读取 `lv_conf.h`。官方文档也推荐把它放在 `lvgl` 目录旁边，或者通过 `LV_CONF_INCLUDE_SIMPLE` / `LV_CONF_PATH` 告诉编译器去哪里找。

但在 CMake 集成里，LVGL 还支持通过 `CONFIG_LV_*` 和 `CONFIG_LV_BUILD_*` 覆盖对应配置。换句话说：

1. `lv_conf.h` 负责给出基线配置。
2. CMake 可以再用 `-DCONFIG_LV_...=...` 覆盖其中一部分值。
3. 还有少量选项只存在于构建系统里，例如 `BUILD_SHARED_LIBS`。

一个很常见的情况就是：

- 你在 `lv_conf.h` 里把某个驱动写成了 `0`
- 但构建命令又传了 `-DCONFIG_LV_USE_SDL=ON`
- 最终产物仍然会把 SDL 路径编进去

所以排查配置是否生效时，不要只盯着 `lv_conf.h`，还要一起看 CMake 命令行和缓存。

像当前仓库这样把 `lv_conf.h` 放在 `libs/` 目录，而不是直接放到 `libs/lvgl/` 顶层时，通常就应该显式告诉 CMake 去哪里找，例如：

```bash
cmake -DLV_BUILD_CONF_DIR=/path/to/LVGLSharp/libs \
      -DCONFIG_LV_USE_SDL=ON \
      -DCONFIG_LV_BUILD_DEMOS=OFF \
      ..
```

这条命令的意思不是“把所有配置都写到 CMake 里”，而是：

- `LV_BUILD_CONF_DIR` 负责找到那份 `lv_conf.h`
- `CONFIG_LV_USE_SDL` 负责覆盖其中一部分开关
- `CONFIG_LV_BUILD_DEMOS` 负责决定当前这次构建要不要真的编 demo 目标

## 当前仓库这份 `lv_conf.h` 的重点配置

当前仓库的 `libs/lv_conf.h` 头部标注的是 `v9.5.0` 配置模板。它非常长，但真正优先要理解的，其实只有几组。

## 一组最先看的参数：颜色、内存与节奏

当前配置里比较关键的基线是：

- `LV_COLOR_DEPTH 16`
- `LV_MEM_SIZE (64 * 1024U)`
- `LV_DEF_REFR_PERIOD 33`
- `LV_DPI_DEF 130`

这组参数的意思可以直接理解成：

- 当前默认按 `RGB565` 走，优先照顾嵌入式和较小显存压力
- LVGL 内建内存池默认给了 `64KB`
- 默认刷新周期大约按 `33ms` 来看，接近 `30 FPS`
- 默认控件尺寸和间距按 `130 DPI` 估算

如果你后面遇到的是“控件能创建但复杂页面开始内存吃紧”，通常第一批要看的就是这组参数，尤其是 `LV_MEM_SIZE`。

## 第二组：渲染能力相关参数

当前配置的渲染路线相当明确，核心思路是“先用通用软件渲染跑通，再按需开硬件加速”。

最关键的几个值是：

- `LV_USE_DRAW_SW 1`
- `LV_DRAW_SW_COMPLEX 1`
- `LV_DRAW_LAYER_SIMPLE_BUF_SIZE (24 * 1024)`
- `LV_DRAW_LAYER_MAX_MEMORY 0`
- `LV_DRAW_THREAD_STACK_SIZE (8 * 1024)`
- `LV_DRAW_SW_DRAW_UNIT_CNT 1`
- `LV_USE_DRAW_SW_COMPLEX_GRADIENTS 0`

可以把它们理解成下面这组取舍：

- 软件渲染默认开启
- 阴影、圆角、圆弧这类“复杂绘制”是打开的
- 复杂渐变先关掉，避免体积和复杂度继续上涨
- 分层绘制缓冲区目标大小是 `24KB`
- 图层总内存不上限，由外部实际内存情况决定
- 只开一个 draw unit，不走多线程并行绘制

同时，这份配置虽然默认走 `RGB565`，但软件渲染里保留了很多颜色格式支持，比如：

- `RGB565`
- `RGB888`
- `XRGB8888`
- `ARGB8888`
- `A8`
- `L8`

这说明当前配置更偏“通用兼容优先”，而不是“把体积裁到最小”。

如果你的目标是极限裁剪包体，往往要先从这些 `LV_DRAW_SW_SUPPORT_*` 开关下手。

## 第三组：日志、断言和性能分析

当前配置在稳定性和运行时开销之间做了一种比较保守的平衡：

- `LV_USE_LOG 0`
- `LV_USE_ASSERT_NULL 1`
- `LV_USE_ASSERT_MALLOC 1`
- `LV_USE_ASSERT_STYLE 0`
- `LV_USE_ASSERT_MEM_INTEGRITY 0`
- `LV_USE_ASSERT_OBJ 0`
- `LV_USE_SYSMON 0`
- `LV_USE_PROFILER 0`

这组取舍的含义是：

- 默认不打日志，避免把运行期噪声和性能开销直接带进去
- 保留最基础、性价比最高的空指针和内存分配断言
- 更重的对象一致性、内存完整性检查先关闭
- 性能监控、Profiler 也默认关闭

如果你现在处在“先把程序跑起来”的阶段，这样配很合理。

如果你进入问题定位阶段，最值得先打开的通常不是所有检查，而是：

- `LV_USE_LOG`
- 更高一级的 `LV_LOG_LEVEL`
- 必要时再开更重的 assert 或 profiler

## 第四组：功能模块到底编进了多少

LVGL 的体积差异，很大一部分不是来自核心，而是来自你把多少功能模块一起编进去了。

当前配置里，一些很常用的基础能力是打开的：

- `LV_USE_THEME_DEFAULT 1`
- `LV_USE_THEME_SIMPLE 1`
- `LV_USE_THEME_MONO 1`
- `LV_USE_FLEX 1`
- `LV_USE_GRID 1`
- `LV_USE_OBSERVER 1`

但很多扩展能力是关着的，例如：

- `LV_USE_FREETYPE 0`
- `LV_USE_TINY_TTF 0`
- `LV_USE_VECTOR_GRAPHIC 0`
- `LV_USE_SVG 0`
- `LV_USE_FFMPEG 0`
- `LV_USE_LODEPNG 0`
- `LV_USE_LIBPNG 0`
- `LV_USE_GIF 0`
- `LV_USE_FS_STDIO 0`
- `LV_USE_FS_POSIX 0`
- `LV_USE_FS_WIN32 0`

这说明当前配置不是“把生态全开”，而是优先保证：

- 常规控件和主题可用
- Flex / Grid 布局可用
- 响应式对象观察能力可用

但运行时字体、向量图、视频、通用文件系统这些更重的依赖链先不编进去。

如果你后面要做这些能力，记住一条就够了：

不是只改一个 `LV_USE_XXX 1` 就结束了，通常还要一起补齐对应三方库、头文件、链接项和运行环境依赖。

## 第五组：Linux 和桌面宿主相关参数

这组参数和当前仓库关系很大，因为仓库里本身就在做 Linux 宿主路线。

`lv_conf.h` 里的默认值是：

- `LV_USE_SDL 0`
- `LV_USE_X11 0`
- `LV_USE_WAYLAND 0`
- `LV_USE_LINUX_DRM 0`

但对于 Linux 设备侧基础驱动，这份配置又做了条件启用：

- 在 `__linux__` 下，`LV_USE_LINUX_FBDEV 1`
- 在 `__linux__` 下，`LV_USE_EVDEV 1`

这说明默认基线更像是：

- 优先保留 Linux 设备侧最基础的显示与输入链路
- 桌面宿主如 `SDL`、`X11`、`Wayland`、`DRM/KMS` 通过构建参数按需打开

而 `libs/build_lvgl.sh` 也确实是这么做的。它会根据 `fb`、`sdl`、`drm`、`wayland` 这些后端选择，分别传入：

- `CONFIG_LV_USE_LINUX_FBDEV`
- `CONFIG_LV_USE_SDL`
- `CONFIG_LV_USE_LINUX_DRM`
- `CONFIG_LV_USE_WAYLAND`

所以 Linux 宿主这部分，当前仓库采用的是“基线配置 + 构建时切换后端”的策略，而不是把所有宿主一次性全开。

## 第六组：示例和 Demo 为什么看起来开着，却又可能没被构建

这也是一个很典型的“看头文件会误判”的点。

在 `lv_conf.h` 里，当前配置是：

- `LV_BUILD_EXAMPLES 1`
- `LV_BUILD_DEMOS 1`
- `LV_USE_DEMO_WIDGETS 1`
- `LV_USE_DEMO_KEYPAD_AND_ENCODER 1`

但在仓库现有构建脚本里，又显式传了：

- `-DCONFIG_LV_BUILD_EXAMPLES=OFF`
- `-DCONFIG_LV_BUILD_DEMOS=OFF`

这意味着对当前构建流程来说，真正结果要以 CMake 覆盖为准。

简单说就是：

- `lv_conf.h` 里的基线能力允许你保留这些路径
- 构建脚本为了产出更干净的库，又把 examples / demos 目标关掉

这也是为什么单看 `lv_conf.h` 时，你很容易以为“Demo 明明开了”，但实际编译结果里却没有把那些目标真正构建出来。

## 除了 `lv_conf.h`，还有几类值得单独注意的构建参数

如果你把“编译参数”理解得更完整一点，会发现还有一些选项并不属于 LVGL 功能宏本身，而是构建产物策略。

比如当前仓库里的两个脚本就不完全一样：

- `build_lvgl.bat` 使用了 `BUILD_SHARED_LIBS=OFF`
- `build_lvgl.sh` 使用了 `BUILD_SHARED_LIBS=ON`

前者更偏静态库产物，后者更偏共享库产物。

另外 Windows 脚本还传了：

- `CONFIG_LV_USE_PRIVATE_API=ON`

这个选项更偏“安装和导出哪些头文件”，而不是某个渲染或控件功能是否启用。

所以当你说“我要改 LVGL 编译参数”时，最好先分清：

- 你是在改功能宏
- 还是在改宿主后端
- 还是在改最终库的产物形式

这三类不是一回事。

## 实际调整时，建议按这个顺序动手

如果你想少走弯路，最稳的顺序通常是：

1. 先确定宿主和平台。
2. 再定颜色深度与内存池。
3. 再定是否需要复杂绘制。
4. 然后才开字体、图片、向量图、视频这类扩展库。
5. 最后再决定是否打开日志、Profiler、SysMon 这类调试能力。

原因很简单：

- 宿主和平台决定你到底需要哪些驱动
- 颜色深度和内存决定你的资源基线
- 绘制能力决定渲染成本
- 三方库最容易把依赖链和包体一起拉大
- 调试功能适合最后按需打开，而不是一开始全开

## 几个常见场景的调整建议

### 如果你在做设备侧最小化构建

优先看这些：

- `LV_COLOR_DEPTH`
- `LV_MEM_SIZE`
- `LV_DRAW_SW_SUPPORT_*`
- 关闭不用的主题、文件系统和图片解码器
- 关闭不用的桌面宿主

这时目标通常不是“功能最全”，而是“先把显示、输入和核心控件稳定跑通”。

### 如果你在做 Linux 桌面调试

优先看这些：

- 目标宿主是 `SDL`、`X11` 还是 `Wayland`
- 对应的 `CONFIG_LV_USE_*` 是否真的在 CMake 里传进去了
- 是否需要临时打开 `LV_USE_LOG`

这时最容易出问题的不是控件本身，而是宿主后端没有按你以为的方式生效。

### 如果你要上运行时字体、SVG 或视频

优先看这些：

- `LV_USE_FREETYPE`
- `LV_USE_TINY_TTF`
- `LV_USE_VECTOR_GRAPHIC`
- `LV_USE_SVG`
- `LV_USE_FFMPEG`

但一定要同步核对三方库依赖，不要只改宏。

## 结语

LVGL 的编译参数看起来像一张很长的表，但真正理解它，不是把所有宏背下来，而是先建立三层认识：

- 哪些是 `lv_conf.h` 基线配置
- 哪些会被 `CONFIG_LV_*` 覆盖
- 哪些其实属于构建产物策略

如果你先把这三层分开，再去改颜色、内存、驱动、调试和扩展库，后面的配置会清晰很多。

## 延伸阅读

- [LVGL 官方 `lv_conf.h` API 页面](https://docs.lvgl.io/master/API/lv_conf_h.html)
- [LVGL 官方集成文档入口](https://docs.lvgl.io/master/integration/index.html)
