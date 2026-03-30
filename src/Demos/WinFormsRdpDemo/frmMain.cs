namespace WinFormsRdpDemo;

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
        lblTitle.Text = "LVGLSharp WinForms RDP Demo";
        Text = "WinFormsRdpDemo";
        lvglHostPanel.BackColor = new Color(24, 30, 42);

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
            Text = "This window currently runs through RdpView so the RDP path has a dedicated demo entry point.",
            Location = new Point(18, 18),
            Size = new Size(580, 42),
            ForeColor = new Color(255, 255, 255),
        };

        _messageInput = new TextBox
        {
            Text = "hello from WinForms over RDP",
            PlaceholderText = "Type some text and then click the button on the right.",
            Location = new Point(18, 72),
            Size = new Size(420, 36),
        };

        var echoButton = new Button
        {
            Text = "Update status",
            Location = new Point(456, 72),
            Size = new Size(140, 36),
        };
        echoButton.Click += EchoButton_Click;

        _statusLabel = new Label
        {
            Text = "Status: the RDP transport is wired into the lifecycle and the protocol handshake is still in progress.",
            Location = new Point(18, 128),
            Size = new Size(578, 42),
            ForeColor = new Color(255, 255, 255),
        };

        var hintLabel = new Label
        {
            Text = "Tip: this demo currently validates RdpView registration, window hosting, and the later remote-host integration path.",
            Location = new Point(18, 178),
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
            ? "<empty>"
            : _messageInput.Text;
        _statusLabel.Text = $"Status: latest local interaction -> {value}";
    }
}
