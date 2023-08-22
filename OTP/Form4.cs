using ScreenCapturerNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OTP
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ScreenCapturer.StartCapture((Bitmap bitmap) => {
                // 处理截图 (bitmap) 这里
                // 例如，你可以将截图显示在某个 PictureBox 控件中
                pictureBox1.Image = bitmap;
                ScreenCapturer.StopCapture();
            });
        }
    }
}
