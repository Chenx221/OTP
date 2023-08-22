using ScreenCapturerNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;

namespace OTP
{
    public partial class Form3 : Form
    {
        private string connectionString = "Data Source=key.db;Version=3;";
        private bool isCapturing = false; // 添加标识截图状态的成员变量
        private bool isLocalfile = false;

        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopCapturing(); // 关闭窗口时停止截图
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string title = textBox1.Text;
                string secretKey = textBox2.Text;

                if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(secretKey))
                {
                    using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();

                        using (SQLiteCommand command = new SQLiteCommand("INSERT INTO TotpData (Title, SecretKey) VALUES (@Title, @SecretKey)", connection))
                        {
                            command.Parameters.AddWithValue("@Title", title);
                            command.Parameters.AddWithValue("@SecretKey", secretKey);
                            command.ExecuteNonQuery();
                        }
                    }
                    this.DialogResult = DialogResult.OK;
                    MessageBox.Show("Record added successfully.");

                    // 关闭当前的 Form3 实例
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please enter both title and secret key.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        //这里摆烂，自用的怎么会犯操作上的错误，主要是遇到bug不想修，先晾着
        //private void deactivateButton(int btn)
        //{
        //    if (btn == 3)
        //    {
        //        button1.Enabled = false;
        //        button4.Enabled = false;
        //        button5.Enabled = false;
        //        button6.Enabled = false;
        //    }
        //}
        //private void activeButton()
        //{
        //    for (int i = 1; i <= 6; i++)
        //    {
        //        string buttonName = "button" + i;
        //        Control[] controls = this.Controls.Find(buttonName, true);

        //        if (controls.Length > 0 && controls[0] is Button)
        //        {
        //            Button button = (Button)controls[0];
        //            button.Enabled = true;
        //        }
        //    }
        //}
        private void button3_Click(object sender, EventArgs e)
        {
            if (isCapturing)
            {
                label5.Text = "Stopped";
                StopCapturing(); // 停止截图
            }
            else
            {
                label3.Text = "None";
                label5.Text = "Running";
                label4.Text = "No QR code found.";
                StartCapturing(); // 开始截图
            }
        }

        private void StartCapturing()
        {
            //deactivateButton(3);
            isCapturing = true; // 设置截图状态为正在截图
            ScreenCapturer.StartCapture((Bitmap bitmap) => {
                // 进行二维码扫描
                ScanQRCode(bitmap);
            });
        }

        private void StopCapturing()
        {
            //activeButton();
            isCapturing = false; // 设置截图状态为未截图
            ScreenCapturer.StopCapture(); // 停止截图
        }


        private void ScanQRCode(Bitmap bitmap)
        {
            // 创建二维码读取器实例
            BarcodeReader barcodeReader = new BarcodeReader();

            // 尝试解码二维码
            Result result = barcodeReader.Decode(bitmap);
            if (result != null)
            {
                // 扫描成功，显示扫描结果
                string decodedText = result.Text;
                this.Invoke((MethodInvoker)(() => label3.Text = decodedText));

                // 判断是否符合期望的格式
                if (decodedText.StartsWith("otpauth://totp/") && decodedText.Contains("?secret="))
                {
                    if (!isLocalfile)
                    {
                        StopCapturing();
                        this.Invoke((MethodInvoker)(() => label5.Text = "Stopped"));
                    }
                    else
                    {
                        isLocalfile = false;
                    }
                    string[] parts = decodedText.Split(new char[] { '/', '=', '&','?' });
                    if (parts.Length >= 6)
                    {
                        string title = parts[3]; // 获取 title 部分
                        string secretKey = parts[5]; // 获取 secretKey 部分

                        // 在窗体上显示 title 和 secretKey
                        this.Invoke((MethodInvoker)(() => textBox1.Text = title));
                        this.Invoke((MethodInvoker)(() => textBox2.Text = secretKey));
                        this.Invoke((MethodInvoker)(() => label4.Text = "Success"));
                    }
                    else
                    {
                        this.Invoke((MethodInvoker)(() => label4.Text = "Invalid QR code format."));
                    }
                }
                else
                {
                    this.Invoke((MethodInvoker)(() => label4.Text = "QR code format not recognized."));
                }
            }
            else
            {
                if (isLocalfile)
                {
                    label3.Text = "None";
                    label4.Text = "QR code not detected.";
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            isLocalfile = true;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    using (Bitmap image = new Bitmap(filePath))
                    {
                        ScanQRCode(image);
                    }
                    
                }
            }
        }
    }
}
