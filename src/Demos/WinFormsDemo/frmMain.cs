using System.IO;
using System.IO.Ports;
using System.Text;

namespace WinFormsDemo
{
    public partial class frmMain : Form
    {
        private static readonly int[] s_baudRates = [9600, 19200, 38400, 57600, 115200];

        private SerialPort? _serialPort;

        public frmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConfigureDemoControls();
            WireEvents();
            RefreshBaudRates();
            RefreshSerialPorts();
            LoadSampleImage(logResult: false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            recv_textarea.Text = string.Empty;
        }

        private void send_btn_Click(object sender, EventArgs e)
        {
            string payload = textBox1.Text?.TrimEnd('\r', '\n') ?? string.Empty;
            if (string.IsNullOrWhiteSpace(payload))
            {
                AppendLog("系统: 请输入发送内容");
                return;
            }

            if (_serialPort?.IsOpen != true)
            {
                AppendLog("系统: 串口未打开，未发送数据");
                return;
            }

            if (checkBox1.Checked)
            {
                payload += Environment.NewLine;
            }

            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(payload);
                _serialPort.Write(bytes, 0, bytes.Length);
                AppendLog($"TX: {FormatBytesForDisplay(bytes)}");

                Thread.Sleep(150);

                if (_serialPort.BytesToRead > 0)
                {
                    byte[] receiveBuffer = new byte[_serialPort.BytesToRead];
                    int receivedLength = _serialPort.Read(receiveBuffer, 0, receiveBuffer.Length);
                    if (receivedLength > 0)
                    {
                        if (receivedLength != receiveBuffer.Length)
                        {
                            Array.Resize(ref receiveBuffer, receivedLength);
                        }

                        AppendLog($"RX: {FormatBytesForDisplay(receiveBuffer)}");
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog($"系统: 发送失败 - {ex.Message}");
            }
        }

        private void ConfigureDemoControls()
        {
            port_dropdown.DropDownStyle = ComboBoxStyle.DropDownList;
            baud_dropdown.DropDownStyle = ComboBoxStyle.DropDownList;

            button1.Text = "复制接收";
            button2.Text = "填充示例";
            button3.Text = "加载Logo";
            button4.Text = "清空图片";
            checkBox1.Text = "发送换行";
            radioButton1.Text = "缩放预览";
            checkBox2.Text = "拉伸预览";
            hex_switch.Text = "HEX显示";

            checkBox1.Checked = true;
            radioButton1.Checked = true;
            checkBox2.Checked = false;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void WireEvents()
        {
            ref_btn.Click += ref_btn_Click;
            open_btn.Click += open_btn_Click;
            button1.Click += copy_recv_btn_Click;
            button2.Click += fill_sample_btn_Click;
            button3.Click += load_logo_btn_Click;
            button4.Click += clear_logo_btn_Click;
            checkBox2.CheckedChanged += preview_mode_CheckedChanged;
            radioButton1.Click += preview_zoom_Click;
        }

        private void RefreshBaudRates()
        {
            string? previousSelection = baud_dropdown.SelectedItem?.ToString();

            baud_dropdown.Items.Clear();
            foreach (int baudRate in s_baudRates)
            {
                baud_dropdown.Items.Add(baudRate.ToString());
            }

            if (!string.IsNullOrWhiteSpace(previousSelection) &&
                baud_dropdown.Items.Contains(previousSelection))
            {
                baud_dropdown.SelectedItem = previousSelection;
            }
            else
            {
                int defaultIndex = Array.IndexOf(s_baudRates, 115200);
                baud_dropdown.SelectedIndex = defaultIndex >= 0 ? defaultIndex : 0;
            }
        }

        private void RefreshSerialPorts()
        {
            string? previousSelection = port_dropdown.SelectedItem?.ToString();
            string[] portNames;

            try
            {
                portNames = SerialPort.GetPortNames()
                    .OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch (Exception ex)
            {
                AppendLog($"系统: 刷新串口失败 - {ex.Message}");
                return;
            }

            port_dropdown.Items.Clear();
            foreach (string portName in portNames)
            {
                port_dropdown.Items.Add(portName);
            }

            if (!string.IsNullOrWhiteSpace(previousSelection) &&
                port_dropdown.Items.Contains(previousSelection))
            {
                port_dropdown.SelectedItem = previousSelection;
            }
            else if (port_dropdown.Items.Count > 0)
            {
                port_dropdown.SelectedIndex = 0;
            }
            else
            {
                port_dropdown.SelectedIndex = -1;
            }

            open_btn.Enabled = port_dropdown.Items.Count > 0;
            AppendLog(port_dropdown.Items.Count > 0
                ? $"系统: 已发现 {port_dropdown.Items.Count} 个串口"
                : "系统: 未发现可用串口");
        }

        private void ref_btn_Click(object? sender, EventArgs e)
        {
            RefreshSerialPorts();
        }

        private void open_btn_Click(object? sender, EventArgs e)
        {
            if (_serialPort?.IsOpen == true)
            {
                CloseSerialPort(logResult: true);
                return;
            }

            string? portName = port_dropdown.SelectedItem?.ToString();
            string? baudText = baud_dropdown.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(portName))
            {
                AppendLog("系统: 请选择串口");
                return;
            }

            if (!int.TryParse(baudText, out int baudRate))
            {
                AppendLog("系统: 请选择有效的波特率");
                return;
            }

            try
            {
                CloseSerialPort(logResult: false);

                _serialPort = new SerialPort(portName, baudRate)
                {
                    Encoding = Encoding.UTF8,
                    ReadTimeout = 300,
                    WriteTimeout = 300,
                };

                _serialPort.Open();
                open_btn.Text = "关闭串口";
                AppendLog($"系统: 已打开串口 {portName} @ {baudRate}");
            }
            catch (Exception ex)
            {
                CloseSerialPort(logResult: false);
                AppendLog($"系统: 打开串口失败 - {ex.Message}");
            }
        }

        private void copy_recv_btn_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(recv_textarea.Text))
            {
                AppendLog("系统: 接收区为空，无内容可复制");
                return;
            }

            Clipboard.SetText(recv_textarea.Text);
            AppendLog("系统: 接收内容已复制到剪贴板");
        }

