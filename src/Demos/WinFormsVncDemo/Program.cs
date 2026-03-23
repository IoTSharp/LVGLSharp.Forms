using LVGLSharp.Forms;
using LVGLSharp.Runtime.Remote;
using LVGLSharp.Runtime.Remote.Vnc;
using System;
using System.Threading;

namespace WinFormsVncDemo;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);


        // LVGLSharp 布局：外层 TableLayoutPanel 纵向分区，每行一个 FlowLayoutPanel 承载控件
        var form = new Form { Text = "LVGLSharp VNC PictureBox Demo", Width = 800, Height = 600 };
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
            BackColor = Color.White,
        };
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));   // 标题
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // 输入区
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));  // 图片区
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // 按钮区

        // 第一行：标题
        var row1 = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.White };
        var label = new Label { Text = "图片演示：", AutoSize = true, Font = new Font("微软雅黑", 14, FontStyle.Bold), Margin = new Padding(10, 8, 0, 0) };
        row1.Controls.Add(label);
        table.Controls.Add(row1, 0, 0);

        // 第二行：输入区
        var rowInput = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.White };
        var inputLabel = new Label { Text = "输入：", AutoSize = true, Font = new Font("微软雅黑", 10), Margin = new Padding(20, 15, 0, 0) };
        var inputBox = new TextBox { Width = 300, Font = new Font("微软雅黑", 10), Margin = new Padding(10, 10, 0, 0) };
        var inputEcho = new Label { Text = "", AutoSize = true, Font = new Font("微软雅黑", 10), Margin = new Padding(20, 15, 0, 0), ForeColor = Color.Gray };
        var btnEcho = new Button { Text = "显示输入", Width = 100, Height = 30, Margin = new Padding(10, 10, 0, 0) };
        btnEcho.Click += (s, e) => inputEcho.Text = inputBox.Text;
        rowInput.Controls.Add(inputLabel);
        rowInput.Controls.Add(inputBox);
        rowInput.Controls.Add(btnEcho);
        rowInput.Controls.Add(inputEcho);
        table.Controls.Add(rowInput, 0, 1);

        // 第三行：图片区
        var row2 = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.White };
        var pictureBox = new PictureBox
        {
            Width = 320,
            Height = 200,
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom,
            Margin = new Padding(20, 5, 0, 0)
        };
        row2.Controls.Add(pictureBox);
        table.Controls.Add(row2, 0, 2);

        // 第四行：按钮区
        var row3 = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.White };
        var btnLoad = new Button { Text = "加载图片", Width = 120, Height = 40, Margin = new Padding(20, 10, 0, 0) };
        var btnClear = new Button { Text = "清空", Width = 120, Height = 40, Margin = new Padding(20, 10, 0, 0) };
        btnLoad.Click += (s, e) =>
        {
            using var ofd = new OpenFileDialog { Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp|所有文件|*.*" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBox.Image = Image.FromFile(ofd.FileName);
            }
        };
        btnClear.Click += (s, e) => pictureBox.Image = null;
        row3.Controls.Add(btnLoad);
        row3.Controls.Add(btnClear);
        table.Controls.Add(row3, 0, 3);

        form.Controls.Add(table);
        form.Show();

        // 创建 RemoteFrameSource
        var frameSource = new WinFormsRemoteFrameSource(form);
        // 创建 VNC 传输
        var vncOptions = new VncSessionOptions { Host = "0.0.0.0", Port = 5900, Width = form.Width, Height = form.Height };
        var vncTransport = new VncTransport(vncOptions);
        vncTransport.Start();

        // 绑定 RemoteRuntimeSession
        var session = new RemoteRuntimeSession(frameSource, vncTransport, vncOptions);

        Console.WriteLine($"VNC 服务已启动，监听 {vncOptions.Host}:{vncOptions.Port}，请用 VNC 客户端连接。");
        Console.WriteLine("按 Ctrl+C 退出。");

        // 简单帧推送循环
        // 帧推送由 runtime.remote 内部自动管理，无需手动循环

        Application.Run(form);
    }
}
