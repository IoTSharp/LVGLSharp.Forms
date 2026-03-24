namespace WinFormsVncDemo
{
    partial class frmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        private LVGLSharp.Forms.TableLayoutPanel tpMain;
        private LVGLSharp.Forms.FlowLayoutPanel pnlTop;
        private LVGLSharp.Forms.Label lblTitle;
        private LVGLSharp.Forms.FlowLayoutPanel pnlContent;
        private LVGLSharp.Forms.Panel lvglHostPanel;

        private void InitializeComponent()
        {
            this.tpMain = new LVGLSharp.Forms.TableLayoutPanel();
            this.pnlTop = new LVGLSharp.Forms.FlowLayoutPanel();
            this.lblTitle = new LVGLSharp.Forms.Label();
            this.pnlContent = new LVGLSharp.Forms.FlowLayoutPanel();
            this.lvglHostPanel = new LVGLSharp.Forms.Panel();
            this.SuspendLayout();
            // 
            // tpMain
            // 
            this.tpMain.ColumnCount = 1;
            this.tpMain.ColumnStyles.Add(new LVGLSharp.Forms.ColumnStyle(LVGLSharp.Forms.SizeType.Percent, 100F));
            this.tpMain.RowCount = 2;
            this.tpMain.RowStyles.Add(new LVGLSharp.Forms.RowStyle(LVGLSharp.Forms.SizeType.Absolute, 48F));
            this.tpMain.RowStyles.Add(new LVGLSharp.Forms.RowStyle(LVGLSharp.Forms.SizeType.Percent, 100F));
            this.tpMain.Dock = LVGLSharp.Forms.DockStyle.Fill;
            this.tpMain.Controls.Add(this.pnlTop, 0, 0);
            this.tpMain.Controls.Add(this.pnlContent, 0, 1);
            // 
            // pnlTop
            // 
            this.pnlTop.Dock = LVGLSharp.Forms.DockStyle.Fill;
            this.pnlTop.Controls.Add(this.lblTitle);
            // 
            // lblTitle
            // 
            this.lblTitle.Text = "LVGLSharp VNC Demo";
            this.lblTitle.Font = new LVGLSharp.Drawing.Font("Microsoft YaHei", 14F);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Margin = new LVGLSharp.Forms.Padding(10, 8, 0, 0);
            // 
            // pnlContent
            // 
            this.pnlContent.Dock = LVGLSharp.Forms.DockStyle.Fill;
            this.pnlContent.Controls.Add(this.lvglHostPanel);
            // 
            // lvglHostPanel
            // 
            this.lvglHostPanel.Name = "lvglHostPanel";
            this.lvglHostPanel.Size = new LVGLSharp.Drawing.Size(640, 400);
            this.lvglHostPanel.BackColor = new LVGLSharp.Drawing.Color(0, 0, 0);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new LVGLSharp.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = LVGLSharp.Forms.AutoScaleMode.Font;
            this.ClientSize = new LVGLSharp.Drawing.Size(800, 480);
            this.Controls.Add(this.tpMain);
            this.Name = "frmMain";
            this.Text = "WinFormsVncDemo";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
