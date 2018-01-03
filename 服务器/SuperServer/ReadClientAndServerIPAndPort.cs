using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace SuperServer
{
    class ReadClientAndServerIP
    {
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key,
   string val, string filePath);

        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);

        public ReadClientAndServerIP()
        {
            foreach (string Device in Section)
            {
                IpOfClientAndServerDic.Add(Device, IPAddress.Parse(ReadIniData(Device, "IP", "", IPConfigPath)));
            }
        }

        public static Dictionary<string, IPAddress> IpOfClientAndServerDic = new Dictionary<string, IPAddress>();
        StringBuilder temp = new StringBuilder(300);
        string IPConfigPath = System.AppDomain.CurrentDomain.BaseDirectory + "ClientAndServerIPList.ini";
        string[] Section = { "Client", "ServerToCatch", "ServerToClient" };

        #region 读Ini文件
        public string ReadIniData(string Section, string Key, string NoText, string iniFilePath)
        {
            if (File.Exists(iniFilePath))
            {
                GetPrivateProfileString(Section, Key, NoText, temp, 1024, iniFilePath);

                return temp.ToString();
            }
            else
            {
                return String.Empty;
            }
        }
        #endregion
    }
}
