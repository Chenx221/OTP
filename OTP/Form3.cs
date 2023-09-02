using FlashCap;
using FlashCap.Utilities;
using ScreenCapturerNS;
using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ZXing;

namespace OTP
{
    public partial class Form3 : Form
    {
        private string connectionString = "Data Source=key.db;Version=3;";
        private bool isCapturing = false; // 添加标识截图状态的成员变量
        private bool isLocalfile = false;
        private bool isScanningPaused = false;
        private bool isRecording = false;
        private SynchronizationContext synchContext;
        private System.Windows.Forms.Timer timer;

        private CaptureDevice captureDevice;

        public Form3()
        {
            InitializeComponent();
            // 初始化定时器，间隔为1秒（1000毫秒）
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;

        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null && isRecording)
            {
                ScanQRCode((Bitmap)pictureBox1.Image);
            }
        }


        private async void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopCapturing();
            if (isRecording)
            {
                await this.captureDevice.StopAsync();
                pictureBox1.Image = null;
                isRecording = false;
                // Discard capture device.
                this.captureDevice?.Dispose();
                this.captureDevice = null;
            }
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
            ScreenCapturer.StartCapture((Bitmap bitmap) =>
            {
                ScanQRCode(bitmap);
            });
        }

        private void StopCapturing()
        {
            //activeButton();
            isCapturing = false; // 设置截图状态为未截图
            ScreenCapturer.StopCapture(); // 停止截图
        }

        private async void ScanQRCode(Bitmap bitmap)
        {
            // 创建二维码读取器实例
            var barcodeReader = new ZXing.Windows.Compatibility.BarcodeReader();

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
                    if (!isLocalfile && !isScanningPaused)
                    {
                        StopCapturing();
                        this.Invoke((MethodInvoker)(() => label5.Text = "Stopped"));
                    }
                    else
                    {
                        isLocalfile = false;
                    }
                    string[] parts = decodedText.Split(new char[] { '/', '=', '&', '?' });
                    if (parts.Length >= 6)
                    {
                        //扫码成功
                        string title = parts[3]; // 获取 title 部分
                        string secretKey = parts[5]; // 获取 secretKey 部分

                        // 在窗体上显示 title 和 secretKey
                        this.Invoke((MethodInvoker)(() => textBox1.Text = title));
                        this.Invoke((MethodInvoker)(() => textBox2.Text = secretKey));
                        this.Invoke((MethodInvoker)(() => label4.Text = "Success"));
                        if (isRecording)
                        {
                            await this.captureDevice.StopAsync();
                            pictureBox1.Image = null;
                            isRecording = false;
                            // Discard capture device.
                            this.captureDevice?.Dispose();
                            this.captureDevice = null;
                        }
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
                else if (isScanningPaused)
                {
                    this.Invoke((MethodInvoker)(() => label3.Text = "None"));
                    this.Invoke((MethodInvoker)(() => label4.Text = "QR code not detected."));
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

        private async void button5_Click(object sender, EventArgs e)
        {
            this.synchContext = SynchronizationContext.Current;
            var devices = new CaptureDevices();

            var descriptors = devices.EnumerateDescriptors().
                //Where(d => d.DeviceType == DeviceTypes.DirectShow).  // Only DirectShow device.
                Where(d => d.Characteristics.Length >= 1).             // One or more valid video characteristics.
                ToArray();

            // Use first device.
            var descriptor0 = descriptors.ElementAtOrDefault(0);

            if (descriptor0 != null)
            {
                var characteristics = descriptor0.Characteristics.
                    FirstOrDefault(c => c.PixelFormat != PixelFormats.Unknown);
                if (characteristics != null)
                {
                    this.captureDevice = await descriptor0.OpenAsync(
                        characteristics,
                        this.OnPixelBufferArrived);

                    await this.captureDevice.StartAsync();
                }
            }

        }
        private void OnPixelBufferArrived(PixelBufferScope bufferScope)
        {
            ArraySegment<byte> image = bufferScope.Buffer.ReferImage();
            using (var stream = image.AsStream())
            {
                var bitmap = Image.FromStream(stream);
                bufferScope.ReleaseNow();
                this.synchContext.Post(_ =>
                {
                    pictureBox1.Image = bitmap;
                    if (!isRecording)
                    {
                        isRecording = true;
                        timer.Start();
                    }
                }, null);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string clipboardText = Clipboard.GetText();

            if (!string.IsNullOrWhiteSpace(clipboardText) &&
                clipboardText.StartsWith("otpauth://totp/") &&
                clipboardText.Contains("?secret="))
            {
                string[] parts = clipboardText.Split(new char[] { '/', '=', '&', '?' });
                if (parts.Length >= 6)
                {
                    string title = parts[3]; // 获取 title 部分
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

        private async void button8_Click(object sender, EventArgs e)
        {
            if (isRecording)
            {
                await this.captureDevice.StopAsync();
                pictureBox1.Image = null;
                isRecording = false;
                // Discard capture device.
                this.captureDevice?.Dispose();
                this.captureDevice = null;
            }
        }
    }
}
