## Unshipped

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-----
LVGL001 | Layout | Warning | Enforces LVGLSharp layout FlowLayoutPanel rows inside main TableLayoutPanel layouts.
LVGL002 | Layout | Warning | Disallows percent-based RowStyle usage in LVGLSharp layout designer layouts.
LVGL003 | Usage | Warning | Reports when LVGLSharp.Forms is referenced without LVGLSharp.Runtime.Windows or LVGLSharp.Runtime.Linux.
LVGL004 | Style | Warning | Reports file-level LVGLSharp using directives inside remote demos that should be moved to project-level global usings.
LVGL005 | Style | Warning | Reports fully qualified LVGLSharp type names inside remote demos that should rely on the shared global-using style.
