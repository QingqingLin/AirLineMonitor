using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serialize;

namespace SuperServer
{

    public class ServerManagement
    {
        bool CatchFlag = false;
        CommunitationToClient communitationToClient;
        ServerToClientSend serverToClientSend;
        CatchData Catch;
        UploadNew uploadnew;
        FilterInfo filterInfo;

        public ServerManagement()
        {
            communitationToClient = new CommunitationToClient();
            serverToClientSend = new ServerToClientSend();
        }

        public IPEndPoint ClientEndPoint { get; private set; }

        /// <summary>
        /// 等待客服端发送命令
        /// </summary>
        public void waitForClient(IPAddress serveIP, int listenPort)
        {
            //注册接收数据函数
            communitationToClient.eRecData += recClientCmd;
            //侦听端口
            communitationToClient.listenClient(listenPort);
        }

        /// <summary>
        /// 接收客服端命令
        /// </summary>
        /// <param name="receiveData">从客户端收到的TCP命令数组</param>
        /// <param name="lenth">从客户端收到的TCP命令数组长度</param>
        /// <returns></returns>
        private bool recClientCmd(byte[] receiveData, int lenth)
        {
            try
            {
                Unpack unpack = GetHeader(receiveData);
                if (unpack.Header.identifier != 0xA1)
                {
                    return false;
                }
                #region 客户端发来的连接请求，将用户筛选信息加载过来
                if (unpack.Header.cTosCommandType == CToSCommandType.ConnectAndFilterInfo)
                {
                    ConnectAndFilter(unpack);
                }
                #endregion
                #region 客户端点击开始监测
                else if (unpack.Header.cTosCommandType == CToSCommandType.RealTimeDataReq)
                {
                    StartMonitor();
                }
                #endregion
                #region 客户端点击暂停或停止
                else if (unpack.Header.cTosCommandType == CToSCommandType.RealTimeDataPause)
                {
                    PauseOrStop();
                }
                #endregion
                #region 客户端请求文件保存目录
                else if (unpack.Header.cTosCommandType == CToSCommandType.GetFileDirectory)
                {
                    FilDirectory();
                }
                #endregion
                #region 客户端请求下载文件（下载指定文件）
                else if (unpack.Header.cTosCommandType == CToSCommandType.DownSeletedFile)
                {
                    DownLoadFileByChoosed(unpack);
                }
                #endregion
                #region 客户端请求文件下载（根据指定时间段下载）
                else if (unpack.Header.cTosCommandType == CToSCommandType.DownTimeFile)
                {
                    DownLoadFileByTime(unpack);
                }
                #endregion
                return true;
            }
            catch
            {
                MessageBox.Show("解命令包问题");
                return false;
            }
        }

        private void DownLoadFileByTime(Unpack unpack)
        {
            UInt16 fileInfoLength = unpack.GetUint16();
            byte[] fileInfo = unpack.GetBytes(fileInfoLength);
            List<DateTime> timeOfFileList = MySerialize.DeSerializeObject(fileInfo) as List<DateTime>;
            List<string> fileList = FileFilter.FindFileList(timeOfFileList);

            uploadnew = new UploadNew(fileList);
            uploadnew.StartSend();
        }

        private void DownLoadFileByChoosed(Unpack unpack)
        {
            List<string> fileList;
            UInt16 fileInfoLength = unpack.GetUint16();
            byte[] fileInfo = unpack.GetBytes(fileInfoLength);
            fileList = MySerialize.DeSerializeObject(fileInfo) as List<string>;
            uploadnew = new UploadNew(fileList);
            uploadnew.StartSend();
        }

        private void FilDirectory()
        {
            //获取文件目录
            DirectoryInfo theFolder = null;
            FileDirectory rootFldrc = new FileDirectory();
            SToCSendDir dirPacket = new SToCSendDir();
            try
            {
                theFolder = new DirectoryInfo(Application.StartupPath + "\\pcap");
                rootFldrc.Name = theFolder.Name;
                rootFldrc.FileType = theFolder.Attributes;
                rootFldrc.FileCreateTime = theFolder.CreationTime;
                rootFldrc.Path = theFolder.FullName;
                rootFldrc.FileDiretoryList = analysisDirectory(theFolder);
                dirPacket.Diretory = MySerialize.SerializeObject(rootFldrc);     //组包
            }
            catch (Exception ex)
            {
                dirPacket.Diretory = null;
            }
            dirPacket.PacketLength = dirPacket.Diretory.Length + 2 + 4;
            dirPacket.PackToClient(dirPacket.PacketLength);
            serverToClientSend.sendSeverCmd(SocketTypes.ClientToServerCommand, dirPacket.Packet.buf_, dirPacket.PacketLength);
        }

