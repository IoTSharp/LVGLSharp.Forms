namespace WinFormsVncDemo;

public partial class frmMain : Form
{
    private Label? _statusLabel;
    private TextBox? _messageInput;

    public frmMain()
    {
        InitializeComponent();
    }

    private void frmMain_Load(object sender, EventArgs e)
    {
        lblTitle.Text = "LVGLSharp WinForms Demo";
        Text = "WinFormsVncDemo";
        lvglHostPanel.BackColor = new Color(18, 18, 18);

        BuildDemoSurface();
    }

    private void BuildDemoSurface()
    {
        if (_statusLabel is not null)
        {
            return;
        }

        var introLabel = new Label
        {
            Text = "这个窗口内容会通过外部配置的远程宿主发布。",
            Location = new Point(18, 18),
            Size = new Size(580, 28),
            ForeColor = new Color(255, 255, 255),
        };

        _messageInput = new TextBox
        {
            Text = "hello from WinForms over VNC",
            PlaceholderText = "输入一些文字，然后点右侧按钮",
            Location = new Point(18, 64),
            Size = new Size(420, 36),
        };

        var echoButton = new Button
        {
            Text = "更新状态",
            Location = new Point(456, 64),
            Size = new Size(140, 36),
        };
        echoButton.Click += EchoButton_Click;

        _statusLabel = new Label
        {
            Text = "状态：等待 VNC 客户端交互",
            Location = new Point(18, 122),
            Size = new Size(578, 28),
            ForeColor = new Color(255, 255, 255),
        };

        var hintLabel = new Label
        {
            Text = "提示：连接远程客户端后，可以键入文本并点击按钮验证输入事件是否正常。",
            Location = new Point(18, 164),
            Size = new Size(578, 52),
            ForeColor = new Color(220, 220, 220),
        };

        lvglHostPanel.Controls.Add(introLabel);
        lvglHostPanel.Controls.Add(_messageInput);
        lvglHostPanel.Controls.Add(echoButton);
        lvglHostPanel.Controls.Add(_statusLabel);
        lvglHostPanel.Controls.Add(hintLabel);
    }

    private void EchoButton_Click(object? sender, EventArgs e)
    {
        if (_statusLabel is null || _messageInput is null)
        {
            return;
        }

        var value = string.IsNullOrWhiteSpace(_messageInput.Text)
            ? "<空文本>"
            : _messageInput.Text;
        _statusLabel.Text = $"状态：最近一次提交 -> {value}";
    }
}
