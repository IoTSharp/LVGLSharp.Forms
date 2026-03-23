namespace MacOsAotDemo;

partial class frmMacOsAotDemo
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        tpMain = new TableLayoutPanel();
        pnlTop = new FlowLayoutPanel();
        lblTitle = new Label();
        txtTitle = new TextBox();
        lblSize = new Label();
        txtSize = new TextBox();
        lblDpi = new Label();
        txtDpi = new TextBox();
        pnlContent = new FlowLayoutPanel();
        picPreview = new PictureBox();
        pnlBottom = new FlowLayoutPanel();
        lblRuntime = new Label();
        txtRuntime = new TextBox();
        btnShowSummary = new Button();
        lblState = new Label();
        txtState = new TextBox();
        lblHostContext = new Label();
        txtHostContext = new TextBox();
        lblStatus = new Label();
        tpMain.SuspendLayout();
        pnlTop.SuspendLayout();
        pnlContent.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
        pnlBottom.SuspendLayout();
        SuspendLayout();
        // 
        // tpMain
        // 
        tpMain.ColumnCount = 1;
        tpMain.ColumnStyles.Add(new ColumnStyle());
        tpMain.Controls.Add(pnlTop, 0, 0);
        tpMain.Controls.Add(pnlContent, 0, 1);
        tpMain.Controls.Add(pnlBottom, 0, 2);
        tpMain.Dock = DockStyle.Fill;
        tpMain.Location = new Point(0, 0);
        tpMain.Margin = new Padding(0);
        tpMain.Name = "tpMain";
        tpMain.RowCount = 3;
        tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 84F));
        tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 420F));
        tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 104F));
        tpMain.Size = new Size(960, 600);
        tpMain.TabIndex = 0;
        // 
        // pnlTop
        // 
        pnlTop.Controls.Add(lblTitle);
        pnlTop.Controls.Add(txtTitle);
        pnlTop.Controls.Add(lblSize);
        pnlTop.Controls.Add(txtSize);
        pnlTop.Controls.Add(lblDpi);
        pnlTop.Controls.Add(txtDpi);
        pnlTop.Dock = DockStyle.Fill;
        pnlTop.Location = new Point(0, 0);
        pnlTop.Margin = new Padding(0);
        pnlTop.Name = "pnlTop";
        pnlTop.Size = new Size(960, 84);
        pnlTop.TabIndex = 0;
        // 
        // lblTitle
        // 
        lblTitle.Location = new Point(3, 0);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(72, 32);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "标题:";
        lblTitle.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtTitle
        // 
        txtTitle.Location = new Point(81, 3);
        txtTitle.ReadOnly = true;
        txtTitle.Size = new Size(260, 23);
        txtTitle.TabIndex = 1;
        // 
        // lblSize
        // 
        lblSize.Location = new Point(347, 0);
        lblSize.Name = "lblSize";
        lblSize.Size = new Size(72, 32);
        lblSize.TabIndex = 2;
        lblSize.Text = "尺寸:";
        lblSize.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtSize
        // 
        txtSize.Location = new Point(425, 3);
        txtSize.ReadOnly = true;
        txtSize.Size = new Size(140, 23);
        txtSize.TabIndex = 3;
        // 
        // lblDpi
        // 
        lblDpi.Location = new Point(571, 0);
        lblDpi.Name = "lblDpi";
        lblDpi.Size = new Size(72, 32);
        lblDpi.TabIndex = 4;
        lblDpi.Text = "DPI:";
        lblDpi.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtDpi
        // 
        txtDpi.Location = new Point(649, 3);
        txtDpi.ReadOnly = true;
        txtDpi.Size = new Size(100, 23);
        txtDpi.TabIndex = 5;
        // 
        // pnlContent
        // 
        pnlContent.Controls.Add(picPreview);
        pnlContent.Dock = DockStyle.Fill;
        pnlContent.Location = new Point(0, 84);
        pnlContent.Margin = new Padding(0);
        pnlContent.Name = "pnlContent";
        pnlContent.Padding = new Padding(8);
        pnlContent.Size = new Size(960, 420);
        pnlContent.TabIndex = 1;
        // 
        // picPreview
        // 
        picPreview.Location = new Point(8, 8);
        picPreview.Margin = new Padding(0);
        picPreview.Name = "picPreview";
        picPreview.Size = new Size(500, 360);
        picPreview.SizeMode = PictureBoxSizeMode.Zoom;
        picPreview.TabIndex = 0;
        picPreview.TabStop = false;
        // 
        // pnlBottom
        // 
        pnlBottom.Controls.Add(lblRuntime);
        pnlBottom.Controls.Add(txtRuntime);
        pnlBottom.Controls.Add(btnShowSummary);
        pnlBottom.Controls.Add(lblState);
        pnlBottom.Controls.Add(txtState);
        pnlBottom.Controls.Add(lblHostContext);
        pnlBottom.Controls.Add(txtHostContext);
        pnlBottom.Controls.Add(lblStatus);
        pnlBottom.Dock = DockStyle.Fill;
        pnlBottom.Location = new Point(0, 504);
        pnlBottom.Margin = new Padding(0);
        pnlBottom.Name = "pnlBottom";
        pnlBottom.Size = new Size(960, 96);
        pnlBottom.TabIndex = 2;
        // 
        // lblRuntime
        // 
        lblRuntime.Location = new Point(3, 0);
        lblRuntime.Name = "lblRuntime";
        lblRuntime.Size = new Size(72, 32);
        lblRuntime.TabIndex = 0;
        lblRuntime.Text = "运行时:";
        lblRuntime.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtRuntime
        // 
        txtRuntime.Location = new Point(81, 3);
        txtRuntime.ReadOnly = true;
        txtRuntime.Size = new Size(280, 23);
        txtRuntime.TabIndex = 1;
        // 
        // btnShowSummary
        // 
        btnShowSummary.Location = new Point(367, 3);
        btnShowSummary.Name = "btnShowSummary";
        btnShowSummary.Size = new Size(120, 29);
        btnShowSummary.TabIndex = 2;
        btnShowSummary.Text = "刷新摘要";
        btnShowSummary.UseVisualStyleBackColor = true;
        btnShowSummary.Click += btnShowSummary_Click;
        // 
        // lblState
        // 
        lblState.Location = new Point(493, 0);
        lblState.Name = "lblState";
        lblState.Size = new Size(72, 32);
        lblState.TabIndex = 3;
        lblState.Text = "状态:";
        lblState.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtState
        // 
        txtState.Location = new Point(571, 3);
        txtState.ReadOnly = true;
        txtState.Size = new Size(340, 23);
        txtState.TabIndex = 4;
        // 
        // lblHostContext
        // 
        lblHostContext.Location = new Point(3, 35);
        lblHostContext.Name = "lblHostContext";
        lblHostContext.Size = new Size(72, 32);
        lblHostContext.TabIndex = 5;
        lblHostContext.Text = "上下文:";
        lblHostContext.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtHostContext
        // 
        txtHostContext.Location = new Point(81, 38);
        txtHostContext.ReadOnly = true;
        txtHostContext.Size = new Size(830, 23);
        txtHostContext.TabIndex = 6;
        // 
        // lblStatus
        // 
        lblStatus.Location = new Point(3, 64);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(908, 32);
        lblStatus.TabIndex = 7;
        lblStatus.Text = "就绪";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // frmMacOsAotDemo
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(960, 600);
        Controls.Add(tpMain);
        MinimumSize = new Size(760, 520);
        Name = "frmMacOsAotDemo";
        Text = "macOS AOT Demo - LVGLSharp";
        Load += frmMacOsAotDemo_Load;
        SizeChanged += frmMacOsAotDemo_SizeChanged;
        tpMain.ResumeLayout(false);
        pnlTop.ResumeLayout(false);
        pnlTop.PerformLayout();
        pnlContent.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
        pnlBottom.ResumeLayout(false);
        pnlBottom.PerformLayout();
        ResumeLayout(false);
    }

    private TableLayoutPanel tpMain;
    private FlowLayoutPanel pnlTop;
    private Label lblTitle;
    private TextBox txtTitle;
    private Label lblSize;
    private TextBox txtSize;
    private Label lblDpi;
    private TextBox txtDpi;
    private FlowLayoutPanel pnlContent;
    private PictureBox picPreview;
    private FlowLayoutPanel pnlBottom;
    private Label lblRuntime;
    private TextBox txtRuntime;
    private Button btnShowSummary;
    private Label lblState;
    private TextBox txtState;
    private Label lblHostContext;
    private TextBox txtHostContext;
    private Label lblStatus;
}