using System.IO;
using LVGLSharp.Runtime.MacOs;

namespace MacOsAotDemo;

public partial class frmMacOsAotDemo : Form
{
    private readonly MacOsViewOptions _viewOptions;
    private readonly MacOsView _hostView;

    public frmMacOsAotDemo(MacOsViewOptions viewOptions)
    {
        _viewOptions = viewOptions;
        _hostView = new MacOsView(viewOptions);
        InitializeComponent();
    }

    private void frmMacOsAotDemo_Load(object? sender, EventArgs e)
    {
        txtTitle.Text = _viewOptions.Title;
        txtSize.Text = $"{_viewOptions.Width} x {_viewOptions.Height}";
        txtDpi.Text = _viewOptions.Dpi.ToString("0.##");
        txtRuntime.Text = "LVGLSharp.Forms + LVGLSharp.Runtime.MacOs";
        txtState.Text = _hostView.Diagnostics.ToString();
        txtHostContext.Text = _hostView.HostContext.ToString();
        LoadSampleImage();
        ApplyLvglSharpLayout();
    }

    private void frmMacOsAotDemo_SizeChanged(object? sender, EventArgs e)
    {
        ApplyLvglSharpLayout();
    }

    private void ApplyLvglSharpLayout()
    {
        const int topRowHeight = 84;
        const int bottomRowHeight = 104;
        const int minimumContentHeight = 180;
        const int minimumContentWidth = 180;
        const int contentPadding = 8;

        int contentHeight = Math.Max(minimumContentHeight, ClientSize.Height - topRowHeight - bottomRowHeight);
        tpMain.RowStyles[1] = new RowStyle(SizeType.Absolute, contentHeight);
        tpMain.PerformLayout();

        int previewWidth = Math.Max(minimumContentWidth, pnlContent.ClientSize.Width - (contentPadding * 2));
        int previewHeight = Math.Max(minimumContentHeight, pnlContent.ClientSize.Height - (contentPadding * 2));
        picPreview.Size = new Size(previewWidth, previewHeight);
    }

    private void LoadSampleImage()
    {
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, "Assets", "lvgl128x128.png"),
            Path.Combine(AppContext.BaseDirectory, "Assets", "lvgl64x64.png"),
        ];

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                picPreview.Load(candidate);
                lblStatus.Text = $"已加载示例图像: {Path.GetFileName(candidate)}";
                return;
            }
        }

        lblStatus.Text = "未找到示例图像";
    }

    private void btnShowSummary_Click(object? sender, EventArgs e)
    {
        txtState.Text = _hostView.Diagnostics.ToString();
        txtHostContext.Text = _hostView.HostContext.ToString();
        lblStatus.Text = "已刷新宿主摘要";
    }
}