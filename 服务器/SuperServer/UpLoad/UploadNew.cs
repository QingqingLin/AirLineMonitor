using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperServer
{
    class UploadNew
    {
        private List<string> FileLists;
        private TcpClient client;
        private int SendNum = 0;
        private Random r = new Random();
        private TcpListener Sendlistener;
        private NetworkStream streamToClient;
        private const int IsSendBufferSize = 1;
        private byte[] IsSendbuffer;
        private byte[] fileBuffer;

        public UploadNew(List<string> fileLists)
        {
            IsSendbuffer = new byte[IsSendBufferSize];
            this.FileLists = fileLists;
            try
            {
                client = new TcpClient();
                client.Connect(ReadClientAndServerIP.IpOfClientAndServerDic["Client"], 8500);
                streamToClient = client.GetStream();
            }
            catch (Exception ex)
            {

            }
        }

        public void StartSend()
        {
            int port;
            while (PortInUse(port = r.Next(1024, 65535))) { }
            IPAddress ip = ReadClientAndServerIP.IpOfClientAndServerDic["ServerToClient"];
            Sendlistener = new TcpListener(ip, port);
            Sendlistener.Start();

            while (SendNum < FileLists.Count)
            {
                SendFileName(FileLists[SendNum]);
                SendNum++;
            }
            byte[] b = { 0 };
            streamToClient.Write(b,0,1);
            Sendlistener.Stop();
            Array.Clear(fileBuffer,0,fileBuffer.Length);
            fileBuffer = null;
            streamToClient.Dispose();
            client.Close();
            //发完了，做内存处理
        }

        private void SendFileName(string file)
        {
            IPEndPoint endPoint = Sendlistener.LocalEndpoint as IPEndPoint;
            int listeningPort = endPoint.Port;

            string fileName = Path.GetFileName(file);
            double length = new FileInfo(file).Length;
            FileProtocol protocol = new FileProtocol(FileRequestMode.Send, listeningPort, fileName, length);
            string pro = protocol.ToString();

            byte[] temp = Encoding.Unicode.GetBytes(pro);

            try
            {
                streamToClient.Write(temp, 0, temp.Length);
                int bytesRead = streamToClient.Read(IsSendbuffer, 0, IsSendBufferSize);

                if (bytesRead > 0)
                {
                    SendFile(IsSendbuffer,file ,protocol);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void SendFile(byte[] isSendBuffer, string fileName, FileProtocol protocol)
        {
            try
            {
                if (IsSendbuffer[0] == 1)
                {
                    TcpClient localClient = Sendlistener.AcceptTcpClient();
                    NetworkStream stream = localClient.GetStream();

                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    fileBuffer = new byte[12345678];
                    double totalLength = 0;
                    int bytesRead;
                    try
                    {
                        do
                        {
                            bytesRead = fs.Read(fileBuffer, 0, fileBuffer.Length);
                            totalLength += bytesRead;
                            stream.Write(fileBuffer, 0, bytesRead);
                        } while (totalLength < protocol.Length);

                        bytesRead = streamToClient.Read(IsSendbuffer, 0, IsSendBufferSize);
                    }
                    catch (Exception ex)
                    { }
                    finally
                    {
                        stream.Dispose();
                        fs.Dispose();
                        localClient.Close();
                    }
                }
                else if (IsSendbuffer[0] == 0)
                {

                }
            }
            catch { }
        }

        private static bool PortInUse(int port)
        {
            bool inUse = false;

            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }
    }
}
