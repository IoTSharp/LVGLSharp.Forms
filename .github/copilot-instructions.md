# Copilot Instructions

## Project Guidelines
- 在该仓库中优先按 WinForms 标准生命周期/方法实现（如 Show、CreateHandle、Application.Run），避免暴露非标准 API（如 Form.Init）。
- 该仓库后续实现要求全部基于 LVGL，不使用任何 Windows/Win32 相关实现或 API。
- `LVGLSharp.Forms` 项目本身不再追求设计器可打开；移除其中所有 `System.Drawing` 依赖，并保持跨平台封装。只有 `WinFormsDemo` 需要支持设计器；不要给 `LVGLSharp.Forms` 增加 `net10.0-windows` 目标。
- Demo 界面布局统一采用 `WinFormsDemo` 风格：外层使用一个 `TableLayoutPanel` 只做纵向分区，内部每一行都放一个 `FlowLayoutPanel` 承载实际控件。
- Demo 的主 `TableLayoutPanel` 行高使用固定绝对值，不使用百分比行高；不要把业务控件直接挂到主 `TableLayoutPanel` 上。
