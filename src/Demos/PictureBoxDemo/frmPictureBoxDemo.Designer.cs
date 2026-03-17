namespace PictureBoxDemo
{
    partial class frmPictureBoxDemo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tpMain = new TableLayoutPanel();
            pnlTop = new FlowLayoutPanel();
            lblImagePath = new Label();
            txtImagePath = new TextBox();
            btnLoadImage = new Button();
            lblSizeMode = new Label();
            cmbSizeMode = new ComboBox();
            chkAntiAlias = new CheckBox();
            pnlContent = new FlowLayoutPanel();
            picMain = new PictureBox();
            pnlBottom = new FlowLayoutPanel();
            btnRotateLeft = new Button();
            btnRotateRight = new Button();
            btnZoomIn = new Button();
            btnZoomOut = new Button();
            btnReset = new Button();
            pnlStatus = new FlowLayoutPanel();
            lblStatus = new Label();
            tpMain.SuspendLayout();
            pnlTop.SuspendLayout();
            pnlContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picMain).BeginInit();
            pnlBottom.SuspendLayout();
            pnlStatus.SuspendLayout();
            SuspendLayout();
            // 
            // tpMain
            // 
            tpMain.ColumnCount = 1;
            tpMain.ColumnStyles.Add(new ColumnStyle());
            tpMain.Controls.Add(pnlTop, 0, 0);
            tpMain.Controls.Add(pnlContent, 0, 1);
            tpMain.Controls.Add(pnlBottom, 0, 2);
            tpMain.Controls.Add(pnlStatus, 0, 3);
            tpMain.Dock = DockStyle.Fill;
            tpMain.Location = new Point(0, 0);
            tpMain.Margin = new Padding(0);
            tpMain.Name = "tpMain";
            tpMain.RowCount = 4;
            tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 490F));
            tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            tpMain.Size = new Size(913, 645);
            tpMain.TabIndex = 0;
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(lblImagePath);
            pnlTop.Controls.Add(txtImagePath);
            pnlTop.Controls.Add(btnLoadImage);
            pnlTop.Controls.Add(lblSizeMode);
            pnlTop.Controls.Add(cmbSizeMode);
            pnlTop.Controls.Add(chkAntiAlias);
            pnlTop.Dock = DockStyle.Fill;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Margin = new Padding(0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(913, 54);
            pnlTop.TabIndex = 0;
            // 
            // lblImagePath
            // 
            lblImagePath.Location = new Point(3, 0);
            lblImagePath.Name = "lblImagePath";
            lblImagePath.Size = new Size(72, 32);
            lblImagePath.TabIndex = 0;
            lblImagePath.Text = "图像路径:";
            lblImagePath.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtImagePath
            // 
            txtImagePath.Location = new Point(81, 3);
            txtImagePath.Name = "txtImagePath";
            txtImagePath.PlaceholderText = "输入 LVGL 路径 (如 A:/images/sample.png)";
            txtImagePath.Size = new Size(356, 23);
            txtImagePath.TabIndex = 1;
            // 
            // btnLoadImage
            // 
            btnLoadImage.Location = new Point(443, 3);
            btnLoadImage.Name = "btnLoadImage";
            btnLoadImage.Size = new Size(100, 29);
            btnLoadImage.TabIndex = 2;
            btnLoadImage.Text = "加载图像";
            btnLoadImage.UseVisualStyleBackColor = true;
            btnLoadImage.Click += btnLoadImage_Click;
            // 
            // lblSizeMode
            // 
            lblSizeMode.Location = new Point(549, 0);
            lblSizeMode.Name = "lblSizeMode";
            lblSizeMode.Size = new Size(72, 32);
            lblSizeMode.TabIndex = 3;
            lblSizeMode.Text = "显示模式:";
            lblSizeMode.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbSizeMode
            // 
            cmbSizeMode.FormattingEnabled = true;
            cmbSizeMode.Location = new Point(627, 3);
            cmbSizeMode.Name = "cmbSizeMode";
            cmbSizeMode.Size = new Size(126, 25);
            cmbSizeMode.TabIndex = 4;
            cmbSizeMode.SelectedIndexChanged += cmbSizeMode_SelectedIndexChanged;
            // 
            // chkAntiAlias
            // 
            chkAntiAlias.AutoSize = true;
            chkAntiAlias.CheckState = CheckState.Checked;
            chkAntiAlias.Checked = true;
            chkAntiAlias.Location = new Point(759, 3);
            chkAntiAlias.Name = "chkAntiAlias";
            chkAntiAlias.Size = new Size(63, 21);
            chkAntiAlias.TabIndex = 5;
            chkAntiAlias.Text = "抗锯齿";
            chkAntiAlias.UseVisualStyleBackColor = true;
            chkAntiAlias.CheckedChanged += chkAntiAlias_CheckedChanged;
            // 
            // pnlContent
            // 
            pnlContent.Controls.Add(picMain);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 54);
            pnlContent.Margin = new Padding(0);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(8);
            pnlContent.Size = new Size(913, 490);
            pnlContent.TabIndex = 1;
            // 
            // picMain
            // 
            picMain.Location = new Point(8, 8);
            picMain.Margin = new Padding(0);
            picMain.Name = "picMain";
            picMain.Size = new Size(824, 474);
            picMain.SizeMode = PictureBoxSizeMode.Zoom;
            picMain.TabIndex = 1;
            picMain.TabStop = false;
            // 
            // pnlBottom
            // 
            pnlBottom.Controls.Add(btnRotateLeft);
            pnlBottom.Controls.Add(btnRotateRight);
            pnlBottom.Controls.Add(btnZoomIn);
            pnlBottom.Controls.Add(btnZoomOut);
            pnlBottom.Controls.Add(btnReset);
            pnlBottom.Dock = DockStyle.Fill;
            pnlBottom.Location = new Point(0, 544);
            pnlBottom.Margin = new Padding(0);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(913, 52);
            pnlBottom.TabIndex = 2;
            // 
            // btnRotateLeft
            // 
            btnRotateLeft.Location = new Point(3, 3);
            btnRotateLeft.Name = "btnRotateLeft";
            btnRotateLeft.Size = new Size(100, 32);
            btnRotateLeft.TabIndex = 0;
            btnRotateLeft.Text = "左旋 90°";
            btnRotateLeft.UseVisualStyleBackColor = true;
            btnRotateLeft.Click += btnRotateLeft_Click;
            // 
            // btnRotateRight
            // 
            btnRotateRight.Location = new Point(109, 3);
            btnRotateRight.Name = "btnRotateRight";
            btnRotateRight.Size = new Size(100, 32);
            btnRotateRight.TabIndex = 1;
            btnRotateRight.Text = "右旋 90°";
            btnRotateRight.UseVisualStyleBackColor = true;
            btnRotateRight.Click += btnRotateRight_Click;
            // 
            // btnZoomIn
            // 
            btnZoomIn.Location = new Point(215, 3);
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Size = new Size(100, 32);
            btnZoomIn.TabIndex = 2;
            btnZoomIn.Text = "放大";
            btnZoomIn.UseVisualStyleBackColor = true;
            btnZoomIn.Click += btnZoomIn_Click;
            // 
            // btnZoomOut
            // 
            btnZoomOut.Location = new Point(321, 3);
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Size = new Size(100, 32);
            btnZoomOut.TabIndex = 3;
            btnZoomOut.Text = "缩小";
            btnZoomOut.UseVisualStyleBackColor = true;
            btnZoomOut.Click += btnZoomOut_Click;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(427, 3);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(100, 32);
            btnReset.TabIndex = 4;
            btnReset.Text = "重置";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += btnReset_Click;
            // 
            // pnlStatus
            // 
            pnlStatus.Controls.Add(lblStatus);
            pnlStatus.Dock = DockStyle.Fill;
            pnlStatus.Location = new Point(0, 596);
            pnlStatus.Margin = new Padding(0);
            pnlStatus.Name = "pnlStatus";
            pnlStatus.Size = new Size(913, 49);
            pnlStatus.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(3, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(828, 38);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "就绪";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // frmPictureBoxDemo
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(913, 645);
            Controls.Add(tpMain);
            MinimumSize = new Size(720, 480);
            Name = "frmPictureBoxDemo";
            Text = "PictureBox 演示程序 - LVGLSharp";
            Load += frmPictureBoxDemo_Load;
            SizeChanged += frmPictureBoxDemo_SizeChanged;
            tpMain.ResumeLayout(false);
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picMain).EndInit();
            pnlBottom.ResumeLayout(false);
            pnlStatus.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tpMain;
        private FlowLayoutPanel pnlTop;
        private Label lblImagePath;
        private TextBox txtImagePath;
        private Button btnLoadImage;
        private Label lblSizeMode;
        private ComboBox cmbSizeMode;
        private CheckBox chkAntiAlias;
        private FlowLayoutPanel pnlContent;
        private PictureBox picMain;
        private FlowLayoutPanel pnlBottom;
        private Button btnRotateLeft;
        private Button btnRotateRight;
        private Button btnZoomIn;
        private Button btnZoomOut;
        private Button btnReset;
        private FlowLayoutPanel pnlStatus;
        private Label lblStatus;
    }
}
