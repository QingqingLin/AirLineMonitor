using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperServer
{
    public partial class MainForm : Form
    {
        public static FilterInfo filterInfo = new FilterInfo();

        public ServerManagement serverManagement;

        //服务器通信套接字集合
        public static Dictionary<SocketTypes, Socket> CommunicationSocketDic = new Dictionary<SocketTypes, Socket>();

        //服务器通信线程集合
        public static Dictionary<ThreadType, Thread> CommunicationThreadDic = new Dictionary<ThreadType, Thread>();

        public static bool IsFirstCatch = true;

        public static System.Threading.Timer DelelateTimer;

        public MainForm()
        {
            InitializeComponent();
            ReadClientAndServerIP read = new ReadClientAndServerIP();
            Thread mainThread = new Thread(Start);
            mainThread.IsBackground = true;
            mainThread.Start();
        }

        private void Start()
        {
            try
            {
                serverManagement = new ServerManagement();
                DelelateTimer = new System.Threading.Timer(new TimerCallback(serverManagement.DeleteData), null, -1, 0);
                serverManagement.waitForClient(ReadClientAndServerIP.IpOfClientAndServerDic["ServerToClient"], 9001);
            }
            catch
            {
                //MessageBox.Show("等待客户端问题");
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                myMenu.Show();
            }
        }

        private void toolMenuCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}