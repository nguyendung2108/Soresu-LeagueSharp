using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using cb = System.Windows.Forms.Clipboard;
using System.Timers;
using System.Threading;
using System.Net;
using System.Collections.Specialized;

namespace Tesztek
{
    public partial class Form1 : Form
    {
        int XP;
        int yp;
        System.Drawing.Point NewPoint;
        NotifyIcon notifyIcon2 = new NotifyIcon();
        private int notcount = 0;
        public Form1()
        {
            InitializeComponent();
            time.Text = DateTime.Now.ToString("HH:mm:ss");
            statuslabel.Text = "OUTDATED";
            statuslabel.ForeColor = Color.Red;
            GetStatus();
        }

        public void Alert()
        {
            NotifyIcon notifyIcon1 = new NotifyIcon();
            notifyIcon1.BalloonTipText = "L# is probably updated, check it!";
            notifyIcon1.BalloonTipTitle = "L#Checker";
            notifyIcon1.Icon = global::UpdateChecker.Properties.Resources.appicon;
            notifyIcon1.Visible = true;
            notifyIcon1.ShowBalloonTip(3000);

            if (statuslabel.Text == "UPDATED")
            {
                for (int i = 0; i < 5; i++)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(1000);
                    System.Media.SystemSounds.Hand.Play();
                }

            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string min = minutes.Text;
            int min2 = 2;
            button1.Text = "Checking...";
            button1.Enabled = false;
            if (IsNumeric(min))
            {
                min2 = int.Parse(min);
            }
            timer1.Interval = min2*60*1000;
            timer1.Start();
        }

        public static bool IsNumeric(string s)
        {
            float output;
            return float.TryParse(s, out output);
        }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            XP = MousePosition.X - this.Location.X;
            yp = MousePosition.Y - this.Location.Y;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                NewPoint = MousePosition;
                NewPoint.X = NewPoint.X - XP;
                NewPoint.Y = NewPoint.Y - yp;
                this.Location = NewPoint;
            }
        }
   

        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private async void GetStatus()
        {
            System.Uri uri = new System.Uri("http://www.niratisnordkyn.com/DontDeleteThis/ls.php");
            string data = await DownloadStringAsync(uri);
            data = data.ToLower();
            Console.WriteLine(data);
            this.Invoke((MethodInvoker)delegate
                {
                    statuslabel.Text = "OUTDATED";
                    statuslabel.ForeColor = Color.Red;
                });
            if (!data.Contains("outdated") && data.Length>5)
            {
                this.Invoke((MethodInvoker)delegate
                    {
                        statuslabel.Text = "UPDATED";
                        statuslabel.ForeColor = Color.ForestGreen;
                        Alert();
                    });
            }
            this.Invoke((MethodInvoker)delegate
                {
                time.Text = DateTime.Now.ToString("HH:mm:ss");
                });
            Console.WriteLine("Checked: "+DateTime.Now.ToString("HH:mm:ss"));
            
        }
        public static Task<string> DownloadStringAsync(Uri url)
        {

            var tcs = new TaskCompletionSource<string>();
            var wc = new WebClient();
            wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0");
            wc.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");
            wc.Encoding = Encoding.UTF8;
            wc.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null) tcs.TrySetException(e.Error);
                else if (e.Cancelled) tcs.TrySetCanceled();
                else tcs.TrySetResult(e.Result);
            };
            wc.DownloadStringAsync(url);
            
            return tcs.Task;
        }

        private void minimizeToTray_Click(object sender, EventArgs e)
        {
            if (notcount == 0)
            {
                notifyIcon2.BalloonTipText = "You can find me here";
                notifyIcon2.BalloonTipTitle = "L#Checker";
                notifyIcon2.Icon = global::UpdateChecker.Properties.Resources.appicon;
                notifyIcon2.Visible = true;
                notifyIcon2.ShowBalloonTip(2000);
                notcount++;
            }
            else
            {
                notifyIcon2.BalloonTipText = "";
                notifyIcon2.BalloonTipTitle = "";
                notifyIcon2.Icon = global::UpdateChecker.Properties.Resources.appicon;
                notifyIcon2.Visible = true;
            }
            notifyIcon2.MouseClick += new MouseEventHandler(notiHandler);
            this.Hide();
        }

        private void notiHandler(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
            this.Show();
            notifyIcon2.Visible = false;
            notifyIcon2.MouseClick -= notiHandler;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            GetStatus();
            timer1.Start();
        }
    }
}