        private void fill_sample_btn_Click(object? sender, EventArgs e)
        {
            textBox1.Text = "LVGLSharp Demo";
        }

        private void load_logo_btn_Click(object? sender, EventArgs e)
        {
            LoadSampleImage(logResult: true);
        }

        private void clear_logo_btn_Click(object? sender, EventArgs e)
        {
            pictureBox1.ImageLocation = null;
            pictureBox1.Image = null;
            AppendLog("系统: 已清空示例图像");
        }

        private void preview_mode_CheckedChanged(object? sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                radioButton1.Checked = false;
            }
            else if (!radioButton1.Checked)
            {
                radioButton1.Checked = true;
            }

            ApplyPreviewMode();
        }

        private void preview_zoom_Click(object? sender, EventArgs e)
        {
            radioButton1.Checked = true;
            checkBox2.Checked = false;
            ApplyPreviewMode();
        }

        private void ApplyPreviewMode()
        {
            pictureBox1.SizeMode = checkBox2.Checked
                ? PictureBoxSizeMode.StretchImage
                : PictureBoxSizeMode.Zoom;
        }

        private bool LoadSampleImage(bool logResult)
        {
            string? imagePath = ResolveSampleImagePath();
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                if (logResult)
                {
                    AppendLog("系统: 未找到示例图像资源");
                }

                return false;
            }

            try
            {
                pictureBox1.Load(imagePath);
                ApplyPreviewMode();

                if (logResult)
                {
                    AppendLog($"系统: 已加载示例图像 {Path.GetFileName(imagePath)}");
                }

                return true;
            }
            catch (Exception ex)
            {
                if (logResult)
                {
                    AppendLog($"系统: 加载示例图像失败 - {ex.Message}");
                }

                return false;
            }
        }

        private static string? ResolveSampleImagePath()
        {
            string[] localCandidates =
            [
                Path.Combine(AppContext.BaseDirectory, "Assets", "lvgl.png"),
                Path.Combine(AppContext.BaseDirectory, "lvgl.png"),
            ];

            foreach (string candidate in localCandidates)
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            string currentDirectory = AppContext.BaseDirectory;
            for (int depth = 0; depth < 8; depth++)
            {
                string candidate = Path.Combine(currentDirectory, "src", "Share", "lvgl.png");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                string? parentDirectory = Directory.GetParent(currentDirectory)?.FullName;
                if (string.IsNullOrWhiteSpace(parentDirectory))
                {
                    break;
                }

                currentDirectory = parentDirectory;
            }

            return null;
        }

        private void CloseSerialPort(bool logResult)
        {
            if (_serialPort is null)
            {
                open_btn.Text = "打开串口";
                return;
            }

            string portName = _serialPort.PortName;

            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }
            catch (Exception ex)
            {
                if (logResult)
                {
                    AppendLog($"系统: 关闭串口失败 - {ex.Message}");
                }
            }
            finally
            {
                _serialPort.Dispose();
                _serialPort = null;
                open_btn.Text = "打开串口";
            }

            if (logResult)
            {
                AppendLog($"系统: 已关闭串口 {portName}");
            }
        }

        private string FormatBytesForDisplay(byte[] buffer)
        {
            if (buffer.Length == 0)
            {
                return string.Empty;
            }

            return hex_switch.Checked
                ? BitConverter.ToString(buffer).Replace("-", " ")
                : Encoding.UTF8.GetString(buffer);
        }

        private void AppendLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(recv_textarea.Text) || recv_textarea.Text == "接收的数据...")
            {
                recv_textarea.Text = message;
                return;
            }

            recv_textarea.Text = $"{recv_textarea.Text}{Environment.NewLine}{message}";
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            CloseSerialPort(logResult: false);
            base.OnHandleDestroyed(e);
        }
    }
}
