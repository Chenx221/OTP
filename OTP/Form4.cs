using GuerrillaNtp;
using System;
using System.Windows.Forms;

namespace OTP
{
    public partial class Form4 : Form
    {
        private NtpClient ntpClient;
        private NtpClock clock;
        private Timer timer;

        public Form4()
        {
            InitializeComponent();
            ntpClient = new NtpClient("time.pool.aliyun.com");

            // Create and configure the timer
            timer = new Timer();
            timer.Interval = 1000; // 1 second interval
            timer.Tick += Timer_Tick;
        }

        private async void Form4_Load(object sender, EventArgs e)
        {
            try
            {
                // Display local time
                UpdateLocalTimeLabel();

                // Query time from NTP server
                clock = await ntpClient.QueryAsync();

                // Display network time from NTP server
                UpdateNetworkTimeLabel();

                // Start the timer to update network time periodically
                timer.Start();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during NTP query
                label4.Text = "NTP Error: " + ex.Message;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update the time label
            UpdateLocalTimeLabel();
            UpdateNetworkTimeLabel();
        }
        private void UpdateLocalTimeLabel()
        {
            DateTime localTime = DateTime.Now;
            label2.Text = localTime.ToString();
        }
        private void UpdateNetworkTimeLabel()
        {
            if (clock != null)
            {
                DateTimeOffset networkTime = clock.UtcNow;

                // Convert network time to user's local time zone
                TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
                DateTimeOffset localTime = TimeZoneInfo.ConvertTime(networkTime, localTimeZone);

                label4.Text = localTime.ToString();
            }
        }


        // Form4 closing event
        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop the timer when the form is closing
            timer.Stop();
        }
    }
}

