﻿using System;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;

namespace NetMonitor
{
    public partial class NetMonitor : Form
    {
        int InterfaceSelect = 0;
        int ZreoTimes = 0;
        int NetworkInterfacesLength = 0;
        bool Start = false;
        private NetworkInterface[] nicArr;      //网卡集合
        private System.Timers.Timer timers; 
        //计时器
        
        public NetMonitor()
        {
            InitializeComponent();
            InitNetworkInterface();
            InitializeTimer();
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;
       
        private void InitializeTimer()
        {
            timers = new System.Timers.Timer();
            timers.Interval = 1000;
            timers.Elapsed += timer_Elapsed;
            timers.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
           
            this.Invoke((EventHandler)delegate
            {
                UpdateNetworkInterface();
            });
        }

        private void SetGifBackground()
        {
            Image gif = global::NetMonitor.Resource.doge;
            System.Drawing.Imaging.FrameDimension fd = new System.Drawing.Imaging.FrameDimension(gif.FrameDimensionsList[0]);
            int count = gif.GetFrameCount(fd);    //获取帧数(gif图片可能包含多帧，其它格式图片一般仅一帧)
            System.Windows.Forms.Timer giftimer = new System.Windows.Forms.Timer();
            giftimer.Interval = 120;//这里是可以调节速度的
            int i = 0;
            Image bgImg = null;
            giftimer.Tick += (s, e) =>
            {
                if (i >= count) { i = 0; }
                gif.SelectActiveFrame(fd, i);
                System.IO.Stream stream = new System.IO.MemoryStream();
                gif.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                if (bgImg != null) { bgImg.Dispose(); }
                bgImg = Image.FromStream(stream);
                this.pictureBox.BackgroundImage = bgImg;///
                i++;
            };
            giftimer.Start();
        }

        public void UpdateNetworkInterface()
        {
            int netSend;
            int netRecv;
            NetworkInterface nic = nicArr[ComboBox.SelectedIndex]; 
            IPv4InterfaceStatistics interfaceStats = nic.GetIPv4Statistics();
            if (InterfaceSelect == ComboBox.SelectedIndex&&Start)
            {
                netSend = (int)(interfaceStats.BytesSent - double.Parse(Lable_TotalUP.Text));//这个时是干扰？
                netRecv = (int)(interfaceStats.BytesReceived - double.Parse(Lable_TotalDown.Text));
                Lable_TotalUP.Text = interfaceStats.BytesSent.ToString();
                Lable_TotalDown.Text = interfaceStats.BytesReceived.ToString();
                if(netRecv==0&&netSend==0)
                {
                    ZreoTimes++;
                }
                else
                {
                    ZreoTimes = 0;
                }
                if(ZreoTimes>=10)
                {
                    ComboBox.SelectedIndex++;
                    if (ComboBox.SelectedIndex > NetworkInterfacesLength)
                    {
                        ComboBox.SelectedIndex = 0;
                    }
                    //else
                    //{
                    //    ComboBox.SelectedIndex++;
                    //}

                }

            }
            else
            {
                netSend = 0;
                netRecv = 0;
                Lable_TotalUP.Text = interfaceStats.BytesSent.ToString();
                Lable_TotalDown.Text = interfaceStats.BytesReceived.ToString();
                InterfaceSelect = ComboBox.SelectedIndex;
                Start = !Start;
                
            }

            string netRecvText = "";
            string netSendText = "";
            if (netRecv < 1024)
            {
                netRecvText = ((double)netRecv).ToString("0") + "B/S";
            }
            else if (netRecv<1024*1024)
            {
                netRecvText = ((double)netRecv / 1024).ToString("0.00") + "K/S";
            }
            else if (netRecv >= 1024 * 1024)
            {
                netRecvText = ((double)netRecv / (1024 * 1024)).ToString("0.00") + "M/S";
            }


            if (netSend < 1024)
            {
                netSendText = ((double)netSend ).ToString("0") + "B/S";
            }
            else if (netSend < 1024*1024)
            {
                netSendText = ((double)netSend/1024).ToString("0.00") + "K/S";
            }
            else if (netSend >= 1024 * 1024)
            {
                netSendText = ((double)netSend / (1024 * 1024)).ToString("0.00") + "M/S";
            }


            Lable_SpeedUP.Text = "上传：" + netSendText;
            Lable_SpeedDown.Text = "下载：" + netRecvText;


        }

        public void InitNetworkInterface()
        {
            nicArr = NetworkInterface.GetAllNetworkInterfaces();
            for (int i = 0; i < nicArr.Length; i++)
            {
                ComboBox.Items.Add(nicArr[i].Name);
                NetworkInterfacesLength++;
            }
            ComboBox.SelectedIndex = 0;
            
        }

        private void NetMonitor_Load(object sender, EventArgs e)
        {
            this.panel.BackColor = this.Lable_SpeedUP.BackColor = this.Lable_SpeedDown.BackColor = Color.FromArgb(0, 120, 215);
            SetGifBackground();
        }

        private void NetMonitor_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }

        private void Exit_Menu_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Menu.Hide();//隐藏一些东西
                if (MessageBox.Show("你确定关闭流量悬浮窗么？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    base.Dispose();//这个是啥？我忘了
                    Application.Exit();

                }

            }
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            Menu.Hide();
        }
    }
}
