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
using System.IO;
using System.Data.SQLite;
using System.Reflection;
using ZXing.QrCode.Internal;

namespace OTP
{
    public partial class Form1 : Form
    {
        private byte[] secretKey;
        private Totp totp;
        private Timer timer;
        private string connectionString = "Data Source=key.db;Version=3;";
        private string embeddedDatabaseResource = "OTP.empty_key.db";
        private int runonce=0;

        public Form1()
        {
            InitializeComponent();
            //button1.Enabled = false;
            //secretKey = Base32Encoding.ToBytes("JBSWY3DPEHPK3PXP"); // 示例的 base32 编码
            //totp = new Totp(secretKey);

            timer = new Timer();
            timer.Interval = 1000; // 设置计时器间隔为 1 秒
            timer.Tick += Timer_Tick;
        }
        public class TitleInfo
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string SecretKey { get; set; }
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
            if (totp != null)
            {
                GenerateAndDisplayTotp(); // 生成并显示 TOTP 密码
                timer.Start();// 开始计时器
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //button1.Enabled = true; // 在窗体加载时启用按钮
            // 判断是否存在 key.db 数据库文件
            if (!File.Exists("key.db"))
            {
                ExtractEmbeddedDatabase();
            }

            
        }
        private void ExtractEmbeddedDatabase()
        {
            // 从嵌入的资源中复制数据库文件到同目录下
            try
            {
                using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedDatabaseResource))
                {
                    using (FileStream fileStream = new FileStream("key.db", FileMode.Create))
                    {
                        resourceStream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while extracting embedded database: {ex.Message}");
            }
        }

        private void LoadTitlesToListBox()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand("SELECT Id, Title, SecretKey FROM TotpData", connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            listBox1.Items.Clear(); // 清空现有数据
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string title = reader.GetString(1);
                                string secretKey = reader.GetString(2);

                                TitleInfo titleInfo = new TitleInfo
                                {
                                    Id = id,
                                    Title = title,
                                    SecretKey = secretKey
                                };

                                listBox1.Items.Add(titleInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading titles: {ex.Message}");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0) // Ensure an item is selected
            {
                TitleInfo selectedTitleInfo = listBox1.SelectedItem as TitleInfo;
                if (selectedTitleInfo != null)
                {
                    if (runonce == 0)
                    {
                        button1.Enabled = true;
                        runonce++;
                    }
                    secretKey = Base32Encoding.ToBytes(selectedTitleInfo.SecretKey);
                    totp = new Totp(secretKey);
                    //string totpCode = totp.ComputeTotp();
                    //label1.Text = totpCode;
                }
                else
                {
                    MessageBox.Show("Selected item is not of the expected type.");
                }
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 创建 Form2 实例
            Form2 form2 = new Form2();

            // 显示新创建的 Form2 实例
            form2.Show();

            // 隐藏当前的 Form1 实例
            this.Hide();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to exit the application?", "Confirm Exit",
                                                  MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true; // 取消关闭事件，阻止关闭
            }
            // 如果用户选择 Yes，什么都不需要做，窗体会继续关闭
        }

        private void Form1_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible) // 检查窗体是否可见
            {
                LoadTitlesToListBox();
                runonce = 0;
                label1.Text = "TOTP Code";
                button1.Enabled = false;
                button2.Enabled = false;
            }
            else
            {
                timer.Stop();
            }
        }

    }
}
