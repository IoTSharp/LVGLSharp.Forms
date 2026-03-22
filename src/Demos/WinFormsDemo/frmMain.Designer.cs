

 

namespace WinFormsDemo
{
    partial class frmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            toolbar = new FlowLayoutPanel();
            port_label = new Label();
            portDropdown = new ComboBox();
            refreshButton = new Button();
            baud_label = new Label();
            baudDropdown = new ComboBox();
            openButton = new Button();
            tpMain = new TableLayoutPanel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            textBox1 = new TextBox();
            sendButton = new Button();
            button2 = new Button();
            checkBox1 = new CheckBox();
            pictureBox1 = new PictureBox();
            radioButton1 = new RadioButton();
            button3 = new Button();
            button4 = new Button();
            checkBox2 = new CheckBox();
            recv_container = new FlowLayoutPanel();
            receiveTextArea = new TextBox();
            button1 = new Button();
            clearButton = new Button();
            hexSwitch = new CheckBox();
            toolbar.SuspendLayout();
            tpMain.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            recv_container.SuspendLayout();
            SuspendLayout();
            // 
            // toolbar
            // 
            toolbar.Controls.Add(port_label);
            toolbar.Controls.Add(portDropdown);
            toolbar.Controls.Add(refreshButton);
            toolbar.Controls.Add(baud_label);
            toolbar.Controls.Add(baudDropdown);
            toolbar.Controls.Add(openButton);
            toolbar.Location = new Point(3, 3);
            toolbar.Name = "toolbar";
            toolbar.Size = new Size(771, 49);
            toolbar.TabIndex = 0;
            // 
            // port_label
            // 
            port_label.Location = new Point(3, 0);
            port_label.Name = "port_label";
            port_label.Size = new Size(100, 50);
            port_label.TabIndex = 0;
            port_label.Text = "串口";
            port_label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // portDropdown
            // 
            portDropdown.DropDownHeight = 105;
            portDropdown.FlatStyle = FlatStyle.Flat;
            portDropdown.Font = new Font("Microsoft YaHei UI", 9F);
            portDropdown.FormattingEnabled = true;
            portDropdown.IntegralHeight = false;
            portDropdown.ItemHeight = 17;
            portDropdown.Location = new Point(109, 3);
            portDropdown.Name = "portDropdown";
            portDropdown.Size = new Size(150, 25);
            portDropdown.TabIndex = 2;
            // 
            // refreshButton
            // 
            refreshButton.Location = new Point(265, 3);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(111, 46);
            refreshButton.TabIndex = 1;
            refreshButton.Text = "刷新串口";
            refreshButton.UseVisualStyleBackColor = true;
            // 
            // baud_label
            // 
            baud_label.Location = new Point(382, 0);
            baud_label.Name = "baud_label";
            baud_label.Size = new Size(100, 49);
            baud_label.TabIndex = 3;
            baud_label.Text = "波特率";
            baud_label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // baudDropdown
            // 
            baudDropdown.Font = new Font("Microsoft YaHei UI", 9F);
            baudDropdown.FormattingEnabled = true;
            baudDropdown.Location = new Point(488, 3);
            baudDropdown.Name = "baudDropdown";
            baudDropdown.Size = new Size(121, 25);
            baudDropdown.TabIndex = 4;
            // 
            // openButton
            // 
            openButton.Location = new Point(615, 3);
            openButton.Name = "openButton";
            openButton.Size = new Size(75, 49);
            openButton.TabIndex = 5;
            openButton.Text = "打开串口";
            openButton.UseVisualStyleBackColor = true;
            // 
            // tpMain
            // 
            tpMain.ColumnCount = 1;
            tpMain.ColumnStyles.Add(new ColumnStyle());
            tpMain.Controls.Add(flowLayoutPanel1, 0, 2);
            tpMain.Controls.Add(toolbar, 0, 0);
            tpMain.Controls.Add(recv_container, 3, 1);
            tpMain.Dock = DockStyle.Fill;
            tpMain.Location = new Point(0, 0);
            tpMain.Name = "tpMain";
            tpMain.RowCount = 3;
            tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
            tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 185F));
            tpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            tpMain.Size = new Size(800, 441);
            tpMain.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(textBox1);
            flowLayoutPanel1.Controls.Add(sendButton);
            flowLayoutPanel1.Controls.Add(button2);
            flowLayoutPanel1.Controls.Add(checkBox1);
            flowLayoutPanel1.Controls.Add(pictureBox1);
            flowLayoutPanel1.Controls.Add(radioButton1);
            flowLayoutPanel1.Controls.Add(button3);
            flowLayoutPanel1.Controls.Add(button4);
            flowLayoutPanel1.Controls.Add(checkBox2);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(3, 243);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(794, 195);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(3, 3);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.PlaceholderText = "输入的数据";
            textBox1.Size = new Size(570, 50);
            textBox1.TabIndex = 0;
            // 
            // sendButton
            // 
            sendButton.Location = new Point(579, 3);
            sendButton.Name = "sendButton";
            sendButton.Size = new Size(106, 50);
            sendButton.TabIndex = 2;
            sendButton.Text = "发送";
            sendButton.UseVisualStyleBackColor = true;
            sendButton.Click += send_btn_Click;
            // 
            // button2
            // 
            button2.Location = new Point(691, 3);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 4;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(3, 59);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(89, 21);
            checkBox1.TabIndex = 5;
            checkBox1.Text = "checkBox1";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new Point(98, 59);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(100, 50);
            pictureBox1.TabIndex = 6;
            pictureBox1.TabStop = false;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Location = new Point(204, 59);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(102, 21);
            radioButton1.TabIndex = 7;
            radioButton1.TabStop = true;
            radioButton1.Text = "radioButton1";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(312, 59);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 8;
            button3.Text = "button3";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(393, 59);
            button4.Name = "button4";
            button4.Size = new Size(75, 23);
            button4.TabIndex = 9;
            button4.Text = "button4";
            button4.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Location = new Point(474, 59);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(89, 21);
            checkBox2.TabIndex = 10;
            checkBox2.Text = "checkBox2";
            checkBox2.UseVisualStyleBackColor = true;
            // 
            // recv_container
            // 
            tpMain.SetColumnSpan(recv_container, 3);
            recv_container.Controls.Add(receiveTextArea);
            recv_container.Controls.Add(button1);
            recv_container.Controls.Add(clearButton);
            recv_container.Controls.Add(hexSwitch);
            recv_container.Dock = DockStyle.Fill;
            recv_container.Location = new Point(3, 58);
            recv_container.Name = "recv_container";
            recv_container.Size = new Size(794, 179);
            recv_container.TabIndex = 1;
            // 
            // receiveTextArea
            // 
            receiveTextArea.Location = new Point(3, 3);
            receiveTextArea.Multiline = true;
            receiveTextArea.Name = "receiveTextArea";
            receiveTextArea.Size = new Size(504, 183);
            receiveTextArea.TabIndex = 0;
            receiveTextArea.Text = "接收的数据...";
            // 
            // button1
            // 
            button1.Location = new Point(513, 3);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 4;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // clearButton
            // 
            clearButton.Location = new Point(594, 3);
            clearButton.Name = "clearButton";
            clearButton.Size = new Size(75, 41);
            clearButton.TabIndex = 2;
            clearButton.Text = "清空";
            clearButton.UseVisualStyleBackColor = true;
            clearButton.Click += button1_Click;
            // 
            // hexSwitch
            // 
            hexSwitch.Location = new Point(675, 3);
            hexSwitch.Name = "hexSwitch";
            hexSwitch.Size = new Size(91, 41);
            hexSwitch.TabIndex = 3;
            hexSwitch.Text = "HEX模式";
            hexSwitch.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 441);
            Controls.Add(tpMain);
            Name = "frmMain";
            Text = "LVGLSharp";
            Load += Form1_Load;
            toolbar.ResumeLayout(false);
            tpMain.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            recv_container.ResumeLayout(false);
            recv_container.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel toolbar;
        private Label port_label;
        private ComboBox portDropdown;
        private Button refreshButton;
        private TableLayoutPanel tpMain;
        private FlowLayoutPanel recv_container;
        private Label baud_label;
        private ComboBox baudDropdown;
        private Button openButton;
        private TextBox receiveTextArea;
        private Button clearButton;
        private CheckBox hexSwitch;
        private FlowLayoutPanel flowLayoutPanel1;
        private TextBox textBox1;
        private Button sendButton;
        private Button button2;
        private CheckBox checkBox1;
        private PictureBox pictureBox1;
        private RadioButton radioButton1;
        private Button button3;
        private Button button4;
        private CheckBox checkBox2;
        private Button button1;
    }

 
}



