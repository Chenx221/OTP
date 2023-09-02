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
//using AForge.Video;
//using AForge.Video.DirectShow;

namespace OTP
{
    public partial class Form3 : Form
    {
        private string connectionString = "Data Source=key.db;Version=3;";
        private bool isCapturing = false; // 添加标识截图状态的成员变量
        private bool isLocalfile = false;
        //private FilterInfoCollection videoDevices; // 声明用于存储摄像头设备信息的变量
        //private VideoCaptureDevice camera; // 声明用于摄像头捕获的变量
        private bool isCameraRunning = false; // 用于标识摄像头的运行状态
        private bool isScanningPaused = false;
        //private Bitmap previousBitmap = null;
        private SynchronizationContext synchContext;

        // Constructed capture device.
        private CaptureDevice captureDevice;

        public Form3()
        {
            InitializeComponent();
            //videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopCapturing(); // 关闭窗口时停止截图
            if (isCameraRunning)
            {
                StopCamera(); // 关闭窗口时停止摄像头
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
                        string title = parts[3]; // 获取 title 部分
                        string secretKey = parts[5]; // 获取 secretKey 部分

                        // 在窗体上显示 title 和 secretKey
                        this.Invoke((MethodInvoker)(() => textBox1.Text = title));
                        this.Invoke((MethodInvoker)(() => textBox2.Text = secretKey));
                        this.Invoke((MethodInvoker)(() => label4.Text = "Success"));
                        //StopCamera();
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

            ////////////////////////////////////////////////
            // Initialize and start capture device

            // Capture device enumeration:
            var devices = new CaptureDevices();

            var descriptors = devices.EnumerateDescriptors().
                // You could filter by device type and characteristics.
                //Where(d => d.DeviceType == DeviceTypes.DirectShow).  // Only DirectShow device.
                Where(d => d.Characteristics.Length >= 1).             // One or more valid video characteristics.
                ToArray();

            // Use first device.
            var descriptor0 = descriptors.ElementAtOrDefault(0);

            if (descriptor0 != null)
            {
#if false
            // Request video characteristics strictly:
            // Will raise exception when parameters are not accepted.
            var characteristics = new VideoCharacteristics(
                PixelFormats.JPEG, 1920, 1080, 60);
#else
                // Or, you could choice from device descriptor:
                // Hint: Show up video characteristics into ComboBox and like.
                var characteristics = descriptor0.Characteristics.
                    FirstOrDefault(c => c.PixelFormat != PixelFormats.Unknown);
#endif
                if (characteristics != null)
                {
                    // Open capture device:
                    this.captureDevice = await descriptor0.OpenAsync(
                        characteristics,
                        this.OnPixelBufferArrived);

                    // Start capturing.
                    await this.captureDevice.StartAsync();
                }
            }

        }
        private void OnPixelBufferArrived(PixelBufferScope bufferScope)
        {
            ////////////////////////////////////////////////
            // Pixel buffer has arrived.
            // NOTE: Perhaps this thread context is NOT UI thread.
#if false
        // Get image data binary:
        byte[] image = bufferScope.Buffer.ExtractImage();
#else
            // Or, refer image data binary directly.
            // (Advanced manipulation, see README.)
            ArraySegment<byte> image = bufferScope.Buffer.ReferImage();
#endif
            // Convert to Stream (using FlashCap.Utilities)
            using (var stream = image.AsStream())
            {
                // Decode image data to a bitmap:
                var bitmap = Image.FromStream(stream);

                // `bitmap` is copied, so we can release pixel buffer now.
                bufferScope.ReleaseNow();

                // Switch to UI thread.
                // HACK: Here is using `SynchronizationContext.Post()` instead of `Control.Invoke()`.
                // Because in sensitive states when the form is closing,
                // `Control.Invoke()` can fail with exception.
                this.synchContext.Post(_ =>
                {
                    // HACK: on .NET Core, will be leaked (or delayed GC?)
                    //   So we could release manually before updates.
                    var oldImage = this.BackgroundImage;
                    if (oldImage != null)
                    {
                        //this.BackgroundImage = null;
                        oldImage.Dispose();
                    }

                    // Update a bitmap.
                    pictureBox1.Image = bitmap;
                }, null);
            }
        }
        private void StartCamera()
        {
            //if (!isCameraRunning) // 添加条件检查摄像头是否已经在运行
            //{
            //    if (videoDevices != null && videoDevices.Count > 0)
            //    {
            //        if (camera == null)
            //        {
            //            camera = new VideoCaptureDevice(videoDevices[0].MonikerString);
            //        }
            //        camera.NewFrame += Camera_NewFrame; // 注册摄像头捕获图像的事件
            //        camera.Start(); // 启动摄像头
            //        isCameraRunning = true; // 设置摄像头运行状态为 true
            //    }
            //    else
            //    {
            //        MessageBox.Show("No camera device found.");
            //    }
            //}
        }

        private void StopCamera()
        {
            //if (camera != null && camera.IsRunning)
            //{
            //    camera.SignalToStop();
            //    camera.WaitForStop();
            //    camera.NewFrame -= Camera_NewFrame; // 取消注册摄像头捕获图像的事件
            //    camera = null; // 将摄像头对象置为null，以便下次重新创建
            //    isCameraRunning = false; // 设置摄像头运行状态为 false
            //    isScanningPaused = false;
            //    button7.Text = "Pause";
            //}
        }

        //private void Camera_NewFrame(object sender, NewFrameEventArgs eventArgs)
        //{
        //    //Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
        //    //if (!isScanningPaused)
        //    //{
        //    //    if (previousBitmap != null)
        //    //    {
        //    //        previousBitmap.Dispose();
        //    //    }
        //    //    pictureBox1.Image = bitmap;
        //    //    previousBitmap = bitmap;
        //    //}
        //    //else
        //    //{
        //    //    ScanQRCode(bitmap);
        //    //    bitmap.Dispose();
        //    //}
        //}

        private void button7_Click(object sender, EventArgs e)
        {
            isScanningPaused = !isScanningPaused;
            button7.Text = isScanningPaused ? "Resume" : "Pause";
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

        private void button8_Click(object sender, EventArgs e)
        {
            // Discard capture device.
            this.captureDevice?.Dispose();
            this.captureDevice = null;
        }
    }
}
