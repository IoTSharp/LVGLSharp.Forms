using LVGLSharp.Forms;

namespace PictureBoxDemo
{
    public partial class frmPictureBoxDemo : Form
    {
        private int _currentRotationAngle = 0;
        private uint _currentZoom = 256; // 256 = 100%

        public frmPictureBoxDemo()
        {
            InitializeComponent();
        }

        private void frmPictureBoxDemo_Load(object? sender, EventArgs e)
        {
            InitializePictureBox();
            ApplyWinFormsDemoLayout();
        }

        private void frmPictureBoxDemo_SizeChanged(object? sender, EventArgs e)
        {
            ApplyWinFormsDemoLayout();
        }

        private void InitializePictureBox()
        {
            picMain.SizeMode = PictureBoxSizeMode.Zoom;
            picMain.SetAntiAlias(true);
            _currentZoom = 256;
            _currentRotationAngle = 0;
            
            cmbSizeMode.Items.Add("Normal");
            cmbSizeMode.Items.Add("StretchImage");
            cmbSizeMode.Items.Add("AutoSize");
            cmbSizeMode.Items.Add("CenterImage");
            cmbSizeMode.Items.Add("Zoom");
            cmbSizeMode.SelectedIndex = 4;
            
            lblStatus.Text = "就绪";
        }

        private void ApplyWinFormsDemoLayout()
        {
            const int topRowHeight = 60;
            const int bottomRowHeight = 60;
            const int statusRowHeight = 30;
            const int minimumContentHeight = 180;
            const int minimumContentWidth = 180;

            int contentHeight = Math.Max(minimumContentHeight, ClientSize.Height - topRowHeight - bottomRowHeight - statusRowHeight);
            tpMain.RowStyles[1] = new RowStyle(SizeType.Absolute, contentHeight);
            tpMain.PerformLayout();

            int previewWidth = Math.Max(minimumContentWidth, pnlContent.ClientSize.Width - 6);
            int previewHeight = Math.Max(minimumContentHeight, pnlContent.ClientSize.Height - 6);
            picMain.Size = new Size(previewWidth, previewHeight);
            lblStatus.Size = new Size(Math.Max(120, pnlStatus.ClientSize.Width - 6), lblStatus.Height);
        }

        private void btnLoadImage_Click(object? sender, EventArgs e)
        {
            try
            {
                string imagePath = txtImagePath.Text.Trim();
                if (string.IsNullOrEmpty(imagePath))
                {
                    lblStatus.Text = "请输入图像路径";
                    return;
                }

                lblStatus.Text = "正在加载图像...";
                
#if NET10_0
                picMain.Load(imagePath);
#else
                if (File.Exists(imagePath))
                {
                    picMain.Image?.Dispose();
                    picMain.Image = Image.FromFile(imagePath);
                    picMain.UpdateOriginalImage();
                }
                else
                {
                    lblStatus.Text = $"文件不存在: {imagePath}";
                    return;
                }
#endif
                
                _currentRotationAngle = 0;
                _currentZoom = 256;
                picMain.SetRotation(_currentRotationAngle);
                picMain.SetZoom(_currentZoom);
                
                lblStatus.Text = $"图像已加载: {imagePath}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"加载失败: {ex.Message}";
            }
        }

        private void btnRotateLeft_Click(object? sender, EventArgs e)
        {
            _currentRotationAngle = (_currentRotationAngle - 90 + 360) % 360;
            picMain.SetRotation(_currentRotationAngle);
            UpdateStatusLabel();
        }

        private void btnRotateRight_Click(object? sender, EventArgs e)
        {
            _currentRotationAngle = (_currentRotationAngle + 90) % 360;
            picMain.SetRotation(_currentRotationAngle);
            UpdateStatusLabel();
        }

        private void btnZoomIn_Click(object? sender, EventArgs e)
        {
            _currentZoom = (uint)(_currentZoom * 1.2);
            if (_currentZoom > 2048) _currentZoom = 2048;
            picMain.SetZoom(_currentZoom);
            UpdateStatusLabel();
        }

        private void btnZoomOut_Click(object? sender, EventArgs e)
        {
            _currentZoom = (uint)(_currentZoom * 0.8);
            if (_currentZoom < 64) _currentZoom = 64;
            picMain.SetZoom(_currentZoom);
            UpdateStatusLabel();
        }

        private void btnReset_Click(object? sender, EventArgs e)
        {
            _currentRotationAngle = 0;
            _currentZoom = 256;
            picMain.SetRotation(_currentRotationAngle);
            picMain.SetZoom(_currentZoom);
            picMain.SizeMode = PictureBoxSizeMode.Zoom;
            UpdateStatusLabel();
        }

        private void cmbSizeMode_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int index = cmbSizeMode.SelectedIndex;
            picMain.SizeMode = index switch
            {
                0 => PictureBoxSizeMode.Normal,
                1 => PictureBoxSizeMode.StretchImage,
                2 => PictureBoxSizeMode.AutoSize,
                3 => PictureBoxSizeMode.CenterImage,
                4 => PictureBoxSizeMode.Zoom,
                _ => PictureBoxSizeMode.Zoom
            };
            UpdateStatusLabel();
        }

        private void chkAntiAlias_CheckedChanged(object? sender, EventArgs e)
        {
            picMain.SetAntiAlias(chkAntiAlias.Checked);
            lblStatus.Text = $"抗锯齿: {(chkAntiAlias.Checked ? "开启" : "关闭")}";
        }

        private void UpdateStatusLabel()
        {
            int zoomPercent = (int)(_currentZoom * 100 / 256);
            lblStatus.Text = $"模式: {picMain.SizeMode} | 旋转: {_currentRotationAngle}° | 缩放: {zoomPercent}% | 抗锯齿: {(chkAntiAlias.Checked ? "开启" : "关闭")}";
        }
    }
}

