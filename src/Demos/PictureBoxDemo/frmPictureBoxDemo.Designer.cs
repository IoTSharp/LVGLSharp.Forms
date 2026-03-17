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
            tpMain.Name = "tpMain";
            tpMain.RowCount = 4;
            tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 450F));
            tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tpMain.Size = new Size(800, 600);
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
            pnlTop.Location = new Point(3, 3);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(794, 54);
            pnlTop.TabIndex = 0;
            // 
            // lblImagePath
            // 
            lblImagePath.Location = new Point(3, 0);
            lblImagePath.Name = "lblImagePath";
            lblImagePath.Size = new Size(80, 50);
            lblImagePath.TabIndex = 0;
            lblImagePath.Text = "图像路径:";
            lblImagePath.TextAlign = ContentAlignment.MiddleRight;
            // 
            // txtImagePath
            // 
            txtImagePath.Location = new Point(89, 3);
            txtImagePath.Multiline = true;
            txtImagePath.Name = "txtImagePath";
            txtImagePath.PlaceholderText = "输入 LVGL 路径 (如 A:/images/sample.png)";
            txtImagePath.Size = new Size(300, 40);
            txtImagePath.TabIndex = 1;
            // 
            // btnLoadImage
            // 
            btnLoadImage.Location = new Point(395, 3);
            btnLoadImage.Name = "btnLoadImage";
            btnLoadImage.Size = new Size(100, 40);
            btnLoadImage.TabIndex = 2;
            btnLoadImage.Text = "加载图像";
            btnLoadImage.UseVisualStyleBackColor = true;
            btnLoadImage.Click += btnLoadImage_Click;
            // 
            // lblSizeMode
            // 
            lblSizeMode.Location = new Point(501, 0);
            lblSizeMode.Name = "lblSizeMode";
            lblSizeMode.Size = new Size(80, 50);
            lblSizeMode.TabIndex = 3;
            lblSizeMode.Text = "显示模式:";
            lblSizeMode.TextAlign = ContentAlignment.MiddleRight;
            // 
            // cmbSizeMode
            // 
            cmbSizeMode.FormattingEnabled = true;
            cmbSizeMode.Location = new Point(587, 3);
            cmbSizeMode.Name = "cmbSizeMode";
            cmbSizeMode.Size = new Size(121, 25);
            cmbSizeMode.TabIndex = 4;
            cmbSizeMode.SelectedIndexChanged += cmbSizeMode_SelectedIndexChanged;
            // 
            // chkAntiAlias
            // 
            chkAntiAlias.AutoSize = true;
            chkAntiAlias.CheckState = CheckState.Checked;
            chkAntiAlias.Checked = true;
            chkAntiAlias.Location = new Point(714, 3);
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
            pnlContent.Location = new Point(3, 63);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(794, 444);
            pnlContent.TabIndex = 1;
            // 
            // picMain
            // 
            picMain.Location = new Point(3, 3);
            picMain.Name = "picMain";
            picMain.Size = new Size(788, 438);
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
            pnlBottom.Location = new Point(3, 513);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(794, 54);
            pnlBottom.TabIndex = 2;
            // 
            // btnRotateLeft
            // 
            btnRotateLeft.Location = new Point(3, 3);
            btnRotateLeft.Name = "btnRotateLeft";
            btnRotateLeft.Size = new Size(100, 40);
            btnRotateLeft.TabIndex = 0;
            btnRotateLeft.Text = "左旋 90°";
            btnRotateLeft.UseVisualStyleBackColor = true;
            btnRotateLeft.Click += btnRotateLeft_Click;
            // 
            // btnRotateRight
            // 
            btnRotateRight.Location = new Point(109, 3);
            btnRotateRight.Name = "btnRotateRight";
            btnRotateRight.Size = new Size(100, 40);
            btnRotateRight.TabIndex = 1;
            btnRotateRight.Text = "右旋 90°";
            btnRotateRight.UseVisualStyleBackColor = true;
            btnRotateRight.Click += btnRotateRight_Click;
            // 
            // btnZoomIn
            // 
            btnZoomIn.Location = new Point(215, 3);
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Size = new Size(100, 40);
            btnZoomIn.TabIndex = 2;
            btnZoomIn.Text = "放大";
            btnZoomIn.UseVisualStyleBackColor = true;
            btnZoomIn.Click += btnZoomIn_Click;
            // 
            // btnZoomOut
            // 
            btnZoomOut.Location = new Point(321, 3);
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Size = new Size(100, 40);
            btnZoomOut.TabIndex = 3;
            btnZoomOut.Text = "缩小";
            btnZoomOut.UseVisualStyleBackColor = true;
            btnZoomOut.Click += btnZoomOut_Click;
            // 
            // btnReset
            // 
            btnReset.Location = new Point(427, 3);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(100, 40);
            btnReset.TabIndex = 4;
            btnReset.Text = "重置";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Click += btnReset_Click;
            // 
            // pnlStatus
            // 
            pnlStatus.Controls.Add(lblStatus);
            pnlStatus.Dock = DockStyle.Fill;
            pnlStatus.Location = new Point(3, 573);
            pnlStatus.Name = "pnlStatus";
            pnlStatus.Size = new Size(794, 24);
            pnlStatus.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(3, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(780, 24);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "就绪";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // frmPictureBoxDemo
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 600);
            Controls.Add(tpMain);
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
