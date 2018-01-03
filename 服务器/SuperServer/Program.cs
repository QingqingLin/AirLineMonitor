using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperServer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //HardwareInfo hardwareInfo = new HardwareInfo();
            //string cpuID = hardwareInfo.GetCpuID();
            //string hardID = hardwareInfo.GetHardDiskID();
            //if (cpuID == "BFEBFBFF000406F1" && hardID == "24094624476481423")
            //{
                new MainForm();
                Application.Run();
            //}
            //else
            //{
            //    MessageBox.Show("非本机，不能使用！");
            //    Application.Exit();
            //}
        }
    }
}
