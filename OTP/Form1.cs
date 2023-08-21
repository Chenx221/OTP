using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OtpNet;

namespace OTP
{
    public partial class Form1 : Form
    {
        private byte[] secretKey;
        private Totp totp;
        private Timer timer;

        public Form1()
        {
            InitializeComponent();

            secretKey = Base32Encoding.ToBytes("JBSWY3DPEHPK3PXP"); // 示例的 base32 编码
            totp = new Totp(secretKey);

            timer = new Timer();
            timer.Interval = 1000; // 设置计时器间隔为 1 秒
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            int remainingSeconds = totp.RemainingSeconds();
            circularProgressBar1.Text = remainingSeconds.ToString();
            circularProgressBar1.Value = remainingSeconds;
            if (remainingSeconds == 30)
            {
                GenerateAndDisplayTotp();
            }
        }

        private void GenerateAndDisplayTotp()
        {
            // 生成新的 TOTP 密码
            string newTotpCode = totp.ComputeTotp();

            // 在标签上显示新生成的密码
            // 使用Invoke确保在UI线程上更新UI元素
            this.Invoke((MethodInvoker)(() => label1.Text = newTotpCode));
        }


        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false; // 禁用生成按钮
            button2.Enabled = true; // 启用复制按钮
            GenerateAndDisplayTotp(); // 生成并显示 TOTP 密码

            // 开始计时器
            timer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = true; // 在窗体加载时启用按钮
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label1.Text);
        }
    }
}
