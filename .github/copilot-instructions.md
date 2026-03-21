# Copilot Instructions

## Project Guidelines
- 在该仓库中优先按 WinForms 标准生命周期/方法实现（如 Show、CreateHandle、Application.Run），避免暴露非标准 API（如 Form.Init）。
- 该仓库后续实现要求全部基于 LVGL，不使用任何 Windows/Win32 相关实现或 API。
- `LVGLSharp.Forms` 项目本身不再追求设计器可打开；移除其中所有 `System.Drawing` 依赖，并保持跨平台封装。只有 `WinFormsDemo` 需要支持设计器；不要给 `LVGLSharp.Forms` 增加 `net10.0-windows` 目标。
- 该仓库中的此类界面布局统一称为 `LVGLSharp 布局`：外层使用一个 `TableLayoutPanel` 只做纵向分区，内部每一行都放一个 `FlowLayoutPanel` 承载实际控件。不要在分析器、说明或回复中称其为 WinFormsDemo 风格或 Demo 布局。
- 采用 `LVGLSharp 布局` 时，主 `TableLayoutPanel` 行高使用固定绝对值，不使用百分比行高；不要把业务控件直接挂到主 `TableLayoutPanel` 上。

## AOT Compatibility
- 在该仓库中，必须严格执行 AOT 兼容性：不要仅通过注释来抑制修剪器/AOT 警告；应替换基于反射的运行时激活路径，使用 AOT 安全的显式实现。

## Recent Learnings
- 排查 Linux/X11 黑屏时，先区分是“窗口已创建但未绘制”还是“控件树创建阶段卡住”。如果 `Load` 都没走到，优先检查控件树创建路径，不要先把原因归到字体解析。
- 在 X11/LVGL 下做多页整屏界面时，不要在启动阶段一次性把所有隐藏整页都挂到同一个 viewport。首页先挂载，其他页按需懒加载或渐进预热，否则可能出现首屏黑屏或被隐藏页覆盖。
- `Label`、纯展示 `Panel` 这类被动控件不要默认桥接全量 LVGL 事件；只给真正需要输入/手势的控件开启事件桥接。这样能显著降低首屏创建成本。
- 动态 UI 刷新优先挂到主消息循环节拍里，不要从后台线程直接持续推进 LVGL/Forms 控件状态；跨线程推进在 Linux/X11 路径上更容易造成异常渲染或刷新时序问题。
- 验证 X11 显示问题时，不能只看窗口标题栏是否出现；应直接抓取 viewport 区域截图确认实际像素内容。
