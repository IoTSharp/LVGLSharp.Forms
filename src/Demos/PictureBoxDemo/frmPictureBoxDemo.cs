using LVGLSharp.Forms;

#if PICTUREBOXDEMO_WINFORMS
using System.Drawing;
using System.Drawing.Drawing2D;
#endif

namespace PictureBoxDemo
{
    public partial class frmPictureBoxDemo : Form
    {
        private int _currentRotationAngle;
        private uint _currentZoom = 256;
        private bool _antiAliasEnabled = true;

#if PICTUREBOXDEMO_WINFORMS
        private Image? _winFormsOriginalImage;
#endif

        public frmPictureBoxDemo()
        {
            InitializeComponent();
        }

        private void frmPictureBoxDemo_Load(object? sender, EventArgs e)
        {
            InitializePictureBox();
            ApplyLvglSharpLayout();
        }

        private void frmPictureBoxDemo_SizeChanged(object? sender, EventArgs e)
        {
            ApplyLvglSharpLayout();
        }

        private void InitializePictureBox()
        {
            picMain.SizeMode = PictureBoxSizeMode.Zoom;
            ApplyPictureBoxAntiAlias(true);
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

        private void ApplyLvglSharpLayout()
        {
            const int topRowHeight = 54;
            const int bottomRowHeight = 52;
            const int statusRowHeight = 24;
            const int minimumContentHeight = 180;
            const int minimumContentWidth = 180;
            const int contentPadding = 8;

            int contentHeight = Math.Max(minimumContentHeight, ClientSize.Height - topRowHeight - bottomRowHeight - statusRowHeight);
            tpMain.RowStyles[1] = new RowStyle(SizeType.Absolute, contentHeight);
            tpMain.PerformLayout();

            int previewWidth = Math.Max(minimumContentWidth, pnlContent.ClientSize.Width - (contentPadding * 2));
            int previewHeight = Math.Max(minimumContentHeight, pnlContent.ClientSize.Height - (contentPadding * 2));
            picMain.Size = new Size(previewWidth, previewHeight);
            lblStatus.Size = new Size(Math.Max(120, pnlStatus.ClientSize.Width - 12), lblStatus.Height);
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
                    var previousImage = picMain.Image;
                    picMain.Image = Image.FromFile(imagePath);

                    if (previousImage is not null)
                    {
                        previousImage.Dispose();
                    }
                }
                else
                {
                    lblStatus.Text = $"文件不存在: {imagePath}";
                    return;
                }
#endif

                _currentRotationAngle = 0;
                _currentZoom = 256;
                CapturePictureBoxSourceImage();
                ApplyPictureBoxTransforms();

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
            ApplyPictureBoxTransforms();
            UpdateStatusLabel();
        }

        private void btnRotateRight_Click(object? sender, EventArgs e)
        {
            _currentRotationAngle = (_currentRotationAngle + 90) % 360;
            ApplyPictureBoxTransforms();
            UpdateStatusLabel();
        }

        private void btnZoomIn_Click(object? sender, EventArgs e)
        {
            _currentZoom = (uint)(_currentZoom * 1.2);
            if (_currentZoom > 2048)
            {
                _currentZoom = 2048;
            }

            ApplyPictureBoxTransforms();
            UpdateStatusLabel();
        }

        private void btnZoomOut_Click(object? sender, EventArgs e)
        {
            _currentZoom = (uint)(_currentZoom * 0.8);
            if (_currentZoom < 64)
            {
                _currentZoom = 64;
            }

            ApplyPictureBoxTransforms();
            UpdateStatusLabel();
        }

        private void btnReset_Click(object? sender, EventArgs e)
        {
            _currentRotationAngle = 0;
            _currentZoom = 256;
            picMain.SizeMode = PictureBoxSizeMode.Zoom;
            ApplyPictureBoxTransforms();
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
                _ => PictureBoxSizeMode.Zoom,
            };

            UpdateStatusLabel();
        }

        private void chkAntiAlias_CheckedChanged(object? sender, EventArgs e)
        {
            ApplyPictureBoxAntiAlias(chkAntiAlias.Checked);
            lblStatus.Text = $"抗锯齿: {(chkAntiAlias.Checked ? "开启" : "关闭")}";
        }

        private void ApplyPictureBoxAntiAlias(bool enabled)
        {
            _antiAliasEnabled = enabled;
            ApplyPictureBoxTransforms();
        }

        private void ApplyPictureBoxTransforms()
        {
#if PICTUREBOXDEMO_WINFORMS
            if (_winFormsOriginalImage is null)
            {
                return;
            }

            if (_currentRotationAngle == 0 && _currentZoom == 256)
            {
                if (!ReferenceEquals(picMain.Image, _winFormsOriginalImage))
                {
                    var previousImage = picMain.Image;
                    picMain.Image = _winFormsOriginalImage;

                    if (previousImage is not null && !ReferenceEquals(previousImage, _winFormsOriginalImage))
                    {
                        previousImage.Dispose();
                    }
                }

                return;
            }

            var transformedImage = CreateWinFormsTransformedImage(_winFormsOriginalImage, _currentRotationAngle, _currentZoom / 256f, _antiAliasEnabled);
            var oldImage = picMain.Image;
            picMain.Image = transformedImage;

            if (oldImage is not null && !ReferenceEquals(oldImage, _winFormsOriginalImage))
            {
                oldImage.Dispose();
            }
#else
            picMain.SetRotation(_currentRotationAngle);
            picMain.SetZoom(_currentZoom);
            picMain.SetAntiAlias(_antiAliasEnabled);
#endif
        }

        private void CapturePictureBoxSourceImage()
        {
#if PICTUREBOXDEMO_WINFORMS
            _winFormsOriginalImage?.Dispose();
            _winFormsOriginalImage = picMain.Image is null ? null : (Image)picMain.Image.Clone();
#endif
        }

#if PICTUREBOXDEMO_WINFORMS
        private static Image CreateWinFormsTransformedImage(Image originalImage, int rotation, float zoom, bool antiAlias)
        {
            double angleRad = rotation * Math.PI / 180.0;
            double cos = Math.Abs(Math.Cos(angleRad));
            double sin = Math.Abs(Math.Sin(angleRad));

            int newWidth = Math.Max(1, (int)Math.Ceiling((originalImage.Width * cos + originalImage.Height * sin) * zoom));
            int newHeight = Math.Max(1, (int)Math.Ceiling((originalImage.Width * sin + originalImage.Height * cos) * zoom));
            var bitmap = new Bitmap(newWidth, newHeight);

            using var graphics = Graphics.FromImage(bitmap);
            if (antiAlias)
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            }
            else
            {
                graphics.SmoothingMode = SmoothingMode.None;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
            }

            graphics.Clear(Color.Transparent);
            graphics.TranslateTransform(newWidth / 2f, newHeight / 2f);
            graphics.RotateTransform(rotation);
            graphics.ScaleTransform(zoom, zoom);
            graphics.DrawImage(originalImage, -originalImage.Width / 2f, -originalImage.Height / 2f, originalImage.Width, originalImage.Height);

            return bitmap;
        }
#endif

        private void UpdateStatusLabel()
        {
            int zoomPercent = (int)(_currentZoom * 100 / 256);
            lblStatus.Text = $"模式: {picMain.SizeMode} | 旋转: {_currentRotationAngle}° | 缩放: {zoomPercent}% | 抗锯齿: {(chkAntiAlias.Checked ? "开启" : "关闭")}";
        }
    }
}

