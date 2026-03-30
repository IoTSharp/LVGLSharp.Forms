namespace WinFormsRdpDemo
{
    partial class frmMain
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

        private TableLayoutPanel tpMain;
        private FlowLayoutPanel pnlTop;
        private Label lblTitle;
        private FlowLayoutPanel pnlContent;
        private Panel lvglHostPanel;

        private void InitializeComponent()
        {
            this.tpMain = new TableLayoutPanel();
            this.pnlTop = new FlowLayoutPanel();
            this.lblTitle = new Label();
            this.pnlContent = new FlowLayoutPanel();
            this.lvglHostPanel = new Panel();
            this.SuspendLayout();
            // 
            // tpMain
            // 
            this.tpMain.ColumnCount = 1;
            this.tpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tpMain.RowCount = 2;
            this.tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            this.tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 432F));
            this.tpMain.Dock = DockStyle.Fill;
            this.tpMain.Controls.Add(this.pnlTop, 0, 0);
            this.tpMain.Controls.Add(this.pnlContent, 0, 1);
            // 
            // pnlTop
            // 
            this.pnlTop.Dock = DockStyle.Fill;
            this.pnlTop.Controls.Add(this.lblTitle);
            // 
            // lblTitle
            // 
            this.lblTitle.Text = "LVGLSharp RDP Demo";
            this.lblTitle.Font = new Font("Microsoft YaHei", 14F);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Margin = new Padding(10, 8, 0, 0);
            // 
            // pnlContent
            // 
            this.pnlContent.Dock = DockStyle.Fill;
            this.pnlContent.Controls.Add(this.lvglHostPanel);
            // 
            // lvglHostPanel
            // 
            this.lvglHostPanel.Name = "lvglHostPanel";
            this.lvglHostPanel.Size = new Size(640, 400);
            this.lvglHostPanel.BackColor = new Color(0, 0, 0);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new SizeF(8F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 480);
            this.Controls.Add(this.tpMain);
            this.Name = "frmMain";
            this.Text = "WinFormsRdpDemo";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
