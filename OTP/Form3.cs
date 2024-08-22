using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using ZXing;

namespace OTP
{
    public partial class Form3 : Form
    {
        private const string connectionString = "Data Source=key.db;Version=3;";
        private bool isLocalfile = false;

        public Form3()
        {
            InitializeComponent(); // 确保调用了这个方法
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                string title = textBox1.Text;
                string secretKey = textBox2.Text;

                if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(secretKey))
                {
                    using (SQLiteConnection connection = new(connectionString))
                    {
                        connection.Open();

                        using SQLiteCommand command = new("INSERT INTO TotpData (Title, SecretKey) VALUES (@Title, @SecretKey)", connection);
                        command.Parameters.AddWithValue("@Title", title);
                        command.Parameters.AddWithValue("@SecretKey", secretKey);
                        command.ExecuteNonQuery();
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

        private void ScanQRCode(Bitmap bitmap)
        {
            // 创建二维码读取器实例
            var barcodeReader = new ZXing.Windows.Compatibility.BarcodeReader();

            // 尝试解码二维码
            Result result = barcodeReader.Decode(bitmap);
            if (result != null)
            {
                // 扫描成功，显示扫描结果
                string decodedText = result.Text;
                label3.Text = decodedText;

                // 判断是否符合期望的格式
                if (decodedText.StartsWith("otpauth://totp/") && decodedText.Contains("?secret="))
                {
                    if (isLocalfile)
                    {
                        isLocalfile = false;
                    }
                    string[] parts = decodedText.Split(['/', '=', '&', '?']);
                    if (parts.Length >= 6)
                    {
                        // 扫码成功
                        string title = Uri.UnescapeDataString(parts[3]); // 解码 title 部分
                        string secretKey = parts[5]; // 获取 secretKey 部分

                        // 在窗体上显示 title 和 secretKey
                        textBox1.Text = title;
                        textBox2.Text = secretKey;
                        label4.Text = "Success";
                    }
                    else
                    {
                        label4.Text = "Invalid QR code format.";
                    }
                }
                else
                {
                    label4.Text = "QR code format not recognized.";
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

        private void Button4_Click(object sender, EventArgs e)
        {
            isLocalfile = true;
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                using Bitmap image = new(filePath);
                ScanQRCode(image);
            }
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            string clipboardText = Clipboard.GetText();

            if (!string.IsNullOrWhiteSpace(clipboardText) &&
                clipboardText.StartsWith("otpauth://totp/") &&
                clipboardText.Contains("?secret="))
            {
                string[] parts = clipboardText.Split(['/', '=', '&', '?']);
                if (parts.Length >= 6)
                {
                    string title = Uri.UnescapeDataString(parts[3]); // 解码 title 部分
                    string secretKey = parts[5]; // 获取 secretKey 部分

                    MessageBox.Show("OTP link successfully pasted.");

                    // 填充文本框
                    textBox1.Text = title;
                    textBox2.Text = secretKey;
                }
            }
            else
            {
                MessageBox.Show("Clipboard content is not a valid OTP link.");
            }
        }
    }
}