        private void PauseOrStop()
        {
            Catch.OutputDataTimer.Dispose();
            Catch.CreatPcapPerDay.Dispose();
            Catch.KeepCatching = false;
            try
            {
                if (MainForm.CommunicationThreadDic.ContainsKey(ThreadType.SerCatchRealDataThread))
                {
                    MainForm.CommunicationThreadDic[ThreadType.SerCatchRealDataThread].Abort();
                    MainForm.CommunicationThreadDic.Remove(ThreadType.SerCatchRealDataThread);
                }
            }
            catch (Exception)
            {
                MainForm.CommunicationThreadDic.Remove(ThreadType.SerCatchRealDataThread);
            }
        }

        private void StartMonitor()
        {
            try
            {
                if (MainForm.CommunicationThreadDic.Keys.Contains(ThreadType.SerCatchRealDataThread))
                {
                    MainForm.CommunicationThreadDic[ThreadType.SerCatchRealDataThread].Abort();
                    MainForm.CommunicationThreadDic.Remove(ThreadType.SerCatchRealDataThread);
                }
                Catch = new CatchData();
                Thread RealTimeDataThread = new Thread(Catch.StartCatch);
                RealTimeDataThread.IsBackground = true;
                RealTimeDataThread.Start(ClientEndPoint);
                MainForm.CommunicationThreadDic.Add(ThreadType.SerCatchRealDataThread, RealTimeDataThread);
            }
            catch
            {
                MessageBox.Show("解开始监测命令问题");
            }
        }

        private void ConnectAndFilter(Unpack unpack)
        {
            SToCConnectAnswer answer = new SToCConnectAnswer();
            GetFilterInfo(unpack);
            if (establishRealTimeConnection())
            {
                answer.RealTImeConnectionFlag = 0x01;
            }
            if (establishCatchSocket())
            {
                answer.CatchSocketFlag = 0x01;
            }
            answer.Packet.PackByte(answer.RealTImeConnectionFlag);
            answer.Packet.PackByte(answer.CatchSocketFlag);
            serverToClientSend.sendSeverCmd(SocketTypes.ClientToServerCommand, answer.Packet.buf_, answer.PacketLength);
        }

        /// <summary>
        /// 解析目录
        /// </summary>
        /// <param name="directoryInfo">存储pcap的根目录</param>
        /// <returns>目录下的文件</returns>
        private List<FileDirectory> analysisDirectory(DirectoryInfo directoryInfo)
        {
            List<FileDirectory> fdlist = new List<FileDirectory>();
            foreach (DirectoryInfo item1 in directoryInfo.GetDirectories())
            {
                if (item1.Attributes == FileAttributes.Directory)
                {
                    //递归调用
                    List<FileDirectory> fdlist1 = analysisDirectory(item1);
                    FileDirectory directoryFd = new FileDirectory();
                    directoryFd.Name = item1.Name;
                    directoryFd.FileType = FileAttributes.Directory;
                    directoryFd.FileCreateTime = item1.CreationTime;
                    directoryFd.Path = item1.FullName;
                    directoryFd.FileDiretoryList = fdlist1;
                    fdlist.Add(directoryFd);
                }
            }
            foreach (FileInfo item1 in directoryInfo.GetFiles("*.pcap"))
            {
                if (item1.Attributes == FileAttributes.Archive)
                {
                    FileDirectory fd = new FileDirectory();
                    fd.Name = item1.Name;
                    fd.FileType = FileAttributes.Archive;
                    fd.FileCreateTime = item1.CreationTime;
                    fd.Size = item1.Length;
                    fd.Path = item1.FullName;
                    fdlist.Add(fd);
                }
            }
            if (fdlist.Count == 0)
            {
                return null;
            }
            return fdlist;
        }

