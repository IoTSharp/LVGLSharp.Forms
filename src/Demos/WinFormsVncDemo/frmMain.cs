using System;
using LVGLSharp.Drawing;
using LVGLSharp.Runtime.Remote.Vnc;

namespace WinFormsVncDemo
{
    public partial class frmMain : Form
    {
        private LVGLSharp.Runtime.Remote.VncView? _remoteView;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            var options = new VncSessionOptions
            {
                Host = "0.0.0.0",
                Port = 5900,
                Width = 640,
                Height = 400,
            };

            _remoteView = new LVGLSharp.Runtime.Remote.VncView(options);
            _remoteView.Open();

            lblTitle.Text = $"LVGLSharp VNC Demo - {options.Host}:{options.Port}";
            lvglHostPanel.BackColor = new Color(18, 18, 18);
        }
    }
}
