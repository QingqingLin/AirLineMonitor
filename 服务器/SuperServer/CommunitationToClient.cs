using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperServer
{
    public enum SocketTypes
    {
        /// <summary>
        /// 客户端向服务器端发送命令
        /// </summary>
        ClientToServerCommand,
        /// <summary>
        /// 服务器向客户端发送数据
        /// </summary>
        ServerToClientData,
        /// <summary>
        /// 服务器抓取数据Socket
        /// </summary>
        ServerCatchSocket,
    }

    public enum ThreadType
    {
        /// <summary>
        /// 服务器抓取实时数据线程
        /// </summary>
        SerCatchRealDataThread,
        /// <summary>
        /// 循环接收命令线程
        /// </summary>
        ReceiveCmdThread
    }

    public enum CToSCommandType : Byte
    {
        /// <summary>
        /// 客户端向服务器连接并发送用户筛选信息
        /// </summary>
        ConnectAndFilterInfo = 0x01,
        /// <summary>
        /// 客户端向服务器请求发送实时数据
        /// </summary>
        RealTimeDataReq = 0x02,
        /// <summary>
        /// 客户端向服务器发送实时数据暂停命令
        /// </summary>
        RealTimeDataPause = 0x03,
        /// <summary>
        /// 客户端向服务器请求文件目录信息命令
        /// </summary>
        GetFileDirectory = 0x04,
        /// <summary>
        /// 客户端向服务器请求下载选中文件命令
        /// </summary>
        DownSeletedFile = 0x05,
        /// <summary>
        /// 客户端向服务器请求下载指定时间段内文件命令
        /// </summary>
        DownTimeFile = 0x06
    }

    public enum SToCAnswerType : Byte
    {
        /// <summary>
        /// 服务器向客户端回复连接成功相应
        /// </summary>
        ConnectAndFilterInfoAnswer = 0x01,
        /// <summary>
        /// 服务器向客户端回复文件目录信息
        /// </summary>
        FileListAnswer = 0x04
    }

    class CommunitationToClient
    {
        #region 委托和事件的定义
        public delegate bool DelRecvData(byte[] recData, int lenth);
        public event DelRecvData eRecData = null;
        #endregion

        /// <summary>
        /// 侦听客服端请求
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void listenClient(int port)
        {
            Socket TCPCommandWatchSocket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipe = new IPEndPoint(ReadClientAndServerIP.IpOfClientAndServerDic["ServerToClient"], port);
            TCPCommandWatchSocket.Bind(ipe);
            TCPCommandWatchSocket.Listen(1);
            listenPort(TCPCommandWatchSocket);
        }

        private void listenPort(Socket TCPCommandWatchSocket)
        {
            while (true)
            {
                //侦听连接请求
                Socket socket = TCPCommandWatchSocket.Accept();
                if (!MainForm.CommunicationSocketDic.Keys.Contains(SocketTypes.ClientToServerCommand))
                {
                    MainForm.CommunicationSocketDic.Add(SocketTypes.ClientToServerCommand, socket);
                }
                else
                {
                    MainForm.CommunicationSocketDic[SocketTypes.ClientToServerCommand].Close();
                    MainForm.CommunicationSocketDic[SocketTypes.ClientToServerCommand].Dispose();
                    MainForm.CommunicationSocketDic[SocketTypes.ClientToServerCommand] = socket;
                }

                Thread ReceiveThread = new Thread(communicate);
                ReceiveThread.IsBackground = true;
                ReceiveThread.Start(socket);

                if (MainForm.CommunicationThreadDic.Keys.Contains(ThreadType.ReceiveCmdThread))
                {
                    MainForm.CommunicationThreadDic[ThreadType.ReceiveCmdThread].Abort();
                    MainForm.CommunicationThreadDic[ThreadType.ReceiveCmdThread] = ReceiveThread;
                }
                else
                {
                    MainForm.CommunicationThreadDic.Add(ThreadType.ReceiveCmdThread, ReceiveThread);
                }
            }
        }

        private void communicate(object Socket)
        {
            Socket socket = Socket as Socket;

            byte[] header = new byte[4];
            byte[] tem = new byte[300];
            try
            {
                while (true)
                {
                    int receiveNum = socket.Receive(header);
                    Int32 Length = 0;
                    if (receiveNum == 4)
                    {
                        Length = BitConverter.ToInt16(header, 2);
                        int totalLength = Length;
                        if (Length > 4)
                        {
                            byte[] total = new byte[Length];
                            Array.Copy(header, 0, total, 0, receiveNum);
                            int index = receiveNum;
                            Array.Clear(header, 0, header.Length);
                            while ((Length = Length - receiveNum) > 0)
                            {
                                receiveNum = socket.Receive(tem);
                                Array.Copy(tem, 0, total, index, receiveNum);
                                index = index + receiveNum;
                                Array.Clear(tem, 0, tem.Length);
                            }
                            if (eRecData != null)
                            {
                                eRecData(total, totalLength);
                            }
                            Array.Clear(tem, 0, tem.Length);
                        }
                        else if (Length == 4)
                        {
                            if (eRecData != null)
                            {
                                eRecData(header, Length);
                            }
                            Array.Clear(header, 0, header.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