        /// <summary>
        /// 建立捕获实时数据的套接字Socket
        /// </summary>
        private bool establishCatchSocket()
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Raw, ProtocolType.IP);
                socket.Blocking = false;
                socket.Bind(new IPEndPoint(ReadClientAndServerIP.IpOfClientAndServerDic["ServerToCatch"], 0));
                return SetCatchSocketOption(socket);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 设置获取实时数据包的Socket选项
        /// </summary>
        /// <param name="socket"></param>
        private bool SetCatchSocketOption(Socket socket)
        {
            try
            {
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 1);
                byte[] inValue = new byte[4] { 1, 0, 0, 0 };
                byte[] outValue = new byte[4];
                int ioControlCode = unchecked((int)0x98000001);
                int returnCode = socket.IOControl(ioControlCode, inValue, outValue);
                returnCode = outValue[0] + outValue[1] + outValue[2] + outValue[3];
                if (returnCode != 0)
                {
                    return false;
                }
                MainForm.CommunicationSocketDic.Add(SocketTypes.ServerCatchSocket, socket);
                return true;
            }
            catch (SocketException ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 建立转发数据的套接字Socket
        /// </summary>
        private bool establishRealTimeConnection()
        {
            try
            {
                ClientEndPoint = new IPEndPoint(ReadClientAndServerIP.IpOfClientAndServerDic["Client"], 5000);

                Socket RemoteManagementSocket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, ProtocolType.Udp);
                RemoteManagementSocket.Blocking = false;
                RemoteManagementSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                IPEndPoint ServerEndPoint = new IPEndPoint(ReadClientAndServerIP.IpOfClientAndServerDic["ServerToClient"], 5000);
                RemoteManagementSocket.Bind(ServerEndPoint);
                RemoteManagementSocket.Blocking = true;
                if (MainForm.CommunicationSocketDic.Keys.Contains(SocketTypes.ServerToClientData) == false)
                {
                    MainForm.CommunicationSocketDic.Add(SocketTypes.ServerToClientData, RemoteManagementSocket);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取客户端设置的筛选条件
        /// </summary>
        /// <param name="unpack"></param>
        private void GetFilterInfo(Unpack unpack)
        {
            MainForm.DelelateTimer.Change(-1, 0);

            filterInfo = MainForm.filterInfo;
            UInt16 pcapLengthNum = unpack.GetUint16();
            if ((PcapLengthUnit)unpack.GetByte() == PcapLengthUnit.MB)
            {
                filterInfo.PcapLengthNum = pcapLengthNum * 1024 * 1024;
            }
            else if ((PcapLengthUnit)unpack.GetByte() == PcapLengthUnit.GB)
            {
                filterInfo.PcapLengthNum = pcapLengthNum * 1024 * 1024 * 1024;
            }
            filterInfo.Source.Clear();
            filterInfo.Dest.Clear();
            AddDevice(unpack, filterInfo.Source);
            AddDevice(unpack, filterInfo.Dest);
            filterInfo.MaxVOBCToZC = unpack.GetUint16();
            filterInfo.MaxZCToVOBC = unpack.GetUint16();
            filterInfo.MaxVOBCToCI = unpack.GetUint16();
            filterInfo.MaxCIToVOBC = unpack.GetUint16();
            filterInfo.MaxVOBCToATS = unpack.GetUint16();
            filterInfo.MaxATSToVOBC = unpack.GetUint16();
            filterInfo.DeleteCycle = unpack.GetByte();


            MainForm.DelelateTimer.Change(2, 1000 * 60 * 60);
        }

        /// <summary>
        /// 添加源和目的设备
        /// </summary>
        /// <param name="unpack"> TCP命令数据包结构 </param>
        /// <param name="DeviceList"> 用户选择的源和目的设备表 </param>
        private void AddDevice(Unpack unpack, List<DeviceInfo> DeviceList)
        {
            int Num = unpack.GetUint16();
            for (int i = 0; i < Num; i++)
            {
                string first = unpack.GetByte().ToString();
                string second = unpack.GetByte().ToString();
                string third = unpack.GetByte().ToString();
                string fourth = unpack.GetByte().ToString();
                DeviceInfo device = new DeviceInfo();

                device.Port = unpack.GetUint16();
                device.IP = IPAddress.Parse(first + "." + second + "." + third + "." + fourth);
                DeviceList.Add(device);
            }
        }

        /// <summary>
        /// 解TCP命令数据包头部
        /// </summary>
        /// <param name="receiveData"> 捕获的TCP命令数据包 </param>
        /// <returns></returns>
        private Unpack GetHeader(byte[] receiveData)
        {
            Unpack unPack = new Unpack();
            unPack.Reset(receiveData);
            unPack.Header.identifier = unPack.GetByte();
            unPack.Header.cTosCommandType = (CToSCommandType)unPack.GetByte();
            unPack.Header.PacketLength = unPack.GetUint16();
            return unPack;
        }

        public void DeleteData(object o)
        {
            DeleteData delete = new DeleteData(filterInfo.DeleteCycle);
            delete.DeleteOverdueData();
        }
    }
}
