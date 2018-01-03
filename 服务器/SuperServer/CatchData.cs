using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperServer
{
    class CatchData
    {
        Socket socket;
        public bool KeepCatching;
        private int receiveBufferLength;
        public byte[] receiveBufferBytes;
        public static GeneratePcapFile genePcapFile;
        private static string _fileSavePath;
        private static string _fileName;
        public System.Threading.Timer CreatPcapPerDay;
        public System.Threading.Timer OutputDataTimer;

        IPEndPoint RealTimeClientEndPoint;
        public static ConcurrentQueue<PacketProperties> TemporaryStorage;

        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
        }
        public string FileSavePath
        {
            get { return _fileSavePath; }
            set { _fileSavePath = value; }
        }

        public CatchData()
        {
            if (MainForm.IsFirstCatch)
            {
                CreatPcapFile();
            }
            receiveBufferLength = 2000;
            receiveBufferBytes = new byte[receiveBufferLength];
            DateTime dt1 = DateTime.Parse(DateTime.Now.ToShortDateString() + " 23:59:59");
            DateTime dt2 = DateTime.Now;
            TimeSpan ts = new TimeSpan();
            ts = dt1 - dt2;
            Int64 IntervalToTomorrow0 = Convert.ToInt64(ts.TotalMilliseconds + 1800000);
            CreatPcapPerDay = new System.Threading.Timer(new TimerCallback(CreatPcapFilePerDay), null, IntervalToTomorrow0, IntervalToTomorrow0);
            OutputDataTimer = new System.Threading.Timer(new TimerCallback(OutToPcap), null, 200, 200);
        }

        public void StartCatch(object o)
        {
            try
            {
                this.RealTimeClientEndPoint = o as IPEndPoint;
                if (MainForm.IsFirstCatch)
                {
                    this.socket = MainForm.CommunicationSocketDic[SocketTypes.ServerCatchSocket];
                    SetTemporaryStorge();
                    KeepCatching = true;
                    BeginReceive();
                    MainForm.IsFirstCatch = false;
                }
                else
                {
                    this.socket = MainForm.CommunicationSocketDic[SocketTypes.ServerCatchSocket];
                    KeepCatching = true;
                    BeginReceive();
                }
            }
            catch
            {
                MessageBox.Show("抓包问题");
            }
        }

        private void SetTemporaryStorge()
        {
            if (TemporaryStorage == null)
            {
                TemporaryStorage = new ConcurrentQueue<PacketProperties>();
            }
        }

        public void BeginReceive()
        {
            if (KeepCatching)
            {
                if (socket != null)
                {
                    object state = null;
                    state = socket;
                    IAsyncResult ar = socket.BeginReceive(receiveBufferBytes, 0, receiveBufferLength, SocketFlags.None, new AsyncCallback(CallReceive), state);
                }
            }
        }

        HandlePacket Handle;
        private void CallReceive(IAsyncResult ar)
        {
            int receivedLength = socket.EndReceive(ar);
            Handle = new HandlePacket(RealTimeClientEndPoint, receiveBufferBytes, receivedLength);
            Array.Clear(receiveBufferBytes, 0, receiveBufferBytes.Length);
            BeginReceive();
        }


        public void CreatPcapFile()
        {
            genePcapFile = new GeneratePcapFile();
            FileName = System.DateTime.Now.ToString("yyyy") + "." + System.DateTime.Now.ToString("MM") + "." + System.DateTime.Now.ToString("dd") + "  " + System.DateTime.Now.ToString("HH：mm");
            FileSavePath = SetFileSavePath();
            if (File.Exists(FileSavePath + "\\" + FileName + ".pcap"))
            {
                File.Delete(FileSavePath + "\\" + FileName + ".pcap");
                genePcapFile.CreatPcap(FileSavePath, FileName);
            }
            else
            {
                genePcapFile.CreatPcap(FileSavePath, FileName);
            }
        }

        public void CreatPcapFilePerDay(object o)
        {
            try
            {
                genePcapFile = new GeneratePcapFile();
                FileName = System.DateTime.Now.ToString("yyyy") + "." + System.DateTime.Now.ToString("MM") + "." + System.DateTime.Now.ToString("dd") + "  " + System.DateTime.Now.ToString("HH：mm");
                FileSavePath = SetFileSavePath();
                genePcapFile.CreatPcap(FileSavePath, FileName);

                DateTime dt1 = DateTime.Parse(DateTime.Now.ToShortDateString() + " 23:59:59");
                DateTime dt2 = DateTime.Now;
                TimeSpan ts = new TimeSpan();
                ts = dt1 - dt2;
                Int64 IntervalToTomorrow0 = Convert.ToInt64(ts.TotalMilliseconds + 1800000);
                CreatPcapPerDay.Change(IntervalToTomorrow0, IntervalToTomorrow0);
            }
            catch
            {
                MessageBox.Show("每天生成一个Pcap问题");
            }
        }

        private string SetFileSavePath()
        {
            string startPath = Application.StartupPath + "\\pcap";
            string FilePath = startPath + "\\" + System.DateTime.Now.Year + "\\" + System.DateTime.Now.Month + "\\" + System.DateTime.Now.Day;
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }
            return FilePath;
        }

        public void OutToPcap(object o)
        {
            Output(TemporaryStorage);
        }

        private void Output(ConcurrentQueue<PacketProperties> PropertiesList)
        {
            FileInfo File = new FileInfo(FileSavePath + "\\" + FileName + ".pcap");
            while (!PropertiesList.IsEmpty)
            {
                PacketProperties properties;
                PropertiesList.TryDequeue(out properties);
                if (File.Length < MainForm.filterInfo.PcapLengthNum)
                {
                    WriteToPcap(properties.ReceiveBuf, properties.BufLength, properties.CaptureTime);
                }
                else
                {
                    CreatPcapFile();
                    File = new FileInfo(FileSavePath + "\\" + FileName + ".pcap");
                    WriteToPcap(properties.ReceiveBuf, properties.BufLength, properties.CaptureTime);
                }
                HandlePacket.SendRealTimePacket(properties.ReceiveBuf, properties.BufLength, properties.CaptureTime);
            }
        }



        private void WriteToPcap(byte[] packet, int packetlenth, DateTime captureTime)
        {
            genePcapFile.WritePacketData(captureTime, packet, packetlenth);
        }
    }
}
