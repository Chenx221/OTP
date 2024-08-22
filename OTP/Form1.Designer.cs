namespace OTP
{
    partial class Form1
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

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            listBox1 = new System.Windows.Forms.ListBox();
            label1 = new System.Windows.Forms.Label();
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            button3 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            progressBar1 = new System.Windows.Forms.ProgressBar();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // listBox1
            // 
            listBox1.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 27;
            listBox1.Location = new System.Drawing.Point(49, 44);
            listBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listBox1.Name = "listBox1";
            listBox1.Size = new System.Drawing.Size(551, 409);
            listBox1.TabIndex = 0;
            listBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold);
            label1.Location = new System.Drawing.Point(640, 56);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(230, 43);
            label1.TabIndex = 1;
            label1.Text = "TOTP Code";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button1
            // 
            button1.Enabled = false;
            button1.Location = new System.Drawing.Point(640, 354);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(107, 27);
            button1.TabIndex = 3;
            button1.Text = "Generate";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1_Click;
            // 
            // button2
            // 
            button2.Enabled = false;
            button2.Location = new System.Drawing.Point(755, 354);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(115, 94);
            button2.TabIndex = 3;
            button2.Text = "Copy";
            button2.UseVisualStyleBackColor = true;
            button2.Click += Button2_Click;
            // 
            // label2
            // 
            label2.BackColor = System.Drawing.Color.Transparent;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            label2.Location = new System.Drawing.Point(640, 297);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(230, 23);
            label2.TabIndex = 5;
            label2.Text = "Remaining Time";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button3
            // 
            button3.Location = new System.Drawing.Point(640, 388);
            button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(107, 27);
            button3.TabIndex = 7;
            button3.Text = "Manage";
            button3.UseVisualStyleBackColor = true;
            button3.Click += Button3_Click;
            // 
            // button4
            // 
            button4.Location = new System.Drawing.Point(640, 421);
            button4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(107, 27);
            button4.TabIndex = 8;
            button4.Text = "Refresh";
            button4.UseVisualStyleBackColor = true;
            button4.Click += Button4_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new System.Drawing.Point(640, 177);
            progressBar1.Maximum = 30;
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(230, 51);
            progressBar1.TabIndex = 10;
            // 
            // label3
            // 
            label3.Font = new System.Drawing.Font("Segoe UI", 24F);
            label3.Location = new System.Drawing.Point(640, 231);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(230, 45);
            label3.TabIndex = 11;
            label3.Text = "1";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new System.Drawing.Font("Segoe UI", 12F);
            label4.Location = new System.Drawing.Point(640, 129);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(45, 21);
            label4.TabIndex = 12;
            label4.Text = "Next:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            label5.Location = new System.Drawing.Point(713, 130);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(92, 20);
            label5.TabIndex = 13;
            label5.Text = "Next Code";
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(904, 508);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(progressBar1);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(label2);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(label1);
            Controls.Add(listBox1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "Form1";
            Text = "TOTP";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            VisibleChanged += Form1_VisibleChanged;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}

